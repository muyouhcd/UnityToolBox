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

public class Cleaner : EditorWindow
{
    private Vector2 scrollPosition;
    private string directoryPath = "Assets";

    [MenuItem("美术工具/清理工具/Cleaner")]
    public static void ShowWindow()
    {
        GetWindow<Cleaner>("Cleaner");
    }

    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.Space(10); // 添加一些空隙

        GUILayout.Label("清理空文件夹", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("清理路径（递归）", GUILayout.Width(100));
        directoryPath = EditorGUILayout.TextField(directoryPath);

        if (GUILayout.Button("浏览", GUILayout.Width(100)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("选择路径", directoryPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                directoryPath = selectedPath.Replace("/", "\\");
            }
        }
        if (GUILayout.Button("清理", GUILayout.Width(100)))
        {
            Clean(directoryPath);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.EndScrollView();
    }
    private void Clean(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("路径不能为空");
            return;
        }

        if (!Directory.Exists(path))
        {
            Debug.LogError("指定的目录不存在: " + path);
            return;
        }

        int deletedFolders = CleanEmptyDirectories(path);
        Debug.Log($"清理完成，共删除了 {deletedFolders} 个空文件夹。");
        AssetDatabase.Refresh();
    }
    private int CleanEmptyDirectories(string dir)
    {
        int deletedCount = 0;

        try
        {
            // 递归清理子文件夹
            foreach (var subdir in Directory.GetDirectories(dir))
            {
                deletedCount += CleanEmptyDirectories(subdir);
            }

            // 如果是空文件夹，删除它
            if (Directory.GetFileSystemEntries(dir).Length == 0 &&
                (dir != Application.dataPath))
            {
                Directory.Delete(dir);
                File.Delete(dir + ".meta");  // 删除.meta文件
                Debug.Log("Deleted empty folder: " + dir);
                deletedCount++;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error cleaning directories: " + ex.Message);
        }

        return deletedCount;
    }


    // private string ()
    // {}


}