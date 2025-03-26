using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class FBXChecker : EditorWindow
{
    private string folderPath = "Assets/";
    private List<string> noRootFBXFiles = new List<string>();
    private Vector2 scrollPosition;
    private bool showFullPath = false;

    [MenuItem("美术工具/检查工具/FBX骨骼检查器")]
    public static void ShowWindow()
    {
        GetWindow<FBXChecker>("FBX骨骼检查器");
    }

    private void OnGUI()
    {
        GUILayout.Label("选择文件夹路径", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        folderPath = EditorGUILayout.TextField("路径", folderPath, GUILayout.Height(20));
        if (GUILayout.Button("浏览", GUILayout.Width(60), GUILayout.Height(20)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("选择FBX文件夹", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // 更新为绝对路径
                folderPath = selectedPath;
                Repaint(); // 强制刷新窗口
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("检查FBX文件"))
        {
            CheckFBXFiles(folderPath);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("清空列表"))
        {
            noRootFBXFiles.Clear();
        }
        showFullPath = EditorGUILayout.Toggle("显示完整路径", showFullPath, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();

        GUILayout.Label("没有Root子物体的FBX文件:", EditorStyles.boldLabel);

        // Calculate adaptive height
        float listHeight = position.height - 160; // Adjust this value as needed
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(listHeight));
        foreach (string fbxFile in noRootFBXFiles)
        {
            string displayFileName = showFullPath ? fbxFile : Path.GetFileName(fbxFile);
            if (GUILayout.Button(displayFileName, EditorStyles.linkLabel))
            {
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fbxFile);
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void CheckFBXFiles(string path)
    {
        noRootFBXFiles.Clear();

        if (!Directory.Exists(path))
        {
            Debug.LogError("路径不存在: " + path);
            return;
        }

        string[] fbxFiles = Directory.GetFiles(path, "*.fbx", SearchOption.AllDirectories);
        foreach (string fbxFile in fbxFiles)
        {
            // Convert absolute path to relative path for AssetDatabase
            string assetPath = "Assets" + fbxFile.Substring(Application.dataPath.Length);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (obj != null)
            {
                bool hasRoot = false;
                foreach (Transform child in obj.transform)
                {
                    if (child.name == "Root")
                    {
                        hasRoot = true;
                        break;
                    }
                }

                if (!hasRoot)
                {
                    noRootFBXFiles.Add(assetPath);
                }
            }
            else
            {
                Debug.LogWarning("无法加载: " + assetPath);
            }
        }

        if (noRootFBXFiles.Count == 0)
        {
            Debug.Log("所有FBX文件都包含Root子物体。");
        }
    }
}