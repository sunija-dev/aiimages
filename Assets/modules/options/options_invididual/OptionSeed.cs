using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class OptionSeed : MonoBehaviour
{
    public bool bRandomSeed = true;

    public CanvasGroup canvasGroupVariation;
    public OptionSlider optionSliderStrength;
    public TMP_InputField input;

    public ImagePreview imagePreviewInput;

    bool bInputChanged = false;
    bool bLastSeedWorked = true;

    private Color colorDefault;
    public ImageInfo imgReference = null;

    public int iSeed 
    { 
        get => seedPackage.iBaseSeed < 0 ? Random.Range(0, int.MaxValue) : Mathf.Clamp(seedPackage.iBaseSeed, 0, int.MaxValue);
    }

    public List<System.Tuple<int, float>> liVariations
    {
        get => seedPackage.liVariations;
    }

    public float fStrength { get => optionSliderStrength.fValue; }

    private SeedPackage seedPackage = new SeedPackage();


    void Awake()
    {
        colorDefault = input.textComponent.color;
        OnInputChanged("");
        input.onValueChanged.AddListener(OnInputChanged);
    }

    private void LateUpdate()
    {
        if (bInputChanged)
        {
            bInputChanged = false;
            (bLastSeedWorked, seedPackage) = ParseSeed(input.text);
            bRandomSeed = seedPackage.iBaseSeed < 0;
            if (imgReference != null && imgReference.prompt.strGetSeed() != Prompt.strGetSeed(iSeed, liVariations))
                imgReference = null;

            UpdateDisplay();
        }
    }

    void OnInputChanged(string _strInput)
    {
        bInputChanged = true;
    }

    void UpdateDisplay()
    {
        input.textComponent.color = bLastSeedWorked ? colorDefault : Color.red;

        canvasGroupVariation.alpha = bRandomSeed ? 0.3f : 1f;
        canvasGroupVariation.interactable = bRandomSeed ? false : true;

        if (imgReference != null)
            imagePreviewInput.DisplayImage(imgReference);
        else
            imagePreviewInput.DisplayEmpty();
    }

    public List<System.Tuple<int, float>> liGetNextVariationList(bool _bAddNew = true)
    {
        List<System.Tuple<int, float>> liNew = new List<System.Tuple<int, float>>(seedPackage.liVariations);
        if (_bAddNew && fStrength > 0f)
            liNew.Add(new System.Tuple<int, float>(Random.Range(0, int.MaxValue), fStrength));

        return liNew;
    }

    public void Set(ImageInfo _img)
    {
        Set(_img, "", false);
    }

    public void Set(ImageInfo _img, string _strReferenceGUID = "", bool _bRandomSeed = false)
    {
        optionSliderStrength.Set(_img.extraOptionsFull.fVariationStrength);

        if (!_bRandomSeed)
        {
            if (string.IsNullOrEmpty(_strReferenceGUID))
                imgReference = _img;
            else
                imgReference = ToolManager.s_history.imgByGUID(_strReferenceGUID);
        }

        Set(_img.prompt.iSeed, _img.prompt.liVariations, _strReferenceGUID, _bRandomSeed);
    }

    private void Set(int _iSeed, List<System.Tuple<int, float>> _liVariations, string _strReferenceGUID = "", bool _bRandomSeed = false)
    {
        seedPackage = new SeedPackage();

        if (_bRandomSeed)
        {
            input.text = "";
            return;
        }
            
        seedPackage.iBaseSeed = _iSeed;

        string strInputText = "";
        if (iSeed > -1)
        {
            seedPackage.liVariations = _liVariations;
            strInputText += $"-S{iSeed}";
            if (liVariations.Count > 0)
            {
                strInputText += $" -V ";
                for (int iVariation = 0; iVariation < liVariations.Count; iVariation++)
                {
                    System.Tuple<int, float> tuVariation = liVariations[iVariation];
                    strInputText += $"{tuVariation.Item1}:{ tuVariation.Item2.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)}{(iVariation + 1 < liVariations.Count ? "," : "")}";
                }
            }
        }
        else
        {
            seedPackage.liVariations = new List<System.Tuple<int, float>>();
        }

        input.text = strInputText; //-S1-V2:0.1,3:0.2
    }

    public (bool, SeedPackage) ParseSeed(string _strSeed)
    {
        SeedPackage seedResult = new SeedPackage();

        if (string.IsNullOrEmpty(_strSeed))
            return (true, seedResult);

        try
        {   //-S1-V2:0.1,3:0.2,4:0.3
            _strSeed = _strSeed.Replace(" ", "").Replace("-S", "").Replace("-V", ",");
            string[] arSplit = _strSeed.Split(',');

            // parse seed
            if (arSplit.Length > 0 && int.TryParse(arSplit[0], out int iSeed))
                seedResult.iBaseSeed = iSeed;
            else
                throw new System.Exception();

            // parse variations
            if (arSplit.Length > 1)
            {
                for (int i = 1; i < arSplit.Length; i++)
                {
                    string[] arVarSplit = arSplit[i].Split(':');
                    if (arVarSplit.Length == 2)
                        seedResult.liVariations.Add(new System.Tuple<int, float>(int.Parse(arVarSplit[0]), float.Parse(arVarSplit[1])));
                    else
                        throw new System.Exception();
                }
            }
        }
        catch
        {
            Debug.Log($"Could not parse seed {_strSeed}");
            return (false, new SeedPackage());
        }

        return (true, seedResult);
    }

    public void Clear()
    {
        input.text = "";
    }

    public class SeedPackage
    {
        public int iBaseSeed = -1;
        public List<System.Tuple<int, float>> liVariations = new List<System.Tuple<int, float>>();
    }
}
