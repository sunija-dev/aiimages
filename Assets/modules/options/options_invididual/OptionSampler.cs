using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class OptionSampler : MonoBehaviour
{
    public string strSampler { get => dropdown.options[dropdown.value].text; }

    public TMP_Dropdown dropdown;

    private bool bDidInit = false;

    private void Awake()
    {
        if (bDidInit)
            return;

        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>() { "k_lms", "ddim", "k_dpm_2_a", "k_dpm_2", "k_euler_a", "k_euler", "k_heun", "k_lms", "plms" } ); 

        bDidInit = true;
    }

    public void Set(string _strSampler)
    {
        if (string.IsNullOrEmpty(_strSampler))
            _strSampler = "k_lms";

        int iValue = dropdown.options.FindIndex(x => x.text == _strSampler);
        dropdown.SetValueWithoutNotify(iValue);
    }
}
