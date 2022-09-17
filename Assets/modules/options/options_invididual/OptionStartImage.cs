using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using SFB;
using System.Linq;

public class OptionStartImage : MonoBehaviour
{
    public StartImage startImage = new StartImage();
    //public float fStrengthValue { get => (1f - optionSlider.fValue); }

    public OptionSlider optionSlider;
    public ImagePreview imagePreviewInput;
    public Texture2D texDefault;
    public DrawWindow drawWindow;

    private ImageInfo imgPreview = new ImageInfo();

    private void Awake()
    {
        optionSlider.eventValueChanged.AddListener((float _fValue) => startImage.fStrength = 1f - _fValue);
    }

    private void Start()
    {
        UpdateDisplay();
    }

    public void LoadImageFromHistory(string _strGUID)
    {
        // load image, move to inputs
        imgPreview = ToolManager.s_history.imgByGUID(_strGUID);

        startImage.strFilePath = imgPreview.strFilePathRelative;
        startImage.strGUID = _strGUID;

        string strPathInOutput = Path.Combine(ToolManager.s_settings.strOutputDirectory, startImage.strFilePath);
        if (!File.Exists(strPathInOutput))
        {
            Debug.LogWarning($"OptionStartImage: Could not find file {strPathInOutput}!");
            return;
        }

        if (!File.Exists(strGetFullFilePath()))
            File.Copy(strPathInOutput, strGetFullFilePath());

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
                LoadImageFromFileName(_img.strFilePathFull());
            }
        }
            
    }

    public void LoadImageFromFileName(string _strFilePathFull)
    {
        startImage.strFilePath = Path.GetFileName(_strFilePathFull);
        startImage.strGUID = "";

        // in case person chose from outputs, get the guid
        if (_strFilePathFull.Contains(ToolManager.s_settings.strOutputDirectory))
        {
            imgPreview = ToolManager.s_history.liOutputs.FirstOrDefault(x => x.strFilePathRelative == _strFilePathFull.Replace(ToolManager.s_settings.strOutputDirectory, ""));
            if (imgPreview != default)
                startImage.strGUID = imgPreview.strGUID;
        }

        Texture2D texInputImage = new Texture2D(1, 1);

        try
        {
            imgPreview = new ImageInfo();

            if (!File.Exists(strGetFullFilePath()))
                File.Copy(_strFilePathFull, strGetFullFilePath());
            texInputImage = Utility.texLoadImageSecure(strGetFullFilePath(), ToolManager.Instance.texDefaultMissing);
            imgPreview.strFilePathRelative = strGetFullFilePath();
            imgPreview.SetTex(texInputImage);
        }
        catch (System.Exception _ex)
        {
            startImage.strFilePath = "";
            Debug.Log(_ex.Message);
        }

        /*
        if (texInputImage.width > ToolManager.Instance.options.optionDimensions.iWidth || texInputImage.height > ToolManager.Instance.options.optionDimensions.iHeight)
        {
            texInputImage = Utility.texCropAndScale(texInputImage, ToolManager.Instance.options.optionDimensions.iWidth, ToolManager.Instance.options.optionDimensions.iHeight);
            startImage.strFilePath = Path.GetFileNameWithoutExtension(startImage.strFilePath) + "_downscaled.png";
            Utility.WritePNG(startImage.strFilePath, texInputImage);
        }
        */

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

        LoadImageFromFileName(arPaths[0]);
    }

    public void OpenDrawWindow()
    {
        if (imgPreview != null)
            drawWindow.OpenImage(imgPreview);
    }

    public void Remove()
    {
        startImage.strFilePath = "";
        startImage.strGUID = "";

        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (!string.IsNullOrEmpty(startImage.strFilePath))
            imagePreviewInput.DisplayImage(imgPreview);
        else
            imagePreviewInput.DisplayEmpty();
    }

    public string strGetFullFilePath(string _strFileName = "")
    {
        string strFileName = string.IsNullOrEmpty(_strFileName) ? startImage.strFilePath : _strFileName;
        return Path.Combine(ToolManager.s_settings.strInputDirectory, strFileName);
    }
}
