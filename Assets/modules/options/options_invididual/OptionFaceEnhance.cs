using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionFaceEnhance : MonoBehaviour
{
    public float fStrengthPreview { get => Mathf.Max(1f, optionSliderPreview.fValue); }
    public float fStrengthRedo { get => optionSliderRedo.fValue; }

    public OptionSlider optionSliderPreview;
    public OptionSlider optionSliderRedo;

    public void Set(float _fPreview, float _fRedo)
    {
        optionSliderPreview.Set(_fPreview);
        optionSliderRedo.Set(_fRedo);
    }
}
