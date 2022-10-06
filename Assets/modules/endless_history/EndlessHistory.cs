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

    private bool bDeletedImage = false;

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
        // remove image from all visible gridDatas
        bDeletedImage = true;
        liGridboxDataVisible.ForEach(grid => grid.oliImgs.RemoveAll(x => x.strGUID == _img.strGUID));

        // remove image from all visible gridDisplays
        List<GridBoxDisplay> liGridDisplayWithImage = liGridBoxDisplays.Where(gridDisplay => gridDisplay.liImagePreviews.Any(x => x.imgDisplayed == _img)).ToList();
        foreach (GridBoxDisplay gridDisplay in liGridDisplayWithImage)
        {
            // destroy/remove the image
            List<ImagePreview> liPreviewsOfImage = gridDisplay.liImagePreviews.Where(imgPreview => imgPreview.liOutputs.Any(x => x == _img)).ToList();
            liPreviewsOfImage.ForEach(x => Destroy(x.gameObject));
            gridDisplay.liImagePreviews.RemoveAll(x => liPreviewsOfImage.Any(y => y == x));

            if (gridDisplay.gridboxData != gridboxCurrent && gridDisplay.liImagePreviews.Count == 0)
            {
                Debug.Log("Gridbox was empty. Deleting it.");
                liSections.RemoveAll(section => section.liGridBoxes.Count == 1 && section.liGridBoxes[0] == gridDisplay.gridboxData);
                //liGridboxDataVisible.Remove(gridDisplay.gridboxData);
                Destroy(gridDisplay.gameObject);
            }     
        }
        liGridBoxDisplays.RemoveAll(x => x == null);

        UpdateView();
    }

    private void UpdateSections(ImageInfo _img, bool _bUpdateView = true)
    {
        if (!System.IO.File.Exists(_img.strFilePathFull()))
            return;

        if (bDeletedImage) // HACKFIX so after deleting image not a new one is added
        {
            bDeletedImage = false;
            return;
        }

        // for simplicity now: just add a new section+gridbox if size changed
        // each section only contains one gridbox for now
        bool bStartNew = false;
        if (imgLast != null)
        {
            int iCurrWidth = _img.iActualWidth > -1 ? _img.iActualWidth : _img.prompt.iWidth;
            int iCurrHeight = _img.iActualHeight > -1 ? _img.iActualHeight : _img.prompt.iHeight;
            int iLastWidth = imgLast.iActualWidth > -1 ? imgLast.iActualWidth : imgLast.prompt.iWidth;
            int iLastHeight = imgLast.iActualHeight > -1 ? imgLast.iActualHeight : imgLast.prompt.iHeight;

            bStartNew = iCurrWidth != iLastWidth || iCurrHeight != iLastHeight || 
                (sectionCurrent.liGridBoxes.Count > 0 && sectionCurrent.liGridBoxes[0].liOutputIDs.Count > 40); // start new section every x images, so vram doesn't overflow
        }
        
        // start new section?
        if (bStartNew)
        {
            sectionCurrent = new SectionData();
            liSections.Add(sectionCurrent);
            gridboxCurrent = new GridBoxData();
            sectionCurrent.liGridBoxes.Add(gridboxCurrent);
        }

        // start new gridbox?
        /*
        if (gridboxCurrent.oliImgs.Count != 0
            && imgLast != null
            && (imgLast.prompt.iWidth != _img.prompt.iWidth
            || imgLast.prompt.iHeight != _img.prompt.iHeight
            || gridboxCurrent.oliImgs.Count >= iMaxItemsPerGridBox))
        {
            gridboxCurrent = new GridBoxData();
            sectionCurrent.liGridBoxes.Add(gridboxCurrent);
        }
        */

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
        float fAdjustedDisplayRange = fDisplayRange / 1080f * Screen.height / ToolManager.s_settings.fUIScale;
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

        bool bBoxesAppearedOrDisappeared = false;

        // remove all disappeared gridboxes
        foreach (GridBoxDisplay gridboxDisplay in liGridBoxDisplays)
        {
            if (!liGridboxDataVisible.Any(x => x == gridboxDisplay.gridboxData))
            {
                Destroy(gridboxDisplay.gameObject);
                bBoxesAppearedOrDisappeared = true;

                if (gridboxDisplay.liImagePreviews.Count == 1)
                    Debug.Log($"Deleted preview with 1 image {gridboxDisplay.liImagePreviews[0].imgDisplayed.strGUID}");
            }    
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
                bBoxesAppearedOrDisappeared = true;

                if (gridboxDisplayNew.liImagePreviews.Count == 1)
                    Debug.Log($"Added preview with 1 image {gridboxDisplayNew.liImagePreviews[0].imgDisplayed.strGUID}");
            }
        }

        if (bBoxesAppearedOrDisappeared)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectContent);
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
}
