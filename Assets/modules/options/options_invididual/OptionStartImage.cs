using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using SFB;
using System.Linq;
using B83.TextureTools;

public class OptionStartImage : MonoBehaviour
{
    [SerializeField] private StartImage startImage = new StartImage();
    //public float fStrengthValue { get => (1f - optionSlider.fValue); }

    public OptionSlider optionSliderStrength;
    public OptionSlider optionSliderScaling;
    public ImagePreview imagePreviewInput;
    public Texture2D texDefault;
    public DrawWindow drawWindow;
    public CanvasGroup canvasOptions;
    public CanvasGroup canvasScaling;

    public string strOriginalFile = "";
    public Vector2Int v2iOriginalSize = Vector2Int.zero;
    public bool bIsExternalImage = false;

    private ImageInfo imgPreview = new ImageInfo();

    private void Awake()
    {
        optionSliderStrength.eventValueChanged.AddListener((float _fValue) => startImage.fStrength = 1f - _fValue);
    }

    private void Start()
    {
        UpdateDisplay();
    }

    public void Set(ImageInfo _img)
    {
        if (!string.IsNullOrEmpty(_img.prompt.startImage.strFilePath))
            LoadImageFromFileName(_img.prompt.startImage.strFilePath, _bIsOriginal: true, _bMightBeExternal: false);
        else
        {
            startImage.strFilePath = "";
            startImage.strGUID = "";
        }

        strOriginalFile = _img.extraOptionsFull.strStartImageOriginalName;
        v2iOriginalSize = _img.extraOptionsFull.v2iOriginalSize;
        
        optionSliderStrength.Set(_img.extraOptionsFull.fStartImageStrengthVariance);
        optionSliderScaling.Set(_img.extraOptionsFull.fStartImageScaling);

        UpdateDisplay();
    }

    public void LoadImageFromHistory(string _strGUID)
    {
        // load image, move to inputs
        imgPreview = ToolManager.s_history.imgByGUID(_strGUID);

        startImage.strFilePath = imgPreview.strFilePathRelative;
        startImage.strGUID = _strGUID;

        string strPathInOutput = Path.Combine(ToolManager.s_settings.strOutputDirectory, imgPreview.strFilePathRelative);
        if (!File.Exists(strPathInOutput))
        {
            Debug.LogWarning($"OptionStartImage: Could not find file {strPathInOutput}!");
            return;
        }

        if (!File.Exists(strGetFullFilePath()))
            File.Copy(strPathInOutput, strGetFullFilePath());

        // reset original settings, as those only apply to external images
        strOriginalFile = imgPreview.strFilePathRelative;
        v2iOriginalSize = new Vector2Int(imgPreview.texGet().width, imgPreview.texGet().height);

        UpdateDisplay();
    }

    public void LoadImageFromHistory(ImageInfo _img)
    {
        if (_img != null)
        {
            if (!string.IsNullOrEmpty(_img.strGUID))
            {
                LoadImageFromHistory(_img.strGUID);
            }
            else if (!string.IsNullOrEmpty(_img.strFilePathFull()))
            {
                LoadImageFromFileName(_img.strFilePathFull(), true, false);
            }
        }
    }

    public void LoadImageFromFileName(string _strFilePathFull)
    {
        LoadImageFromFileName(_strFilePathFull, _bIsOriginal: true, _bMightBeExternal:true);
    }

    public void LoadImageFromFileName(string _strFilePathFull, bool _bIsOriginal, bool _bMightBeExternal)
    {
        bIsExternalImage = false;

        startImage.strFilePath = Path.GetFileName(_strFilePathFull);
        startImage.strGUID = "";

        // in case person chose from outputs, get the guid
        if (_strFilePathFull.Contains(ToolManager.s_settings.strOutputDirectory))
        {
            imgPreview = ToolManager.s_history.liOutputs.FirstOrDefault(x => x.strFilePathRelative == _strFilePathFull.Replace(ToolManager.s_settings.strOutputDirectory, ""));
            if (imgPreview != default)
            {
                startImage.strGUID = imgPreview.strGUID;

                // reset original settings, as those only apply to external images
                strOriginalFile = "";
                v2iOriginalSize = Vector2Int.zero;
            }
        }
        else
        {
            if(_bMightBeExternal)
                bIsExternalImage = true;
        }

        Texture2D texInputImage = new Texture2D(1, 1);

        try
        {
            imgPreview = new ImageInfo();

            if (!File.Exists(strGetFullFilePath()))
                File.Copy(_strFilePathFull, strGetFullFilePath());

            // scale down external images
            if (bIsExternalImage)
            {
                if (_bIsOriginal)
                {
                    strOriginalFile = Path.GetFileName(strGetFullFilePath());

                    Texture2D tex = new Texture2D(1, 1);
                    byte[] arBytes = File.ReadAllBytes(strGetFullFilePath(strOriginalFile));
                    tex.LoadImage(arBytes);
                    v2iOriginalSize = new Vector2Int(tex.width, tex.height);
                    Destroy(tex); // we only want width/height and not keep a (possibly massive) texture in vram
                }
            }

            texInputImage = Utility.texLoadImageSecure(strGetFullFilePath(), ToolManager.Instance.texDefaultMissing);
            imgPreview.strFilePathRelative = strGetFullFilePath();
            imgPreview.SetTex(texInputImage);

            if (!imgPreview.strFilePathRelative.Contains("_masked")) // don't update masked images, so the mask is not removed
                UpdateStartImageScaling();
        }
        catch (System.Exception _ex)
        {
            startImage.strFilePath = "";
            Debug.Log(_ex.Message);
        }

        ToolManager.Instance.options.SetAspectRatio(texInputImage.width, texInputImage.height);

        UpdateDisplay();
    }

    public void LoadImageFromPC()
    {
        var extensions = new[] {
            new ExtensionFilter("Image Files", "bmp", "exr", "gif", "hdr", "iff", "pict", "psd", "tga", "tiff", "png", "jpg", "jpeg" )
        };
        string[] arPaths = StandaloneFileBrowser.OpenFilePanel("Open Picture", "", extensions, false);

        if (arPaths.Length == 0)
        {
            Debug.Log("OptionStartImage: No image selected.");
            return;
        }

        LoadImageFromFileName(arPaths[0], _bIsOriginal:true, _bMightBeExternal:true);
    }

    public void OpenDrawWindow()
    {
        if (imgPreview == null)
            return;

        UpdateStartImageScaling(_b64Multiple:true, _bForceUpdate:true);

        if (!imgPreview.strFilePathRelative.Contains("_masked"))
            optionSliderStrength.Set(0.1f); // turn down influence if inpainting is started. Usually it's not wanted.

        drawWindow.OpenImage(imgPreview, LoadImageFromFileName);
    }

    public void Remove()
    {
        startImage.strFilePath = "";
        startImage.strGUID = "";

        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        bool bImageSet = !string.IsNullOrEmpty(startImage.strFilePath);
        bool bIsExternalImage = string.IsNullOrEmpty(startImage.strGUID);

        if (bImageSet)
            imagePreviewInput.DisplayImage(imgPreview);
        else
            imagePreviewInput.DisplayEmpty();

        canvasOptions.alpha = bImageSet ? 1f : 0.3f;
        canvasOptions.interactable = bImageSet ? true : false;

        if (bImageSet)
        {
            canvasScaling.alpha = bIsExternalImage ? 1f : 0.3f;
            canvasScaling.interactable = bIsExternalImage ? true : false;
        }
    }

    public StartImage startimageGet()
    {
        UpdateStartImageScaling();
        return startImage;
    }

    /// <summary>
    /// If necessary, replaces the StartImage with a downscaled version of the original. Only for external images.
    /// </summary>
    private void UpdateStartImageScaling(bool _b64Multiple = false, bool _bForceUpdate = false)
    {
        if (!_bForceUpdate && !bIsExternalImage)
            return;

        // check if scaling is necessary (+-32)
        int iPixelBudgetMin = (int)Mathf.Pow(optionSliderScaling.fValue - 32, 2);
        int iPixelBudgetMax = (int)Mathf.Pow(optionSliderScaling.fValue + 32, 2);
        int iPixelSize = imgPreview.texGet().width * imgPreview.texGet().height;

        if (iPixelSize > iPixelBudgetMin && iPixelSize < iPixelBudgetMax)
            return;

        // pixel size was wrong, so scale original pixel to correct size

        // scale down
        int iPixelBudget = (int)Mathf.Pow(optionSliderScaling.fValue, 2); // basis: 512x512 image
        Vector2Int v2iPixelSize = Utility.v2iGetPixelSize(v2iOriginalSize.x, v2iOriginalSize.y, iPixelBudget);
        if (_b64Multiple)
        {
            v2iPixelSize.x = Mathf.RoundToInt((float)v2iPixelSize.x / 64f) * 64;
            v2iPixelSize.y = Mathf.RoundToInt((float)v2iPixelSize.y / 64f) * 64;
        }

        if (imgPreview.texGet().width == v2iPixelSize.x && imgPreview.texGet().height == v2iPixelSize.y) // don't update image if it already has right size. Might slip through pixel budget check because of rounding
            return; 

        try
        {
            // load original texture
            Texture2D texOriginal = new Texture2D(1, 1);
            byte[] arBytes = File.ReadAllBytes(strGetFullFilePath(strOriginalFile));
            texOriginal.LoadImage(arBytes);

            Texture2D texNew = texOriginal.ResampleAndLetterbox(v2iPixelSize.x, v2iPixelSize.y, Color.black, _bNoAlpha:true);
            Destroy(texOriginal);

            // save downscaled to file
            string strFileName = Path.Combine(Path.GetDirectoryName(strGetFullFilePath()), $"{System.Guid.NewGuid().ToString().Replace('-', '_')}.png");
            File.WriteAllBytes(strFileName, ImageConversion.EncodeToPNG(texNew));

            // load said file
            LoadImageFromFileName(strFileName, _bIsOriginal: false, _bMightBeExternal: false);
        }
        catch
        {
            Debug.LogError($"Could not downscale {strOriginalFile}. Removing image.");
            Remove();
            return;
        }
    }

    public string strGetFullFilePath(string _strFileName = "")
    {
        string strFileName = string.IsNullOrEmpty(_strFileName) ? startImage.strFilePath : _strFileName;
        return Path.Combine(ToolManager.s_settings.strInputDirectory, strFileName);
    }
}
