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


public class DropObjects : EditorWindow
{
    private Vector2 scrollPosition;

    [MenuItem("美术工具/地编工具/DropObjects")]
    public static void ShowWindow()
    {
        GetWindow<DropObjects>("DropObjects");
    }

    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        if (GUILayout.Button("下落物体"))
        {
            DropSelectedObjectsToMesh();
        }

        GUILayout.EndScrollView();
    }

    // private string ()
    // {}

    public static void DropSelectedObjectsToMesh()
    {
        var selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.Log("No objects selected. Please select at least one GameObject.");
            return;
        }

        List<Collider> addedColliders = new List<Collider>();
        EnsureCollidersExistAndGather(ref addedColliders);

        DropObjectsToMesh(selectedObjects, ref addedColliders);

        CleanupAddedColliders(addedColliders);
    }
    private static void CleanupAddedColliders(List<Collider> addedColliders)
    {
        foreach (Collider addedCollider in addedColliders)
        {
            if (addedCollider != null)
            {
                DestroyImmediate(addedCollider);
            }
        }
    }


    private static void EnsureCollidersExistAndGather(ref List<Collider> addedColliders)
    {
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            if (obj.GetComponent<MeshRenderer>() != null && obj.GetComponent<Collider>() == null)
            {
                MeshCollider newCollider = obj.AddComponent<MeshCollider>();
                addedColliders.Add(newCollider);
            }
        }
    }
    private static void DropObjectsToMesh(GameObject[] selectedObjects, ref List<Collider> addedColliders)
    {
        foreach (GameObject obj in selectedObjects)
        {
            Collider collider = obj.GetComponent<Collider>();
            if (collider == null)
            {
                collider = obj.AddComponent<BoxCollider>();
                addedColliders.Add(collider);
            }

            RaycastHit hit;
            Vector3 colliderBottom = GetColliderBottomPoint(collider);

            if (Physics.Raycast(colliderBottom, Vector3.down, out hit))
            {
                obj.transform.position = new Vector3(obj.transform.position.x, hit.point.y, obj.transform.position.z);
                Debug.Log(obj.name + " dropped to mesh at " + hit.point);
            }
            else
            {
                Debug.LogWarning("No mesh found under " + obj.name + ". Object has not been moved.");
            }
        }
    }
    private static Vector3 GetColliderBottomPoint(Collider collider)
    {
        Vector3 bottomCenterPoint = collider.bounds.center;
        bottomCenterPoint.y -= collider.bounds.extents.y;
        return bottomCenterPoint;
    }


}