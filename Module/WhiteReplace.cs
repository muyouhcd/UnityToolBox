using UnityEngine;
using UnityEditor;
using System.IO;

namespace DYM.ToolBox
{

//替换指定路径的prefab中的材质为指定材质
public class ReplaceMaterialsInPrefabs : EditorWindow
{
    private string searchInFolder = "Assets/Prefabs"; // 默认搜索文件夹路径
    private Material newMaterial; // 用户指定的新材质

    [MenuItem("美术工具/替换材质/替换prefab中的材质")]
    public static void ShowWindow()
    {
        GetWindow<ReplaceMaterialsInPrefabs>("Replace Prefab Materials");
    }

    void OnGUI()
    {
        GUILayout.Label("Replace Materials in Prefabs", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Folder Path:", GUILayout.Width(70));
        searchInFolder = GUILayout.TextField(searchInFolder);
        GUILayout.EndHorizontal();

        // 新材质的拖放框
        newMaterial = (Material)EditorGUILayout.ObjectField("New Material", newMaterial, typeof(Material), false);

        if (GUILayout.Button("Replace Materials"))
        {
            if (newMaterial == null)
            {
                Debug.LogError("New material not set.");
                return;
            }
            ReplaceMaterials();
        }
    }

    private void ReplaceMaterials()
    {
        string[] prefabFiles = Directory.GetFiles(searchInFolder, "*.prefab", SearchOption.AllDirectories);
        foreach (string prefabPath in prefabFiles)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            ReplaceMaterialsInPrefab(prefab);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Materials replaced in all prefabs.");
    }

    private void ReplaceMaterialsInPrefab(GameObject prefab)
    {
        if (prefab == null)
            return;

        foreach (Renderer renderer in prefab.GetComponentsInChildren<Renderer>(true))
        {
            bool changed = false;
            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && renderer is MeshRenderer)
                {
                    materials[i] = newMaterial;
                    changed = true;
                }
            }

            if (changed)
            {
                renderer.sharedMaterials = materials;
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }
    }
}
}