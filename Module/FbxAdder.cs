using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class FbxAdder : MonoBehaviour
{
    [Header("资产查找器设置")]
    [SerializeField] private string searchPath = "Assets/"; // 默认搜索路径
    [SerializeField] private string nameList = ""; // 名称列表，换行分隔
    [SerializeField] private bool searchSubfolders = true; // 是否搜索子文件夹
    [SerializeField] private Vector3 spawnOffset = Vector3.zero; // 生成位置偏移
    [SerializeField] private float spacing = 2f; // 模型间距

    private List<string> foundAssetPaths = new List<string>();
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
        private List<string> foundAssetPaths = new List<string>();
        private List<string> notFoundNames = new List<string>();

        // 资产格式选择
        public enum AssetFormat
        {
            FBX,
            Prefab,
            OBJ,
            All
        }
        private AssetFormat selectedFormat = AssetFormat.FBX;
        private bool showFullResults = false; // 新增：控制是否显示完整结果

        [MenuItem("美术工具/批量添加工具/FbxAdder")]
        public static void ShowWindow()
        {
            GetWindow<FbxAdderEditor>("资产查找器");
        }

        private void OnGUI()
        {
            // 设置最小窗口大小，确保UI元素有足够空间
            minSize = new Vector2(400, 600);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("资产查找器", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 搜索路径设置
            EditorGUILayout.LabelField("搜索设置", EditorStyles.boldLabel);
            searchPath = EditorGUILayout.TextField("搜索路径:", searchPath);
            searchSubfolders = EditorGUILayout.Toggle("搜索子文件夹", searchSubfolders);

            // 资产格式选择 - 使用按钮形式
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("资产格式选择", EditorStyles.boldLabel);

            // 创建格式选择按钮 - 使用均分宽度
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = (selectedFormat == AssetFormat.FBX) ? Color.green : Color.white;
            if (GUILayout.Button("FBX格式", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                selectedFormat = AssetFormat.FBX;
                Debug.Log("选择FBX格式");
            }
            GUI.backgroundColor = (selectedFormat == AssetFormat.Prefab) ? Color.green : Color.white;
            if (GUILayout.Button("Prefab格式", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                selectedFormat = AssetFormat.Prefab;
                Debug.Log("选择Prefab格式");
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = (selectedFormat == AssetFormat.OBJ) ? Color.green : Color.white;
            if (GUILayout.Button("OBJ格式", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                selectedFormat = AssetFormat.OBJ;
                Debug.Log("选择OBJ格式");
            }
            GUI.backgroundColor = (selectedFormat == AssetFormat.All) ? Color.green : Color.white;
            if (GUILayout.Button("所有格式", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                selectedFormat = AssetFormat.All;
                Debug.Log("选择所有格式");
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            // 显示当前选择的格式
            EditorGUILayout.LabelField($"当前选择: {selectedFormat}", EditorStyles.boldLabel);

            // 添加格式说明
            string formatDescription = GetFormatDescription(selectedFormat);
            EditorGUILayout.LabelField(formatDescription, EditorStyles.helpBox);

            EditorGUILayout.Space();

            // 名称列表输入
            EditorGUILayout.LabelField("名称列表 (换行分隔，不包含文件扩展名):", EditorStyles.boldLabel);
            nameList = EditorGUILayout.TextArea(nameList, GUILayout.Height(80), GUILayout.ExpandWidth(true)); // 使用ExpandWidth确保宽度一致

            // 添加快速操作按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("清空输入", GUILayout.ExpandWidth(true), GUILayout.Height(20)))
            {
                nameList = "";
                GUI.FocusControl(null); // 取消焦点
            }
            if (GUILayout.Button("示例格式", GUILayout.ExpandWidth(true), GUILayout.Height(20)))
            {
                nameList = "模型1\n模型2\n建筑A\n树木B";
                GUI.FocusControl(null); // 取消焦点
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 生成设置
            EditorGUILayout.LabelField("生成设置", EditorStyles.boldLabel);
            spawnOffset = EditorGUILayout.Vector3Field("生成位置偏移:", spawnOffset);
            spacing = EditorGUILayout.FloatField("模型间距:", spacing);

            EditorGUILayout.Space();

            // 操作按钮 - 使用均分宽度，与窗口对齐
            EditorGUILayout.LabelField("操作", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("查找资产文件", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                FindAssetFiles();
            }
            if (GUILayout.Button("搜索所有格式", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                FindMultipleFormats();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("添加到场景", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                AddAssetsToScene();
            }
            if (GUILayout.Button("清空搜索结果", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                ClearSearchResults();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("清空所有", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                ClearLists();
            }

            EditorGUILayout.Space();

            // 显示查找结果 - 使用可折叠区域
            if (foundAssetPaths.Count > 0 || notFoundNames.Count > 0)
            {
                EditorGUILayout.LabelField("搜索结果", EditorStyles.boldLabel);

                // 添加展开/折叠按钮
                if (foundAssetPaths.Count > 10 || notFoundNames.Count > 10)
                {
                    showFullResults = EditorGUILayout.Toggle("显示完整结果", showFullResults);
                    EditorGUILayout.Space();
                }

                if (foundAssetPaths.Count > 0)
                {
                    EditorGUILayout.LabelField($"找到 {foundAssetPaths.Count} 个资产文件:", EditorStyles.boldLabel);

                    if (showFullResults)
                    {
                        // 显示所有结果
                        foreach (string path in foundAssetPaths)
                        {
                            EditorGUILayout.LabelField($"✓ {path}");
                        }
                    }
                    else
                    {
                        // 限制显示的文件数量，避免UI过长
                        int maxDisplay = Mathf.Min(foundAssetPaths.Count, 10);
                        for (int i = 0; i < maxDisplay; i++)
                        {
                            EditorGUILayout.LabelField($"✓ {foundAssetPaths[i]}");
                        }
                        if (foundAssetPaths.Count > 10)
                        {
                            EditorGUILayout.LabelField($"... 还有 {foundAssetPaths.Count - 10} 个文件", EditorStyles.miniLabel);
                        }
                    }
                }

                if (notFoundNames.Count > 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField($"未找到 {notFoundNames.Count} 个文件:", EditorStyles.boldLabel);

                    if (showFullResults)
                    {
                        // 显示所有结果
                        foreach (string name in notFoundNames)
                        {
                            EditorGUILayout.LabelField($"✗ {name}");
                        }
                    }
                    else
                    {
                        // 限制显示的名称数量
                        int maxDisplay = Mathf.Min(notFoundNames.Count, 10);
                        for (int i = 0; i < maxDisplay; i++)
                        {
                            EditorGUILayout.LabelField($"✗ {notFoundNames[i]}");
                        }
                        if (notFoundNames.Count > 10)
                        {
                            EditorGUILayout.LabelField($"... 还有 {notFoundNames.Count - 10} 个名称", EditorStyles.miniLabel);
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        // 获取格式说明
        private string GetFormatDescription(AssetFormat format)
        {
            switch (format)
            {
                case AssetFormat.FBX:
                    return "搜索 .fbx 3D模型文件";
                case AssetFormat.Prefab:
                    return "搜索 .prefab Unity预制体文件";
                case AssetFormat.OBJ:
                    return "搜索 .obj 3D模型文件";
                case AssetFormat.All:
                    return "搜索所有支持的格式 (.fbx, .prefab, .obj)";
                default:
                    return "未知格式";
            }
        }

        private void FindAssetFiles()
        {
            foundAssetPaths.Clear();
            notFoundNames.Clear();

            Debug.Log($"开始查找资产文件，选择的格式: {selectedFormat}");

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

            Debug.Log($"要查找的名称数量: {names.Length}");

            foreach (string name in names)
            {
                Debug.Log($"正在查找: {name}");
                string assetPath = FindAssetFile(name);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    foundAssetPaths.Add(assetPath);
                    Debug.Log($"找到: {assetPath}");
                }
                else
                {
                    notFoundNames.Add(name);
                    Debug.Log($"未找到: {name}");
                }
            }

            Debug.Log($"查找完成: 找到 {foundAssetPaths.Count} 个文件，未找到 {notFoundNames.Count} 个文件");
        }

        // 新增：搜索多种格式并合并结果
        private void FindMultipleFormats()
        {
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

            Debug.Log($"开始多格式搜索，名称数量: {names.Length}");

            // 保存当前格式
            AssetFormat originalFormat = selectedFormat;

            // 搜索所有格式
            foreach (AssetFormat format in System.Enum.GetValues(typeof(AssetFormat)))
            {
                if (format == AssetFormat.All) continue; // 跳过All选项

                selectedFormat = format;
                Debug.Log($"搜索格式: {format}");

                foreach (string name in names)
                {
                    string assetPath = FindAssetFile(name);
                    if (!string.IsNullOrEmpty(assetPath) && !foundAssetPaths.Contains(assetPath))
                    {
                        foundAssetPaths.Add(assetPath);
                        Debug.Log($"找到 ({format}): {assetPath}");
                    }
                }
            }

            // 恢复原始格式
            selectedFormat = originalFormat;

            // 计算未找到的名称
            notFoundNames.Clear();
            foreach (string name in names)
            {
                bool found = false;
                foreach (string path in foundAssetPaths)
                {
                    if (Path.GetFileNameWithoutExtension(path).Equals(name, System.StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    notFoundNames.Add(name);
                }
            }

            Debug.Log($"多格式搜索完成: 找到 {foundAssetPaths.Count} 个文件，未找到 {notFoundNames.Count} 个文件");
        }

        private string FindAssetFile(string fileName)
        {
            string[] guids;
            string searchPattern = "";

            Debug.Log($"查找文件: {fileName}，格式: {selectedFormat}");

            // 根据选择的格式构建搜索模式
            switch (selectedFormat)
            {
                case AssetFormat.FBX:
                    searchPattern = fileName + ".fbx";
                    guids = AssetDatabase.FindAssets($"t:Model {fileName}");
                    Debug.Log($"FBX模式，搜索模式: {searchPattern}，找到GUID数量: {guids.Length}");
                    break;
                case AssetFormat.Prefab:
                    searchPattern = fileName + ".prefab";
                    guids = AssetDatabase.FindAssets($"t:Prefab {fileName}");
                    Debug.Log($"Prefab模式，搜索模式: {searchPattern}，找到GUID数量: {guids.Length}");
                    break;
                case AssetFormat.OBJ:
                    searchPattern = fileName + ".obj";
                    guids = AssetDatabase.FindAssets($"t:Model {fileName}");
                    Debug.Log($"OBJ模式，搜索模式: {searchPattern}，找到GUID数量: {guids.Length}");
                    break;
                case AssetFormat.All:
                    // 搜索所有支持的格式
                    Debug.Log("使用All模式搜索所有格式");
                    return FindAssetFileInAllFormats(fileName);
                default:
                    searchPattern = fileName + ".fbx";
                    guids = AssetDatabase.FindAssets($"t:Model {fileName}");
                    Debug.Log($"默认模式，搜索模式: {searchPattern}，找到GUID数量: {guids.Length}");
                    break;
            }

            if (searchSubfolders)
            {
                // 通过GUID查找
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.EndsWith(searchPattern, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return path;
                    }
                }
            }
            else
            {
                // 在指定路径下搜索
                string[] files = Directory.GetFiles(searchPath, searchPattern, SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    return files[0].Replace('\\', '/');
                }
            }

            return null;
        }

        private string FindAssetFileInAllFormats(string fileName)
        {
            // 搜索所有支持的格式
            string[] extensions = { ".fbx", ".prefab", ".obj" };

            foreach (string extension in extensions)
            {
                string searchPattern = fileName + extension;
                string[] guids;

                if (extension == ".prefab")
                {
                    guids = AssetDatabase.FindAssets($"t:Prefab {fileName}");
                }
                else
                {
                    guids = AssetDatabase.FindAssets($"t:Model {fileName}");
                }

                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.EndsWith(searchPattern, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return path;
                    }
                }

                // 如果不搜索子文件夹，也在指定路径下搜索
                if (!searchSubfolders)
                {
                    string[] files = Directory.GetFiles(searchPath, searchPattern, SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        return files[0].Replace('\\', '/');
                    }
                }
            }

            return null;
        }

        private void AddAssetsToScene()
        {
            if (foundAssetPaths.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "没有找到资产文件，请先执行查找", "确定");
                return;
            }

            Vector3 currentPosition = spawnOffset;
            int successCount = 0;
            int failCount = 0;

            Debug.Log($"开始添加 {foundAssetPaths.Count} 个资产到场景...");

            foreach (string assetPath in foundAssetPaths)
            {
                try
                {
                    GameObject assetObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (assetObject != null)
                    {
                        GameObject instance = PrefabUtility.InstantiatePrefab(assetObject) as GameObject;
                        if (instance != null)
                        {
                            instance.transform.position = currentPosition;
                            instance.name = Path.GetFileNameWithoutExtension(assetPath);
                            currentPosition += Vector3.right * spacing;

                            // 选中新创建的对象
                            Selection.activeGameObject = instance;
                            successCount++;

                            Debug.Log($"成功添加: {assetPath}");
                        }
                        else
                        {
                            Debug.LogError($"实例化失败: {assetPath}");
                            failCount++;
                        }
                    }
                    else
                    {
                        Debug.LogError($"无法加载资产文件: {assetPath}");
                        failCount++;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"添加资产时发生错误 {assetPath}: {e.Message}");
                    failCount++;
                }
            }

            // 显示结果
            if (failCount == 0)
            {
                EditorUtility.DisplayDialog("成功", $"成功添加 {successCount} 个资产到场景", "确定");
                Debug.Log($"成功添加 {successCount} 个资产文件到场景");
            }
            else
            {
                EditorUtility.DisplayDialog("部分成功", $"成功添加 {successCount} 个资产，失败 {failCount} 个", "确定");
                Debug.Log($"添加完成: 成功 {successCount} 个，失败 {failCount} 个");
            }
        }

        private void ClearLists()
        {
            foundAssetPaths.Clear();
            notFoundNames.Clear();
            nameList = "";
            showFullResults = false; // 重置显示完整结果选项
        }

        private void ClearSearchResults()
        {
            foundAssetPaths.Clear();
            notFoundNames.Clear();
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
