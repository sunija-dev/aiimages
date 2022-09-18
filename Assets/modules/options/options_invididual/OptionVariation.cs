using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionVariation : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public bool bIsActive = false;
    public bool bRandomVariation = true;

    public TMP_InputField input;
    public OptionSlider optionSlider;

    private int iSeedInternal = -1;

    private void Start()
    {
        SetActive(bIsActive);
    }

    void SetActive(bool _bActive)
    {
        canvasGroup.alpha = _bActive ? 1f : 0.3f;
        canvasGroup.interactable = false;
    }

}
