using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class Setup : MonoBehaviour
{
    public ToolManager toolmanager;
    public GeneratorConnection genConnection;

    public GameObject goInstallPage;
    public GameObject goInstallButton;
    public TMP_Text textFeedback;
    public TMP_Text textGraphicsDevice;
    public Toggle toggleCreateShortcut;

    public string strDownloadWebsite = "https://aiimag.es/data/";
    private string strDownloadFolder = "";

    public Coroutine coSetupProcess = null;

    public DownloadNames downloadNames = new DownloadNames();

    private List<UnzipProcess> liUnzipProcesses = new List<UnzipProcess>();

    [System.Serializable]
    public class DownloadNames
    {
        public string strStableDiffusionRepo = "";
        public string strModel = "";
        public string strEnvironment = "";
        public string strAiCache = "";
    }

    void Awake()
    {
        strDownloadFolder = Path.Combine(Application.dataPath, "../downloads");

        textGraphicsDevice.text = toolmanager.strGetGPUText();
        textFeedback.text = "You will need <b>19 GB of space during installation (12 GB after)</b> whereever you put that exe.";
    }

    public void Open()
    {
        goInstallPage.SetActive(true);
        genConnection.Close();

        goInstallButton.SetActive(true);
    }

    public void StartSetupProcess()
    {
        if (coSetupProcess != null)
            StopCoroutine(coSetupProcess);

        StartCoroutine(SetupProcess());
        goInstallButton.SetActive(false);
    }

    IEnumerator SetupProcess()
    {
        // check for
        // - space in path
        // - path is "programs"

        if (Application.dataPath.Contains(Environment.ExpandEnvironmentVariables("%ProgramW6432%"))
            || Application.dataPath.Contains(Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%")))
        {
            textFeedback.text = $"AIimages cannot be installed in Program Files folder, sorry. :(\nPlease close it, copy it somewhere else (e.g. documents) and start it again.";
            yield break;
        }

        Directory.CreateDirectory(strDownloadFolder);
        string strApplicationFolder = Path.Combine(Application.dataPath, "../");

        // download sd_version.zip
        string strDLPathStableDiffusion = Path.Combine(strDownloadFolder, downloadNames.strStableDiffusionRepo);
        if (File.Exists(strDLPathStableDiffusion) && new FileInfo(strDLPathStableDiffusion).Length < 5000)
            File.Delete(strDLPathStableDiffusion);
        if (!File.Exists(strDLPathStableDiffusion)) // check that file not broken
        {
            Download dl = new Download(strDownloadWebsite + downloadNames.strStableDiffusionRepo, strDownloadFolder);
            while (!dl.bFinished)
            {
                textFeedback.text = $"Step 1/5: Downloading stable diffusion LStein version (200 MB)... ({dl.fProcess}%)\n{strGetUnzipProcessText()}";
                yield return null;
            }
        }
        liUnzipProcesses.Add(new UnzipProcess(strDLPathStableDiffusion, strApplicationFolder, this));

        // download model_version.zip
        string strDLPathModel = Path.Combine(strDownloadFolder, downloadNames.strModel);
        if (File.Exists(strDLPathModel) && new FileInfo(strDLPathModel).Length < 5000)
            File.Delete(strDLPathModel);
        if (!File.Exists(strDLPathModel))
        {
            Download dl = new Download(strDownloadWebsite + downloadNames.strModel, strDownloadFolder);
            while (!dl.bFinished)
            {
                textFeedback.text = $"Step 2/5: Downloading model (3.7 GB)... ({dl.fProcess}%)\n{strGetUnzipProcessText()}";
                yield return null;
            }
        }
        liUnzipProcesses.Add(new UnzipProcess(strDLPathModel, Path.Combine(strApplicationFolder, "stable-diffusion/models/ldm/stable-diffusion-v1"), this));

        // download env_version.zip
        string strDLEnvPath = Path.Combine(strDownloadFolder, downloadNames.strEnvironment);
        if (File.Exists(strDLEnvPath) && new FileInfo(strDLEnvPath).Length < 5000)
            File.Delete(strDLEnvPath);
        if (!File.Exists(strDLEnvPath))
        {
            Download dl = new Download(strDownloadWebsite + downloadNames.strEnvironment, strDownloadFolder);
            while (!dl.bFinished)
            {
                textFeedback.text = $"Step 3/5: Downloading env (2.8 GB)... ({dl.fProcess}%)\n{strGetUnzipProcessText()}";
                yield return null;
            }
        }
        liUnzipProcesses.Add(new UnzipProcess(strDLEnvPath, strApplicationFolder, this));

        // download ai_cache_version.zip
        string strDLCachePath = Path.Combine(strDownloadFolder, downloadNames.strAiCache);
        if (File.Exists(strDLCachePath) && new FileInfo(strDLCachePath).Length < 5000)
            File.Delete(strDLCachePath);
        if (!File.Exists(strDLCachePath))
        {
            Download dl = new Download(strDownloadWebsite + downloadNames.strAiCache, strDownloadFolder);
            while (!dl.bFinished)
            {
                textFeedback.text = $"Step 4/5: Downloading ai_cache (1 GB)... ({dl.fProcess}%)\n{strGetUnzipProcessText()}";
                yield return null;
            }
        }
        liUnzipProcesses.Add(new UnzipProcess(strDLCachePath, strApplicationFolder, this));

        while (liUnzipProcesses.Count > 0 && liUnzipProcesses.Any(x => !x.bFinished))
        { 
            textFeedback.text = $"Almost done!\nThis will take some minutes now...\n\n{strGetUnzipProcessText()}";
            yield return null;
        }

        textFeedback.text = $"Step 5/5: Done!";

        if (toggleCreateShortcut.isOn)
            CreateDesktopShortcut();

        ToolManager.s_settings.bDidSetup = true;
        ToolManager.s_settings.Save();

        ToolManager.Instance.OpenPage(ToolManager.Page.Main);
    }

    public bool bEverythingIsThere()
    {
        // env, ai_cache, stable-diffusion, model
        if (!File.Exists(Path.Combine(ToolManager.s_settings.strMainFolder, "ai_cache/torch/hub/checkpoints/checkpoint_liberty_with_aug.pth")))
        {
            Debug.Log("Check everything: Didn't find 'checkpoint_liberty_with_aug.pth' in ai_cache");
            return false;
        }

        if (!File.Exists(Path.Combine(ToolManager.s_settings.strEnvPath, "python.exe")))
        {
            Debug.Log("Check everything: Didn't find 'python.exe' in env.");
            return false;
        }

        if (!File.Exists(Path.Combine(ToolManager.s_settings.strSDDirectory, "scripts/dream.py")))
        {
            Debug.Log("Check everything: Didn't find 'dream.py' in stable-diffusion dir.");
            return false;
        }

        if (!File.Exists(Path.Combine(ToolManager.s_settings.strSDDirectory, "models/ldm/stable-diffusion-v1/model.ckpt")))
        {
            Debug.Log("Check everything: Didn't find 'model.ckpt' in stable-diffusion dir.");
            return false;
        }

        return true;
    }


    // ==================== HELPERS ====================

    private string strGetUnzipProcessText()
    {
        string strOutput = "";
        foreach (UnzipProcess unzipProcess in liUnzipProcesses)
        {
            if (!unzipProcess.bFinished)
                strOutput += $"Unzipping {unzipProcess.strFile}... ({unzipProcess.fUnzipTime.ToString("0")}s)\n";
        }
        return strOutput;
    }

    public static IEnumerator ieCreateBatAndRun(TextAsset _textAsset, string _strArguments)
    {
        string strFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "aiimages");
        Directory.CreateDirectory(strFolder);

        string strBatPath = Path.Combine(strFolder, $"{_textAsset.name}.bat");
        File.WriteAllText(strBatPath, _textAsset.text);

        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.FileName = strBatPath;
        if (!string.IsNullOrEmpty(_strArguments))
            process.StartInfo.Arguments = _strArguments;
        process.Start();

        yield return new WaitUntil(() => process.HasExited);
    }

    public static void RenewDesktopShortcut()
    {
        string strShortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/aiimages.lnk";
        if (File.Exists(strShortcutPath))
            CreateDesktopShortcut();
    }

    public static void CreateDesktopShortcut()
    {
        string strExePath = Application.dataPath + "/../aiimages.exe";
        string strShortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/aiimages.lnk";
        ShortcutCreator.CreateShortcut(strExePath, strShortcutPath, "aiimages");
    }

    public class Download
    {
        public bool bFinished = false;
        public float fProcess = 0f;

        public Download(string _strUrl, string _strTargetFolder, bool _bStartImmediately = true)
        {
            if (_bStartImmediately)
                Start(_strUrl, _strTargetFolder);
        }

        public void Start(string _strUrl, string _strTargetFolder)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += (object _sender, DownloadProgressChangedEventArgs _args) => fProcess = _args.ProgressPercentage;
                    webClient.DownloadFileCompleted += (object _sender, System.ComponentModel.AsyncCompletedEventArgs _args) => bFinished = true;
                    webClient.DownloadFileAsync(new Uri(_strUrl), Path.Combine(_strTargetFolder, Path.GetFileName(new Uri(_strUrl).AbsolutePath)));
                }
            }
            catch
            {
                Debug.Log("Download went wrong! D:");
            }
        }

        public void StartWithLogin(string _strUrl, string _strTargetFolder)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add(HttpRequestHeader.Cookie, "cookievalue");
                    webClient.DownloadProgressChanged += (object _sender, DownloadProgressChangedEventArgs _args) => fProcess = _args.ProgressPercentage;
                    webClient.DownloadFileCompleted += (object _sender, System.ComponentModel.AsyncCompletedEventArgs _args) => bFinished = true;
                    webClient.DownloadFileAsync(new Uri(_strUrl), Path.Combine(_strTargetFolder, Path.GetFileName(new Uri(_strUrl).AbsolutePath)));
                }
            }
            catch
            {
                Debug.Log("Download went wrong! D:");
            }
        }
    }

    public class UnzipProcess
    {
        public string strFile = "file";
        public bool bFinished = false;
        public float fUnzipTime = 0f;

        public UnzipProcess(string _strFilePath, string _strTargetFolder, MonoBehaviour _monoParent)
        {
            strFile = Path.GetFileName(_strFilePath);
            _monoParent.StartCoroutine(Start(_strFilePath, _strTargetFolder, _monoParent));
        }

        public IEnumerator Start(string _strFilePath, string _strTargetFolder, MonoBehaviour _monoParent)
        {
            strFile = Path.GetFileName(_strFilePath);
            string strZipPath = _strFilePath;

            yield return _monoParent.StartCoroutine(Utility.ieWaitUntilFileUnlocked(strZipPath));

            bool bSubProcessFinished = false;
            _monoParent.StartCoroutine(Utility.ieUnzip(_strFilePath, _strTargetFolder, () => bSubProcessFinished = true, _bOverride: true));
            while (!bSubProcessFinished)
            {
                fUnzipTime += Time.deltaTime;
                yield return null;
            }

            bFinished = true;
        }
    }

    /*
    
    
    public string strIsSomethingMissing()
    {
        string strMissing = "";
        strMissing += strCheckRepo();
        strMissing += string.IsNullOrEmpty(strMissing) ? "" : "\n";
        strMissing += strCheckCondaEnv();
        strMissing += string.IsNullOrEmpty(strMissing) ? "" : "\n";
        strMissing += strCheckCachedModels();
        strMissing += string.IsNullOrEmpty(strMissing) ? "" : "\n";
        strMissing += strCheckModel();

        return strMissing;
    }


        // ==================== CHECKS ====================

        IEnumerator SetupProcess()
    {
        Debug.Log("Starting downloads");

        // DL Anaconda
        if (toggleInstallAnaconda.isOn)
        {
            string strAnacondaFilePath = Path.Combine(strDownloadFolder, Path.GetFileName(strAnacondaDownloadLink));
            if (!File.Exists(strAnacondaFilePath))
            {
                Download dlAnaconda = new Download(strAnacondaDownloadLink, strDownloadFolder);
                while (!dlAnaconda.bFinished)
                {
                    textFeedback.text = $"Step 1/2: Downloading Anaconda... ({dlAnaconda.fProcess}%)";
                    yield return null;
                }
            }
            textFeedback.text = $"Step 1/2: Download finished! Now we'll install Anaconda and Python.\nPlease click through the installation.\n\n(Click \"Install now.\", wait until green bar filled, click \"Close\".)";
            System.Diagnostics.Process processAnacondaInstall = System.Diagnostics.Process.Start(strAnacondaFilePath);
            yield return new WaitUntil(() => processAnacondaInstall.HasExited);
        }
        UpdateCheckList();


        // DL Git
        if (toggleInstallGit.isOn)
        {
            string strGitFilePath = Path.Combine(strDownloadFolder, Path.GetFileName(strGitDownloadLink));
            if (!File.Exists(strGitFilePath))
            {
                Download dlGit = new Download(strGitDownloadLink, strDownloadFolder);
                while (!dlGit.bFinished)
                {
                    textFeedback.text = $"Step 2/2: Downloading Git... ({dlGit.fProcess}%)";
                    yield return null;
                }
            }
            textFeedback.text = $"Step 2/2: Download finished! Please click through the installation.\n\n(Click \"Install now.\", wait until green bar filled, click \"Close\".)";
            System.Diagnostics.Process processGitInstall = System.Diagnostics.Process.Start(strGitFilePath);
            yield return new WaitUntil(() => processGitInstall.HasExited);
        }
        UpdateCheckList();


        string strBaseText = $"Almost done! We will set up everything for you now.\nThis might take a bit.\n\n";

        // PREPARE ENVIRONMENT
        textFeedback.text = $"{strBaseText}Cloning git rep. This might take a bit...";

        // run git clone https://github.com/lstein/stable-diffusion.git
        yield return StartCoroutine(ieCloneGitRepo());
        buttonContinue.gameObject.SetActive(false);
        buttonAlternative.gameObject.SetActive(false);
        UpdateCheckList();

        textFeedback.text = $"{strBaseText}Setting up conda environment. This might take a bit...";

        // setup conda env
        yield return StartCoroutine(ieSetupCondaEnv());
        buttonContinue.gameObject.SetActive(false);
        buttonAlternative.gameObject.SetActive(false);
        UpdateCheckList();

        textFeedback.text = $"{strBaseText}Downloading model. This might take a bit...";

        // place model at right place
        yield return StartCoroutine(ieDownloadAndMoveModel());
        buttonContinue.gameObject.SetActive(false);
        buttonAlternative.gameObject.SetActive(false);
        UpdateCheckList();

        textFeedback.text = $"Everything is done! Have fun creating beautiful art. <3";

        if (toggleCreateShortcut.isOn)
            CreateDesktopShortcut();

        ShowContinueButton("Go to image generation");
        yield return new WaitUntil(() => bButtonContinueWasClicked);
        ToolManager.s_settings.bDidSetup = true;
        ToolManager.s_settings.Save();
        toolmanager.OpenPage(ToolManager.Page.Main);
    }

    public IEnumerator ieCloneGitRepo()
    {
        string strError = "";
        do
        {
            if (Directory.Exists(strRepoFolder) && !string.IsNullOrEmpty(strCheckRepo()))
            {
                Debug.Log("Found broken repo. Deleting it.");
                Utility.DeleteFolder(strRepoFolder);
            }

            yield return StartCoroutine(ieCreateBatAndRun(textassetGitDownload, ""));
            yield return new WaitForSeconds(1f); // just in case...

            strError = strCheckRepo();

            if (!string.IsNullOrEmpty(strError))
            {
                Utility.DeleteFolder(strRepoFolder);
                textFeedback.text = $"ERROR: {strError}";
                ShowAlternativeButton("Try again", null);
                ShowContinueButton("Continue anyway", null);
                yield return new WaitUntil(() => bButtonAlternativeWasClicked || bButtonContinueWasClicked);
            }
        }
        while (!string.IsNullOrEmpty(strError) && !bButtonContinueWasClicked);
    }

    public IEnumerator ieSetupCondaEnv()
    {
        string strError = "";
        do
        {
            yield return StartCoroutine(ieCreateBatAndRun(textassetCondaSetup, $"\"{ToolManager.s_settings.strAnacondaBatPath}\" \"{ToolManager.s_settings.strAnacondaPath}\""));
            yield return new WaitForSeconds(1f); // just in case...

            yield return StartCoroutine(ieCreateBatAndRun(textassetModelCache, $"\"{ToolManager.s_settings.strAnacondaBatPath}\" \"{ToolManager.s_settings.strAnacondaPath}\""));
            yield return new WaitForSeconds(1f); // just in case...

            strError = strCheckCondaEnv() + strCheckCachedModels();

            if (!string.IsNullOrEmpty(strError))
            {
                textFeedback.text = $"ERROR: {strError}";
                ShowAlternativeButton("Try again", null);
                ShowContinueButton("Continue anyway", null);
                yield return new WaitUntil(() => bButtonAlternativeWasClicked || bButtonContinueWasClicked);
            }
        }
        while (!string.IsNullOrEmpty(strError) && !bButtonContinueWasClicked);
    }

    public IEnumerator ieDownloadAndMoveModel()
    {
        string strError = "";
        do
        {
            string strModelPath = Path.Combine(strDownloadFolder, Path.GetFileName(strModelDownloadLink));
            if (!File.Exists(strModelPath))
            {
                Download dlModel = new Download(strModelDownloadLink, strDownloadFolder);
                while (!dlModel.bFinished)
                {
                    textFeedback.text = $" Downloading model (4 GB)... ({dlModel.fProcess}%)";
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.5f); // just in case...

            // (mkdir -p models\ldm\stable-diffusion-v1, copy C:\path\to\sd-v1-4.ckpt models\ldm\stable-diffusion-v1\model.ckpt
            Directory.CreateDirectory(Path.Combine(strUserFolder, "stable-diffusion"));
            Directory.CreateDirectory(Path.Combine(strUserFolder, "stable-diffusion/models"));
            Directory.CreateDirectory(Path.Combine(strUserFolder, "stable-diffusion/models/ldm"));
            Directory.CreateDirectory(Path.Combine(strUserFolder, "stable-diffusion/models/ldm/stable-diffusion-v1"));

            string strModelDestinationPath = Path.Combine(strUserFolder, "stable-diffusion/models/ldm/stable-diffusion-v1/model.ckpt");
            if (File.Exists(strModelDestinationPath))
                File.Delete(strModelDestinationPath);
            File.Copy(strModelPath, strModelDestinationPath);

            yield return new WaitForSeconds(1f); // just in case...

            strError = strCheckModel();

            if (!string.IsNullOrEmpty(strError))
            {
                textFeedback.text = $"ERROR: {strError}";
                ShowAlternativeButton("Try again", null);
                ShowContinueButton("Continue anyway", null);
                yield return new WaitUntil(() => bButtonAlternativeWasClicked || bButtonContinueWasClicked);
            }   
        }
        while (!string.IsNullOrEmpty(strError) && !bButtonContinueWasClicked);
    }

    public void UpdateCheckList()
    {
        string strDoneText = " - DONE!";

        string strChecklist = "Checklist:";
        strChecklist += $"\nGit installed {(bIsGitInstalled() ? strDoneText : "")}";
        strChecklist += $"\nAnaconda installed {(bIsAnacondaInstalled() ? strDoneText : "")}";
        strChecklist += $"\nRepo cloned {(string.IsNullOrEmpty(strCheckRepo()) ? strDoneText : strCheckRepo())}";
        strChecklist += $"\nConda env installed {(string.IsNullOrEmpty(strCheckCondaEnv()) ? strDoneText : strCheckCondaEnv())}";
        strChecklist += $"\nCached models {(string.IsNullOrEmpty(strCheckCachedModels()) ? strDoneText : strCheckCachedModels())}";
        strChecklist += $"\nModel downloaded {(string.IsNullOrEmpty(strCheckModel()) ? strDoneText : strCheckModel())}";
        strChecklist += $"\nFinished {(string.IsNullOrEmpty(strIsSomethingMissing()) ? strDoneText : "- Not yet.")}";

        textChecklist.text = strChecklist;
    }

    /// <summary>
    /// Roughly checks that repo was downloaded correctly.
    /// </summary>
    /// <returns>Returns error if it didn't work</returns>
    public string strCheckRepo()
    {
        if (!File.Exists(Path.Combine(strRepoFolder, "main.py")))
            return "Git clone failed, files not downloaded.";

        return "";
    }

    public string strCheckModel()
    {
        string strModelDestinationPath = Path.Combine(strUserFolder, "stable-diffusion/models/ldm/stable-diffusion-v1/model.ckpt");
        if (!File.Exists(strModelDestinationPath))
            return "Model file is not at its destination.";

        if (new FileInfo(strModelDestinationPath).Length < 200000) // arbitrary byte count that is definitely too small
            return "Model file is too small. Did copying fail?";

        return "";
    }

    public string strCheckCondaEnv()
    {
        if (!Directory.Exists(strEnvFolder))
            return "Conda env directory does not exist.";

        string strTorchDir = Path.Combine(strEnvFolder, "Lib/site-packages/torch");
        if (!Directory.Exists(strTorchDir))
            return "Conda Torch directory does not exist. Did the env setup fail?";

        return "";
    }

    public string strCheckCachedModels()
    {
        string strClipModelCache = Path.Combine(strUserFolder, ".cache/huggingface/transformers");
        if (Directory.GetFiles(strClipModelCache).Length == 0)
            return "Didn't find cached model. Just try again. <3";

        return "";
    }

        public void ToggleUninstallOptions()
    {
        goUninstallOptions.SetActive(!goUninstallOptions.activeSelf);
    }

    private void ShowContinueButton(string _strText = "Continue", Action _actionButton = null)
    {
        bButtonContinueWasClicked = false;
        buttonContinue.gameObject.SetActive(true);
        buttonContinue.GetComponentInChildren<TMP_Text>().text = _strText;

        actionButtonContinue = _actionButton;
    }

    private void ShowAlternativeButton(string _strText = "Alternative", Action _actionButton = null)
    {
        bButtonAlternativeWasClicked = false;
        buttonAlternative.gameObject.SetActive(true);
        buttonAlternative.GetComponentInChildren<TMP_Text>().text = _strText;

        actionButtonAlternative = _actionButton;
    }

    private void OnButtonContinueClicked()
    {
        bButtonContinueWasClicked = true;
        buttonContinue.gameObject.SetActive(false);

        if (actionButtonContinue != null)
            actionButtonContinue.Invoke();
    }

    private void OnButtonAlternativeClicked()
    {
        bButtonAlternativeWasClicked = true;
        buttonAlternative.gameObject.SetActive(false);

        if (actionButtonAlternative != null)
            actionButtonAlternative.Invoke();
    }

    public void DeleteDownloadsFolder()
    {
        Utility.DeleteFolder(strDownloadFolder, _bKeepFolder: true);
    }

    public void DeleteEnvironmentFolder()
    {
        Utility.DeleteFolder(strEnvFolder);
    }

    public void DeleteRepoFolder()
    {
        Utility.DeleteFolder(strRepoFolder);
    }

    public void ShowAdvancedSettings()
    {
        bShowAdvancedSettings = true;
        goAdvancedSettings.SetActive(bShowAdvancedSettings);
    }

    private bool bIsAnacondaInstalled()
    {
        return (Directory.Exists(ToolManager.s_settings.strAnacondaPath));
    }

    private bool bIsGitInstalled()
    {
        return Utility.IsSoftwareInstalled("git");
    }

    */
}




