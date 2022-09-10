using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CanBeDraggedOn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static CanBeDraggedOn s_hovering = null;

    public UnityEvent<ImageInfo> eventOnDraggedOn = new UnityEvent<ImageInfo>();

    public void OnPointerEnter(PointerEventData eventData)
    {
        s_hovering = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (s_hovering = this)
            s_hovering = null;
    }
}
