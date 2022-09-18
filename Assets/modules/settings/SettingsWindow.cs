using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsWindow: MonoBehaviour
{
    public GameObject goDebugWindow;
    public TMP_InputField inputGPU;
    public Toggle toggle;

    public void Start()
    {
        toggle.SetIsOnWithoutNotify(ToolManager.s_settings.bUseBackgroundTexture);
        inputGPU.SetTextWithoutNotify(ToolManager.s_settings.iGPU.ToString());
    }

    public void SetDebugWindowVisible(bool _bVisible)
    {
        goDebugWindow.SetActive(_bVisible);
    }

    public void SelectGPU()
    {
        ToolManager.s_settings.iGPU = int.Parse(inputGPU.text);
        ToolManager.Instance.genConnection.Restart();
        ToolManager.s_settings.Save();
    }

    public void SwitchedBackgroundTexture(bool _bActive)
    {
        ToolManager.s_settings.bUseBackgroundTexture = _bActive;
        ToolManager.Instance.UpdateBackgroundTexture();
        ToolManager.s_settings.Save();
    }
}
