using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(15, 20)]
    public string strText = "HoverInfo";

    private WindowTooltip windowTooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject goTooltip = Instantiate(ToolManager.Instance.goTooltipPrefab, ToolManager.Instance.transTooltipCanvas);
        windowTooltip = goTooltip.GetComponent<WindowTooltip>();
        windowTooltip.Setup(strText);
    }

    /// <summary>
    /// For localization.
    /// </summary>
    public void UpdateText(string _strText)
    {
        strText = _strText;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroySave();
    }

    public void OnDisable()
    {
        DestroySave();
    }

    private void DestroySave()
    {
        if (windowTooltip)
            Destroy(windowTooltip.gameObject);
    }
}
