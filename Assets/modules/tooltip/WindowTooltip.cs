using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WindowTooltip : MonoBehaviour
{
    public Vector2 v2Padding = Vector2.zero;

    [Header("References")]
    public GameObject goBackgroundMask;
    public GameObject goOutline;
    public TMP_Text textInfo;

    private Vector2 v2OriginalScaleBackground = Vector2.zero;
    private RectTransform rectMask;
    private RectTransform rect;

    void Awake()
    {
        rectMask = goBackgroundMask.GetComponent<RectTransform>();
        rect = GetComponent<RectTransform>();
    }

    public void Setup(string _strText)
    {
        // adapt background to text size
        this.gameObject.SetActive(true);
        textInfo.SetText(_strText);
        textInfo.ForceMeshUpdate();
        Vector2 v2TextSize = textInfo.GetRenderedValues(false);

        RectTransform rectText = textInfo.GetComponent<RectTransform>();

        v2OriginalScaleBackground = rectMask.sizeDelta;
        rectMask.sizeDelta = v2TextSize + v2Padding;
        Vector3 v3NewPos = Vector3.zero;
        v3NewPos.x = rectText.position.x + (v2Padding.x / 2f);
        v3NewPos.y = rectText.position.y + (v2Padding.y / 2f);

        rectMask.position = v3NewPos;

        RectTransform rectOutline = goOutline.GetComponent<RectTransform>();
        rectOutline.sizeDelta = v2TextSize + v2Padding;
        rectOutline.position = rectMask.position;

        //Debug.Log($"Padding: {v2Padding} - textSize: {v2TextSize} - OriginalScaleBg: {v2TextSize} - rectMask.pos: {rectMask.position}");
    }

    private void Update()
    {
        //float fDpiScaling = GameIntegration.s_fCanvasScaling;
        int iYOffset = 50;
        Vector2 v2MousePos = Input.mousePosition;
        Vector2 v2Position = v2MousePos;
        v2Position.y -= iYOffset;

        if (v2Position.x + rectMask.sizeDelta.x > Screen.width) v2Position.x = v2MousePos.x - rectMask.sizeDelta.x;
        if (v2Position.y - rectMask.sizeDelta.y < 0) v2Position.y = v2MousePos.y + rectMask.sizeDelta.y; // * fDpiScaling;

        transform.position = v2Position;
    }
}
