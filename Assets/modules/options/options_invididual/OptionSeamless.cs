using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionSeamless : MonoBehaviour
{
    public Toggle toggleSeamless;
    public bool bSeamless { get => toggleSeamless.isOn; }

    public void Set(bool _bActive)
    {
        toggleSeamless.isOn = _bActive;
    }
}
