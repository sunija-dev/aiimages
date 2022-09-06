using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionDimensions : MonoBehaviour
{
    public int iWidth { get => (int)optionSliderWidth.fValue; }
    public int iHeight { get => (int)optionSliderHeight.fValue; }

    public OptionSlider optionSliderWidth;
    public OptionSlider optionSliderHeight;

    public void Set(int _iWidth, int _iHeight)
    {
        optionSliderWidth.Set(_iWidth);
        optionSliderHeight.Set(_iHeight);
    }
}
