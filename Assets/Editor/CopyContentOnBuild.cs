using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEngine;

public class CopyContentOnBuild : IPostprocessBuildWithReport
{
    public int callbackOrder => 0; // Lower number = earlier execution

    public void OnPostprocessBuild(BuildReport report)
    {
        string sourceFolder = Path.Combine(Application.dataPath, "../MyFolderToCopy"); // Folder inside your project that you want to copy

        if (!Directory.Exists(sourceFolder))
        {
            Debug.LogWarning($"Source folder not found: {sourceFolder}");
            return;
        }

        string buildPath = report.summary.outputPath; // Build output location
        
        if (report.summary.platform == BuildTarget.Android)
        {
            Debug.Log($"[Content] Android build detected.");
            Debug.Log($"[Content] Content source: {sourceFolder}");
            Debug.Log($"[Content] Android persistent path will be determined at runtime.");
            return;
        }
        
        string buildDirectory = Path.GetDirectoryName(buildPath); // Directory where the executable lives
        string destinationFolder = Path.Combine(buildDirectory, "MyFolderToCopy"); // Destination folder next to the executable

        CopyDirectory(sourceFolder, destinationFolder);

        Debug.Log("Folder copied successfully!");
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
}