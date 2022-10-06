using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Diagnostics;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;
using System.Runtime.InteropServices;

/// <summary>
/// Prompts go in, textures come out.
/// </summary>
public class GeneratorConnection : MonoBehaviour
{
    public static GeneratorConnection instance;

    public Process process;
    public StreamWriter streamWriter;

    public bool bProcessing = false;
    public bool bInitialized = false;

    private List<string> liLines = new List<string>();
    private List<string> liErrors = new List<string>();

    private int iLastFileCount = -1;
    private OutputStatus outputStatus = OutputStatus.Unfinished;

    private enum OutputStatus { Unfinished, Broken, BrokenNeedsRestart, FinishedSuccessfully  }

    private Coroutine coRequestImage;

    void Awake()
    {
        instance = this;
    }

    public void Init(bool _bStartAI = true)
    {
        process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.EnableRaisingEvents = true;
        process.OutputDataReceived += OnOutputDataReceived;
        process.ErrorDataReceived += OnOutputErrorReceived;
        //process.Exited += new EventHandler(OnProcessExit);

        process.Start();
        process.BeginOutputReadLine();

        streamWriter = process.StandardInput;

        if (streamWriter.BaseStream.CanWrite)
        {
#if !UNITY_EDITOR
            
            streamWriter.WriteLine($"call {ToolManager.s_settings.strAnacondaBatPath}");
#else
            streamWriter.WriteLine($"\"{ToolManager.s_settings.strAnacondaBatPath}\"");
#endif
            if (_bStartAI)
                StartAI();
        }
    }

    public void StartAI()
    {
#if !UNITY_EDITOR
        string strEasyInstallContent = $"{ToolManager.s_settings.strSDDirectory}\\src\\clip\n" +
                                        $"{ToolManager.s_settings.strSDDirectory}\\src\\taming-transformers\n" +
                                        $"{ToolManager.s_settings.strSDDirectory}\n" +
                                        $"{ToolManager.s_settings.strSDDirectory}\\src\\k-diffusion";
        strEasyInstallContent = strEasyInstallContent.Replace("/", "\\").ToLower();
        File.WriteAllText(Path.Combine(ToolManager.s_settings.strEnvPath, "Lib/site-packages/easy-install.pth"), strEasyInstallContent);
#endif

        // change drive
        UnityEngine.Debug.Log("Writing: " + $"{ToolManager.s_settings.strSDDirectory.Substring(0, 2)}");
        streamWriter.WriteLine(ToolManager.s_settings.strSDDirectory.Substring(0, 2)); 
        // open sd directory
        UnityEngine.Debug.Log("Writing: " + $"cd \"{ToolManager.s_settings.strSDDirectory}\"");
        streamWriter.WriteLine($"cd \"{ToolManager.s_settings.strSDDirectory}\"");
        // activate environment
        UnityEngine.Debug.Log("Writing: " + "activate ldm");
        streamWriter.WriteLine("activate ldm");

#if !UNITY_EDITOR
        UnityEngine.Debug.Log("Writing: " + $"set TRANSFORMERS_CACHE={Application.dataPath}/../ai_cache/huggingface/transformers");
        streamWriter.WriteLine($"set \"TRANSFORMERS_CACHE={Application.dataPath}/../ai_cache/huggingface/transformers\"");

        UnityEngine.Debug.Log("Writing: " + $"set TORCH_HOME=\"{Application.dataPath}/../ai_cache/torch\"");
        streamWriter.WriteLine($"set \"TORCH_HOME={Application.dataPath}/../ai_cache/torch\"");
#endif

        // start ai
        UnityEngine.Debug.Log("Writing: " + "python scripts/dream.py");
        streamWriter.WriteLine($"python scripts/dream.py" +
            $" -o \"{ToolManager.s_settings.strOutputDirectory}\"" +
            $" {(ToolManager.s_settings.bFullPrecision ? "--precision=float32" : "")}" +
            $" {(ToolManager.s_settings.bFreeGPUMemory ? "--free_gpu_mem" : "")}" +
            $" {(ToolManager.s_settings.iGPU > 0 ? $"-d cuda:{ToolManager.s_settings.iGPU}" : "")}");
    }

    public void RequestImage(ImageInfo _output, Action<Texture2D, string, bool> _actionTextureReturn)
    {
        outputStatus = OutputStatus.Unfinished;
        coRequestImage = StartCoroutine(ieRequestImage(_output, _actionTextureReturn));
    }

    private IEnumerator ieRequestImage(ImageInfo _img, Action<Texture2D, string, bool> _actionTextureReturn)
    {
        bProcessing = true;

        UnityEngine.Debug.Log($"Requesting {_img.prompt.strToString()}");

        yield return new WaitUntil(() => streamWriter.BaseStream.CanWrite);

        if (_img.bIsRedo())
            streamWriter.WriteLine(_img.strGetRedoPrompt());
        else
            streamWriter.WriteLine(_img.prompt.strToString());

        yield return new WaitUntil(() => outputStatus != OutputStatus.Unfinished);

        if (outputStatus == OutputStatus.Broken || outputStatus == OutputStatus.BrokenNeedsRestart)
        {
            Debug.Log("Could not generate image. Please use a smaller image.");

            _actionTextureReturn.Invoke(null, "", false);
            if (outputStatus == OutputStatus.BrokenNeedsRestart)
            {
                Restart();
                yield break;
            }
        }
        else
        {
            FileInfo fileLatestPng = new DirectoryInfo(ToolManager.s_settings.strOutputDirectory).GetFiles().Where(x => Path.GetExtension(x.Name) == ".png").OrderByDescending(f => f.LastWriteTime).First();

            UnityEngine.Debug.Log($" New file appeared! Loading {fileLatestPng.Name}");

            yield return new WaitUntil(() => !Utility.IsFileLocked(fileLatestPng));
            yield return new WaitForSeconds(0.1f); // just give it some time

            UnityEngine.Debug.Log($"Finished loading image.");

            _actionTextureReturn.Invoke(Utility.texLoadImageSecure(fileLatestPng.FullName, ToolManager.s_texDefaultMissing), fileLatestPng.Name, true);
        }

        bProcessing = false;
    }

    public void Restart()
    {
        bProcessing = false;

        if (coRequestImage != null)
            StopCoroutine(coRequestImage);

        Close();
        Init();
    }

    private void ProcessOutput(string _strOutput)
    {
        UnityEngine.Debug.Log(">>>>>>>> " + _strOutput);

        if (_strOutput.StartsWith("* Initialization done!"))
            bInitialized = true;
        else if (outputStatus == OutputStatus.Unfinished && _strOutput.StartsWith("Outputs:"))
            outputStatus = OutputStatus.FinishedSuccessfully;
        else if (_strOutput.StartsWith("dream> >> Could not generate image."))
            outputStatus = OutputStatus.Broken;
        else if (_strOutput.StartsWith("dream> CUDA out of memory"))
            outputStatus = OutputStatus.BrokenNeedsRestart;
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data))
            return;
        liLines.Add(e.Data);
    }

    private void OnOutputErrorReceived(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data))
            return;
        liErrors.Add(e.Data);
    }

    private void Update()
    {
        foreach (string strLine in liLines)
            ProcessOutput(strLine);
        liLines.Clear();

        foreach (string strError in liErrors)
            Debug.Log(strError);
        liErrors.Clear();
    }

    public bool bNewFileAppeared()
    {
        if (iLastFileCount == -1)
        {
            iLastFileCount = Directory.GetFiles(ToolManager.s_settings.strOutputDirectory).Length;
            return false;
        }

        if (Directory.GetFiles(ToolManager.s_settings.strOutputDirectory).Length != iLastFileCount)
        {
            iLastFileCount = Directory.GetFiles(ToolManager.s_settings.strOutputDirectory).Length;
            return true;
        }

        return false;
    }

    private void OnProcessExit(object sender, EventArgs e)
    {
    }

    private void OnApplicationQuit()
    {
        Close();
    }

    public void Close()
    {
        if (streamWriter != null)
            streamWriter.Close();

        if (process != null)
            process.Close();

        bInitialized = false;
    }

}
