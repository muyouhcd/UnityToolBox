
using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.AI.Navigation; // 确保导入正确的命名空间

namespace DYM.ToolBox
{



public class NavMeshBaker : EditorWindow
{

    [MenuItem("美术工具/Navemesh工具/NaveMeshBaker")]
    public static void ShowWindow()
    {
        GetWindow<NavMeshBaker>("NavMeshBaker");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("烘焙所选预制体NavMesh（可批量）"))
        {
            AddNavMeshSurfaceToSelectedPrefabs();
        }

    }

    private UnityEngine.Object[] prefabsToModify;
    private void AddNavMeshSurfaceToSelectedPrefabs()
    {
        // 获取当前选择的 Prefabs 列表
        prefabsToModify = Selection.objects
                            .Where(o => AssetDatabase.GetAssetPath(o).EndsWith(".prefab"))
                            .ToArray();

        if (prefabsToModify.Length == 0)
        {
            Debug.LogWarning("No prefabs selected.");
            return;
        }

        foreach (UnityEngine.Object prefab in prefabsToModify)
        {
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            if (prefabRoot != null)
            {
                // 检查根节点是否已有 NavMeshSurface 组件
                NavMeshSurface navMeshSurface = prefabRoot.GetComponent<NavMeshSurface>();
                if (navMeshSurface == null)
                {
                    navMeshSurface = prefabRoot.AddComponent<NavMeshSurface>();
                    Debug.Log($"Added NavMeshSurface to {prefab.name}");
                }
                else
                {
                    Debug.Log($"Prefab {prefab.name} already has NavMeshSurface component");
                }

                // 自动烘焙 NavMesh
                navMeshSurface.BuildNavMesh();

                // 保存 NavMesh 数据
                if (navMeshSurface.navMeshData != null)
                {
                    string navMeshDataPath = System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(prefabPath),
                        prefab.name + "_NavMeshData.asset"
                    );

                    AssetDatabase.CreateAsset(navMeshSurface.navMeshData, navMeshDataPath);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"NavMesh data for {prefab.name} saved to {navMeshDataPath}");
                }
                else
                {
                    Debug.LogWarning($"Failed to build NavMesh for {prefab.name}");
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
            else
            {
                Debug.LogWarning($"Failed to load prefab: {prefab.name}");
            }
        }
    }




}
}