using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionVariation : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TMP_InputField input;
    public OptionSlider optionSliderStrength;

    public bool bIsActive = false;
    public bool bRandomSeed = true;

    public int iSeed
    {
        get => bRandomSeed ? Random.Range(0, int.MaxValue) : Mathf.Clamp(iSeedInternal, 0, int.MaxValue);
        set => iSeedInternal = value;
    }

    public float fStrength { get => optionSliderStrength.fValue; }

    private int iSeedInternal = -1;

    void Awake()
    {
        OnInputChanged("");
        input.onValueChanged.AddListener(OnInputChanged);
    }

    private void Start()
    {
        SetActive(bIsActive);
    }

    void SetActive(bool _bActive)
    {
        canvasGroup.alpha = _bActive ? 1f : 0.3f;
        canvasGroup.interactable = false;
    }

    public void Set(int _iSeed, bool _bRandomSeed, float _fStrength)
    {
        input.text = _bRandomSeed ? "" : _iSeed.ToString();
        optionSliderStrength.Set(_fStrength);
    }

    void OnInputChanged(string _strInput)
    {
        if (string.IsNullOrEmpty(_strInput))
        {
            bRandomSeed = true;
            iSeed = -1;
        }
        else
        {
            bRandomSeed = false;
            iSeed = int.Parse(_strInput);
        }
    }

    public void Set(ImageInfo _img)
    {
        Set(_img.prompt.iVariationSeed, _img.prompt.iVariationSeed < 0, _img.prompt.fVariationStrength);
    }
}
