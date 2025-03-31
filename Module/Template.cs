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

namespace DYM.ToolBox
{

public class Template : EditorWindow
{
    private Vector2 scrollPosition;

    [MenuItem("美术工具/Template")]
    public static void ShowWindow()
    {
        GetWindow<Template>("Template");
    }

    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        GUILayout.EndScrollView();
    }

    // private string ()
    // {}


}
}