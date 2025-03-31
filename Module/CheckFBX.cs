using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

using System.Linq;

namespace DYM.ToolBox
{

public class FBXChecker : EditorWindow
{
    private string folderPath = "Assets/ArtResources/Character/Clothing/Upper";
    private List<string> noRootFBXFiles = new List<string>();
    private Vector2 scrollPosition;
    private bool showFullPath = false;
    private string noRootCSVPath = "";


    [MenuItem("美术工具/检查工具/FBX骨骼检查器")]
    public static void ShowWindow()
    {
        GetWindow<FBXChecker>("FBX骨骼检查器");
    }

    private void OnGUI()
    {
        GUILayout.Label("检查指定路径下的fbx中是否存在名称为Root的骨骼内容，以判断这个fbx是否经过绑定");
        GUILayout.Space(10);
        GUILayout.Label("选择文件夹路径", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        folderPath = EditorGUILayout.TextField("路径", folderPath);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("选择FBX文件夹", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                folderPath = selectedPath;
                Repaint();
            }
        }
        EditorGUILayout.EndHorizontal();


        if (GUILayout.Button("检查FBX文件", GUILayout.Height(30)))
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

        // 使用 GUILayout.BeginVertical 和 GUILayout.EndVertical 来创建弹性布局
        GUILayout.BeginVertical();
        
        // 列表区域会自动扩展
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
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
        GUILayout.EndVertical();

        // 底部区域
        GUILayout.Space(10);
        EditorGUILayout.LabelField("导出无Root层级的FBX列表", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        noRootCSVPath = EditorGUILayout.TextField("CSV输出路径", noRootCSVPath);
        if (GUILayout.Button("浏览...", GUILayout.Width(60)))
        {
            string defaultPath = !string.IsNullOrEmpty(noRootCSVPath) 
                ? Path.GetDirectoryName(noRootCSVPath) 
                : Application.dataPath;
                
            string path = EditorUtility.SaveFilePanel(
                "保存CSV文件",
                defaultPath,
                "NoRootFBXList.csv",
                "csv");
            if (!string.IsNullOrEmpty(path))
            {
                noRootCSVPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        // 导出按钮固定在底部
        if (GUILayout.Button("导出无Root层级的FBX列表", GUILayout.Height(30)))
        {
            if (string.IsNullOrEmpty(noRootCSVPath))
            {
                EditorUtility.DisplayDialog("错误", "请先选择CSV输出路径", "确定");
            }
            else
            {
                ExportNoRootFBXToCSV(noRootCSVPath);
            }
        }
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









    private void ExportNoRootFBXToCSV(string outputPath)
    {
        if (noRootFBXFiles.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有需要导出的FBX文件。", "确定");
            return;
        }

        try
        {
            // 确保输出目录存在
            string directory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (StreamWriter writer = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                // Write header with BOM for Excel compatibility
                writer.Write('\uFEFF');
                writer.WriteLine("文件名,文件路径,GUID");

                // Write each FBX file entry
                foreach (string fbxPath in noRootFBXFiles)
                {
                    string guid = AssetDatabase.AssetPathToGUID(fbxPath);
                    // 获取文件名（不带后缀）
                    string fileName = Path.GetFileNameWithoutExtension(fbxPath);
                    // 确保路径中的逗号被正确处理
                    string safePath = fbxPath.Replace(",", "，");
                    writer.WriteLine($"{fileName},{safePath},{guid}");
                }
            }

            Debug.Log($"已成功导出CSV文件到: {outputPath}");
            EditorUtility.DisplayDialog("成功", "CSV文件导出成功！", "确定");
            EditorUtility.RevealInFinder(outputPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"导出CSV时发生错误: {e.Message}");
            EditorUtility.DisplayDialog("错误", $"导出CSV时发生错误: {e.Message}", "确定");
        }
    }

    

}
}