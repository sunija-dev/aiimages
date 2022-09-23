using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class OptionSlider : MonoBehaviour
{
    public TMP_Text textValue;
    public Slider slider;

    public bool bUseInt = true;

    public float fValue = 0f;

    public float fStepSize = 1;
    public float fMin = 1;
    public float fMax = 10;

    public UnityEvent<float> eventValueChanged = new UnityEvent<float>();
    

    private void Start()
    {
        Init();
        Set(fValue);

        slider.onValueChanged.AddListener((float _f) => OnValueChangedSlider());
        OnValueChangedSlider();
    }

    private void Init()
    {
        int iSteps = Mathf.RoundToInt((fMax - fMin) / fStepSize);
        slider.minValue = 0;
        slider.maxValue = iSteps;
    }

    public void OnValueChangedSlider()
    {
        fValue = fMin + slider.value * fStepSize;
        textValue.text = bUseInt ? ((int)fValue).ToString(System.Globalization.CultureInfo.InvariantCulture) : fValue.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        eventValueChanged.Invoke(fValue);
    }

    public void Set(float _fValue)
    {
        slider.value = Mathf.RoundToInt((_fValue - fMin) / fStepSize);
    }
}
