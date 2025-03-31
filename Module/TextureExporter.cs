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


public class TextureExporter : EditorWindow
{
    private Texture2D texture;
    private string fileName = "exportedTexture";
    private Vector2 scrollPosition;

    [MenuItem("美术工具/导出工具/贴图导出")]
    public static void ShowWindow()
    {
        GetWindow<TextureExporter>("Texture Exporter");

    }

    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);


        GUILayout.Label("Export Texture to PNG", EditorStyles.boldLabel);

        texture = (Texture2D)EditorGUILayout.ObjectField("贴图", texture, typeof(Texture2D), false);
        fileName = EditorGUILayout.TextField("文件名", fileName);

        if (GUILayout.Button("导出"))
        {
            ExportTextureAsPNG();
        }

        if (GUILayout.Button("导出所选资产贴图"))
        {
            ExportTexturesFromSelectedObjects();
        }
        if (GUILayout.Button("自动链接同名贴图"))
        {
            AutolinkSelectedMaterials();
        }


        GUILayout.EndScrollView();
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
    private static void ExportTexturesFromSelectedObjects()
    {
        string exportPath = EditorUtility.SaveFolderPanel("Export Textures", "", "");

        if (string.IsNullOrEmpty(exportPath))
        {
            return;
        }

        HashSet<Texture2D> exportedTextures = new HashSet<Texture2D>();

        UnityEngine.Object[] selectedAssets = Selection.objects;

        foreach (UnityEngine.Object obj in selectedAssets)
        {
            if (obj is GameObject go)
            {
                string assetPath = AssetDatabase.GetAssetPath(go);

                if (string.IsNullOrEmpty(assetPath))
                {
                    ExportTexturesFromSceneObject(go, exportPath, exportedTextures);
                }
                else if (AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) is GameObject prefab)
                {
                    ExportTexturesFromPrefab(prefab, exportPath, exportedTextures);
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Texture export completed.");
    }
    private static void AutolinkSelectedMaterials()
    {
        foreach (var obj in Selection.objects)
        {
            if (obj is Material material)
            {
                ProcessMaterial(material);
            }
        }
    }
    private static void ExportTexturesFromSceneObject(GameObject sceneObject, string exportPath, HashSet<Texture2D> exportedTextures)
    {
        Renderer[] renderers = sceneObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;

            foreach (Material material in materials)
            {
                if (material.mainTexture is Texture2D texture && !exportedTextures.Contains(texture))
                {
                    ProcessTextureForExport(texture, exportPath, renderer.gameObject.name, exportedTextures);
                }
            }
        }
    }
    private static void ExportTexturesFromPrefab(GameObject prefab, string exportPath, HashSet<Texture2D> exportedTextures)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;

            foreach (Material material in materials)
            {
                if (material.mainTexture is Texture2D texture && !exportedTextures.Contains(texture))
                {
                    ProcessTextureForExport(texture, exportPath, renderer.gameObject.name, exportedTextures);
                }
            }
        }
    }
    private static void ProcessTextureForExport(Texture2D texture, string exportPath, string objectName, HashSet<Texture2D> exportedTextures)
    {
        string texturePath = AssetDatabase.GetAssetPath(texture);
        if (texturePath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
        {
            // If the texture is already a PNG, copy the file directly
            string fileName = $"{objectName}_{Path.GetFileName(texturePath)}";
            string exportFilePath = Path.Combine(exportPath, fileName);
            File.Copy(texturePath, exportFilePath, overwrite: true);
            Debug.Log($"Copied original PNG texture {texture.name} to {exportFilePath}");
        }
        else
        {
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;

            if (importer != null)
            {
                bool wasReadable = importer.isReadable;

                TextureImporterPlatformSettings platformSettings = importer.GetDefaultPlatformTextureSettings();
                TextureImporterFormat originalFormat = platformSettings.format;
                TextureImporterCompression originalCompression = importer.textureCompression;

                // Set texture as readable and uncompressed (RGBA32) for export
                importer.isReadable = true;
                platformSettings.format = TextureImporterFormat.RGBA32;
                importer.SetPlatformTextureSettings(platformSettings);

                importer.textureCompression = TextureImporterCompression.Uncompressed;
                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);

                ExportTexture(texture, exportPath, objectName);
                exportedTextures.Add(texture);

                // Restore original settings
                importer.isReadable = wasReadable;
                platformSettings.format = originalFormat;
                importer.SetPlatformTextureSettings(platformSettings);

                importer.textureCompression = originalCompression;
                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
            }
        }
    }

    private static void ExportTexture(Texture2D texture, string exportPath, string objectName)
    {
        string fileName = $"{objectName}_{texture.name}.png";
        string exportFilePath = Path.Combine(exportPath, fileName);

        byte[] textureData = texture.EncodeToPNG();
        if (textureData != null)
        {
            File.WriteAllBytes(exportFilePath, textureData);
            Debug.Log($"Exported texture {texture.name} to {exportFilePath}");
        }
        else
        {
            Debug.LogError($"Failed to encode texture {texture.name} to PNG format.");
        }
    }
    private static void ProcessMaterial(Material material)
    {
        // 使用 Basemap 属性代替 BaseColor
        string texturePropertyName = "_BaseMap"; // 更新为 Basemap 属性名称

        // 检查是否有Basemap贴图属性
        if (!material.HasProperty(texturePropertyName))
        {
            Debug.LogWarning($"Material {material.name} does not have a Basemap property.");
            return;
        }

        // 如果Basemap贴图已经存在，跳过处理
        if (material.GetTexture(texturePropertyName) != null)
        {
            Debug.Log($"Material {material.name} already has a Basemap texture.");
            return;
        }

        // 获取材质球的路径
        string materialPath = AssetDatabase.GetAssetPath(material);
        string materialDirectory = Path.GetDirectoryName(materialPath);

        // 提取材质球名称的通用部分，去除_LOD和_mat部分
        string pattern = @"(.+)_LOD\d+_mat";
        Match match = Regex.Match(material.name, pattern);
        if (!match.Success)
        {
            Debug.LogWarning($"Material name {material.name} does not match the expected naming convention.");
            return;
        }

        string baseName = match.Groups[1].Value;

        // 在同目录中查找命名相似的贴图
        string[] allFiles = Directory.GetFiles(materialDirectory, "*.png"); // 假设贴图格式为PNG
        foreach (var file in allFiles)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
            if (Regex.IsMatch(fileNameWithoutExtension, $@"{baseName}_LOD\d+_tex"))
            {
                Texture2D baseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
                if (baseTexture != null)
                {
                    material.SetTexture(texturePropertyName, baseTexture);
                    Debug.Log($"Applied texture {file} to material {material.name}");
                }
                else
                {
                    Debug.LogWarning($"Failed to load texture at path: {file}");
                }
                break;
            }
        }
    }






}
}