using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsWindow: MonoBehaviour
{
    public GameObject goDebugWindow;

    public void SetDebugWindowVisible(bool _bVisible)
    {
        goDebugWindow.SetActive(_bVisible);
    }

}
