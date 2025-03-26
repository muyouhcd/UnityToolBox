using UnityEngine;
using UnityEditor;

public class MissingScriptChecker : MonoBehaviour
{
    [MenuItem("Tools/Check Prefab for Missing Scripts")]
    public static void CheckForMissingScripts()
    {
        // Get the selected GameObjects in the editor
        GameObject[] selectedObjects = Selection.gameObjects;

        foreach (GameObject obj in selectedObjects)
        {
            CheckGameObjectForMissingScripts(obj);
        }
    }

    private static void CheckGameObjectForMissingScripts(GameObject obj)
    {
        // Traverse all components of the GameObject
        Component[] components = obj.GetComponents<Component>();

        foreach (Component component in components)
        {
            if (component == null)
            {
                Debug.LogWarning($"Missing script found in GameObject: {obj.name}", obj);
            }
        }

        // Recursively check all child GameObjects
        foreach (Transform child in obj.transform)
        {
            CheckGameObjectForMissingScripts(child.gameObject);
        }
    }
}