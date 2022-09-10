using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// Used to display any image.
/// </summary>
public class ImagePreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Options")]
    public bool bShowHoverMenu = true;
    public bool bAllowDragging = true;
    public float fAnimationSpeed = 40f;
    public Color colorStepsNormal;
    public UnityEvent eventDraggedOnEmpty = new UnityEvent();
    public bool bIsInput = false;

    [Header("References")]
    public TMP_Text textQueueNumber;
    public GameObject goProcessingCircle;
    public RawImage rawimage;
    public Image imageFavoriteStar;
    public GameObject goStepNumberPrefab;
    public Transform transStepNumberParent;
    public GameObject goHoverOverlay;
    public GameObject goShadeOut;
    public ContextMenu contextMenu;
    public GameObject goDragPreviewPrefab;
    public CanvasGroup canvasGroup;

    [Header("Internal")]
    public List<ImageInfo> liOutputs = new List<ImageInfo>();
    public List<ImageInfo> liOldOutputs = new List<ImageInfo>();

    public ImageInfo outputDisplayed;
    public bool bProcessingThis = false;
    public Vector2 v2MaxSize = Vector2.zero;

    private GameObject goDragPreview = null;
    private List<GameObject> liStepNumbers = new List<GameObject>();

    private void Start()
    {
        ToolManager.Instance.eventQueueUpdated.AddListener(UpdateDisplay);
        v2MaxSize = rawimage.GetComponent<RectTransform>().sizeDelta;

        contextMenu.AddOptions(
            new ContextMenu.Option("Create variations!", () => SetAsInputImage()),
            new ContextMenu.Option("Use these options", () =>
            {
                OptionsVisualizer.instance.LoadOptions(outputDisplayed);
            }),
            new ContextMenu.Option("Copy prompt (only text)", () =>
            {
                if (outputDisplayed != null)
                    Utility.CopyToClipboard(outputDisplayed.prompt.strWithoutOptions());
            }),
            new ContextMenu.Option("Copy prompt (all)", () =>
            {
                if (outputDisplayed != null)
                    Utility.CopyToClipboard(outputDisplayed.prompt.strToString());
            }),
            new ContextMenu.Option("Save content as template", () =>
            {
                SaveAsTemplate(_bContent: true);
            }),
            new ContextMenu.Option("Save style as template", () =>
            {
                SaveAsTemplate(_bContent:false);
            }),
            new ContextMenu.Option("Copy seed", () =>
            {
                if (outputDisplayed != null)
                    Utility.CopyToClipboard(outputDisplayed.prompt.iSeed.ToString());
            }),
            new ContextMenu.Option("Lock seed", () =>
            {
                if (outputDisplayed != null)
                    ToolManager.Instance.options.optionSeed.Set(outputDisplayed.prompt.iSeed, _bRandomSeed: false);
            })
            );

        UpdateDisplay();
    }

    public void DisplayEmpty()
    {
        outputDisplayed = null;
        liOutputs.Clear();
        UpdateDisplay();
    }

    public void DisplayImage(ImageInfo _output)
    {
        StartCoroutine(ieDisplayOutput(_output));
    }

    public IEnumerator ieDisplayOutput(ImageInfo _output)
    {
        if (!liOutputs.Any(x => x.strGUID == _output.strGUID))
        {
            //Debug.LogWarning("ImagePreview: Was asked to display output that it doesn't have.");
            liOutputs.Add(_output);
        }

        outputDisplayed = _output;

        // scale rect
        ImageInfo output = liOutputs[0];
        RectTransform rtrans = rawimage.GetComponent<RectTransform>();
        Utility.ScaleRectToImage(rtrans, v2MaxSize, v2GetDimensions());

        canvasGroup.alpha = 0f;
        yield return Utility.ieLoadImageAsync(strGetFilePath(), rawimage, ToolManager.s_texDefaultMissing);
        canvasGroup.alpha = 1f;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // queue number
        /*
        int iIndex = ToolManager.Instance.liRequestQueue.FindIndex(x => x.Item1 == this);
        bool bInQueue = iIndex >= 0;
        if (bInQueue)
            textQueueNumber.text = (iIndex + 1).ToString();
        */

        // step variations
        foreach (GameObject go in liStepNumbers)
            Destroy(go);
        liStepNumbers.Clear();

        // step numbers
        foreach (ImageInfo output in liOutputs.Where(x => !string.IsNullOrEmpty(x.strFilePathRelative)).OrderBy(y => y.prompt.iSteps))
        {
            GameObject goNumber = Instantiate(goStepNumberPrefab, transStepNumberParent);
            StepsNumber stepsNumber = goNumber.GetComponent<StepsNumber>();
            stepsNumber.imagePreviewParent = this;
            stepsNumber.output = output;
            stepsNumber.textNumber.text = output.prompt.iSteps.ToString();
            stepsNumber.textNumber.color = output == outputDisplayed ? ToolManager.Instance.colorFavorite : colorStepsNormal;
            liStepNumbers.Add(goNumber);
        }

        if (liOutputs.Count > 0)
        {
            ImageInfo output = liOutputs[0];
            RectTransform rtrans = rawimage.GetComponent<RectTransform>();
            Utility.ScaleRectToImage(rtrans, v2MaxSize, v2GetDimensions());
        }

        if (outputDisplayed != null)
        {
            bool bFavorite = ToolManager.s_liFavoriteGUIDs.Any(x => x == outputDisplayed.strGUID);
            imageFavoriteStar.color = bFavorite ? ToolManager.Instance.colorFavorite : ToolManager.Instance.colorButton;
            Button button = imageFavoriteStar.GetComponent<Button>();
            ColorBlock colorBlock = button.colors;
            colorBlock.normalColor = bFavorite ? ToolManager.Instance.colorFavorite : ToolManager.Instance.colorButton;
            colorBlock.pressedColor = bFavorite ? ToolManager.Instance.colorFavorite : ToolManager.Instance.colorButton;
            colorBlock.selectedColor = bFavorite ? ToolManager.Instance.colorFavorite : ToolManager.Instance.colorButton;
            colorBlock.highlightedColor = bFavorite ? ToolManager.Instance.colorFavorite : ToolManager.Instance.colorButton;
            button.colors = colorBlock;
        }
        else
        {
            rawimage.texture = ToolManager.Instance.texDefaultMissing;
        }

        //textQueueNumber.gameObject.SetActive(!bProcessingThis && bInQueue);
        goProcessingCircle.SetActive(bProcessingThis);

        goShadeOut.SetActive(textQueueNumber.gameObject.activeInHierarchy || goProcessingCircle.activeInHierarchy);
    }

    private void Update()
    {
        if (bProcessingThis)
        {
            goProcessingCircle.transform.Rotate(Vector3.forward, -fAnimationSpeed * Time.deltaTime);
        }
    }

    public void RequestRedo()
    {
        Debug.Log($"Requesting HD version for seed {outputDisplayed.prompt.iSeed}");

        ImageInfo outputNew = outputDisplayed.outputCopy();
        outputNew.prompt.iSteps = OptionsVisualizer.instance.optionSteps.iStepsRedo;
        AddOutput(outputNew);

        ToolManager.Instance.RequestImage(outputNew);
    }

    private void AddOutput(ImageInfo _output)
    {
        liOutputs.Add(_output);
        _output.eventStartsProcessing.AddListener(StartedProcessing);
        _output.eventStoppedProcessing.AddListener(StoppedProcessing);
    }

    public void ToggleFavorite()
    {
        bool bFavorite = !ToolManager.s_liFavoriteGUIDs.Any(x => x == outputDisplayed.strGUID);

        string strFavoritePath = System.IO.Path.Combine(ToolManager.s_settings.strOutputDirectory, "favorites", System.IO.Path.GetFileName(outputDisplayed.strFilePathRelative));

        if (!bFavorite)
        {
            ToolManager.s_liFavoriteGUIDs.RemoveAll(x => x == outputDisplayed.strGUID);
            Debug.Log($"Removed favorite {outputDisplayed.strGUID}.");

            if (System.IO.File.Exists(strFavoritePath))
                System.IO.File.Delete(strFavoritePath);
        }
        else
        {
            ToolManager.s_liFavoriteGUIDs.Add(outputDisplayed.strGUID);
            Debug.Log($"Added favorite {outputDisplayed.strGUID}.");

            if (System.IO.File.Exists(strGetFilePath()))
                System.IO.File.Copy(strGetFilePath(), strFavoritePath, overwrite:true);
        }

        UpdateDisplay();
    }

    public string strGetFilePath()
    {
        if (bIsInput)
            return outputDisplayed.strFilePathFullInput();
        else
            return outputDisplayed.strFilePathFull();
    }

    public void SaveAsTemplate(bool _bContent)
    {
        ToolManager.Instance.SaveTemplate(outputDisplayed, _bContent);
    }

    public void SetAsInputImage()
    {
        OptionsVisualizer.instance.optionStartImage.LoadImageFromHistory(outputDisplayed.strGUID);
    }

    private void RemoveOldOutputs()
    {
        foreach (ImageInfo output in liOldOutputs)
        {
            output.eventStartsProcessing.RemoveListener(StartedProcessing);
            output.eventStoppedProcessing.AddListener(StoppedProcessing);
        }
        liOutputs.RemoveAll(x => liOldOutputs.Contains(x));
        liOldOutputs.Clear();
    }

    private void StartedProcessing(ImageInfo _output)
    {
        RemoveOldOutputs();

        bProcessingThis = true;
        UpdateDisplay();
    }

    private void StoppedProcessing(ImageInfo _output, bool _bWorked)
    {
        bProcessingThis = false;

        if (_bWorked)
            DisplayImage(_output);
        UpdateDisplay();
        goProcessingCircle.transform.rotation = Quaternion.identity;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (bShowHoverMenu && liOutputs.Any(x => !string.IsNullOrEmpty(x.strFilePathRelative)))
        {
            goHoverOverlay.SetActive(true);
            PreviewImage.Instance.SetVisible(true, outputDisplayed.texGet(), outputDisplayed.prompt.strToString());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        goHoverOverlay.SetActive(false);
        PreviewImage.Instance.SetVisible(false, null);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!bAllowDragging)
            return;

        if (goDragPreview != null)
            Destroy(goDragPreview);

        goDragPreview = Instantiate(goDragPreviewPrefab, ToolManager.Instance.transContextMenuCanvas.transform);
        ImagePreview imagePreview = goDragPreview.GetComponent<ImagePreview>();
        imagePreview.v2MaxSize = GetComponent<RectTransform>().sizeDelta;
        imagePreview.canvasGroup.alpha = 0.3f;
        imagePreview.canvasGroup.blocksRaycasts = false;
        imagePreview.DisplayImage(outputDisplayed);
        goDragPreview.transform.position = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!bAllowDragging)
            return;

        if (goDragPreview)
            goDragPreview.transform.position = eventData.position;
    }

    private Vector2 v2GetDimensions()
    {
        if (outputDisplayed.prompt != null)
            return new Vector2(outputDisplayed.prompt.iWidth, outputDisplayed.prompt.iHeight);
        else if (outputDisplayed.texGet() != null)
            return new Vector2(outputDisplayed.texGet().width, outputDisplayed.texGet().height);
        else
            return Vector2.zero;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (!bAllowDragging)
            return;

        Destroy(goDragPreview);
        goDragPreview = null;

        if (CanBeDraggedOn.s_hovering != null)
            CanBeDraggedOn.s_hovering.eventOnDraggedOn.Invoke(outputDisplayed);
        else
            eventDraggedOnEmpty.Invoke();
    }

    void OnDestroy()
    {
        if (outputDisplayed != null)
            Destroy(rawimage.texture);
    }
}
