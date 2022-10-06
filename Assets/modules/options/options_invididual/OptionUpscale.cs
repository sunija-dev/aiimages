using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionUpscale : MonoBehaviour
{
    public Toggle toggleHD;

    public GameObject goFastParent;
    public GameObject goEmbiggenParent;

    public string strMethod { get => toggleHD.isOn ? "embiggen" : "esrgan"; }

    // EMBIGGEN

    public float fEmbiggenFactorPreview { get => Mathf.Max(1f, optionSliderEmbiggenPreview.fValue); }
    public float fEmbiggenFactorRedo { get => Mathf.Max(1f, optionSliderEmbiggenRedo.fValue); }

    public OptionSlider optionSliderEmbiggenPreview;
    public OptionSlider optionSliderEmbiggenRedo;


    // ESRGAN

    public float fUpscalePreview { get => Mathf.Max(1f, optionSliderPreview.fValue); }
    public float fUpscaleRedo { get => Mathf.Max(1f, optionSliderRedo.fValue); }

    public OptionSlider optionSliderPreview;
    public OptionSlider optionSliderRedo;

    public float fUpscaleStrengthPreview { get => optionSliderStrengthPreview.fValue; }
    public float fUpscaleStrengthRedo { get => optionSliderStrengthRedo.fValue; }

    public OptionSlider optionSliderStrengthPreview;
    public OptionSlider optionSliderStrengthRedo;

    public void Set(string _strMethod, float _fPreview, float _fRedo, float _fStrengthPreview, float _fStrengthRedo, float _fEmbiggenPreview, float _fEmbiggenRedo)
    {
        optionSliderPreview.Set(_fPreview);
        optionSliderRedo.Set(_fRedo);
        optionSliderStrengthPreview.Set(_fStrengthPreview);
        optionSliderStrengthRedo.Set(_fStrengthRedo);
        optionSliderEmbiggenPreview.Set(_fEmbiggenPreview);
        optionSliderEmbiggenRedo.Set(_fEmbiggenRedo);

        toggleHD.isOn = _strMethod == "embiggen";
    }

    public void ToggledMethod(bool _bHD)
    {
        goFastParent.SetActive(!_bHD);
        goEmbiggenParent.SetActive(_bHD);
    }
}
