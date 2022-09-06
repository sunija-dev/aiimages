using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowTemplate : MonoBehaviour
{
    public GameObject goEntryPrefab;
    public Transform transEntryParent;
    private List<GameObject> liEntries = new List<GameObject>();

    public System.Action<Template> actionOnClick;

    public bool bContent = true;

    public void Open(bool _bContentTemplates, System.Action<Template> _actionOnSelect)
    {
        gameObject.SetActive(true);
        Init(_bContentTemplates, _actionOnSelect);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Init(bool _bContentTemplates, System.Action<Template> _actionOnSelect)
    {
        bContent = _bContentTemplates;
        actionOnClick = _actionOnSelect;

        foreach (GameObject go in liEntries)
            Destroy(go);
        liEntries.Clear();

        List<Template> liTemplates = _bContentTemplates ? ToolManager.s_liContentTemplates : ToolManager.s_liStyleTemplates;
        foreach (Template template in liTemplates)
        {
            GameObject goEntry = Instantiate(goEntryPrefab, transEntryParent);
            liEntries.Add(goEntry);
            TemplatePreview templatePreview = goEntry.GetComponent<TemplatePreview>();
            templatePreview.actionOnClick = OnClick;
            templatePreview.SetTemplate(template);
            templatePreview.windowTemplateParent = this;
            // todo: tooltips? with prompts
        }
    }

    public void OnClick(Template _template)
    {
        actionOnClick.Invoke(_template);
    }

}
