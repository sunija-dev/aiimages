using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionUpscale : MonoBehaviour
{
    public float fUpscalePreview { get => Mathf.Max(1f, optionSliderPreview.fValue); }
    public float fUpscaleRedo { get => optionSliderRedo.fValue; }

    public OptionSlider optionSliderPreview;
    public OptionSlider optionSliderRedo;

    public float fUpscaleStrengthPreview { get => optionSliderStrengthPreview.fValue; }
    public float fUpscaleStrengthRedo { get => (int)optionSliderStrengthRedo.fValue; }

    public OptionSlider optionSliderStrengthPreview;
    public OptionSlider optionSliderStrengthRedo;

    public void Set(float _fPreview, float _fRedo, float _fStrengthPreview, float _fStrengthRedo)
    {
        optionSliderPreview.Set(_fPreview);
        optionSliderRedo.Set(_fRedo);
        optionSliderStrengthPreview.Set(_fStrengthPreview);
        optionSliderStrengthRedo.Set(_fStrengthRedo);
    }
}
