using UnityEngine;
using UnityEditor;
using System.IO;

namespace DYM.ToolBox
{

public class FBXMaterialGenerator : MonoBehaviour
{
    [MenuItem("美术工具/生成工具/FBX生成材质球")]
    public static void GenerateMaterials()
    {
        // 让用户选择要处理的文件夹
        string folderPath = EditorUtility.OpenFolderPanel("选择要处理的文件夹", "Assets", "");
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogWarning("未选择文件夹，操作取消");
            return;
        }

        // 将绝对路径转换为相对路径
        folderPath = "Assets" + folderPath.Substring(Application.dataPath.Length);
        string shaderName = "Miao/CarColorMask";

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError($"文件夹路径无效: {folderPath}");
            return;
        }

        // 检查着色器是否存在
        Shader shader = Shader.Find(shaderName);
        if (shader == null)
        {
            Debug.LogError($"找不到着色器: {shaderName}");
            return;
        }

        string[] fbxFiles = Directory.GetFiles(folderPath, "*.fbx", SearchOption.TopDirectoryOnly);
        if (fbxFiles.Length == 0)
        {
            Debug.LogWarning($"在文件夹 {folderPath} 中没有找到FBX文件");
            return;
        }

        int successCount = 0;
        foreach (string fbxFile in fbxFiles)
        {
            try
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fbxFile);
                string baseMapPath = Path.Combine(folderPath, $"{fileNameWithoutExtension}_Abedel.png");
                string colorMaskPath = Path.Combine(folderPath, $"{fileNameWithoutExtension}_ColorMask.png");
                string emissionMaskPath = Path.Combine(folderPath, $"{fileNameWithoutExtension}_EmissionMask.png");

                Material material = new Material(shader)
                {
                    name = fileNameWithoutExtension
                };

                bool hasTextures = false;
                if (File.Exists(baseMapPath))
                {
                    material.SetTexture("_BaseMap", LoadTexture(baseMapPath));
                    hasTextures = true;
                }

                if (File.Exists(colorMaskPath))
                {
                    material.SetTexture("_ColorMask", LoadTexture(colorMaskPath));
                    hasTextures = true;
                }

                if (File.Exists(emissionMaskPath))
                {
                    material.SetTexture("_EmissionMask", LoadTexture(emissionMaskPath));
                    hasTextures = true;
                }

                if (!hasTextures)
                {
                    Debug.LogWarning($"为 {fileNameWithoutExtension} 没有找到任何贴图");
                    continue;
                }

                string materialPath = Path.Combine(folderPath, $"{fileNameWithoutExtension}.mat");
                AssetDatabase.CreateAsset(material, materialPath);
                successCount++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"处理文件 {fbxFile} 时发生错误: {e.Message}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"材质球生成完成。成功处理 {successCount}/{fbxFiles.Length} 个文件。");
    }

    private static Texture LoadTexture(string path)
    {
        Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
        if (texture == null)
        {
            Debug.LogWarning($"无法加载贴图: {path}");
        }
        return texture;
    }
}
}