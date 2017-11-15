using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public class AssetBundleBuild
{

    public static string platform = string.Empty;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();
    static List<AssetBundleBuild> maps = new List<AssetBundleBuild>();
    public static string sourcePath = Application.dataPath + "/Resources";
    private static Dictionary<string, int> map;
    private static string AssetBundlesOutputPath = "Assets/StreamingAssets";
    private static string outputPaht = Application.dataPath + "/../Assetbundle";

    [MenuItem("Tools/AssetBundle/Build")]
    public static void BuildAssetBundle()
    {
        //AssetBundlesOutputPath = outputPaht;
        Caching.CleanCache();
        ClearAssetBundlesName();
        Pack(sourcePath);
        string outputPath = Path.Combine(AssetBundlesOutputPath, Platform.GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget));
        //outputPath = AssetBundlesOutputPath + "/" + Platform.GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget) + "/";
        outputPath = AssetBundlesOutputPath;
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        Debug.Log(outputPath);
        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.UncompressedAssetBundle, EditorUserBuildSettings.activeBuildTarget);
        BuildFileIndex(outputPath);
        AssetDatabase.Refresh();

        Debug.Log("打包完成");

    }
    static void ClearAssetBundlesName()
    {
        string[] oldAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < oldAssetBundleNames.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(oldAssetBundleNames[i], true);
        }
    }

    static void Pack(string source)
    {
        DirectoryInfo folder = new DirectoryInfo(source);
        FileSystemInfo[] files = folder.GetFileSystemInfos();
        int length = files.Length;
        for (int i = 0; i < length; i++)
        {
            if (files[i] is DirectoryInfo)
            {
                Pack(files[i].FullName);
            }
            else
            {
                if (!files[i].Name.EndsWith(".meta"))
                {
                    file(files[i].FullName);
                }
            }
        }
    }

    static void file(string source)
    {
        string _source = Replace(source);
        string _assetPath = "Assets" + _source.Substring(Application.dataPath.Length);
        string _assetPath2 = _source.Substring(Application.dataPath.Length + 1);

        AssetImporter assetImporter = AssetImporter.GetAtPath(_assetPath);
        string assetName = _assetPath2.Substring(_assetPath2.IndexOf("/") + 1);
        assetName = assetName.Replace(Path.GetExtension(assetName), ".unity3d");
        assetImporter.assetBundleName = assetName;
    }

    static string Replace(string s)
    {
        return s.Replace("\\", "/");
    }

    static void BuildFileIndex(string outputPath)
    {
        string resPath = outputPath;

        ///----------------------创建文件列表-----------------------
        string newFilePath = resPath + "/files.txt";
        if (File.Exists(newFilePath)) File.Delete(newFilePath);

        paths.Clear(); 
        files.Clear();
        Recursive(resPath);

        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            string ext = Path.GetExtension(file);
            if (file.EndsWith(".meta") || file.Contains(".DS_Store")) continue;

            string md5 = Utils.md5file(file);
            resPath = Replace(resPath);
            string value = file.Replace(resPath, string.Empty);
            FileInfo info = new FileInfo(file);
            sw.WriteLine(value + "|" + md5 + "|" + info.Length);
        }
        sw.Close(); 
        fs.Close();
    }
    /// <summary>
    /// 数据目录
    /// </summary>
    static string AppDataPath
    {
        get { return Application.dataPath.ToLower(); }
    }
    static void UpdateProgress(int progress, int progressMax, string desc)
    {
        string title = "Processing...[" + progress + " - " + progressMax + "]";
        float value = (float)progress / (float)progressMax;
        EditorUtility.DisplayProgressBar(title, desc, value);
    }
    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }
}
public class Platform
{
    public static string GetPlatformFolder(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "IOS";
            case BuildTarget.WebPlayer:
                return "WebPlayer";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Windows";
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSXUniversal:
                return "OSX";
            default:
                return null;
        }
    }
}
