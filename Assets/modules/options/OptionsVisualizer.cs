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
    public OptionVariation optionVariation;
    public OptionUpscale optionUpscale;
    public OptionFaceEnhance optionFaceEnhance;
    public OptionSeamless optionSeamless;

    private void Start()
    {
        instance = this;
    }

    public void LoadOptions(ImageInfo _output)
    {
        Prompt prompt = _output.prompt;

        if (!string.IsNullOrEmpty(prompt.startImage.strFilePath))
            optionStartImage.LoadImageFromFileName(optionStartImage.strGetFullFilePath(System.IO.Path.GetFileName(prompt.startImage.strFilePath)));
        optionStartImage.UpdateDisplay();
        optionStartImage.optionSlider.Set(_output.extraOptionsFull.fStartImageStrengthVariance);
        optionSeed.Set(prompt.iSeed, _output.extraOptionsFull.bRandomSeed);
        optionSteps.Set(_output.extraOptionsFull.iStepsPreview, _output.extraOptionsFull.iStepsRedo);
        optionAccuracy.Set(prompt.fCfgScale, _output.extraOptionsFull.fCfgScaleVariance);
        optionDimensions.Set(prompt.iWidth, prompt.iHeight);
        optionContent.Set(_output);
        optionStyle.Set(_output);
        optionVariation.Set(_output);
        optionUpscale.Set(_output.extraOptionsFull.fUpscalePreview, _output.extraOptionsFull.fUpscaleRedo, _output.extraOptionsFull.fUpscaleStrengthPreview, _output.extraOptionsFull.fUpscaleStrengthRedo);
        optionFaceEnhance.Set(_output.extraOptionsFull.fFaceEnhancePreview, _output.extraOptionsFull.fFaceEnhanceRedo);
        optionSeamless.Set(_output.prompt.bSeamless);
    }

    public Prompt promptGet(bool _bPreviewSteps, bool _bPreviewUpscale, bool _bPreviewFaceEnhance)
    {
        Prompt prompt = new Prompt()
        {
            iWidth = optionDimensions.iWidth,
            iHeight = optionDimensions.iHeight,
            startImage = optionStartImage.startImage,
            iSeed = optionSeed.iSeed,
            iSteps = _bPreviewSteps ? optionSteps.iStepsPreview : optionSteps.iStepsRedo,
            fCfgScale = optionAccuracy.fAccuracy + Random.Range(0f, optionAccuracy.fVariance),
            strContentPrompt = optionContent.strPrompt,
            strStylePrompt = optionStyle.strPrompt,
            iVariationSeed = optionSeed.bRandomSeed ? -1 : optionVariation.iSeed, // only use variation seed if seed is set
            fVariationStrength = optionVariation.fStrength,
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
            fStartImageStrengthVariance = optionStartImage.startImage.fStrength,
            fCfgScaleVariance = optionAccuracy.fVariance,
            bRandomSeed = optionSeed.bRandomSeed,
            iStepsPreview = optionSteps.iStepsPreview,
            iStepsRedo = optionSteps.iStepsRedo,
            iVariationSeed = optionVariation.iSeed,
            fVariationStrength = optionVariation.fStrength,
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
        Vector2Int v2iNewSize = Utility.v2iLimitPixelSize(_iWidth, _iHeight, 512 * 512);

        optionDimensions.Set(v2iNewSize.x, v2iNewSize.y);
    }

}
