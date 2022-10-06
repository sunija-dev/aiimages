using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;

public class SettingsWindow: MonoBehaviour
{
    public GameObject goDebugWindow;
    public TMP_InputField inputGPU;
    public TMP_Text textGPU;
    public Toggle toggle;

    public TMP_Dropdown dropdownModel;

    private string strModelPath = "";
    private const string c_strCurrentNameFileName = "current_name.txt";

    public void Awake()
    {
        strModelPath = $"{ToolManager.s_settings.strSDDirectory}/models/ldm/stable-diffusion-v1";
        UpdateModelList();
    }

    public void Start()
    {
        toggle.SetIsOnWithoutNotify(ToolManager.s_settings.bUseBackgroundTexture);
        inputGPU.SetTextWithoutNotify(ToolManager.s_settings.iGPU.ToString());
        textGPU.text = ToolManager.Instance.strGetWarningText();
    }

    public void ReloadModel()
    {
        ToolManager.s_settings.iGPU = int.Parse(inputGPU.text);
        ToolManager.s_settings.Save();
        ToolManager.Instance.genConnection.Restart();
    }

    public void SwitchedBackgroundTexture(bool _bActive)
    {
        ToolManager.s_settings.bUseBackgroundTexture = _bActive;
        ToolManager.Instance.UpdateBackgroundTexture();
        ToolManager.s_settings.Save();
    }

    public void UpdateModelList()
    {
        string strCurrentNameFile = $"{strModelPath}/{c_strCurrentNameFileName}";
        if (!File.Exists(strCurrentNameFile))
            File.WriteAllText(strCurrentNameFile, "default");

        string strCurrentName = File.ReadAllText(strCurrentNameFile);
        List<string> liModels = new List<string>(Directory.GetFiles(strModelPath, "*.ckpt")).Select(x => Path.GetFileNameWithoutExtension(x)).ToList();
        liModels.RemoveAll(x => x == "model");
        liModels.Insert(0, strCurrentName);

        dropdownModel.ClearOptions();
        dropdownModel.AddOptions(liModels);
        dropdownModel.SetValueWithoutNotify(dropdownModel.options.FindIndex(x => x.text == strCurrentName));
    }

    public void SelectModel()
    {
        string strModelNameSelected = dropdownModel.options[dropdownModel.value].text;
        string strCurrentNameFile = $"{strModelPath}/{c_strCurrentNameFileName}";

        // rename the model.ckpt to its real name
        File.Move($"{strModelPath}/model.ckpt", $"{strModelPath}/{File.ReadAllText(strCurrentNameFile)}.ckpt");

        // rename the selected model to model.ckpt and save its name in current_name
        File.WriteAllText(strCurrentNameFile, strModelNameSelected);
        File.Move($"{strModelPath}/{strModelNameSelected}.ckpt", $"{strModelPath}/model.ckpt");
    }

    public void OpenModelFolder()
    {
        Application.OpenURL($"file://{strModelPath}");
    }
}
