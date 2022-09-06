using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class EditorExtensions : MonoBehaviour
{
    [MenuItem("AIImages/Deploy")]
    public static void PackDeploy()
    {
        Deploy(_bRebuildEnv: true);
    }

    [MenuItem("AIImages/DeployNoEnv")]
    /// Zipping environment again takes some minutes and isn't always necessary.
    public static void PackDeployNoEnv()
    {
        Deploy(_bRebuildEnv: false);
    }

    public static void Deploy(bool _bRebuildEnv)
    {
        // create deploy folder
        string strDeployFolder = $"{Application.dataPath}/../deploy";

        // copy build folder
        Debug.Log("Copying build folder...");
        string strUnityBuildFolder = $"{Application.dataPath}/../Build";
        Utility.CopyDirectory(strUnityBuildFolder, Path.Combine(strDeployFolder, "aiimages"));

        // copy cache
        Debug.Log("Copying cache...");
        string strCacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache");
        Utility.CopyDirectory(strCacheFolder, Path.Combine(strDeployFolder, "ai_cache"));

        // copy stable-diff folder
        Debug.Log("Copying sd folder...");
        string strStableDiffusionRepoFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "stable-diffusion");
        Utility.CopyDirectory(strStableDiffusionRepoFolder, Path.Combine(strDeployFolder, "stable-diffusion"), ".git");

        // pack env (to the right position)
        if (_bRebuildEnv)
        {
            string strAnacondaFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/anaconda3";
            Debug.Log("Packing conda environment...");
            string strBatContent = $"@ECHO [off]" +
                $"\ncall {strAnacondaFolder}/Scripts/activate.bat" +
                $"\nconda pack -n ldm -o {Path.Combine(strDeployFolder, "env.zip")} --ignore-missing-files --ignore-editable-packages --format zip";
            Utility.CreateBatAndRun(strBatContent, $"{Application.dataPath}/Resources/pack_env.bat", null);
        }
        

        Debug.Log("Deploy is finished once the command prompt closes.");

        // set right cache paths in bat files?
        // (zip everything?)


        // setup process
        // unzip unity thingy
        // it downloads sd_repo.zip
        // it downloads env.zip (split into parts?)
        // it downloads model.zip
        // unpacks all, puts model in right place
    }
}
