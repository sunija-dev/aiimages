using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[System.Serializable]
public class SectionData
{
    public string strGUID = "";
    public bool bLiveEditing = false;
    public Prompt prompt;
    public ExtraOptions extraOptions;
    public System.DateTime dateCreation;

    public List<GridBoxData> liGridBoxes = new List<GridBoxData>();

    public float fGetHeight()
    {
        float fHeight = EndlessHistory.s_fSeparatorHeight;
        foreach (GridBoxData gridBox in liGridBoxes)
            fHeight += gridBox.fGetHeight();

        return fHeight;
    }
}

[System.Serializable]
public class GridBoxData
{
    // save only ids, but keep references on load
    [System.NonSerialized] public ObservedList<ImageInfo> oliImgs = new ObservedList<ImageInfo>();
    public List<string> liOutputIDs = new List<string>();

    public GridBoxData()
    {
        oliImgs.Init(ToolManager.s_history.liOutputs.Where(x => liOutputIDs.Any(strCachedID => strCachedID == x.strGUID)).ToList());
        oliImgs.Updated += () => liOutputIDs = oliImgs.Select(x => x.strGUID).ToList();
    }

    public float fGetHeight()
    {
        if (oliImgs.Count == 0)
            return 0f;

        // get width/height of one image
        Vector2Int v2iDimensionImage = v2iGetElementSize();

        // check how many fit in one row
        int iImagesInRow = Mathf.FloorToInt((float)EndlessHistory.s_fHistoryWidth / (float)v2iDimensionImage.x);

        // calculate rows
        int iRows = Mathf.CeilToInt((float)oliImgs.Count / (float)iImagesInRow);

        return iRows  * v2iDimensionImage.y;
    }

    public float fGetFittingPadding()
    {
        // get width/height of one image
        Vector2Int v2iDimensionImage = v2iGetElementSize();

        // check how many fit in one row
        int iImagesInRow = Mathf.FloorToInt(EndlessHistory.s_fHistoryWidth / (float)v2iDimensionImage.x);

        return Mathf.FloorToInt((EndlessHistory.s_fHistoryWidth - ((float)iImagesInRow * (float)v2iDimensionImage.x)) / ((float)iImagesInRow -1f));
    }

    public Vector2Int v2iGetElementSize()
    {
        return Utility.v2iGetPixelSize(oliImgs[0].texGet().width, oliImgs[0].texGet().height, EndlessHistory.s_iPixelTarget);
    }
}
