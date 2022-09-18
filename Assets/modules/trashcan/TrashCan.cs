using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    public GameObject goTrashModeIcon;
    private bool bTrashModeActive = false;

    private void Update()
    {
        if (bTrashModeActive)
        {
            goTrashModeIcon.transform.position = Input.mousePosition;
            if (Input.GetMouseButtonDown(0) && ImagePreview.s_imgHovering != null)
            {
                ToolManager.Instance.DeleteImage(ImagePreview.s_imgHovering.imgDisplayed);
            }

            if (Input.GetMouseButtonDown(1))
                SetTrashMode(false);
        }
    }

    public void SetTrashMode(bool _bActive)
    {
        bTrashModeActive = _bActive;
        goTrashModeIcon.SetActive(_bActive);
    }
}
