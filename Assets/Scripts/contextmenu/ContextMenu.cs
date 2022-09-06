using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// ContextMenu with several options that opens on rightclick. Closes if clicked somewhere.
/// </summary>
public class ContextMenu : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    private List<Option> liOptions = new List<Option>();
    private GameObject goContextMenuWindow;

    public class Option
    {
        public string strNameKey = "Option";
        public Action action;

        public Option(string strName, Action action)
        {
            this.strNameKey = strName;
            this.action = action;
        }
    }

    public void Open()
    {
        goContextMenuWindow = Instantiate(ToolManager.Instance.goContextMenuWindowPrefab, Input.mousePosition, Quaternion.identity, ToolManager.Instance.transContextMenuCanvas);
        goContextMenuWindow.GetComponent<ContextMenuWindow>().Init(liOptions);
    }

    public void AddOptions(params Option[] _arOptions)
    {
        foreach (Option option in _arOptions)
            liOptions.Add(option);
    }

    public void ClearOptions()
    {
        if (goContextMenuWindow != null)
            goContextMenuWindow?.GetComponent<ContextMenuWindow>().ClearOptions();
        liOptions.Clear();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Open();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // needs to be here so "OnPointerClick" works
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // needs to be here so "OnPointerClick" works
    }
}
