using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TemplatePreview : MonoBehaviour
{
    public RawImage rawimage;
    public Template template;
    public System.Action<Template> actionOnClick;
    public WindowTemplate windowTemplateParent;
    public RectTransform rtrans;
    public Tooltip tooltip;

    private Vector2 v2MaxSize = Vector2.zero;

    private void Awake()
    {
        v2MaxSize = rtrans.sizeDelta;
    }

    public void SetTemplate(Template _template)
    {
        template = _template;
        rawimage.texture = template.outputTemplate.texGet();
        Utility.ScaleRectToImage(rtrans, v2MaxSize, new Vector2(template.outputTemplate.prompt.iWidth, template.outputTemplate.prompt.iHeight));
        tooltip.UpdateText(windowTemplateParent.bContent ? template.outputTemplate.prompt.strContentPrompt : template.outputTemplate.prompt.strStylePrompt);
    }

    public void OnClick()
    {
        actionOnClick.Invoke(template);
    }

    public void DeleteTemplate()
    {
        ToolManager.Instance.DeleteTemplate(template, windowTemplateParent.bContent);
        windowTemplateParent.Init(windowTemplateParent.bContent, windowTemplateParent.actionOnClick);
    }
}
