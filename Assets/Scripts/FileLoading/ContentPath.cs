using System.IO;
using UnityEngine;

public static class ContentPath
{
    private static string Root
    {
        get
        {
#if UNITY_EDITOR
            return Path.GetFullPath(Path.Combine(Application.dataPath, "../Content")); // In Editor: Assets/../Content → ProjectRoot/Content
#elif UNITY_STANDALONE_WIN
            return Path.GetFullPath(Path.Combine(Application.dataPath, "../Content")); // In Windows Build: AppFolder/Content
#elif UNITY_ANDROID
            var path = Path.GetFullPath(Path.Combine(Application.persistentDataPath, "Content"));
            return path;
#endif
        }
    }
    
    public static string Audio(string languageCode, string filename)
    {
        return Path.Combine(Root, "Audio", languageCode, filename);
    }
    
    public static string Config(string filename)
    {
        return Path.Combine(Root, "Config", filename);
    }
    
    public static string Image(string filename)
    {
        return Path.Combine(Root, "Image", filename);
    }

    public static string Font(string filename)
    {
        return Path.Combine(Root, "Font", filename);
    }
    
    public static string Video(string filename)
    {
        return Path.Combine(Root, "Video", filename);
    }

    public static string Translation(string languageCode)
    {
        return Path.Combine(Root, "Translation", $"{languageCode}.json");
    }
}