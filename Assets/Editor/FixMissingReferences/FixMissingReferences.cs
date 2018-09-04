using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class FixMissingRefences {
    [MenuItem("Assets/Save References")]
    static void SaveReferences() {
        Object obj = (Object)Selection.activeObject;
        
        if (obj is GameObject) {
            SaveGameObjectReferences((GameObject)obj);
        } else if (obj is DefaultAsset) {
            // Support only directories for default assets
            Debug.Log("[SaveReferences] Is a directory");
            string path = AssetDatabase.GetAssetPath(obj);
            bool isValidFolder = AssetDatabase.IsValidFolder(path);

            if (isValidFolder) { 
                SaveDirectoryReferences(path);
            } else {
                Debug.LogError("[SaveReferences] Unhandled type");
            }
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/Fix Missing References")]
    static void FixReferences() {
        Object obj = (Object)Selection.activeObject;

        if (obj is GameObject) {
            FixGameObjectReferences((GameObject)obj);
        } else if (obj is DefaultAsset) {
            // Support only directories for default assets
            Debug.Log("[FixReferneces] Is a directory");
            string path = AssetDatabase.GetAssetPath(obj);
            bool isValidFolder = AssetDatabase.IsValidFolder(path);

            if (isValidFolder) { 
                FixDirectoryReferences(path);
            } else {
                Debug.LogError("[FixReferneces] Unhandled type");
            }
        }
    }

    static void SaveGameObjectReferences(GameObject go) {
        Debug.Log("[SaveGameObjectReferences] Saving refernces for " + go.name);
        string path = "Assets/References";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        string assetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(go));

        StringBuilder sb = new StringBuilder();
        MonoBehaviour[] mbs = go.GetComponents<MonoBehaviour>();

        // order is always maintained so check and load back in the same order
        foreach (MonoBehaviour mb in mbs) {
            sb.AppendLine(mb.GetType().Name);
        }
        File.WriteAllText(path + "/" + assetGUID + "." + go.name, sb.ToString());

        foreach (Transform t in go.transform) {
            SaveGameObjectReferences(t.gameObject);
        }
    }

    static void FixGameObjectReferences(GameObject go) {
        Debug.Log("[FixGameObjectReferences] Fixing refernces for " + go.name);
        string path = "Assets/References";

        if (!Directory.Exists(path)) { 
            Debug.LogError("[FixGameObjectReferences] Reference folder does not exist. Cannot fix references.");
            return;
        }

        string assetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(go));
        string fileName = path + "/" + assetGUID + "." + go.name;
        string[] mbNames = File.ReadAllLines(fileName);
        MonoBehaviour[] mbs = go.GetComponents<MonoBehaviour>();

        for (int i = 0; i < mbs.Length; i++) {
            MonoBehaviour mb = mbs[i];
            if (mb == null) { // Missing reference
                string mbName = mbNames[i];
                // EditorUtility.CopySerialized()
                Debug.Log("[FixGameObjectReferences] Found a missing component. It should be '" + mbName + "'");
            }
        }
        
        foreach (Transform t in go.transform) {
            FixGameObjectReferences(t.gameObject);
        }
    }
    
    static void SaveDirectoryReferences(string path) {
        Debug.Log("[SaveDirectoryReferences] Reading folder " + path);

        // Check for prefabs
        string[] filePaths = Directory.GetFiles(path);
        foreach (string filePath in filePaths) {
            Object obj = AssetDatabase.LoadMainAssetAtPath(filePath);
            if (obj is GameObject) SaveGameObjectReferences((GameObject)obj);
        }

        // Iterate over other sub folders
        string[] childrenFolders = AssetDatabase.GetSubFolders(path);
        foreach (string childFolderPath in childrenFolders) {
            SaveDirectoryReferences(childFolderPath);
        }
    }

    static void FixDirectoryReferences(string path) {
        Debug.Log("[FixDirectoryReferences] Reading folder " + path);

        // Check for prefabs
        string[] filePaths = Directory.GetFiles(path);
        foreach (string filePath in filePaths) {
            Object obj = AssetDatabase.LoadMainAssetAtPath(filePath);
            if (obj is GameObject) SaveGameObjectReferences((GameObject)obj);
        }

        // Iterate over other sub folders
        string[] childrenFolders = AssetDatabase.GetSubFolders(path);
        foreach (string childFolderPath in childrenFolders) {
            FixDirectoryReferences(childFolderPath);
        }
    }
}