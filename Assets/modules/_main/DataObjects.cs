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
    public Prompt prompt;
    public ExtraOptions extraOptionsFull; // special aiimag.es options
    private Texture2D tex;

    // telemetry
    public System.DateTime dateCreation;
    public string strGpuModel = "";
    public User userCreator;
    public float fGenerationTime = 0f;

    public UnityEvent<ImageInfo> eventStartsProcessing = new UnityEvent<ImageInfo>();
    public UnityEvent<ImageInfo, bool> eventStoppedProcessing = new UnityEvent<ImageInfo, bool>();

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
            tex = Utility.texLoadImageSecure(strFilePathFull(), ToolManager.s_texDefaultMissing);
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
    public int iSteps = 50;
    public float fCfgScale = 7.5f;
    public string strContentPrompt = "";
    public string strStylePrompt = "";

    public string strToString()
    {
        string strPrompt = strWithoutOptions();
        strPrompt += $" -s {iSteps} -S {iSeed} -W {iWidth} -H {iHeight} -C {fCfgScale.ToString("0.0", CultureInfo.InvariantCulture)}";

        if (!string.IsNullOrEmpty(startImage.strFilePath))
            strPrompt += $" --init_img=\"{startImage.strGetFullPath()}\" --strength={startImage.fStrength.ToString("0.0", CultureInfo.InvariantCulture)}";

        return strPrompt;
    }

    public string strWithoutOptions()
    {
        string strPrompt = "";

        strPrompt += "\"";
        strPrompt += strContentPrompt.Trim().Replace("\r", "").Replace("\n", "").Replace("\"", "");
        strPrompt += (!string.IsNullOrEmpty(strContentPrompt) && !string.IsNullOrEmpty(strStylePrompt)) ? ", " : "";
        strPrompt += strStylePrompt.Trim().Replace("\r", "").Replace("\n", "").Replace("\"", ""); ;
        strPrompt += "\"";

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
    public int iSeedVariance = 0;
    public float fCfgScaleVariance = 0f;
    public bool bRandomSeed = true;
    public int iStepsPreview = 15;
    public int iStepsRedo = 50;
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
