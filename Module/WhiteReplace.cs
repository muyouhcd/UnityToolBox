using UnityEngine;
using UnityEditor;
using System.IO;

namespace DYM.ToolBox
{

//�滻ָ��·����prefab�����в���Ϊָ������
public class ReplaceMaterialsInPrefabs : EditorWindow
{
    private string searchInFolder = "Assets/Prefabs"; // Ĭ�������ļ���·��
    private Material newMaterial; // �û�ָ�����²���

    [MenuItem("��������/�滻����/�滻prefab�еĲ���")]
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

        // �²��ʵ��Ϸſ�
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