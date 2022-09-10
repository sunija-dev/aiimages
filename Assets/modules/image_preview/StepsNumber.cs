using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class StepsNumber : MonoBehaviour, IPointerDownHandler
{
    public TMP_Text textNumber;

    public ImageInfo output;
    public ImagePreview imagePreviewParent;
    

    public void OnPointerDown(PointerEventData eventData)
    {
        imagePreviewParent.DisplayImage(output);
    }
}
