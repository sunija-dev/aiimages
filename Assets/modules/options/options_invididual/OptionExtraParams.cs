using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionExtraParams : MonoBehaviour
{
    public string strExtraOptions { get => input.text; }

    public TMP_InputField input;

    public void Set(string _strExtraOptions)
    {
        input.SetTextWithoutNotify(_strExtraOptions);
    }

    public void Clear()
    {
        input.SetTextWithoutNotify("");
    }
}
