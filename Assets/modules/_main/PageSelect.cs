using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PageSelect : MonoBehaviour
{
    public TMP_Text textGPU;

    void Start()
    {
        textGPU.text = ToolManager.Instance.strGetWarningText();
    }

}
