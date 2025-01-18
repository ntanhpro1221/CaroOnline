using Microsoft.Win32;
using System;
using System.IO;
using UnityEngine;

public class DeepLinkHelper : MonoBehaviour {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    private static readonly string vbsPath = Path.Combine(
        Environment.CurrentDirectory,
        $"{Guid.NewGuid()}.vbs");
    private static readonly string valPath = Path.Combine(
        Environment.CurrentDirectory,
        $"{Guid.NewGuid()}.txt");
    private static readonly string vbsRunnerPath = @"C:\Windows\System32\wscript.exe";
    private static string windowTitle;

    private static void RegisterCustomUriScheme() {
        using var key = Registry.CurrentUser.CreateSubKey($@"SOFTWARE\Classes\{AuthInfo.app_uri}") 
            ?? throw new Exception("Unable to create registry key.");

        key.SetValue("URL Protocol", "");

        using var commandKey = key.CreateSubKey(@"shell\open\command")
            ?? throw new Exception("Unable to create registry sub key.");

        commandKey.SetValue("", $"\"{vbsRunnerPath}\" \"{vbsPath}\" \"%1\"");
    }

    private static void CreateCommandScript() {
        File.WriteAllLines(vbsPath, new string[] {
            $"CreateObject(\"Scripting.FileSystemObject\").CreateTextFile(\"{valPath}\", True).WriteLine WScript.Arguments(0)",
            $"CreateObject(\"WScript.Shell\").AppActivate \"{windowTitle}\""
        });
    }

    private void OnApplicationFocus(bool focus) {
        if (!focus || !isRunning) return;
        if (!File.Exists(valPath)) return;
        callback?.Invoke(File.ReadAllText(valPath));
        StopListen();
    }
#endif
    private static bool isRunning = false;
    private static Action<string> callback;
    
    /// <summary>
    /// Make sure this game window is the top window before calling
    /// </summary>
    public static void StartListen(Action<string> l_callback) {
        if (isRunning) return; 
        isRunning = true;
        callback = l_callback;
        Application.deepLinkActivated += callback;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        windowTitle = Win32.GetForegroundWindowText();
        RegisterCustomUriScheme();
        CreateCommandScript();
        DontDestroyOnLoad(new GameObject(typeof(DeepLinkHelper).Name, typeof(DeepLinkHelper)));
#endif
    }

    public static void StopListen() {
        if (!isRunning) return; 
        isRunning = false;
        Application.deepLinkActivated -= callback;
        callback = null;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        File.Delete(vbsPath);
        File.Delete(valPath);
        Destroy(FindFirstObjectByType<DeepLinkHelper>().gameObject);
#endif
    }

    private void OnDisable()
        => StopListen();
}
