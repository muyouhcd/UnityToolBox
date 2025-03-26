using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureExporter : EditorWindow
{
    private Texture2D texture;
    private string fileName = "exportedTexture";

    [MenuItem("美术工具/导出工具/贴图导出")]
    public static void ShowWindow()
    {
        GetWindow<TextureExporter>("Texture Exporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Export Texture to PNG", EditorStyles.boldLabel);

        texture = (Texture2D)EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), false);
        fileName = EditorGUILayout.TextField("File Name", fileName);

        if (GUILayout.Button("Export"))
        {
            ExportTextureAsPNG();
        }
    }

    private void ExportTextureAsPNG()
    {
        if (texture == null)
        {
            Debug.LogError("No texture specified!");
            return;
        }

        string path = EditorUtility.SaveFilePanel("Save Texture as PNG", "", fileName, "png");

        if (path.Length != 0)
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
            Debug.Log("Texture exported to " + path);
        }
    }
}