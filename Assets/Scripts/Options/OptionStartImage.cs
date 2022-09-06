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
    public RawImage rawimagePreview;
    public Texture2D texDefault;

    private Vector2 v2PreviewMaxSize = Vector2.zero;

    private void Awake()
    {
        v2PreviewMaxSize = rawimagePreview.GetComponent<RectTransform>().sizeDelta;
        optionSlider.eventValueChanged.AddListener((float _fValue) => startImage.fStrength = 1f - _fValue);
        UpdateDisplay();
    }

    public void LoadImageFromHistory(string _strGUID)
    {
        // load image, move to inputs
        Output output = ToolManager.s_history.outputByGUID(_strGUID);

        startImage.strFilePath = output.strFilePath;
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

    public void LoadImageFromFileName(string _strFilePathFull)
    {
        startImage.strFilePath = Path.GetFileName(_strFilePathFull);
        startImage.strGUID = "";

        // in case person chose from outputs
        if (_strFilePathFull.Contains(ToolManager.s_settings.strOutputDirectory))
        {
            Output outputFitting = ToolManager.s_history.liOutputs.FirstOrDefault(x => x.strFilePath == _strFilePathFull.Replace(ToolManager.s_settings.strOutputDirectory, ""));
            if (outputFitting != default)
                startImage.strGUID = outputFitting.strGUID;
        }

        Texture2D texInputImage = new Texture2D(1, 1);

        try
        {
            if (!File.Exists(strGetFullFilePath()))
                File.Copy(_strFilePathFull, strGetFullFilePath());
            texInputImage = Utility.texLoadImageSecure(strGetFullFilePath(), new Texture2D(1, 1));
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

        /*
        if (string.IsNullOrEmpty(startImage.strGUID) && texInputImage.width != texInputImage.height)
        {
            Debug.Log("Input image was not square!");
            startImage.strFilePath = "";
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



    public void Remove()
    {
        startImage.strFilePath = "";
        startImage.strGUID = "";

        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (string.IsNullOrEmpty(startImage.strFilePath))
            rawimagePreview.texture = texDefault;
        else
            rawimagePreview.texture = Utility.texLoadImageSecure(strGetFullFilePath(), texDefault);

        Utility.ScaleRectToImage(rawimagePreview.GetComponent<RectTransform>(), v2PreviewMaxSize, new Vector2(rawimagePreview.texture.width, rawimagePreview.texture.height));
    }

    public string strGetFullFilePath(string _strFileName = "")
    {
        string strFileName = string.IsNullOrEmpty(_strFileName) ? startImage.strFilePath : _strFileName;
        return Path.Combine(ToolManager.s_settings.strInputDirectory, strFileName);
    }
}
