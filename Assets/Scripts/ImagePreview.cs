using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;


public class ImagePreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text textQueueNumber;
    public GameObject goProcessingCircle;
    public RawImage rawimage;
    public Image imageFavoriteStar;
    public GameObject goStepNumberPrefab;
    public Transform transStepNumberParent;
    public GameObject goHoverOverlay;
    public GameObject goShadeOut;
    public ContextMenu contextMenu;

    private float fAnimationSpeed = 40f;

    
    private List<GameObject> liStepNumbers = new List<GameObject>();

    public Color colorStepsNormal;

    public List<Output> liOutputs = new List<Output>();
    public List<Output> liOldOutputs = new List<Output>();

    public Output outputDisplayed;
    public bool bProcessingThis = false;
    public Vector2 v2MaxSize = Vector2.zero;

    private void Start()
    {
        ToolManager.Instance.eventQueueUpdated.AddListener(UpdateDisplay);
        v2MaxSize = rawimage.GetComponent<RectTransform>().sizeDelta;

        contextMenu.AddOptions(
            new ContextMenu.Option("Create variations!", () => SetAsInputImage()),
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
            new ContextMenu.Option("Save style as template", () =>
            {
                SaveAsTemplate(_bContent:false);
            }),
            new ContextMenu.Option("Save content as template", () =>
            {
                SaveAsTemplate(_bContent: true);
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

    public void SetNewPrompt(Output _output)
    {
        liOldOutputs.Clear();
        liOldOutputs.AddRange(liOutputs);

        AddOutput(_output);
    }

    public void DisplayOutput(Output _output)
    {
        StartCoroutine(ieDisplayOutput(_output));
    }

    public IEnumerator ieDisplayOutput(Output _output)
    {
        if (!liOutputs.Any(x => x.strGUID == _output.strGUID))
        {
            //Debug.LogWarning("ImagePreview: Was asked to display output that it doesn't have.");
            liOutputs.Add(_output);
        }

        outputDisplayed = _output;

        // scale rect
        Output output = liOutputs[0];
        RectTransform rtrans = rawimage.GetComponent<RectTransform>();
        Utility.ScaleRectToImage(rtrans, v2MaxSize, new Vector2(output.prompt.iWidth, output.prompt.iHeight));

        yield return Utility.ieLoadImageAsync(_output.strGetFullPath(), rawimage, ToolManager.s_texDefaultMissing);

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
        bool bInQueue = false;

        // step variations
        foreach (GameObject go in liStepNumbers)
            Destroy(go);
        liStepNumbers.Clear();

        foreach (Output output in liOutputs.Where(x => !string.IsNullOrEmpty(x.strFilePath)).OrderBy(y => y.prompt.iSteps))
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
            Output output = liOutputs[0];
            RectTransform rtrans = rawimage.GetComponent<RectTransform>();
            Utility.ScaleRectToImage(rtrans, v2MaxSize, new Vector2(output.prompt.iWidth, output.prompt.iHeight));
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

        Output outputNew = outputDisplayed.outputCopy();
        outputNew.prompt.iSteps = OptionsVisualizer.instance.optionSteps.iStepsRedo;
        AddOutput(outputNew);

        ToolManager.Instance.RequestImage(outputNew);
    }

    private void AddOutput(Output _output)
    {
        liOutputs.Add(_output);
        _output.eventStartsProcessing.AddListener(StartedProcessing);
        _output.eventStoppedProcessing.AddListener(StoppedProcessing);
    }

    public void ToggleFavorite()
    {
        bool bFavorite = !ToolManager.s_liFavoriteGUIDs.Any(x => x == outputDisplayed.strGUID);

        string strFavoritePath = System.IO.Path.Combine(ToolManager.s_settings.strOutputDirectory, "favorites", System.IO.Path.GetFileName(outputDisplayed.strFilePath));

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

            if (System.IO.File.Exists(outputDisplayed.strGetFullPath()))
                System.IO.File.Copy(outputDisplayed.strGetFullPath(), strFavoritePath, overwrite:true);
        }

        UpdateDisplay();
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
        foreach (Output output in liOldOutputs)
        {
            output.eventStartsProcessing.RemoveListener(StartedProcessing);
            output.eventStoppedProcessing.AddListener(StoppedProcessing);
        }
        liOutputs.RemoveAll(x => liOldOutputs.Contains(x));
        liOldOutputs.Clear();
    }

    private void StartedProcessing(Output _output)
    {
        RemoveOldOutputs();

        bProcessingThis = true;
        UpdateDisplay();
    }

    private void StoppedProcessing(Output _output, bool _bWorked)
    {
        bProcessingThis = false;

        if (_bWorked)
            DisplayOutput(_output);
        UpdateDisplay();
        goProcessingCircle.transform.rotation = Quaternion.identity;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (liOutputs.Any(x => !string.IsNullOrEmpty(x.strFilePath)))
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

    void OnDestroy()
    {
        Debug.Log("Got destroyed");
        foreach (Output output in liOutputs)
        {
            output.UnloadTexture(); // remove texture, so memory can be freed again
        }
    }
}
