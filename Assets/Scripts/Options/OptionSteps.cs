using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionSteps : MonoBehaviour
{
    public int iStepsPreview { get => (int)optionSliderPreview.fValue; }
    public int iStepsRedo { get => (int)optionSliderRedo.fValue; }

    public OptionSlider optionSliderPreview;
    public OptionSlider optionSliderRedo;

    public void Set(int _iPreview, int _iRedo)
    {
        optionSliderPreview.Set(_iPreview);
        optionSliderRedo.Set(_iRedo);
    }

}
