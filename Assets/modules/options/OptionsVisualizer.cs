using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsVisualizer : MonoBehaviour
{
    public static OptionsVisualizer instance;

    public OptionStartImage optionStartImage;
    public OptionSeed optionSeed;
    public OptionSteps optionSteps;
    public OptionAccuracy optionAccuracy;
    public OptionDimensions optionDimensions;
    public OptionPrompt optionContent;
    public OptionPrompt optionStyle;
    public OptionUpscale optionUpscale;
    public OptionFaceEnhance optionFaceEnhance;
    public OptionSeamless optionSeamless;

    private void Start()
    {
        instance = this;
    }

    public void LoadOptions(ImageInfo _img)
    {
        Prompt prompt = _img.prompt;

        optionStartImage.Set(_img);
        optionSeed.Set(_img, _img.extraOptionsFull.strSeedReferenceGUID, _img.extraOptionsFull.bRandomSeed);
        optionSteps.Set(_img.extraOptionsFull.iStepsPreview, _img.extraOptionsFull.iStepsRedo);
        optionAccuracy.Set(prompt.fCfgScale, _img.extraOptionsFull.fCfgScaleVariance);
        optionDimensions.Set(prompt.iWidth, prompt.iHeight);
        optionContent.Set(_img);
        optionStyle.Set(_img);
        optionUpscale.Set(_img.extraOptionsFull.fUpscalePreview, _img.extraOptionsFull.fUpscaleRedo, _img.extraOptionsFull.fUpscaleStrengthPreview, _img.extraOptionsFull.fUpscaleStrengthRedo);
        optionFaceEnhance.Set(_img.extraOptionsFull.fFaceEnhancePreview, _img.extraOptionsFull.fFaceEnhanceRedo);
        optionSeamless.Set(_img.prompt.bSeamless);
    }

    public Prompt promptGet(bool _bPreviewSteps, bool _bPreviewUpscale, bool _bPreviewFaceEnhance)
    {
        Prompt prompt = new Prompt()
        {
            iWidth = optionDimensions.iWidth,
            iHeight = optionDimensions.iHeight,
            startImage = optionStartImage.startimageGet(),
            iSeed = optionSeed.iSeed,
            liVariations = !optionSeed.bRandomSeed ? optionSeed.liGetNextVariationList() : new List<System.Tuple<int, float>>(),
            iSteps = _bPreviewSteps ? optionSteps.iStepsPreview : optionSteps.iStepsRedo,
            fCfgScale = optionAccuracy.fAccuracy + Random.Range(0f, optionAccuracy.fVariance),
            strContentPrompt = optionContent.strPrompt,
            strStylePrompt = optionStyle.strPrompt,
            fUpscaleFactor = _bPreviewUpscale ? optionUpscale.fUpscalePreview : optionUpscale.fUpscaleRedo,
            fUpscaleStrength = _bPreviewUpscale ? optionUpscale.fUpscaleStrengthPreview : optionUpscale.fUpscaleStrengthRedo,
            fFaceEnhanceStrength = _bPreviewFaceEnhance ? optionFaceEnhance.fStrengthPreview : optionFaceEnhance.fStrengthRedo,
            bSeamless = optionSeamless.bSeamless
        };

        return prompt;
    }

    public ExtraOptions extraOptionsGet()
    {
        return new ExtraOptions()
        {
            fStartImageStrengthVariance = optionStartImage.optionSliderStrength.fValue,
            fStartImageScaling = optionStartImage.optionSliderScaling.fValue,
            strStartImageOriginalName = optionStartImage.strOriginalFile,
            v2iOriginalSize = optionStartImage.v2iOriginalSize,
            fCfgScaleVariance = optionAccuracy.fVariance,
            bRandomSeed = optionSeed.bRandomSeed,
            strSeedReferenceGUID = optionSeed.imgReference != null ? optionSeed.imgReference.strGUID : "",
            iStepsPreview = optionSteps.iStepsPreview,
            iStepsRedo = optionSteps.iStepsRedo,
            fVariationStrength = optionSeed.fStrength,
            fUpscalePreview = optionUpscale.fUpscalePreview,
            fUpscaleRedo = optionUpscale.fUpscaleRedo,
            fUpscaleStrengthPreview = optionUpscale.fUpscaleStrengthPreview,
            fUpscaleStrengthRedo = optionUpscale.fUpscaleStrengthRedo,
            fFaceEnhancePreview = optionFaceEnhance.fStrengthPreview,
            fFaceEnhanceRedo = optionFaceEnhance.fStrengthRedo
        };
    }

    /// <summary>
    /// Adapts the width/height to fit the aspect ratio.
    /// </summary>
    public void SetAspectRatio(int _iWidth, int _iHeight)
    {
        Vector2Int v2iNewSize = Utility.v2iGetPixelSize(_iWidth, _iHeight, 512 * 512);

        optionDimensions.Set(v2iNewSize.x, v2iNewSize.y);
    }

}
