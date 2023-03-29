using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Events;
using System.Globalization;

/*
public class ImageData
{
    public string strGUID;
    public string strPathRelative;
    public int iHeight = 512;
    public int iWidth = 512;

    public PromptData prompt;
    public User user;
}

public class PromptData
{
    public string strContent = "";
    public string strStyle = "";

    public int iSeed = 0;
    public int iSteps = 10;
    public float fCfgScale = 7.5f;
    public int iHeight = 512;
    public int iWidth = 512;

    public string strGetFullPrompt()
    {
        // combines the prompt;
        return "";
    }
}
*/

[System.Serializable]
public class ImageInfo
{
    public string strGUID = "";
    public string strFilePathRelative = "";
    public Prompt prompt = new Prompt();
    public ExtraOptions extraOptionsFull = new ExtraOptions(); // special aiimag.es options

    // telemetry
    public System.DateTime dateCreation;
    public int iActualWidth = -1;
    public int iActualHeight = -1;
    public string strGpuModel = "";
    public User userCreator;
    public float fGenerationTime = 0f;

    public UnityEvent<ImageInfo> eventStartsProcessing = new UnityEvent<ImageInfo>();
    public UnityEvent<ImageInfo, bool> eventStoppedProcessing = new UnityEvent<ImageInfo, bool>();

    private Texture2D tex;

    public string strFilePathFull()
    {
        return Path.Combine(ToolManager.s_settings.strOutputDirectory, strFilePathRelative);
    }

    public string strFilePathFullInput()
    {
        return Path.Combine(ToolManager.s_settings.strInputDirectory, strFilePathRelative);
    }

    public Texture2D texGet()
    {
        if (tex == null)
        {
            tex = Utility.texLoadImageSecure(strFilePathFull(), ToolManager.s_texDefaultMissing);
            if (tex != ToolManager.s_texDefaultMissing && iActualWidth < 0) // update for old pictures
            {
                iActualWidth = tex.width;
                iActualHeight = tex.height;
            }   
        }
            
        return tex;
    }

    public void SetTex(Texture2D _tex)
    {
        tex = _tex;
    }

    public bool bHasTexture()
    {
        return tex != null && tex != default;
    }

    /// <summary>
    /// Shallow copy, except for pure data elements. Sets different Guid.
    /// </summary>
    public ImageInfo outputCopy()
    {
        ImageInfo output = new ImageInfo()
        {
            strGUID = System.Guid.NewGuid().ToString(),
            strFilePathRelative = strFilePathRelative,
            prompt = JsonConvert.DeserializeObject<Prompt>(JsonConvert.SerializeObject(prompt)), // not performant, but I'm lazy :(
            extraOptionsFull = JsonConvert.DeserializeObject<ExtraOptions>(JsonConvert.SerializeObject(extraOptionsFull)),
            dateCreation = System.DateTime.UtcNow,
            userCreator = userCreator
        };

        return output;
    }

    public bool bIsRedo()
    {
        return extraOptionsFull.bRedoEmbiggen || extraOptionsFull.bRedoFaceEnhance || extraOptionsFull.bRedoUpscale;
    }

    public string strGetRedoPrompt()
    {
        string strPrompt = $"!fix \"{strFilePathFull()}\"";

        if (extraOptionsFull.bRedoUpscale && prompt.fUpscaleFactor > 1f)
            strPrompt += $" -U {(int)prompt.fUpscaleFactor} {prompt.fUpscaleStrength.ToString("0.00", CultureInfo.InvariantCulture)}";

        if (extraOptionsFull.bRedoEmbiggen && prompt.fEmbiggen > 1f)
            strPrompt += $" --embiggen {prompt.fEmbiggen}";

        if (extraOptionsFull.bRedoFaceEnhance && prompt.fFaceEnhanceStrength > 0f)
            strPrompt += $" -G {prompt.fFaceEnhanceStrength.ToString("0.00", CultureInfo.InvariantCulture)}";

        return strPrompt;
    }
}

/// <summary>
/// Data that could be part of a prompt with options.
/// </summary>
[System.Serializable]
public class Prompt
{
    public int iWidth = 512;
    public int iHeight = 512;
    public StartImage startImage = new StartImage();
    public int iSeed = -1;
    public List<System.Tuple<int, float>> liVariations = new List<System.Tuple<int, float>>();
    public float fUpscaleFactor = 1f;
    public float fUpscaleStrength = 0.75f;
    public float fEmbiggen = 1f;
    public float fFaceEnhanceStrength = 0.0f;
    public bool bSeamless = false;
    public int iSteps = 50;
    public float fCfgScale = 7.5f;
    public string strContentPrompt = "";
    public string strStylePrompt = "";
    public string strSampler = "";
    public string strExtraParams = "";

    public string strToString()
    {
        string strPrompt = $"\"{strWithoutOptions()}\"";
        strPrompt += $" -s {iSteps}" +
            $" -C {fCfgScale.ToString("0.00", CultureInfo.InvariantCulture)}";

        if (string.IsNullOrEmpty(startImage.strFilePath))
            strPrompt += $" -W {iWidth} -H {iHeight}";

        if (!string.IsNullOrEmpty(startImage.strFilePath))
            strPrompt += $" --init_img=\"{startImage.strGetFullPath()}\" --strength={startImage.fStrength.ToString("0.000", CultureInfo.InvariantCulture)}";

        if (!string.IsNullOrEmpty(strSampler))
            strPrompt += $" --sampler {strSampler}";

        strPrompt += strGetSeed();

        if (fUpscaleFactor > 1f)
            strPrompt += $" -U {(int)fUpscaleFactor} {fUpscaleStrength.ToString("0.00", CultureInfo.InvariantCulture)}";

        if (fEmbiggen > 1f)
            strPrompt += $" --embiggen {fEmbiggen}";

        if (fFaceEnhanceStrength > 0f)
            strPrompt += $" -G {fFaceEnhanceStrength.ToString("0.00", CultureInfo.InvariantCulture)}";

        if (bSeamless)
            strPrompt += $" --seamless";

        if (!string.IsNullOrEmpty(strExtraParams))
            strPrompt += $" {strExtraParams}";

        return strPrompt;
    }

    public string strGetSeed()
    {
        return strGetSeed(iSeed, liVariations);
    }

    public static string strGetSeed(int _iSeed, List<System.Tuple<int, float>> _liVariations)
    {
        string strOutput = "";

        strOutput += $" -S {_iSeed}";

        if (_iSeed >= 0 && _liVariations.Count() > 0)
        {
            strOutput += $" -V ";
            for (int iVariation = 0; iVariation < _liVariations.Count; iVariation++)
            {
                System.Tuple<int, float> tuVariation = _liVariations[iVariation];
                strOutput += $"{tuVariation.Item1}:{ tuVariation.Item2.ToString("0.000", CultureInfo.InvariantCulture)}{(iVariation + 1 < _liVariations.Count ? "," : "")}";
            }
        }

        return strOutput;
    }

    public string strWithoutOptions()
    {
        string strPrompt = "";

        strPrompt += strContentPrompt.Trim().Replace("\r", "").Replace("\n", "").Replace("\"", "");
        strPrompt += (!string.IsNullOrEmpty(strContentPrompt) && !string.IsNullOrEmpty(strStylePrompt)) ? ", " : "";
        strPrompt += strStylePrompt.Trim().Replace("\r", "").Replace("\n", "").Replace("\"", ""); ;

        return strPrompt;
    }

    public bool bEqualExceptSteps(Prompt _promptOther, bool _bIgnoreSeed = false)
    {
        return  (_bIgnoreSeed || iSeed == _promptOther.iSeed)
            && startImage == _promptOther.startImage
            && iWidth == _promptOther.iWidth
            && iHeight == _promptOther.iHeight
            && fCfgScale == _promptOther.fCfgScale
            && strContentPrompt == _promptOther.strContentPrompt
            && strStylePrompt == _promptOther.strStylePrompt;
    }

    public bool bEqualContentStyle(Prompt _promptOther)
    {
        return //strContentPrompt.First() == _promptOther.strContentPrompt.First() // catching the obvious cases first
            //&& strStylePrompt.First() == _promptOther.strStylePrompt.First()
            strContentPrompt == _promptOther.strContentPrompt
            && strStylePrompt == _promptOther.strStylePrompt;
    }
}

[System.Serializable]
public class StartImage
{
    public string strFilePath = ""; // relative path of file in inputs folder
    public float fStrength = 0.75f;
    public string strGUID = ""; // optional, if it's an image from the history

    public string strGetFullPath()
    {
        return Path.Combine(ToolManager.s_settings.strInputDirectory, strFilePath);
    }
}

[System.Serializable]
public class User
{
    public string strGUID = "";
    public string strName = "dreamer1";
}

[System.Serializable]
public class ExtraOptions
{
    public float fStartImageStrengthVariance = 0f;
    public float fStartImageScaling = 512f;
    public string strStartImageOriginalName = "";
    public Vector2Int v2iOriginalSize = Vector2Int.zero;
    public float fCfgScaleVariance = 0f;
    public bool bRandomSeed = true;
    public string strSeedReferenceGUID = "";
    public int iStepsPreview = 15;
    public int iStepsRedo = 50;
    public float fVariationStrength = 0.1f;
    public float fUpscalePreview = 1f;
    public float fUpscaleRedo = 2f;
    public float fUpscaleStrengthPreview = 0.5f;
    public float fUpscaleStrengthRedo = 0.75f;
    public float fEmbiggenPreview = 0.0f;
    public float fEmbiggenRedo = 2f;
    public string strUpscaleMethod = "esrgan";
    public float fFaceEnhancePreview = 0.0f;
    public float fFaceEnhanceRedo = 0.75f;
    public bool bRedoEmbiggen = false;
    public bool bRedoUpscale = false;
    public bool bRedoFaceEnhance = false;
}

[System.Serializable]
public class Template
{
    public string strName = "";
    public string strGUID = "";

    public ImageInfo outputTemplate;
}

[System.Serializable]
public class History
{
    public List<ImageInfo> liOutputs = new List<ImageInfo>(); // TODO: That should probably be a GUID->Output dict...?
    public List<SectionData> liSections = new List<SectionData>();
    public string strSaveName = "history.json";

    public ImageInfo imgByGUID(string _strGUID)
    {
        if (!liOutputs.Any(x => x.strGUID == _strGUID))
        {
            Debug.LogWarning($"History: Could not find GUID {_strGUID}!");
            return null;
        }
        else
            return liOutputs.First(x => x.strGUID == _strGUID);
    }

    public void Save()
    {
        Debug.Log("Saving history.");

        string strHistoryPath = Path.Combine(Application.persistentDataPath, strSaveName);
        if (File.Exists(strHistoryPath))
            File.Delete(strHistoryPath);
        File.WriteAllText(strHistoryPath, JsonUtility.ToJson(this, prettyPrint: true));
    }

    public History Load()
    {
        Debug.Log("Loading history.");

        History history = new History();
        string strHistoryPath = Path.Combine(Application.persistentDataPath, strSaveName);
        if (File.Exists(strHistoryPath))
            history = JsonUtility.FromJson<History>(File.ReadAllText(strHistoryPath));
        return history;
    }

    public void MakeBackup()
    {
        int iBackupNo = 1;

        string strHistoryPath = Path.Combine(Application.persistentDataPath, strSaveName);

        string strHistoryFileName = $"history-{iBackupNo}.json";
        while (File.Exists(Path.Combine(Application.persistentDataPath, strHistoryFileName)))
        {
            iBackupNo++;
            strHistoryFileName = $"history-{iBackupNo}.json";
        }

        string strHistoryBackupPath = Path.Combine(Application.persistentDataPath, strHistoryFileName);
        Debug.Log($"Creating history backup at {strHistoryBackupPath}.");
        File.WriteAllText(strHistoryBackupPath, File.ReadAllText(strHistoryPath));
    }
}

[System.Serializable]
public class Palette
{
    public List<ImageInfo> liImages = new List<ImageInfo>();
}

/// <summary>
/// What is actually saved to a json.
/// </summary>
[System.Serializable]
public class SaveData
{
    public List<Template> liStyleTemplates = new List<Template>();
    public List<Template> liContentTemplates = new List<Template>();
    public List<User> liUsers = new List<User>();
    public List<string> liFavoriteGUIDs = new List<string>();
    public Palette palette = new Palette();

    public static string strSaveName = "savedata.json";

    public void Save()
    {
        Debug.Log("Saving savegame.");

        string strHistoryPath = Path.Combine(Application.persistentDataPath, strSaveName);
        if (File.Exists(strHistoryPath))
            File.Delete(strHistoryPath);
        File.WriteAllText(strHistoryPath, JsonUtility.ToJson(this, prettyPrint: true));
    }

    public static SaveData Load()
    {
        Debug.Log("Loading savegame");

        SaveData saveData = new SaveData();
        string strHistoryPath = Path.Combine(Application.persistentDataPath, strSaveName);
        if (File.Exists(strHistoryPath))
            saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(strHistoryPath));
        return saveData;
    }
}
