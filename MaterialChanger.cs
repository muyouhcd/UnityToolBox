
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

public class FbxMatChanger : EditorWindow
{
    private Vector2 scrollPosition;
    private Material selectedMaterial;
    private static readonly string materialImportModePattern = @"materialImportMode: \d";
    private static readonly string externalObjectsPattern = @"externalObjects:\s*\{\s*\}";


    [MenuItem("美术工具/材质修改工具/材质修改（批量）")]
    public static void ShowWindow()
    {
        GetWindow<FbxMatChanger>("FbxMatChanger");
    }

    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("FBX材质修改（支持批量）", EditorStyles.boldLabel);

        selectedMaterial = EditorGUILayout.ObjectField(selectedMaterial, typeof(Material), false) as Material;  // 控制对象字段的宽度

        if (GUILayout.Button("更改导入方式"))
        {
            ModifySelectedMetaFiles(changeMaterialImportMode: true);
        }
        // 如果用户点击了这个按钮，将触发更新.meta文件的操作
        if (GUILayout.Button("更新材质引用"))
        {
            if (selectedMaterial == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select a material to assign.", "OK");
            }
            else
            {
                ModifySelectedMetaFiles(updateExternalObjects: true);
            }
        }



        if (GUILayout.Button("批量更改shader为URPLit"))
        {
            ChangeShader();

        }

        GUILayout.EndScrollView();
    }

    // private string ()
    // {}

    private void ModifySelectedMetaFiles(bool changeMaterialImportMode = false, bool updateExternalObjects = false)
    {
        foreach (UnityEngine.Object obj in Selection.objects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            string metaPath = AssetDatabase.GetTextMetaFilePathFromAssetPath(assetPath);

            if (changeMaterialImportMode)
            {
                ModifyMaterialImportMode(metaPath);
            }

            if (updateExternalObjects)
            {
                UpdateExternalObjectsWithName(metaPath, obj);
            }
        }

        AssetDatabase.Refresh();

    }
    private void ModifyMaterialImportMode(string metaPath)
    {
        string metaContent = File.ReadAllText(metaPath);
        metaContent = Regex.Replace(metaContent, materialImportModePattern, "materialImportMode: 1");
        File.WriteAllText(metaPath, metaContent);
    }
    private void UpdateExternalObjectsWithName(string metaPath, UnityEngine.Object asset)
    {
        string content = File.ReadAllText(metaPath);

        // 从选中的资产获取材质球名称
        string materialName = GetAssetMaterialName(asset);

        // 获取选中的Material的GUID
        string materialGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(selectedMaterial));

        // 准备替换的内容
        string replacement = $"externalObjects:\n" +
                             "  - first:\n" +
                             "      type: UnityEngine:Material\n" +
                             "      assembly: UnityEngine.CoreModule\n" +
                             $"      name: {materialName}\n" +
                             "    second: {fileID: 2100000, guid: " + materialGUID + ", type: 2}"; // 这里我们直接将fileID的值嵌入到字符串中

        // 替换externalObjects字段
        content = Regex.Replace(content, externalObjectsPattern, replacement, RegexOptions.Singleline);
        File.WriteAllText(metaPath, content);
    }
    private string GetAssetMaterialName(UnityEngine.Object asset)
    {
        var assetPath = AssetDatabase.GetAssetPath(asset);
        var assetImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;

        if (assetImporter != null)
        {
            // Load the model prefab to access its materials
            var assetObj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (assetObj != null)
            {
                var renderer = assetObj.GetComponentInChildren<Renderer>();
                if (renderer != null && renderer.sharedMaterials.Length > 0 && renderer.sharedMaterials[0] != null)
                {
                    return renderer.sharedMaterials[0].name;
                }
            }
        }
        return null;
    }


    private static void ChangeShader()
    {
        // Define the URP Lit Shader
        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");

        // Check that the URP Lit Shader exists
        if (urpLitShader == null)
        {
            Debug.LogError("Universal Render Pipeline/Lit shader not found in the project.");
            return;
        }

        // Get the selected GameObjects in the Editor
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No GameObjects selected. Please select GameObjects to change their shaders.");
            return;
        }

        int changedMaterialsCount = 0;

        // Loop through each selected GameObject
        foreach (GameObject obj in selectedObjects)
        {
            // Get all renderers in the GameObject (and its children)
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                // Loop through all materials in the renderer
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material != null && material.shader != urpLitShader)
                    {
                        Undo.RecordObject(material, "Change Material Shader");
                        material.shader = urpLitShader;
                        changedMaterialsCount++;
                    }
                }
            }
        }

        Debug.Log($"Changed shader to Universal Render Pipeline/Lit for {changedMaterialsCount} materials.");
    }

}