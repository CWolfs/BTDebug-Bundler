using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateAssetBundles {

    [MenuItem("Assets/Build Standalone AssetBundles")]
    static void BuildStandaloneAssetBundles() {
        BuildAssetBundles("Assets/AssetBundles/Standalone");
    }
      
    static void BuildAssetBundles(string path, BuildTarget platform=BuildTarget.StandaloneWindows) {
        // ensure the directory exists and build the bundle there
        PreBuildDirectoryCheck(path);
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, platform);
    }
      
    static void PreBuildDirectoryCheck(string directory) {
        // if the directory doesn't exist, create it
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    [MenuItem("Assets/Set Asset Bundle")]
    static void SetToAssetBundleRecursive() {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        bool isValidFolder = AssetDatabase.IsValidFolder(path);

        if (isValidFolder) {
            SetFolderToAssetBundle(path);
        } else {
            SetObjectToAssetBundle(path);
        }
        
        AssetDatabase.Refresh();
        Debug.Log("[Set Asset Bundle] Assets set to 'debug'");
    }

    static void SetFolderToAssetBundle(string path) {
        string[] childrenFolders = AssetDatabase.GetSubFolders(path);

        string[] filePaths = Directory.GetFiles(path);
        foreach (string filePath in filePaths) {
            if (filePath.EndsWith(".cs")) continue;

            Object obj = AssetDatabase.LoadMainAssetAtPath(filePath);
            if (obj) SetObjectToAssetBundle(filePath);
        }

        foreach (string childFolderPath in childrenFolders) {
            SetFolderToAssetBundle(childFolderPath);
        }
    }

    static void SetObjectToAssetBundle(string path) {
        AssetImporter.GetAtPath(path).SetAssetBundleNameAndVariant("btdebug-bundle", "");
    }
}