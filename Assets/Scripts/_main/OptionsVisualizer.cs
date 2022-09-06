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
    public OptionInput optionContent;
    public OptionInput optionStyle;

    private void Start()
    {
        instance = this;
    }

    public void LoadOptions(Output _output)
    {
        Prompt prompt = _output.prompt;

        if (!string.IsNullOrEmpty(prompt.startImage.strFilePath))
            optionStartImage.LoadImageFromFileName(optionStartImage.strGetFullFilePath(System.IO.Path.GetFileName(prompt.startImage.strFilePath)));
        optionStartImage.optionSlider.fValue = _output.extraOptionsFull.fStartImageStrengthVariance;
        optionSeed.Set(prompt.iSeed, _output.extraOptionsFull.bRandomSeed);
        optionSteps.Set(_output.extraOptionsFull.iStepsPreview, _output.extraOptionsFull.iStepsRedo);
        optionAccuracy.Set(prompt.fCfgScale, _output.extraOptionsFull.fCfgScaleVariance);
        optionDimensions.Set(prompt.iWidth, prompt.iHeight);
        optionContent.Set(_output);
        optionStyle.Set(_output);
    }

    public Prompt promptGet(bool _bIsPreview)
    {
        Prompt prompt = new Prompt()
        {
            iWidth = optionDimensions.iWidth,
            iHeight = optionDimensions.iHeight,
            startImage = optionStartImage.startImage,
            iSeed = optionSeed.iSeed,
            iSteps = _bIsPreview ? optionSteps.iStepsPreview : optionSteps.iStepsRedo,
            fCfgScale = optionAccuracy.fAccuracy,
            strContentPrompt = optionContent.strPrompt,
            strStylePrompt = optionStyle.strPrompt
        };

        return prompt;
    }

    public ExtraOptions extraOptionsGet()
    {
        return new ExtraOptions()
        {
            fStartImageStrengthVariance = optionStartImage.startImage.fStrength,
            iSeedVariance = optionSeed.iSeedVariance,
            fCfgScaleVariance = optionAccuracy.fVariance,
            bRandomSeed = optionSeed.bRandomSeed,
            iStepsPreview = optionSteps.iStepsPreview,
            iStepsRedo = optionSteps.iStepsRedo
        };
    }

    /// <summary>
    /// Adapts the width/height to fit the aspect ratio.
    /// </summary>
    public void SetAspectRatio(float _fWidth, float _fHeight)
    {
        int iPixelGoal = 512 * 512;
        float fAspectRatio = _fWidth / _fHeight;
        
        float fWidthNew = Mathf.Sqrt(fAspectRatio * iPixelGoal);
        float fHeightNew = fWidthNew / fAspectRatio;

        optionDimensions.Set((int)fWidthNew, (int)fHeightNew);
    }

}
