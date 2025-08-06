using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class FbxAdder : MonoBehaviour
{
    [Header("FBX查找器设置")]
    [SerializeField] private string searchPath = "Assets/"; // 默认搜索路径
    [SerializeField] private string nameList = ""; // 名称列表，换行分隔
    [SerializeField] private bool searchSubfolders = true; // 是否搜索子文件夹
    [SerializeField] private Vector3 spawnOffset = Vector3.zero; // 生成位置偏移
    [SerializeField] private float spacing = 2f; // 模型间距

    private List<string> foundFbxPaths = new List<string>();
    private List<string> notFoundNames = new List<string>();

    // 在编辑器中显示GUI
    [System.Serializable]
    public class FbxAdderEditor : EditorWindow
    {
        private string searchPath = "Assets/ArtResources/";
        private string nameList = "";
        private bool searchSubfolders = true;
        private Vector3 spawnOffset = Vector3.zero;
        private float spacing = 2f;
        private Vector2 scrollPosition;
        private List<string> foundFbxPaths = new List<string>();
        private List<string> notFoundNames = new List<string>();

        [MenuItem("美术工具/批量添加工具/FbxAdder")]
        public static void ShowWindow()
        {
            GetWindow<FbxAdderEditor>("FBX查找器");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("FBX查找器", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 搜索路径设置
            EditorGUILayout.LabelField("搜索设置", EditorStyles.boldLabel);
            searchPath = EditorGUILayout.TextField("搜索路径:", searchPath);
            searchSubfolders = EditorGUILayout.Toggle("搜索子文件夹", searchSubfolders);

            EditorGUILayout.Space();

            // 名称列表输入
            EditorGUILayout.LabelField("名称列表 (换行分隔，不包含.fbx后缀):", EditorStyles.boldLabel);
            nameList = EditorGUILayout.TextArea(nameList, GUILayout.Height(100));

            EditorGUILayout.Space();

            // 生成设置
            EditorGUILayout.LabelField("生成设置", EditorStyles.boldLabel);
            spawnOffset = EditorGUILayout.Vector3Field("生成位置偏移:", spawnOffset);
            spacing = EditorGUILayout.FloatField("模型间距:", spacing);

            EditorGUILayout.Space();

            // 操作按钮
            if (GUILayout.Button("查找FBX文件", GUILayout.Height(30)))
            {
                FindFbxFiles();
            }

            if (GUILayout.Button("添加到场景", GUILayout.Height(30)))
            {
                AddFbxToScene();
            }

            if (GUILayout.Button("清空列表", GUILayout.Height(30)))
            {
                ClearLists();
            }

            EditorGUILayout.Space();

            // 显示查找结果
            if (foundFbxPaths.Count > 0)
            {
                EditorGUILayout.LabelField($"找到 {foundFbxPaths.Count} 个FBX文件:", EditorStyles.boldLabel);
                foreach (string path in foundFbxPaths)
                {
                    EditorGUILayout.LabelField($"✓ {path}");
                }
            }

            if (notFoundNames.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"未找到 {notFoundNames.Count} 个文件:", EditorStyles.boldLabel);
                foreach (string name in notFoundNames)
                {
                    EditorGUILayout.LabelField($"✗ {name}");
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void FindFbxFiles()
        {
            foundFbxPaths.Clear();
            notFoundNames.Clear();

            if (string.IsNullOrEmpty(nameList))
            {
                EditorUtility.DisplayDialog("错误", "请输入名称列表", "确定");
                return;
            }

            string[] names = nameList.Split('\n', '\r')
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .ToArray();

            if (names.Length == 0)
            {
                EditorUtility.DisplayDialog("错误", "没有找到有效的名称", "确定");
                return;
            }

            foreach (string name in names)
            {
                string fbxPath = FindFbxFile(name);
                if (!string.IsNullOrEmpty(fbxPath))
                {
                    foundFbxPaths.Add(fbxPath);
                }
                else
                {
                    notFoundNames.Add(name);
                }
            }

            Debug.Log($"查找完成: 找到 {foundFbxPaths.Count} 个文件，未找到 {notFoundNames.Count} 个文件");
        }

        private string FindFbxFile(string fileName)
        {
            // 构建搜索模式
            string searchPattern = fileName + ".fbx";
            string[] guids;

            if (searchSubfolders)
            {
                guids = AssetDatabase.FindAssets($"t:Model {fileName}");
            }
            else
            {
                // 在指定路径下搜索
                string[] files = Directory.GetFiles(searchPath, searchPattern, SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    return files[0].Replace('\\', '/');
                }
                return null;
            }

            // 通过GUID查找
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(searchPattern, System.StringComparison.OrdinalIgnoreCase))
                {
                    return path;
                }
            }

            return null;
        }

        private void AddFbxToScene()
        {
            if (foundFbxPaths.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "没有找到FBX文件，请先执行查找", "确定");
                return;
            }

            Vector3 currentPosition = spawnOffset;

            foreach (string fbxPath in foundFbxPaths)
            {
                GameObject fbxObject = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                if (fbxObject != null)
                {
                    GameObject instance = PrefabUtility.InstantiatePrefab(fbxObject) as GameObject;
                    if (instance != null)
                    {
                        instance.transform.position = currentPosition;
                        instance.name = Path.GetFileNameWithoutExtension(fbxPath);
                        currentPosition += Vector3.right * spacing;

                        // 选中新创建的对象
                        Selection.activeGameObject = instance;
                    }
                }
                else
                {
                    Debug.LogError($"无法加载FBX文件: {fbxPath}");
                }
            }

            Debug.Log($"成功添加 {foundFbxPaths.Count} 个FBX文件到场景");
        }

        private void ClearLists()
        {
            foundFbxPaths.Clear();
            notFoundNames.Clear();
            nameList = "";
        }
    }

    // 运行时方法（如果需要的话）
    void Start()
    {
        // 运行时初始化
    }

    void Update()
    {
        // 运行时更新
    }
}
