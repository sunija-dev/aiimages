using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using B83.Win32;
using System.Linq;

// adapted from https://github.com/Bunny83/UnityWindowsFileDrag-Drop
public class ImageDropHook : MonoBehaviour
{
    public UnityEvent<string> eventImageDroppedIn = new UnityEvent<string>();

    DropInfo dropInfo = null;

    private List<string> liAllowedExtensions = new List<string> { "bmp", "exr", "gif", "hdr", "iff", "pict", "psd", "tga", "tiff", "png", "jpg", "jpeg" };

    class DropInfo
    {
        public string file;
        public Vector2 pos;
    }

    void OnEnable()
    {
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;

    }
    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    void OnFiles(List<string> aFiles, POINT aPos)
    {
        string strFilePath = "";
        // scan through dropped files and filter out supported image types
        foreach (var f in aFiles)
        {
            var fi = new System.IO.FileInfo(f);
            var ext = fi.Extension.ToLower();
            ext = ext.Replace(".", "");
            if (liAllowedExtensions.Contains(ext))
            {
                strFilePath = f;
                break;
            }
        }

        // If the user dropped a supported file, create a DropInfo
        if (strFilePath != "")
        {
            var info = new DropInfo
            {
                file = strFilePath,
                pos = new Vector2(aPos.x, aPos.y)
            };
            dropInfo = info;

            eventImageDroppedIn.Invoke(dropInfo.file);
        }
    }
}
