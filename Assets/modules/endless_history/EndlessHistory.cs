using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class EndlessHistory : MonoBehaviour
{
    public static float s_fSeparatorHeight = 0f;
    public static int s_iPixelTarget = 150 * 150;
    public static float s_fHistoryWidth = 1000;

    public float fDisplayRange = 2000;

    public float fMaxScale = 300f;
    public float fMinScale = 100f;
    public int iMaxItemsPerGridBox = 30;

    public Scrollbar scrollbar;
    //public Transform transScrollContent;
    public RectTransform rectContent;
    //public RectTransform rectContentDisplay;
    public RectTransform rectOffsetter;

    public List<SectionData> liSections = new List<SectionData>();
    public GameObject goGridBoxPrefab;
    public Slider sliderScale;

    private History history;

    private List<GridBoxDisplay> liGridBoxDisplays = new List<GridBoxDisplay>();
    private List<SectionData> liSectionsVisible = new List<SectionData>();
    private List<GridBoxData> liGridboxDataVisible = new List<GridBoxData>();

    private float fScrollbarValueLast = 0f;
    private float fFirstElementPosition = 0f;

    SectionData sectionCurrent = new SectionData();
    GridBoxData gridboxCurrent = new GridBoxData();
    ImageInfo imgLast = null;

    IEnumerator Start()
    {
        // get history
        history = ToolManager.s_history;
        liSections = ToolManager.s_history.liSections;

        // old version that didn't save sections? generate them
        liSections.Clear();
        if (true) //(liSections.Count == 0)
        {
            liSections.Add(sectionCurrent);
            sectionCurrent.liGridBoxes.Add(gridboxCurrent);

            foreach (ImageInfo output in history.liOutputs)
            {
                UpdateSections(output, false);
            }
        }
        

        OnScaleUpdate();

        ToolManager.Instance.eventHistoryUpdated.AddListener(() => UpdateSections(history.liOutputs.Last()));
        ToolManager.Instance.eventHistoryElementDeleted.AddListener(OnElementDeleted);

        yield return null;

        UpdateView();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectContent);
        //rectContent.gameObject.GetComponent<VerticalLayoutGroup>()
    }

    private void Update()
    {
        if (fScrollbarValueLast != scrollbar.value)
            UpdateView();

        fScrollbarValueLast = scrollbar.value;
    }

    private void OnElementDeleted(ImageInfo _img)
    {
        liGridboxDataVisible.ForEach(grid => grid.oliImgs.RemoveAll(x => x.strGUID == _img.strGUID));

        List<GridBoxDisplay> liGridDisplayWithImage = liGridBoxDisplays.Where(gridDisplay => gridDisplay.liImagePreviews.Any(x => x.imgDisplayed == _img)).ToList();
        foreach (GridBoxDisplay gridDisplay in liGridDisplayWithImage)
        {
            foreach (ImagePreview imagePreview in gridDisplay.liImagePreviews)
            {
                if (imagePreview.liOutputs.Any(x => x == _img))
                    Destroy(imagePreview.gameObject);
            }
            gridDisplay.liImagePreviews.RemoveAll(x => x == null);

            if (gridDisplay.liImagePreviews.Count == 0)
            {
                liGridboxDataVisible.Remove(gridDisplay.gridboxData);
                Destroy(gridDisplay.gameObject);
            }     
        }
        liGridBoxDisplays.RemoveAll(x => x == null);
    }

    private void UpdateSections(ImageInfo _img, bool _bUpdateView = true)
    {
        if (!System.IO.File.Exists(_img.strFilePathFull()))
            return;

        // start new section?
        
        if (imgLast != null && !_img.prompt.bEqualContentStyle(imgLast.prompt))
        {
            sectionCurrent = new SectionData();
            liSections.Add(sectionCurrent);
            gridboxCurrent = new GridBoxData();
            sectionCurrent.liGridBoxes.Add(gridboxCurrent);
        }

        // start new gridbox?
        if (gridboxCurrent.oliImgs.Count != 0
            && imgLast != null
            && (imgLast.prompt.iWidth != _img.prompt.iWidth
            || imgLast.prompt.iHeight != _img.prompt.iHeight
            || gridboxCurrent.oliImgs.Count >= iMaxItemsPerGridBox))
        {
            gridboxCurrent = new GridBoxData();
            sectionCurrent.liGridBoxes.Add(gridboxCurrent);
        }

        // add
        gridboxCurrent.oliImgs.Add(_img);

        imgLast = _img;

        if (_bUpdateView)
            UpdateView();
    }

    /*
    private SectionData sectionCreateUnsorted()
    {
        SectionData sectionUnsorted = new SectionData();
        GridBoxData gridboxCurrent = new GridBoxData();
        sectionUnsorted.liGridBoxes.Add(gridboxCurrent);
        int iMaxItemsPerGridBox = 30;

        foreach (Output output in history.liOutputs)
        {
            if (!System.IO.File.Exists(output.strGetFullPath()))
                continue;

            // start new GridBox if new image has other dimensions
            if (gridboxCurrent.liOutputs.Count != 0)
            {
                Prompt promptLast = gridboxCurrent.liOutputs[0].prompt;
                if (promptLast.iWidth != output.prompt.iWidth || promptLast.iHeight != output.prompt.iHeight || gridboxCurrent.liOutputs.Count >= iMaxItemsPerGridBox)
                {
                    gridboxCurrent = new GridBoxData();
                    sectionUnsorted.liGridBoxes.Add(gridboxCurrent);
                }
            }

            gridboxCurrent.liOutputs.Add(output);
        }

        return sectionUnsorted;
    }
    */

    public void UpdateView()
    {
        float fPosition = 1f - scrollbar.value;

        float fHistoryHeight = fGetHeight();
        rectContent.sizeDelta = new Vector2(rectContent.sizeDelta.x, fHistoryHeight);

        float fPositionInHistory = fHistoryHeight * fPosition;
        float fAdjustedDisplayRange = fDisplayRange / 1080f * Screen.height;
        float fMinDisplay = fPositionInHistory - fAdjustedDisplayRange / 2f;
        float fMaxDisplay = fPositionInHistory + fAdjustedDisplayRange / 2f;

        // go through sections. If in visible range, do the same for gridboxes inside them.
        liSectionsVisible.Clear();
        liGridboxDataVisible.Clear();

        bool bIsFirstElement = true;
        float fSectionStart = 0f;
        float fSectionEnd = 0f;

        // go through sections in reverse order
        for (int iSection = 0; iSection < liSections.Count; iSection++)
        {
            SectionData sectionData = liSections[liSections.Count - 1 - iSection];

            // check if section is visible
            fSectionEnd = fSectionStart + sectionData.fGetHeight();
            if (Utility.bOverlap(fSectionStart, fSectionEnd, fMinDisplay, fMaxDisplay))
            {
                liSectionsVisible.Add(sectionData);

                float fStartInside = fSectionStart;
                float fEndInside = 0; 

                // go through gridboxes in reverse order
                for (int i = 0; i < sectionData.liGridBoxes.Count; i++) 
                {
                    GridBoxData gridbox = sectionData.liGridBoxes[sectionData.liGridBoxes.Count - 1 - i];

                    // check if gridboxes are visible
                    fEndInside = fStartInside + gridbox.fGetHeight();
                    if (Utility.bOverlap(fStartInside, fEndInside, fMinDisplay, fMaxDisplay))
                    {
                        liGridboxDataVisible.Add(gridbox);

                        if (bIsFirstElement)
                        {
                            bIsFirstElement = false;
                            fFirstElementPosition = fStartInside;
                            //Debug.Log($"UPDATED {fFirstElementPosition} : {Utility.fOverlap(fStartInside, fEndInside, fMinDisplay, fMaxDisplay)} // {fStartInside} - {fMinDisplay} ({fEndInside}, {fMaxDisplay})");
                        }
                    }
                    fStartInside = fEndInside;
                }
            }
            fSectionStart = fSectionEnd;
        }

        // remove all disappeared gridboxes
        foreach (GridBoxDisplay gridboxDisplay in liGridBoxDisplays)
        {
            if (!liGridboxDataVisible.Any(x => x == gridboxDisplay.gridboxData))
                Destroy(gridboxDisplay.gameObject);
        }
        //Debug.Log($"Destroyed {liGridBoxDisplays.Count(display => !liGridboxDataVisible.Any(x => x == display.gridboxData))}");
        liGridBoxDisplays.RemoveAll(display => !liGridboxDataVisible.Any(x => x == display.gridboxData));

        // spawn new ones
        //Debug.Log($"Spawning {liGridBoxDisplays.Count(x => liGridboxDataVisible.Contains(x.gridboxData))}");
        foreach (GridBoxData gridboxData in liGridboxDataVisible)
        {
            GridBoxDisplay gridboxDisplay = liGridBoxDisplays.FirstOrDefault(x => x.gridboxData == gridboxData);

            if (gridboxDisplay != default)
                gridboxDisplay.transform.SetAsLastSibling(); // make sure sorting works
            else
            {
                GridBoxDisplay gridboxDisplayNew = Instantiate(goGridBoxPrefab, rectContent.transform).GetComponent<GridBoxDisplay>();
                gridboxDisplayNew.SetData(gridboxData);
                gridboxDisplayNew.UpdateDisplay();
                gridboxDisplayNew.transform.SetAsLastSibling();

                liGridBoxDisplays.Add(gridboxDisplayNew);
            }
        }

        //LayoutRebuilder.ForceRebuildLayoutImmediate(rectContent);
        rectOffsetter.SetHeight(fFirstElementPosition);
    }

    public void OnScaleUpdate()
    {
        s_fHistoryWidth = rectContent.GetWidth();

        s_iPixelTarget = (int)Mathf.Pow(Mathf.Lerp(fMinScale, fMaxScale, sliderScale.value), 2);
        foreach (GridBoxDisplay gridBoxDisplay in liGridBoxDisplays)
            gridBoxDisplay.UpdateCellSize();

        Vector2 v2SizeContent = rectContent.sizeDelta;
        v2SizeContent.y = fGetHeight();
        rectContent.sizeDelta = v2SizeContent;

        UpdateView();
    }

    public float fGetHeight()
    {
        float fHeight = 0f;
        foreach (SectionData section in liSections)
            fHeight += section.fGetHeight();

        return fHeight;
    }

    public void UpdateDisplayedIcons(bool _bForceUpdate = false)
    {

        /*
        int iStartIndex = (int)((1f - (float)scrollRect.verticalScrollbar.value) * liIconIdsFound.Count);
        iStartIndex = (int)Mathf.Round(iStartIndex / 10) * 10;
        iStartIndex = Mathf.Clamp(iStartIndex, 0, liIconIdsFound.Count);

        int iEndIndex = iStartIndex + iDisplayedIcons;
        iEndIndex = Mathf.Clamp(iEndIndex, 0, liIconIdsFound.Count);

        if (!_bForceUpdate && iStartIndex == iLastStartIndex) // don't update if index didn't change
            return;

        liGridIcons.ForEach(x => Destroy(x));
        liGridIcons.Clear();

        for (int i = iStartIndex; i < iEndIndex; i++)
        {
            int iIconId = liIconIdsFound[i];
            GameObject goGridIcon = Instantiate(goGridIconPrefab, goGrid.transform);
            goGridIcon.GetComponentInChildren<Image>().sprite = IconUtility.spriteLoadIcon(iIconId);
            GridIcon gridIcon = goGridIcon.GetComponent<GridIcon>();
            gridIcon.iIcon = iIconId;
            gridIcon.tooltip.strText = iIconId + "\n" + string.Join(", ", IconDB.s_dictIcons[iIconId].liTags.Select(x => IconDB.s_liTags[x].Item2));

            // add callback to tell us that is was selected
            gridIcon.eOnClick += (e, _iIcon) => SelectIcon(_iIcon);

            liGridIcons.Add(goGridIcon);
        }

        iLastStartIndex = iStartIndex;

        StartCoroutine(coMoveHighlightDelayed());

        */
    }
}
