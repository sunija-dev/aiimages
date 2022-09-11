using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionAccuracy : MonoBehaviour
{
    public float fAccuracy { get => optionSliderCfgScale.fValue; }
    public float fVariance { get => optionSliderVariance.fValue; }

    public OptionSlider optionSliderCfgScale;
    public OptionSlider optionSliderVariance;

    public void Set(float _fCfg, float _fVariance)
    {
        optionSliderCfgScale.Set(_fCfg);
        optionSliderVariance.Set(_fVariance);
    }
}
