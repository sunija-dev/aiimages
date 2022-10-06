using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WindowTooltip : MonoBehaviour
{
    public Vector2 v2Padding = Vector2.zero;

    [Header("References")]
    public GameObject goBackgroundMask;
    public GameObject goOutline;
    public TMP_Text textInfo;

    private RectTransform rect;
    private CanvasScaler canvasScaler;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasScaler = GetComponentInParent<CanvasScaler>();
    }

    public void Setup(string _strText)
    {
        textInfo.SetText(_strText);
        //textInfo.ForceMeshUpdate();

        if (rect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }

    private void Update()
    {
        //float fDpiScaling = GameIntegration.s_fCanvasScaling;
        int iYOffset = 50;
        Vector2 v2MousePos = Input.mousePosition;
        Vector2 v2Position = v2MousePos;
        v2Position.y -= iYOffset;
        float fScaling = (float)Screen.width / 1920f * canvasScaler.scaleFactor;

        if (v2Position.x + rect.sizeDelta.x * fScaling > Screen.width) 
            v2Position.x = v2MousePos.x - rect.sizeDelta.x * fScaling;
        if (v2Position.y - rect.sizeDelta.y * fScaling < 0) 
            v2Position.y = v2MousePos.y + rect.sizeDelta.y * fScaling;

        transform.position = v2Position;
    }
}
