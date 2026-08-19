using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class CopyContentOnBuild : IPostprocessBuildWithReport
{
    public int callbackOrder => 0; // Lower number = earlier execution

    public void OnPostprocessBuild(BuildReport report)
    {
        string sourceFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Content"));
        
        UnityEngine.Debug.Log($"[Content] Application.dataPath: {Application.dataPath}");
        UnityEngine.Debug.Log($"[Content] Source folder: {sourceFolder}");
        UnityEngine.Debug.Log($"[Content] Source files: {Directory.GetFiles(sourceFolder).Length}");
        UnityEngine.Debug.Log($"[Content] Source directories: {Directory.GetDirectories(sourceFolder).Length}");

        if (!Directory.Exists(sourceFolder))
        {
            UnityEngine.Debug.LogWarning($"Source folder not found: {sourceFolder}");
            return;
        }

        string buildPath = report.summary.outputPath; // Build output location
        
        if (report.summary.platform == BuildTarget.Android)
        {
            PushContentToQuest(sourceFolder);
            return;
        }
        
        string buildDirectory = Path.GetDirectoryName(buildPath); // Directory where the executable lives
        string destinationFolder = Path.Combine(buildDirectory, "MyFolderToCopy"); // Destination folder next to the executable

        CopyDirectory(sourceFolder, destinationFolder);

        UnityEngine.Debug.Log("Folder copied successfully!");
    }

    private static void PushContentToQuest(string sourceFolder)
    {
        string destination = "/data/local/tmp/Content";

        UnityEngine.Debug.Log($"[Content] Pushing: {sourceFolder}");
        UnityEngine.Debug.Log($"[Content] To: {destination}");

        RunAdb($"shell rm -rf \"{destination}\"");
        RunAdb($"shell mkdir -p \"{destination}\"");

        // Push each top-level item individually.
        // Avoids the adb push . behaviour that caused the huge memory spike.
        foreach (string file in Directory.GetFiles(sourceFolder))
        {
            RunAdb($"push \"{file}\" \"{destination}/\"");
        }

        foreach (string directory in Directory.GetDirectories(sourceFolder))
        {
            string directoryName = Path.GetFileName(directory);
            RunAdb($"push \"{directory}\" \"{destination}/{directoryName}\"");
        }

        string persistentContent = "/storage/emulated/0/Android/data/com.UnityTechnologies.com.unity.template.urpblank/files/Content";

        RunAdb($"shell rm -rf \"{persistentContent}\"");
        RunAdb($"shell mkdir -p \"{persistentContent}\"");
        RunAdb($"shell cp -r \"{destination}/.\" \"{persistentContent}/\"");

        UnityEngine.Debug.Log("[Content] Content copied to persistent storage.");
    }
    
    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string targetFilePath = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, targetFilePath, true);
        }

        foreach (string directory in Directory.GetDirectories(sourceDir))
        {
            string targetSubDir = Path.Combine(targetDir, Path.GetFileName(directory));
            CopyDirectory(directory, targetSubDir);
        }
    }
    
    private static void RunAdb(string arguments)
    {
        using var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = "/opt/homebrew/bin/adb", //TODO this will only work when building on Mac OS
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        UnityEngine.Debug.Log($"[ADB] Running: /opt/homebrew/bin/adb {arguments}");
        process.Start();
        
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(output)) UnityEngine.Debug.Log($"[ADB] {output}");
        if (!string.IsNullOrWhiteSpace(error)) UnityEngine.Debug.Log($"[ADB] {error}");

        if (process.ExitCode != 0)
        {
            throw new System.Exception($"ADB failed with exit code {process.ExitCode}");
        }
    }
}