using UnityEngine;
using UnityEditor;

public class ReplaceWithPrefabWindow : EditorWindow
{
    private GameObject prefabToReplaceWith;

    [MenuItem("美术工具/替换工具/替换物体")]
    public static void ShowWindow()
    {
        GetWindow<ReplaceWithPrefabWindow>("Replace With Prefab");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace Selected Objects", EditorStyles.boldLabel);

        prefabToReplaceWith = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabToReplaceWith, typeof(GameObject), false);

        if (GUILayout.Button("Replace Selected"))
        {
            ReplaceSelectedObjects();
        }
    }

    private void ReplaceSelectedObjects()
    {
        if (prefabToReplaceWith == null || !PrefabUtility.IsPartOfPrefabAsset(prefabToReplaceWith))
        {
            Debug.LogError("请选择一个有效的 Prefab 资产。");
            return;
        }

        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogError("请在场景中选择一个或多个物体。");
            return;
        }

        Undo.RegisterCompleteObjectUndo(selectedObjects, "Replace with Prefab");

        foreach (GameObject obj in selectedObjects)
        {
            Vector3 originalPosition = obj.transform.position;
            Quaternion originalRotation = obj.transform.rotation;

            Undo.DestroyObjectImmediate(obj);

            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabToReplaceWith);
            newObject.transform.position = originalPosition;
            newObject.transform.rotation = originalRotation;

            Undo.RegisterCreatedObjectUndo(newObject, "Replace with Prefab");
        }
    }
}