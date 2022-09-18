using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreviewImage : MonoBehaviour
{
    public static PreviewImage Instance;

    public float fWaitBeforeFade = 0.1f;
    public float fFadeSpeed = 5f;

    public RawImage rawimageBigPreview;
    public TMP_Text textPrompt;
    public Transform transPreviewRight;
    public Transform transPreviewLeft;
    public Transform transPreviewSmall;
    public CanvasGroup canvasGroup;

    private Vector2 v2MaxSize = Vector2.zero;
    private Coroutine coSetVisible = null;
    public RectTransform rtrans;

    private bool bBig = false;

    private void Awake()
    {
        Instance = this;
        v2MaxSize = GetComponent<RectTransform>().sizeDelta;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (bBig)
        {
            if (Input.mousePosition.x > Camera.main.scaledPixelWidth / 2f)
                transform.position = transPreviewLeft.transform.position;
            else
                transform.position = transPreviewRight.transform.position;
        }
        else
        {
            transform.position = transPreviewSmall.transform.position;
        }
    }

    public void SetVisible(bool _bVisible, Texture _tex, string _strPrompt = "")
    {
        if (coSetVisible != null)
            StopCoroutine(coSetVisible);

        gameObject.SetActive(true); // else we cannot start the coroutine
        coSetVisible = StartCoroutine(ieSetVisible(_bVisible, _tex, _strPrompt));
    }

    private IEnumerator ieSetVisible(bool _bVisible, Texture _tex, string _strPrompt = "")
    {
        float fTarget = _bVisible ? 1f : 0f;

        if (_bVisible)
            ApplyImage(_tex, _strPrompt);

        if (!_bVisible)
            yield return new WaitForSeconds(fWaitBeforeFade);

        while (Mathf.Abs(canvasGroup.alpha - fTarget) > 0.05f)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, fTarget, fFadeSpeed * Time.deltaTime);
            yield return null;
        }
        canvasGroup.alpha = fTarget;

        if (!_bVisible)
        {
            ApplyImage(_tex, _strPrompt);
            gameObject.SetActive(false);
        } 
    }

    private void ApplyImage(Texture _tex, string _strPrompt = "")
    {
        rawimageBigPreview.texture = _tex;
        textPrompt.text = _strPrompt;
        if (_tex != null)
            Utility.ScaleRectToImage(rtrans, v2MaxSize, new Vector2(_tex.width, _tex.height));
    }

    public void SetScale(bool _bBig)
    {
        bBig = _bBig;
        transform.localScale = Vector3.one * (_bBig ? 1f : 0.55f);
    }

    public void ToggleScale()
    {
        SetScale(!bBig);
    }
}
