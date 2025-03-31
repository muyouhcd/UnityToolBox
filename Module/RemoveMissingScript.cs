using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace DYM.ToolBox
{

public class BatchMissingScriptCleaner : EditorWindow
{
    private string prefabFolderPath = "Assets"; // 默认路径

    [MenuItem("美术工具/检查工具/丢失脚本清除工具")]
    public static void ShowWindow()
    {
        GetWindow(typeof(BatchMissingScriptCleaner), false, "Batch Clean Missing Scripts");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Clean Missing Scripts", EditorStyles.boldLabel);
        prefabFolderPath = EditorGUILayout.TextField("Prefabs Folder Path", prefabFolderPath);

        if (GUILayout.Button("Clean Missing Scripts"))
        {
            CleanMissingScriptsInAllPrefabs(prefabFolderPath);
        }
    }

    private void CleanMissingScriptsInAllPrefabs(string path)
    {
        string[] prefabFiles = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
        List<GameObject> modifiedPrefabs = new List<GameObject>();

        foreach (string prefabFile in prefabFiles)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabFile);

            if (prefab == null)
            {
                Debug.LogWarning($"Cannot load prefab at {prefabFile}");
                continue;
            }

            int totalRemoved = RemoveMissingScriptsInPrefab(prefab);
            if (totalRemoved > 0)
            {
                Debug.Log($"Marked {totalRemoved} missing scripts removal in {prefabFile}");
                modifiedPrefabs.Add(prefab);
            }
        }

        foreach (var prefab in modifiedPrefabs)
        {
            PrefabUtility.SavePrefabAsset(prefab);
        }

        AssetDatabase.Refresh();
        Debug.Log("Completed cleaning all prefabs.");
    }

    private int RemoveMissingScriptsInPrefab(GameObject prefab)
    {
        int totalRemoved = 0;
        foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
        {
            totalRemoved += RemoveMissingScriptsFromGameObject(child.gameObject);
        }
        return totalRemoved;
    }

    private int RemoveMissingScriptsFromGameObject(GameObject go)
    {
        int count = 0;
        var components = go.GetComponents<Component>();

        foreach (var component in components)
        {
            if (component == null)
            {
                Undo.RegisterCompleteObjectUndo(go, "Remove Missing Script");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                count++;
            }
        }

        return count;
    }
}
}