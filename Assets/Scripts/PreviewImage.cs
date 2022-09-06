using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreviewImage : MonoBehaviour
{
    public static PreviewImage Instance;

    public RawImage rawimageBigPreview;
    public TMP_Text textPrompt;
    public Transform transPreviewRight;
    public Transform transPreviewLeft;

    private Vector2 v2MaxSize = Vector2.zero;
    public RectTransform rtrans;

    private void Awake()
    {
        Instance = this;
        SetVisible(false, null);
        v2MaxSize = GetComponent<RectTransform>().sizeDelta;
    }

    void Update()
    {
        if (Input.mousePosition.x > Camera.main.scaledPixelWidth / 2f)
            transform.position = transPreviewLeft.transform.position;
        else
            transform.position = transPreviewRight.transform.position;
    }

    public void SetVisible(bool _bVisible, Texture _tex, string _strPrompt = "")
    {
        rawimageBigPreview.texture = _tex;
        textPrompt.text = _strPrompt;
        if (_tex != null)
            Utility.ScaleRectToImage(rtrans, v2MaxSize, new Vector2(_tex.width, _tex.height));

        gameObject.SetActive(_bVisible);
    }
}
