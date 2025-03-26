using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using UnityEditor.ShortcutManagement;
using Unity.AI.Navigation; // 确保导入正确的命名空间
using UnityEditor.Experimental.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Security.Cryptography;

public class Replacer : EditorWindow
{
    private Vector2 scrollPosition;
    private GameObject targetPrefab;  // 要查找的嵌套预制体
    private GameObject replacementPrefab;  // 要替换的预制体

    public GameObject assetToReplace; // 要替换的资产



    private string pathA;
    private string pathB;

    [MenuItem("美术工具/替换工具/Replacer")]
    public static void ShowWindow()
    {
        GetWindow<Replacer>("Replacer");
    }

    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField("单个替换", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("将：", GUILayout.Width(100));  // 控制标签的宽度
        targetPrefab = (GameObject)EditorGUILayout.ObjectField(targetPrefab, typeof(GameObject), false);  // 自动调整宽度
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("替换为：", GUILayout.Width(100));  // 控制标签的宽度
        replacementPrefab = (GameObject)EditorGUILayout.ObjectField(replacementPrefab, typeof(GameObject), false);  // 自动调整宽度
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("替换预制体资产中的组件"))
        {
            if (targetPrefab == null || replacementPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both Target Prefab and Replacement Prefab", "OK");
            }
            else
            {
                ReplaceNestedPrefabsInSelectedPrefabs(targetPrefab, replacementPrefab);
            }
        }
        EditorGUILayout.Space();
        GUILayout.Space(10); // 添加一些空隙

        EditorGUILayout.LabelField("批量替换", EditorStyles.boldLabel);
        pathA = EditorGUILayout.TextField("将此路径下prefab包含资产（vox）：", pathA);
        pathB = EditorGUILayout.TextField("替换为此路径下同名称资产（prefab）：", pathB);
        if (GUILayout.Button("选取prefab资产后按路径批量替换同名组件"))
        {
            if (string.IsNullOrEmpty(pathA) || string.IsNullOrEmpty(pathB))
            {
                EditorUtility.DisplayDialog("Error", "Please assign both Path A and Path B", "OK");
            }
            else if (!Directory.Exists(pathA) || !Directory.Exists(pathB))
            {
                EditorUtility.DisplayDialog("Error", "One or both paths do not exist", "OK");
            }
            else
            {
                BatchReplacePrefabs(pathA, pathB);
            }
        }

        GUILayout.Space(10); // 添加一些空隙

        GUILayout.Label("资产批量替换", EditorStyles.boldLabel);
        assetToReplace = (GameObject)EditorGUILayout.ObjectField("将所选组件替换为：", assetToReplace, typeof(GameObject), false);
        if (GUILayout.Button("使用新资产替换物体"))
        {
            {
                Replace();
            }
        }

        GUILayout.EndScrollView();
    }

    // private string ()
    // {}
    private void ReplaceNestedPrefabsInSelectedPrefabs(GameObject targetPrefab, GameObject replacementGO)
    {
        GameObject[] selectedPrefabs = Selection.gameObjects;

        if (selectedPrefabs.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Please select at least one prefab in the Project window", "OK");
            return;
        }

        foreach (GameObject prefab in selectedPrefabs)
        {
            string pathToPrefab = AssetDatabase.GetAssetPath(prefab);
            if (!pathToPrefab.EndsWith(".prefab"))
            {
                Debug.LogWarning("Selected object is not a prefab: " + prefab.name);
                continue;
            }

            GameObject prefabContents = PrefabUtility.LoadPrefabContents(pathToPrefab);
            if (prefabContents == null)
            {
                Debug.LogError("Failed to load the prefab: " + prefab.name);
                continue;
            }

            List<Transform> nestedPrefabsToReplace = new List<Transform>();
            GetNestedPrefabsToReplace(prefabContents.transform, nestedPrefabsToReplace, targetPrefab);

            foreach (var nestedPrefabTransform in nestedPrefabsToReplace)
            {
                GameObject newGOInstance =
                    (GameObject)PrefabUtility.InstantiatePrefab(replacementGO, nestedPrefabTransform.parent);
                newGOInstance.name = replacementGO.name;
                newGOInstance.transform.SetSiblingIndex(nestedPrefabTransform.GetSiblingIndex());

                // 复制局部变换
                newGOInstance.transform.localPosition = nestedPrefabTransform.localPosition;
                newGOInstance.transform.localRotation = nestedPrefabTransform.localRotation;
                newGOInstance.transform.localScale = nestedPrefabTransform.localScale;

                DestroyImmediate(nestedPrefabTransform.gameObject);
            }

            PrefabUtility.SaveAsPrefabAsset(prefabContents, pathToPrefab);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        Debug.Log("Nested prefab replacement completed successfully.");
    }
    void BatchReplacePrefabs(string pathA, string pathB)
    {
        // 递归获取路径下的所有文件
        string[] filesA = Directory.GetFiles(pathA, "*.*", SearchOption.AllDirectories);
        string[] filesB = Directory.GetFiles(pathB, "*.*", SearchOption.AllDirectories);

        // 使用Dictionary存放文件名和对应文件路径，同时记录文件层级
        Dictionary<string, (string filePath, int depth)> nameToFileB = new Dictionary<string, (string, int)>();

        foreach (string fileB in filesB)
        {
            string fileName = Path.GetFileName(fileB);
            int depth = GetFileDepth(fileB, pathB);

            // 在字典中添加或更新文件，确保保存的是最上层的文件
            if (!nameToFileB.ContainsKey(fileName) || nameToFileB[fileName].depth > depth)
            {
                nameToFileB[fileName] = (fileB, depth);
            }
        }

        foreach (string fileA in filesA)
        {
            string fileName = Path.GetFileName(fileA);

            if (nameToFileB.TryGetValue(fileName, out var fileBData))
            {
                GameObject targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fileA);
                GameObject replacementPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fileBData.filePath);

                if (targetPrefab != null && replacementPrefab != null)
                {
                    ReplaceNestedPrefabsInSelectedPrefabs(targetPrefab, replacementPrefab);
                }
            }
        }

        Debug.Log("Batch prefab replacement completed successfully.");
    }
    private void Replace()
    {
        if (assetToReplace == null)
        {
            Debug.LogError("Please assign an asset to replace with.");
            return;
        }

        Transform[] selectedTransforms = Selection.transforms;

        if (selectedTransforms.Length == 0)
        {
            Debug.LogError("Please select at least one object to replace.");
            return;
        }

        foreach (Transform selectedTransform in selectedTransforms)
        {
            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(assetToReplace);

            if (newObject != null)
            {
                Undo.RegisterCreatedObjectUndo(newObject, "Replace With Asset");
                newObject.transform.position = selectedTransform.position;
                newObject.transform.rotation = selectedTransform.rotation;
                newObject.transform.localScale = selectedTransform.localScale;

                Undo.DestroyObjectImmediate(selectedTransform.gameObject); // 删除原始对象
            }
            else
            {
                Debug.LogError("Failed to instantiate the specified asset.");
            }
        }
    }

    private void GetNestedPrefabsToReplace(Transform parent, List<Transform> nestedPrefabsToReplace, GameObject targetPrefab)
    {
        foreach (Transform child in parent)
        {
            // Check if the child is an instance of the target prefab
            if (PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject) == targetPrefab)
            {
                nestedPrefabsToReplace.Add(child);
            }
            else
            {
                // 递归搜索子物体中的嵌套预制体
                GetNestedPrefabsToReplace(child, nestedPrefabsToReplace, targetPrefab);
            }
        }
    }

    int GetFileDepth(string filePath, string basePath)
    {
        return filePath.Substring(basePath.Length).Split(Path.DirectorySeparatorChar).Length;
    }

}