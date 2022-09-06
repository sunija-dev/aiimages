using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Runtime.InteropServices.ComTypes;
using System.Text;


// based on https://stackoverflow.com/questions/4897655/create-a-shortcut-on-desktop
public static class ShortcutCreator 
{
    public static void CreateShortcut(string _strExePath, string _strShortcutPath, string _strShortcutDescription, string _strArguments = "")
    {
        IShellLink link = (IShellLink)new ShellLink();

        // setup shortcut information
        Debug.Log("Setting exe path " + _strExePath);
        link.SetDescription(_strShortcutDescription);
        if (!string.IsNullOrEmpty(_strArguments))
            link.SetArguments(_strArguments);
        link.SetPath(_strExePath);


        // save it
        IPersistFile file = (IPersistFile)link;
        //string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        try
        {
            file.Save(_strShortcutPath, false);
        }
        catch
        {
            Debug.Log("ERROR: Could not create shortcut!");
        }
    }

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    internal class ShellLink
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    internal interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }
}
