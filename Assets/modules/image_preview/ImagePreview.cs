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
    public UnityEvent<ImagePreview> eventDraggedOnEmpty = new UnityEvent<ImagePreview>();
    public bool bIsInput = false;
    public float fDisplayDelay = 0.1f; // how long curser has to be over image to display big view
    public float fDisplayThreshold = 5f; // how little cursor has to move to display big view

    public static ImagePreview s_imgHovering = null;
    public static ImagePreview s_imgDragging = null;

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

    public ImageInfo imgDisplayed;
    public bool bProcessingThis = false;
    public Vector2 v2MaxSize = Vector2.zero;

    public bool bMouseCursorHovers = false;

    private GameObject goDragPreview = null;
    private List<GameObject> liStepNumbers = new List<GameObject>();
    private float fTargetAlpha = 1f;
    private bool bDontInit = false;
    private Coroutine coDisplayDelay = null;


    private void Start()
    {
        if (bDontInit)
            return;

        ToolManager.Instance.eventQueueUpdated.AddListener(UpdateDisplay);
        if (v2MaxSize == Vector2.zero)
            v2MaxSize = rawimage.GetComponent<RectTransform>().sizeDelta;

        contextMenu.AddOptions(
            new ContextMenu.Option("Create variations!", () => OptionsVisualizer.instance.optionSeed.Set(imgDisplayed)),
            new ContextMenu.Option("Set as input image", () => SetAsInputImage()),
            new ContextMenu.Option("Use these options", () =>
            {
                OptionsVisualizer.instance.LoadOptions(imgDisplayed);
            }),
            new ContextMenu.Option("Copy prompt (only text)", () =>
            {
                if (imgDisplayed != null)
                    Utility.CopyToClipboard(imgDisplayed.prompt.strWithoutOptions());
            }),
            new ContextMenu.Option("Copy prompt (all)", () =>
            {
                if (imgDisplayed != null)
                    Utility.CopyToClipboard(imgDisplayed.prompt.strToString());
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
                if (imgDisplayed != null)
                    Utility.CopyToClipboard(imgDisplayed.prompt.iSeed.ToString());
            }),
            new ContextMenu.Option("Lock seed", () =>
            {
                if (imgDisplayed != null)
                    ToolManager.Instance.options.optionSeed.Set(imgDisplayed.prompt.iSeed, _bRandomSeed: false);
            })
            );

        UpdateDisplay();
    }

    public void DisplayEmpty()
    {
        imgDisplayed = null;
        liOutputs.Clear();
        UpdateDisplay();
    }

    public void DisplayImage(ImageInfo _output, bool _bAddtive = false, bool _bDontInit = false)
    {
        bDontInit = _bDontInit;

        StartCoroutine(ieDisplayImage(_output));
    }

    public IEnumerator ieDisplayImage(ImageInfo _img, bool _bAddtive = false)
    {
        if (!_bAddtive)
        {
            RemoveDisplayedImages();
            imgDisplayed = _img;
        }

        ToolManager.AddDisplayer(_img, this);

        liOutputs.Add(_img);
        _img.eventStartsProcessing.AddListener(StartedProcessing);
        _img.eventStoppedProcessing.AddListener(StoppedProcessing);

        float fTargetAlpha = canvasGroup.alpha;
        //canvasGroup.alpha = 0f;
        yield return StartCoroutine(ieLoadImageInfoTexture(_img));
        canvasGroup.alpha = fTargetAlpha;

        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
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
            stepsNumber.textNumber.color = output == imgDisplayed ? ToolManager.Instance.colorFavorite : colorStepsNormal;
            liStepNumbers.Add(goNumber);
        }

        if (imgDisplayed != null)
        {
            // scale image
            RectTransform rtrans = rawimage.GetComponent<RectTransform>();
            Utility.ScaleRectToImage(rtrans, v2MaxSize, v2GetDimensions());

            // color favorite star
            bool bFavorite = ToolManager.s_liFavoriteGUIDs.Any(x => x == imgDisplayed.strGUID);
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

        if (!bShowHoverMenu)
            goHoverOverlay.SetActive(false);

        // processing info

        // queue number
        /*
        int iIndex = ToolManager.Instance.liRequestQueue.FindIndex(x => x.Item1 == this);
        bool bInQueue = iIndex >= 0;
        if (bInQueue)
            textQueueNumber.text = (iIndex + 1).ToString();
        */
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

    public void RequestStepRedo()
    {
        Debug.Log($"Requesting step redo version for seed {imgDisplayed.prompt.iSeed}");

        ImageInfo outputNew = imgDisplayed.outputCopy();
        outputNew.prompt.iSteps = OptionsVisualizer.instance.optionSteps.iStepsRedo;
        DisplayImage(outputNew, _bAddtive:true);

        ToolManager.Instance.RequestImage(outputNew);
    }

    public void RequestUpscaleRedo()
    {
        Debug.Log($"Requesting upscale redo version for seed {imgDisplayed.prompt.iSeed}");

        ImageInfo outputNew = imgDisplayed.outputCopy();
        outputNew.prompt.fUpscaleFactor = OptionsVisualizer.instance.optionUpscale.fUpscaleRedo;
        outputNew.prompt.fUpscaleStrength = OptionsVisualizer.instance.optionUpscale.fUpscaleStrengthRedo;
        DisplayImage(outputNew, _bAddtive: true);

        ToolManager.Instance.RequestImage(outputNew);
    }

    public void RequestFaceEnhanceRedo()
    {
        Debug.Log($"Requesting face_enhance redo version for seed {imgDisplayed.prompt.iSeed}");

        ImageInfo outputNew = imgDisplayed.outputCopy();
        outputNew.prompt.fFaceEnhanceStrength = OptionsVisualizer.instance.optionFaceEnhance.fStrengthRedo;
        DisplayImage(outputNew, _bAddtive: true);

        ToolManager.Instance.RequestImage(outputNew);
    }

    public void ToggleFavorite()
    {
        bool bFavorite = !ToolManager.s_liFavoriteGUIDs.Any(x => x == imgDisplayed.strGUID);

        string strFavoritePath = System.IO.Path.Combine(ToolManager.s_settings.strOutputDirectory, "favorites", System.IO.Path.GetFileName(imgDisplayed.strFilePathRelative));

        if (!bFavorite)
        {
            ToolManager.s_liFavoriteGUIDs.RemoveAll(x => x == imgDisplayed.strGUID);
            Debug.Log($"Removed favorite {imgDisplayed.strGUID}.");

            if (System.IO.File.Exists(strFavoritePath))
                System.IO.File.Delete(strFavoritePath);
        }
        else
        {
            ToolManager.s_liFavoriteGUIDs.Add(imgDisplayed.strGUID);
            Debug.Log($"Added favorite {imgDisplayed.strGUID}.");

            if (System.IO.File.Exists(strGetFilePath()))
                System.IO.File.Copy(strGetFilePath(), strFavoritePath, overwrite:true);
        }

        UpdateDisplay();
    }

    public string strGetFilePath()
    {
        if (bIsInput)
            return imgDisplayed.strFilePathFullInput();
        else
            return imgDisplayed.strFilePathFull();
    }

    public void SaveAsTemplate(bool _bContent)
    {
        ToolManager.Instance.SaveTemplate(imgDisplayed, _bContent);
    }

    public void SetAsInputImage()
    {
        OptionsVisualizer.instance.optionStartImage.LoadImageFromHistory(imgDisplayed.strGUID);
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
        bMouseCursorHovers = true;
        s_imgHovering = this;

        if (bShowHoverMenu)
        {
            goHoverOverlay.SetActive(true);
        }

        if (s_imgDragging == null && liOutputs.Any(x => !string.IsNullOrEmpty(x.strFilePathRelative)))
        {
            coDisplayDelay = StartCoroutine(ieDisplayDelay());
        }
    }

    private IEnumerator ieDisplayDelay()
    {
        float fHoverTime = 0f;
        Vector2 v2LastMousePos = Input.mousePosition;

        // don't display if time is not far enough, or cursor was still
        while (fHoverTime < fDisplayDelay && Vector2.Distance(v2LastMousePos, Input.mousePosition) > fDisplayThreshold)
        {
            v2LastMousePos = Input.mousePosition;
            fHoverTime += Time.deltaTime;
            yield return null;
        }

        PreviewImage.Instance.SetVisible(true, imgDisplayed.texGet(), imgDisplayed.prompt.strToString());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (s_imgHovering == this)
            s_imgHovering = null;
        bMouseCursorHovers = false;
        goHoverOverlay.SetActive(false);
        PreviewImage.Instance.SetVisible(false, null);
        if (coDisplayDelay != null)
            StopCoroutine(coDisplayDelay);
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
        imagePreview.bShowHoverMenu = false;
        imagePreview.DisplayImage(imgDisplayed);

        goDragPreview.transform.localScale = Vector3.one * 0.5f; // HACKFIX, because the image is somehow too big
        goDragPreview.transform.position = transform.position;

        s_imgDragging = this;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!bAllowDragging)
            return;

        if (goDragPreview)
            goDragPreview.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!bAllowDragging)
            return;

        if (s_imgDragging == this)
            s_imgDragging = null;

        Destroy(goDragPreview);
        goDragPreview = null;

        if (CanBeDraggedOn.s_hovering != null)
            CanBeDraggedOn.s_hovering.eventOnDraggedOn.Invoke(imgDisplayed);
        else
            eventDraggedOnEmpty.Invoke(this);
    }

    public void OpenImage()
    {
        try
        {
            Application.OpenURL($"file://{strGetFilePath()}");
        }
        catch
        {
            Debug.Log($"Could not open file {strGetFilePath()}.");
        }
        
    }

    private Vector2 v2GetDimensions()
    {
        if (imgDisplayed.prompt != null)
            return new Vector2(imgDisplayed.prompt.iWidth, imgDisplayed.prompt.iHeight);
        else if (imgDisplayed.texGet() != null)
            return new Vector2(imgDisplayed.texGet().width, imgDisplayed.texGet().height);
        else
            return Vector2.zero;
    }

    private void RemoveDisplayedImages()
    {
        foreach (ImageInfo _img in liOutputs)
        {
            ToolManager.RemoveDisplayer(imgDisplayed, this);
        }
        liOutputs.Clear();
    }

    public void TogglePreviewScale()
    {
        PreviewImage.Instance.ToggleScale();
    }

    void OnDestroy()
    {
        RemoveDisplayedImages();
        //if (imageDisplayed != null)
        //    Destroy(rawimage.texture);
    }

    private IEnumerator ieLoadImageInfoTexture(ImageInfo _img)
    {
        if (true /* !_img.bHasTexture()*/) // hasTexture doesn't completely work, because it also recognizes the placeholder
        {
            yield return StartCoroutine(Utility.ieLoadImageAsync(strGetFilePath(), rawimage, ToolManager.s_texDefaultMissing));
            _img.SetTex((Texture2D)rawimage.texture);
        }
        else
            rawimage.texture = _img.texGet();
    }
}
