using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.AI.Navigation;

namespace DYM.ToolBox
{
public class TransformManager : EditorWindow
{
    private int SpaceHeight = 10;
    private Vector2 scrollPosition;
    private const float ScaleFactor = 0.01f;
    private Vector3 copiedPosition;
    private Quaternion copiedRotation;
    private GameObject targetPrefab;  // 要查找的嵌套预制体
    private static List<Vector3> recordedPositions = new List<Vector3>();

    // 线性阵列参数
    private int numberOfCopies = 5;
    private float distance = 1.0f;
    private bool xAxis = true;
    private bool yAxis = false;
    private bool zAxis = false;

    // 矩形阵列参数
    private float xSpacing = 1f;
    private float zSpacing = 1f;

    // 圆形阵列参数
    private GameObject centerObject;
    private GameObject objectToDuplicate;
    private int count = 8;
    private float radius = 5f;
    private bool autoCalculateRadius = true;
    private bool lookAtCenter = true;


    private float quarterValue = 0.25f; // 默认取整值

    [MenuItem("美术工具/地编工具/变换操作")]
    public static void ShowWindow()
    {
        GetWindow<TransformManager>("TransformManager");
    }

    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        // 基础变换工具
        if (GUILayout.Button("下落物体"))
        {
            DropSelectedObjectsToMesh();
        }

        GUILayout.Space(SpaceHeight);

        if (GUILayout.Button("复制位置旋转变换"))
        {
            CopyTransform();
        }
        if (GUILayout.Button("粘贴位置旋转变换"))
        {
            PasteTransform();
        }
        
        GUILayout.Space(SpaceHeight);
        
        if (GUILayout.Button("选取两个物体交换位置旋转"))
        {
            SwapTransforms();
        }
        
        GUILayout.Space(SpaceHeight);
        
        if (GUILayout.Button("批量复制底部中心位置（需要同名）"))
        {
            RecordPositions();
        }
        if (GUILayout.Button("批量粘贴底部中心位置（需要同名）"))
        {
            PastePositions();
        }
        
        GUILayout.Space(SpaceHeight);
        
        if (GUILayout.Button("尺寸缩小100比例"))
        {
            ResizeSelectedObject();
        }

        GUILayout.Space(SpaceHeight);

        // 线性阵列
        GUILayout.Label("线性阵列选项", EditorStyles.boldLabel);

        numberOfCopies = EditorGUILayout.IntField("复制数量", numberOfCopies);
        distance = EditorGUILayout.FloatField("间距", distance);

        GUILayout.Label("方向");
        EditorGUILayout.BeginHorizontal();
        xAxis = EditorGUILayout.Toggle("X", xAxis);
        yAxis = EditorGUILayout.Toggle("Y", yAxis);
        zAxis = EditorGUILayout.Toggle("Z", zAxis);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("复制所选物体"))
        {
            ArrayDuplicate();
        }

        GUILayout.Space(SpaceHeight);

        // 矩形阵列
        EditorGUILayout.LabelField("矩形阵列所选物体", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("X 间距", GUILayout.Width(100));
        xSpacing = EditorGUILayout.FloatField(xSpacing);
        EditorGUILayout.LabelField("Z 间距", GUILayout.Width(100));
        zSpacing = EditorGUILayout.FloatField(zSpacing);
        if (GUILayout.Button("阵列所选物体"))
        {
            ArrangeSelectedObjects();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(SpaceHeight);

        // 圆周阵列
        EditorGUILayout.LabelField("圆周阵列", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("中心参照物", GUILayout.Width(100));
        centerObject = (GameObject)EditorGUILayout.ObjectField(centerObject, typeof(GameObject), true);
        EditorGUILayout.LabelField("阵列物体", GUILayout.Width(100));
        objectToDuplicate = (GameObject)EditorGUILayout.ObjectField(objectToDuplicate, typeof(GameObject), true);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("计数", GUILayout.Width(100));
        count = EditorGUILayout.IntField(count, GUILayout.Width(100));
        EditorGUILayout.LabelField("自动半径", GUILayout.Width(100));
        autoCalculateRadius = EditorGUILayout.Toggle(autoCalculateRadius, GUILayout.Width(16));
        if (!autoCalculateRadius)
        {
            EditorGUILayout.LabelField("直径", GUILayout.Width(50));
            radius = EditorGUILayout.FloatField(radius, GUILayout.Width(50));
        }
        EditorGUILayout.LabelField("朝向中心", GUILayout.Width(100));
        lookAtCenter = EditorGUILayout.Toggle(lookAtCenter, GUILayout.Width(16));
        if (GUILayout.Button("计算阵列"))
        {
            if (centerObject != null && objectToDuplicate != null)
            {
                if (autoCalculateRadius)
                {
                    radius = Vector3.Distance(centerObject.transform.position, objectToDuplicate.transform.position);
                }
                GenerateCircleArray();
            }
            else
            {
                Debug.LogWarning("Please assign the Center Object and Object to Duplicate.");
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(SpaceHeight);


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("位置取整参数", GUILayout.Width(100));
        quarterValue = EditorGUILayout.FloatField(quarterValue, GUILayout.Width(50));
        if (GUILayout.Button("位置取整"))
        {
            RoundPositionToQuarter(quarterValue);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.EndScrollView();
    }

    #region 基础变换方法
    private void CopyTransform()
    {
        if (Selection.activeTransform != null)
        {
            copiedPosition = Selection.activeTransform.position;
            copiedRotation = Selection.activeTransform.rotation;
            Debug.Log("World Position & Rotation Copied");
        }
        else
        {
            Debug.Log("No object selected to copy transform");
        }
    }

    private void PasteTransform()
    {
        if (Selection.activeTransform != null)
        {
            Undo.RecordObject(Selection.activeTransform, "Paste World Position & Rotation");
            Selection.activeTransform.position = copiedPosition;
            Selection.activeTransform.rotation = copiedRotation;
            Debug.Log("World Position & Rotation Pasted to selected object");
        }
        else
        {
            Debug.Log("No object selected to paste transform");
        }
    }

    public static void SwapTransforms()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length != 2)
        {
            EditorUtility.DisplayDialog("Invalid Selection", "Please select exactly two objects to swap their transforms.", "OK");
            return;
        }

        GameObject obj1 = selectedObjects[0];
        GameObject obj2 = selectedObjects[1];

        // 暂存第一个物体的变换数据
        Vector3 position1 = obj1.transform.position;
        Vector3 rotation1 = obj1.transform.eulerAngles;

        // 交换位置
        obj1.transform.position = obj2.transform.position;
        obj2.transform.position = position1;

        // 交换旋转
        obj1.transform.eulerAngles = obj2.transform.eulerAngles;
        obj2.transform.eulerAngles = rotation1;

        EditorUtility.SetDirty(obj1);
        EditorUtility.SetDirty(obj2);
    }

    private void RecordPositions()
    {
        if (Selection.activeGameObject != null)
        {
            GameObject selectedObject = Selection.activeGameObject;
            targetPrefab = PrefabUtility.GetCorrespondingObjectFromSource(selectedObject) ?? selectedObject;
            recordedPositions.Clear();

            foreach (GameObject obj in FindObjectsOfType<GameObject>())
            {
                if (PrefabUtility.GetCorrespondingObjectFromSource(obj) == targetPrefab)
                {
                    Vector3 bottomCenter = GetBottomCenterPosition(obj.transform);
                    recordedPositions.Add(bottomCenter);
                }
            }

            Debug.Log($"Recorded {recordedPositions.Count} positions for prefab: {targetPrefab.name}");
        }
        else
        {
            Debug.LogWarning("No object selected!");
        }
    }

    private void PastePositions()
    {
        if (targetPrefab == null)
        {
            Debug.LogWarning("No positions recorded yet!");
            return;
        }

        GameObject[] instances = FindObjectsOfType<GameObject>();
        int index = 0;

        foreach (GameObject obj in instances)
        {
            if (PrefabUtility.GetCorrespondingObjectFromSource(obj) == targetPrefab && index < recordedPositions.Count)
            {
                Vector3 position = recordedPositions[index];
                obj.transform.position = position;
                index++;
            }
        }

        if (index < recordedPositions.Count)
        {
            Debug.LogWarning("Not all recorded positions were used!");
        }
        else
        {
            Debug.Log($"Pasted {index} positions to instances of prefab: {targetPrefab.name}");
        }
    }

    private static void ResizeSelectedObject()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length > 0)
        {
            Undo.RecordObjects(selectedObjects, "Resize Selected Object");

            foreach (GameObject obj in selectedObjects)
            {
                obj.transform.localScale *= ScaleFactor;
            }
        }
        else
        {
            Debug.LogWarning("没有选中任何物体！");
        }
    }

    private Vector3 GetBottomCenterPosition(Transform objTransform)
    {
        Renderer renderer = objTransform.GetComponent<Renderer>();

        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            Vector3 bottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            return bottomCenter;
        }
        else
        {
            Debug.LogWarning("Selected object does not have a Renderer component!");
            return Vector3.zero;
        }
    }
    #endregion

    #region 阵列方法
    private void ArrayDuplicate()
    {
        if (Selection.transforms.Length == 0)
        {
            Debug.LogWarning("No objects selected. Please select objects to array.");
            return;
        }

        Vector3 axis = Vector3.zero;
        if (xAxis) axis += Vector3.right;
        if (yAxis) axis += Vector3.up;
        if (zAxis) axis += Vector3.forward;

        if (axis == Vector3.zero)
        {
            Debug.LogWarning("No axis selected. Please select at least one axis.");
            return;
        }

        foreach (Transform original in Selection.transforms)
        {
            for (int i = 1; i <= numberOfCopies; i++)
            {
                GameObject clone;
                if (PrefabUtility.IsPartOfPrefabInstance(original.gameObject))
                {
                    clone = (GameObject)PrefabUtility.InstantiatePrefab(PrefabUtility.GetCorrespondingObjectFromSource(original.gameObject), original.parent);
                }
                else
                {
                    clone = Instantiate(original.gameObject, original.parent);
                }

                Undo.RegisterCreatedObjectUndo(clone, "Array Duplicate");
                clone.transform.localPosition = original.localPosition + original.rotation * (axis.normalized * distance * i);
                clone.transform.localRotation = original.localRotation;
            }
        }
    }

    private void ArrangeSelectedObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No objects selected!", "OK");
            return;
        }

        // 按名称自然排序
        Array.Sort(selectedObjects, (x, y) => NaturalCompare(x.name, y.name));

        // 计算网格大小
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(selectedObjects.Length));

        // 排列物体
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            int row = i / gridSize;
            int col = i % gridSize;
            Vector3 newPosition = new Vector3(col * xSpacing, selectedObjects[i].transform.position.y, row * zSpacing);
            selectedObjects[i].transform.position = newPosition;
        }
    }

    private void GenerateCircleArray()
    {
        // 先计算位置和旋转
        Vector3[] positions = new Vector3[count];
        Quaternion[] rotations = new Quaternion[count];

        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            positions[i] = CalculatePosition(angle);
            rotations[i] = CalculateRotation(angle);
        }

        // 创建物体
        for (int i = 0; i < count; i++)
        {
            GameObject instance = InstantiateObject(objectToDuplicate);
            instance.transform.position = positions[i];
            instance.transform.rotation = rotations[i];

            if (lookAtCenter && centerObject != null)
            {
                instance.transform.LookAt(centerObject.transform);
            }

            Undo.RegisterCreatedObjectUndo(instance, "Create Object Array");
        }
    }

    private Vector3 CalculatePosition(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float x = centerObject != null ? centerObject.transform.position.x + radius * Mathf.Cos(radians) : 0;
        float z = centerObject != null ? centerObject.transform.position.z + radius * Mathf.Sin(radians) : 0;
        return new Vector3(x, centerObject != null ? centerObject.transform.position.y : 0, z);
    }

    private GameObject InstantiateObject(GameObject original)
    {
        GameObject instance;
        if (PrefabUtility.IsPartOfPrefabAsset(original))
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(original);
        }
        else
        {
            instance = Instantiate(original);
        }
        instance.name = original.name + " (Instance)";
        return instance;
    }

    private Quaternion CalculateRotation(float angle)
    {
        return Quaternion.Euler(0, angle, 0);
    }
    #endregion

    #region 自然排序
    private int NaturalCompare(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            return string.Compare(a, b);

        int i1 = 0, i2 = 0, r = 0;
        while ((i1 < a.Length || i2 < b.Length) && r == 0)
        {
            if (i1 < a.Length && char.IsDigit(a[i1]) && i2 < b.Length && char.IsDigit(b[i2]))
            {
                int j1 = i1, j2 = i2;
                while (j1 < a.Length && char.IsDigit(a[j1])) j1++;
                while (j2 < b.Length && char.IsDigit(b[j2])) j2++;
                string s1 = a.Substring(i1, j1 - i1), s2 = b.Substring(i2, j2 - i2);
                int n1 = int.Parse(s1), n2 = int.Parse(s2);
                r = n1.CompareTo(n2);
                i1 = j1;
                i2 = j2;
            }
            else
            {
                if (i1 < a.Length && i2 < b.Length)
                {
                    r = char.ToLower(a[i1]).CompareTo(char.ToLower(b[i2]));
                    if (r == 0) r = a[i1++].CompareTo(b[i2++]);
                }
                else if (i1 < a.Length) r = 1;
                else if (i2 < b.Length) r = -1;
            }
        }
        return r;
    }
    #endregion

    #region 物体下落方法
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
    #endregion






    static void RoundPositionToQuarter(float QuarterNum)
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RecordObject(obj.transform, "Round Position to Quarter");
            Vector3 currentPosition = obj.transform.position;
            Vector3 roundedPosition = new Vector3(
                RoundToNearestQuarter(currentPosition.x),
                RoundToNearestQuarter(currentPosition.y),
                RoundToNearestQuarter(currentPosition.z)
            );
            obj.transform.position = roundedPosition;
        }
    }

    static float RoundToNearestQuarter(float value)
    {
        return Mathf.Round(value * 4f) / 4f;
    }
}
}