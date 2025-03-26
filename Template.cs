using UnityEditor;
using UnityEngine;

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