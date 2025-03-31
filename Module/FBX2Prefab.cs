using UnityEngine;
using UnityEditor;
using System.IO;

namespace DYM.ToolBox
{

public class FBXToPrefabConverterWindow : EditorWindow
{
    private string fbxFolderPath = "Assets/YourFBXFolder";
    private string prefabFolderPath = "Assets/Prefabs";

    [MenuItem("美术工具/生成工具/FBX转换为Prefab")]
    public static void ShowWindow()
    {
        GetWindow<FBXToPrefabConverterWindow>("FBX to Prefab Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("FBX to Prefab Converter", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        fbxFolderPath = EditorGUILayout.TextField("FBX Folder Path:", fbxFolderPath);
        prefabFolderPath = EditorGUILayout.TextField("Prefab Folder Path:", prefabFolderPath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Convert"))
        {
            ConvertFBXToPrefab(fbxFolderPath, prefabFolderPath);
        }
    }

    private void ConvertFBXToPrefab(string inputPath, string outputPath)
    {
        if (!AssetDatabase.IsValidFolder(inputPath))
        {
            Debug.LogError("FBX folder path is not valid.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        string[] fbxPaths = Directory.GetFiles(inputPath, "*.fbx", SearchOption.AllDirectories);

        foreach (string fbxPath in fbxPaths)
        {
            GameObject fbxObject = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);

            if (fbxObject == null)
            {
                Debug.LogWarning("Cannot load FBX: " + fbxPath);
                continue;
            }

            foreach (Transform child in fbxObject.GetComponentsInChildren<Transform>())
            {
                Debug.Log("Found child: " + child.name);
            }

            GameObject prefabInstance = new GameObject(fbxObject.name);

            foreach (Transform child in fbxObject.transform)
            {
                string childNameLower = child.name.ToLower();

                if (childNameLower.Contains("bound"))
                {
                    Debug.Log("Checking for collision object: " + child.name);
                    MeshFilter meshFilter = child.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        Debug.Log("Adding MeshCollider for: " + child.name);
                        MeshCollider meshCollider = prefabInstance.AddComponent<MeshCollider>();
                        meshCollider.sharedMesh = meshFilter.sharedMesh;
                        meshCollider.convex = true;
                    }
                    else
                    {
                        Debug.LogWarning("No MeshFilter found for: " + child.name);
                    }
                }
                else
                {
                    GameObject newChild = Instantiate(child.gameObject);
                    newChild.name = child.gameObject.name;
                    newChild.transform.SetParent(prefabInstance.transform);
                }
            }

            string prefabPath = Path.Combine(outputPath, fbxObject.name + ".prefab");
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            DestroyImmediate(prefabInstance);
        }

        AssetDatabase.Refresh();
    }
}
}