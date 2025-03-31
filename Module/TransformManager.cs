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


public class TransformManager : EditorWindow
{
    private Vector2 scrollPosition;
    private const float ScaleFactor = 0.01f;
    private Vector3 copiedPosition;
    private Quaternion copiedRotation;
    private GameObject targetPrefab;  // 要查找的嵌套预制体
    private static List<Vector3> recordedPositions = new List<Vector3>();

    private int numberOfCopies = 5;
    private float distance = 1.0f;
    private bool xAxis = true;
    private bool yAxis = false;
    private bool zAxis = false;


    private float xSpacing = 1f;
    private float zSpacing = 1f;

    private GameObject centerObject;
    private GameObject objectToDuplicate;
    private int count = 8;
    private float radius = 5f;
    private bool autoCalculateRadius = true; // 增加自动计算半径选项
    private bool lookAtCenter = true;


    [MenuItem("美术工具/地编工具/变换操作")]
    public static void ShowWindow()
    {
        GetWindow<TransformManager>("TransformManager");
    }



    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        if (GUILayout.Button("下落物体"))
        {
            DropSelectedObjectsToMesh();
        }

        GUILayout.Space(10); // 添加一些空隙

        if (GUILayout.Button("复制位置旋转变换"))
        {
            CopyTransform();
        }
        if (GUILayout.Button("粘贴位置旋转变换"))
        {
            PasteTransform();
        }
        GUILayout.Space(10); // 添加一些空隙
        if (GUILayout.Button("选取两个物体交换位置旋转"))
        {
            SwapTransforms();
        }
        GUILayout.Space(10); // 添加一些空隙
        if (GUILayout.Button("批量复制底部中心位置（需要同名）"))
        {
            RecordPositions();
        }
        if (GUILayout.Button("批量粘贴底部中心位置（需要同名）"))
        {
            PastePositions();
        }
        GUILayout.Space(10); // 添加一些空隙
        if (GUILayout.Button("尺寸缩小100比例"))
        {
            ResizeSelectedObject();
        }

        GUILayout.Space(10); // 添加一些空隙

        GUILayout.Label("Array Duplicate Options", EditorStyles.boldLabel);

        numberOfCopies = EditorGUILayout.IntField("Number of Copies", numberOfCopies);
        distance = EditorGUILayout.FloatField("Distance", distance);

        GUILayout.Label("Axis");
        EditorGUILayout.BeginHorizontal();
        xAxis = EditorGUILayout.Toggle("X", xAxis);
        yAxis = EditorGUILayout.Toggle("Y", yAxis);
        zAxis = EditorGUILayout.Toggle("Z", zAxis);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Duplicate Selected"))
        {
            ArrayDuplicate();
        }

        EditorGUILayout.LabelField("矩形阵列所选物体", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("X Spacing", GUILayout.Width(100));  // 控制标签的宽度
        xSpacing = EditorGUILayout.FloatField(xSpacing);  // 自动调整宽度
        EditorGUILayout.LabelField("Z Spacing", GUILayout.Width(100));  // 控制标签的宽度
        zSpacing = EditorGUILayout.FloatField(zSpacing);  // 自动调整宽度
        if (GUILayout.Button("阵列所选物体"))
        {
            ArrangeSelectedObjects();
        }
        EditorGUILayout.EndHorizontal();


        GUILayout.Space(10); // 添加一些空隙

        //圆周阵列
        EditorGUILayout.LabelField("圆周阵列", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("中心参照物", GUILayout.Width(100));  // 控制标签的宽度
        centerObject = (GameObject)EditorGUILayout.ObjectField(centerObject, typeof(GameObject), true);  // 控制对象字段的宽度
        EditorGUILayout.LabelField("阵列物体", GUILayout.Width(100));  // 控制标签的宽度
        objectToDuplicate = (GameObject)EditorGUILayout.ObjectField(objectToDuplicate, typeof(GameObject), true);  // 控制对象字段的宽度
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("计数", GUILayout.Width(100));  // 控制标签的宽度
        count = EditorGUILayout.IntField(count, GUILayout.Width(100));  // 控制输入字段的宽度
        EditorGUILayout.LabelField("自动半径", GUILayout.Width(100));
        autoCalculateRadius = EditorGUILayout.Toggle(autoCalculateRadius, GUILayout.Width(16));  // 控制Toggle的宽度
        if (!autoCalculateRadius)
        {
            EditorGUILayout.LabelField("直径", GUILayout.Width(50));  // 控制标签的宽度
            radius = EditorGUILayout.FloatField(radius, GUILayout.Width(50));  // 控制输入字段的宽度
        }
        EditorGUILayout.LabelField("朝向中心", GUILayout.Width(100));
        lookAtCenter = EditorGUILayout.Toggle(lookAtCenter, GUILayout.Width(16));  // 控制Toggle的宽度
        if (GUILayout.Button("计算阵列"))
        {
            if (centerObject != null && objectToDuplicate != null)
            {
                if (autoCalculateRadius)
                {
                    // 自动计算半径
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












        GUILayout.EndScrollView();
    }

    // private string ()
    // {}

    private void CopyTransform()
    {
        if (Selection.activeTransform != null)
        {
            copiedPosition = Selection.activeTransform.position; // 正确的。位置赋值给 Vector3
            copiedRotation = Selection.activeTransform.rotation; // 正确的。旋转赋值给 Quaternion
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

            Selection.activeTransform.position = copiedPosition; // 正确的。Vector3 赋值给位置
            Selection.activeTransform.rotation = copiedRotation; // 正确的。Quaternion 赋值给旋转
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

        // Store the transform data
        Vector3 position1 = obj1.transform.position;
        Vector3 rotation1 = obj1.transform.eulerAngles; // Use eulerAngles for simplicity

        // Swap positions
        obj1.transform.position = obj2.transform.position;
        obj2.transform.position = position1;

        // Swap rotations
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
        // 获取当前选中的游戏对象
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

    void ArrangeSelectedObjects()
    {
        // 获取当前选中的所有游戏对象
        GameObject[] selectedObjects = Selection.gameObjects;

        // 如果没有选中任何对象，显示一个错误对话框并退出方法
        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No objects selected!", "OK");
            return;
        }

        // 根据名称进行排序，按照自然数顺序
        Array.Sort(selectedObjects, (x, y) => NaturalCompare(x.name, y.name));

        // 根据选中的对象数量计算网格的大小
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(selectedObjects.Length));

        // 遍历所有选中的对象并在网格中排列它们
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            int row = i / gridSize; // 计算当前对象应该在第几行
            int col = i % gridSize; // 计算当前对象应该在第几列
            Vector3 newPosition = new Vector3(col * xSpacing, selectedObjects[i].transform.position.y, row * zSpacing); // 设定新位置
            selectedObjects[i].transform.position = newPosition; // 应用新位置
        }
    }

    private void GenerateCircleArray()
    {
        // 先计算出每个物体的位置和旋转
        Vector3[] positions = new Vector3[count];
        Quaternion[] rotations = new Quaternion[count];

        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            positions[i] = CalculatePosition(angle);
            rotations[i] = CalculateRotation(angle);
        }

        // 然后进行物体实例化并设置其位置和旋转
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

    public class NaturalComparer : IComparer<string>
    {
        private static readonly Regex _regex = new Regex(@"(\d+)|(\D+)", RegexOptions.Compiled);

        public int Compare(string x, string y)
        {
            // If both strings are identical or either is null, no need to compare further.
            if (string.Equals(x, y, StringComparison.OrdinalIgnoreCase))
                return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // Compare each numerical or textual chunk one by one from both strings.
            var xMatches = _regex.Matches(x);
            var yMatches = _regex.Matches(y);
            for (int i = 0; i < Math.Min(xMatches.Count, yMatches.Count); i++)
            {
                var xPart = xMatches[i].Value;
                var yPart = yMatches[i].Value;

                // If both are numeric, compare numerically.
                if (int.TryParse(xPart, out int xNum) && int.TryParse(yPart, out int yNum))
                {
                    int numCompare = xNum.CompareTo(yNum);
                    if (numCompare != 0)
                        return numCompare;
                }
                else // If any or both are non-numeric, compare as text.
                {
                    int stringCompare = string.Compare(xPart, yPart, StringComparison.OrdinalIgnoreCase);
                    if (stringCompare != 0)
                        return stringCompare;
                }
            }

            // If all parts matched but one string has additional chunks, the shorter string goes first.
            return xMatches.Count.CompareTo(yMatches.Count);
        }
    }
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
}