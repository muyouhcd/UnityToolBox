using UnityEngine;
using UnityEditor;
using System.IO;

public class FBXMaterialGenerator : MonoBehaviour
{
    [MenuItem("美术工具/生成工具/FBX生成材质球")]
    public static void GenerateMaterials()
    {
        string folderPath = "Assets/";  // 
        string shaderName = "Miao/CarColorMask";

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError("Folder path is not valid");
            return;
        }

        string[] fbxFiles = Directory.GetFiles(folderPath, "*.fbx", SearchOption.TopDirectoryOnly);

        foreach (string fbxFile in fbxFiles)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fbxFile);

            string baseMapPath = Path.Combine(folderPath, $"{fileNameWithoutExtension}_Abedel.png");
            string colorMaskPath = Path.Combine(folderPath, $"{fileNameWithoutExtension}_ColorMask.png");
            string emissionMaskPath = Path.Combine(folderPath, $"{fileNameWithoutExtension}_EmissionMask.png");

            Material material = new Material(Shader.Find(shaderName))
            {
                name = fileNameWithoutExtension
            };

            if (File.Exists(baseMapPath))
            {
                material.SetTexture("_BaseMap", LoadTexture(baseMapPath));
            }

            if (File.Exists(colorMaskPath))
            {
                material.SetTexture("_ColorMask", LoadTexture(colorMaskPath));
            }

            if (File.Exists(emissionMaskPath))
            {
                material.SetTexture("_EmissionMask", LoadTexture(emissionMaskPath));
            }

            string materialPath = Path.Combine(folderPath, $"{fileNameWithoutExtension}.mat");
            AssetDatabase.CreateAsset(material, materialPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Material generation finished.");
    }

    private static Texture LoadTexture(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Texture>(path);
    }
}