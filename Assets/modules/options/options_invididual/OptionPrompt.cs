using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionPrompt : MonoBehaviour
{
    public string strPrompt { get => inputPrompt.text; }

    public bool bIsContent = true;

    public TMP_InputField inputPrompt;
    //public TMP_InputField inputTemplate;
    public RawImage rawimageTemplatePreview;

    public WindowTemplate windowTemplate;

    private Template template;

    void Awake()
    {
        inputPrompt.onValueChanged.AddListener(OnInputChanged);
    }

    void OnInputChanged(string _strInput)
    {
        if (template != null)
        {
            string strTemplatePrompt = bIsContent ? template.outputTemplate.prompt.strContentPrompt : template.outputTemplate.prompt.strStylePrompt;
            if (_strInput != strTemplatePrompt)
                template = null;
        }
        
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (template != null)
            rawimageTemplatePreview.texture = Utility.texLoadImageSecure(template.outputTemplate.strFilePathFull(), ToolManager.s_texDefaultMissing);
        else
            rawimageTemplatePreview.texture = ToolManager.s_texDefaultMissing;
    }

    public void OpenTemplateWindow()
    {
        windowTemplate.Open(bIsContent, SetTemplate);
    }

    public void SetTemplate(Template _template)
    {
        Debug.Log($"Selected template {_template.strName}.");
        template = _template;
        rawimageTemplatePreview.texture = _template.outputTemplate.texGet();

        inputPrompt.text = bIsContent ? template.outputTemplate.prompt.strContentPrompt : template.outputTemplate.prompt.strStylePrompt;
    }

    public void Set(ImageInfo _output)
    { 
        // TODO: Could also load which template you selected, if we put it the ExtraSetttings. But too lazy. :3
        inputPrompt.text = bIsContent ? _output.prompt.strContentPrompt : _output.prompt.strStylePrompt;
    }
}
