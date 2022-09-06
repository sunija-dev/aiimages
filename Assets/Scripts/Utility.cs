using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Threading.Tasks;

public class Utility : MonoBehaviour
{
    public static Texture2D texLoadImage(string filename)
    {
        byte[] bytes = File.ReadAllBytes(filename);

        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(bytes);

        return texture;
    }

    public static Texture2D texLoadImageSecure(string _strFilePath, Texture2D _texDefault)
    {
        if (!File.Exists(_strFilePath))
        {
            Debug.Log($"Could not load image {_strFilePath}. File does not exist.");
            return _texDefault;
        }

        Texture2D texture = new Texture2D(1, 1);
        try
        {
            byte[] bytes = File.ReadAllBytes(_strFilePath);
            texture.LoadImage(bytes);
            return texture;
        }
        catch
        {
            Debug.Log($"Could not load image {_strFilePath}. Crashed.");
            return _texDefault;
        }
    }

    /// <summary>
    /// Waits until the file was unlocked for at least _fUnlockedFor.
    /// </summary>
    /// <param name="_fUnlockedFor"></param>
    /// <returns></returns>
    public static IEnumerator ieWaitUntilFileUnlocked(string _strFilePath, float _fUnlockedFor = 0.1f)
    {
        FileInfo fileInfo = new FileInfo(_strFilePath);

        float _fUnlockedTimer = 0f;
        while (_fUnlockedTimer < _fUnlockedFor)
        {
            if (IsFileLocked(fileInfo))
                _fUnlockedTimer = 0f;
            else
                _fUnlockedTimer += Time.deltaTime;

            yield return null;
        }
    }


    public static bool IsFileLocked(string _strPath)
    {
        FileInfo fileInfo = new FileInfo(_strPath);
        return IsFileLocked(fileInfo);
    }

    public static bool IsFileLocked(FileInfo file)
    {
        try
        {
            using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
            {
                stream.Close();
            }
        }
        catch (IOException)
        {
            return true;
        }

        return false;
    }

    public static Texture2D texCropAndScale(Texture2D _tex, int _iTargetWidth, int _iTargetHeight)
    {
        Texture2D texNew = new Texture2D(_tex.width, _tex.height, TextureFormat.RGB24, true);
        Texture2D newScreenshot = new Texture2D(_iTargetWidth, _iTargetHeight);
        newScreenshot.SetPixels(texNew.GetPixels(1));
        newScreenshot.Apply();

        return newScreenshot;
    }

    public static void WritePNG(string _strPath, Texture2D _tex)
    {
        byte[] arBytes = _tex.EncodeToPNG();
        File.WriteAllBytes(_strPath, arBytes);
    }

    public static void ScaleRectToImage(RectTransform _rtrans, Vector2 _v2MaxScaling, Vector2 _v2ImageScale)
    {
        Vector2 v2NewSize = _v2MaxScaling;
        float fAspectRatio = _v2ImageScale.x / _v2ImageScale.y;

        if (_v2ImageScale.x >= _v2ImageScale.y)
            v2NewSize.y = _v2MaxScaling.y / fAspectRatio;
        if (_v2ImageScale.y >= _v2ImageScale.x)
            v2NewSize.x = _v2MaxScaling.x * fAspectRatio;
        _rtrans.sizeDelta = v2NewSize;
    }

    public static void DeleteFolder(string _strPath, bool _bKeepFolder = false)
    {
        try
        {
            DirectoryInfo dirInfo = new DirectoryInfo(_strPath);

            foreach (FileInfo file in dirInfo.GetFiles())
                file.Delete();

            foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                dir.Delete(true);

            if (!_bKeepFolder)
                Directory.Delete(_strPath);
        }
        catch (System.Exception _ex)
        {
            Debug.Log(_ex.Message);
        }

    }

    public static void CopyToClipboard(string _strText)
    {
        GUIUtility.systemCopyBuffer = _strText;
    }

    // from https://stackoverflow.com/questions/16379143/check-if-application-is-installed-in-registry
    public static bool IsSoftwareInstalled(string softwareName)
    {
        var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall") ??
                  Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");

        if (key == null)
            return false;

        return key.GetSubKeyNames()
            .Select(keyName => key.OpenSubKey(keyName))
            .Select(subkey => subkey.GetValue("DisplayName") as string)
            .Any(displayName => displayName != null && displayName.Contains(softwareName));
    }

    public static void CreateDesktopShortcut()
    {
        string strExePath = Application.dataPath + "/../aiimages.exe";
        string strShortcutPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/WARP.lnk";
        ShortcutCreator.CreateShortcut(strExePath, strShortcutPath, "WARP");
    }

    // adapted from https://stackoverflow.com/questions/58744/copy-the-entire-contents-of-a-directory-in-c-sharp
    public static void CopyDirectory(string _strSourcePath, string _strTargetPath, string _strIgnoreFolder = "")
    {
        if (!Directory.Exists(_strSourcePath))
        {
            Debug.Log($"Cannot copy directory {_strSourcePath}. Directory does not exist.");
            return;
        }

        Directory.CreateDirectory(_strTargetPath);

        // create all of the directories
        List<string> liFolders = Directory.GetDirectories(_strSourcePath, "*", SearchOption.AllDirectories).ToList();
        if (!string.IsNullOrEmpty(_strIgnoreFolder))
            liFolders = liFolders.Where(x => !x.Contains(_strIgnoreFolder)).ToList();

        foreach (string strPath in liFolders)
            Directory.CreateDirectory(strPath.Replace(_strSourcePath, _strTargetPath));

        // copy all the files & replace any files with the same name
        List<string> liFiles = Directory.GetFiles(_strSourcePath, "*", SearchOption.AllDirectories).ToList();
        if (!string.IsNullOrEmpty(_strIgnoreFolder))
            liFiles = liFiles.Where(x => !x.Contains(_strIgnoreFolder)).ToList();

        foreach (string strPath in liFiles)
        {
            File.Copy(strPath, strPath.Replace(_strSourcePath, _strTargetPath), true);
        }
    }

    public static void CreateBatAndRun(string _strBatContent, string _strPath, string _strArguments)
    {
        File.WriteAllText(_strPath, _strBatContent);
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.FileName = _strPath;
        if (!string.IsNullOrEmpty(_strArguments))
            process.StartInfo.Arguments = _strArguments;
        process.Start();
    }

    public static IEnumerator ieUnzip(string _strFilePath, string _strToFolder, System.Action _actionFinished, bool _bOverride = false)
    {
        bool bFinished = false;
        Unzip(_strFilePath, _strToFolder, () => bFinished = true, _bOverride);
        yield return new WaitUntil(() => bFinished);
        _actionFinished.Invoke();
    }

    public static async void Unzip(string _strFilePath, string _strToFolder, System.Action _actionFinished, bool _bOverride = false)
    {
        try
        {
            //await Task.Run(() => ZipFile.ExtractToDirectory(_strFilePath, _strToFolder)).ContinueWith((_task) => _actionFinished);
            await Task.Run(() => ExtractToDirectory(_strFilePath, _strToFolder, _bOverride));
        }
        catch (System.Exception _ex)
        {
            Debug.LogError($"Failed to extract zip {_strFilePath}. {_ex.Message}");
        }
        _actionFinished.Invoke();
    }

    public static async void Zip(string _strFolderPath, string _strOutput)
    {
        await Task.Run(() => ZipFile.CreateFromDirectory(_strFolderPath, _strOutput));
    }

    // from: https://stackoverflow.com/questions/14795197/forcefully-replacing-existing-files-during-extracting-file-using-system-io-compr
    public static void ExtractToDirectory(string _strFilePath, string destinationDirectoryName, bool overwrite)
    {
        using (ZipArchive zipArchive = ZipFile.OpenRead(_strFilePath))
        {
            if (!overwrite)
            {
                zipArchive.ExtractToDirectory(destinationDirectoryName);
                return;
            }

            DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
            string destinationDirectoryFullPath = di.FullName;

            foreach (ZipArchiveEntry file in zipArchive.Entries)
            {
                string completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, file.FullName));

                if (!completeFileName.StartsWith(destinationDirectoryFullPath, System.StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException("Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");
                }

                if (file.Name == "")
                {// Assuming Empty for Directory
                    Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                    continue;
                }
                file.ExtractToFile(completeFileName, true);
            }
        }
    }
}



