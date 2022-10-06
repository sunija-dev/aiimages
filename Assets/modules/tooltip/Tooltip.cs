using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(15, 20)]
    public string strText = "HoverInfo";

    private float fWaitTime = 0.2f;

    private WindowTooltip windowTooltip;
    private Coroutine coDisplayDelayed = null;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (coDisplayDelayed != null)
            StopCoroutine(coDisplayDelayed);
        coDisplayDelayed = StartCoroutine(ieDisplayDelayed());
    }

    private IEnumerator ieDisplayDelayed()
    {
        yield return new WaitForSeconds(fWaitTime);
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
        StopCoroutine(coDisplayDelayed);
        coDisplayDelayed = null;
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
