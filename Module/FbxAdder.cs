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
        private bool showFoundAssets = true; // 控制找到的资产列表显示
        private bool showNotFoundAssets = true; // 控制未找到的资产列表显示
        private bool showNameListInput = true; // 控制名称列表输入区域显示
        private bool showGenerationSettings = true; // 控制生成设置区域显示

        // LOD文件包含选项
        private bool includeLod0 = false; // 是否包含_Lod0文件
        private bool includeLod1 = false; // 是否包含_Lod1文件
        private bool includeLod2 = false; // 是否包含_Lod2文件

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

            // LOD文件包含选项 - 横向排列
            EditorGUILayout.BeginHorizontal();
            includeLod0 = EditorGUILayout.Toggle("包含 _Lod0", includeLod0, GUILayout.ExpandWidth(true));
            includeLod1 = EditorGUILayout.Toggle("包含 _Lod1", includeLod1, GUILayout.ExpandWidth(true));
            includeLod2 = EditorGUILayout.Toggle("包含 _Lod2", includeLod2, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            // 资产格式选择 - 使用按钮形式
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("资产格式选择", EditorStyles.boldLabel);

            // 创建格式选择按钮 - 使用智能网格布局确保完美对齐
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


            // 名称列表输入 - 可折叠区域
            showNameListInput = EditorGUILayout.Foldout(showNameListInput,
                "名称列表输入 (换行分隔，不包含文件扩展名)", true, EditorStyles.foldoutHeader);

            if (showNameListInput)
            {
                EditorGUI.indentLevel++;

                nameList = EditorGUILayout.TextArea(nameList, GUILayout.Height(80), GUILayout.ExpandWidth(true));

                // 添加快速操作按钮 - 使用智能网格布局确保完美对齐
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

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // 生成设置 - 可折叠区域
            showGenerationSettings = EditorGUILayout.Foldout(showGenerationSettings,
                "生成设置", true, EditorStyles.foldoutHeader);

            if (showGenerationSettings)
            {
                EditorGUI.indentLevel++;

                spawnOffset = EditorGUILayout.Vector3Field("生成位置偏移:", spawnOffset);
                spacing = EditorGUILayout.FloatField("模型间距:", spacing);

                EditorGUI.indentLevel--;
            }



            EditorGUILayout.Space();

            // 添加分隔线
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            // 操作按钮 - 使用网格布局确保完美对齐
            EditorGUILayout.LabelField("操作", EditorStyles.boldLabel);

            // 添加操作说明
            EditorGUILayout.LabelField("按功能分组排列的操作按钮", EditorStyles.miniLabel);

            // 第一行：查找和添加操作
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("查找资产文件", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                FindAssetFiles();
            }
            if (GUILayout.Button("添加到场景", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                AddAssetsToScene();
            }
            EditorGUILayout.EndHorizontal();

            // 第二行：清理操作
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("清空搜索结果", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                ClearSearchResults();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // 显示查找结果 - 使用可折叠区域
            if (foundAssetPaths.Count > 0 || notFoundNames.Count > 0)
            {
                EditorGUILayout.LabelField("搜索结果", EditorStyles.boldLabel);


                // 找到的资产文件 - 可折叠区域
                if (foundAssetPaths.Count > 0)
                {
                    showFoundAssets = EditorGUILayout.Foldout(showFoundAssets,
                        $"✓ 找到 {foundAssetPaths.Count} 个资产文件", true, EditorStyles.foldoutHeader);

                    if (showFoundAssets)
                    {
                        EditorGUI.indentLevel++;

                        // 添加复制按钮
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("复制所有路径", GUILayout.ExpandWidth(true), GUILayout.Height(20)))
                        {
                            string allPaths = string.Join("\n", foundAssetPaths);
                            EditorGUIUtility.systemCopyBuffer = allPaths;
                            Debug.Log($"已复制 {foundAssetPaths.Count} 个路径到剪贴板");
                            EditorUtility.DisplayDialog("复制成功", $"已复制 {foundAssetPaths.Count} 个路径到剪贴板", "确定");
                        }
                        EditorGUILayout.EndHorizontal();

                        // 显示所有结果 - 使用可选择的文本字段
                        foreach (string path in foundAssetPaths)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("✓", GUILayout.Width(20));

                            // 使用只读的TextField，这样更容易选择和复制
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.TextField(path, GUILayout.Height(18));
                            EditorGUI.EndDisabledGroup();

                            // 添加单个复制按钮
                            if (GUILayout.Button("复制", GUILayout.Width(50), GUILayout.Height(18)))
                            {
                                EditorGUIUtility.systemCopyBuffer = path;
                                Debug.Log($"已复制路径: {path}");
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUI.indentLevel--;
                    }
                }

                // 未找到的文件 - 可折叠区域
                if (notFoundNames.Count > 0)
                {
                    EditorGUILayout.Space();
                    showNotFoundAssets = EditorGUILayout.Foldout(showNotFoundAssets,
                        $"✗ 未找到 {notFoundNames.Count} 个文件", true, EditorStyles.foldoutHeader);

                    if (showNotFoundAssets)
                    {
                        EditorGUI.indentLevel++;

                        // 添加复制按钮
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("复制所有未找到的名称", GUILayout.ExpandWidth(true), GUILayout.Height(20)))
                        {
                            string allNames = string.Join("\n", notFoundNames);
                            EditorGUIUtility.systemCopyBuffer = allNames;
                            Debug.Log($"已复制 {notFoundNames.Count} 个未找到的名称到剪贴板");
                            EditorUtility.DisplayDialog("复制成功", $"已复制 {notFoundNames.Count} 个未找到的名称到剪贴板", "确定");
                        }
                        EditorGUILayout.EndHorizontal();

                        // 显示所有结果 - 使用可选择的文本字段
                        foreach (string name in notFoundNames)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("✗", GUILayout.Width(20));

                            // 使用只读的TextField，这样更容易选择和复制
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.TextField(name, GUILayout.Height(18));
                            EditorGUI.EndDisabledGroup();

                            // 添加单个复制按钮
                            if (GUILayout.Button("复制", GUILayout.Width(50), GUILayout.Height(18)))
                            {
                                EditorGUIUtility.systemCopyBuffer = name;
                                Debug.Log($"已复制名称: {name}");
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUI.indentLevel--;
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }


        private void FindAssetFiles()
        {
            foundAssetPaths.Clear();
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
                string assetPath = FindAssetFile(name);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    foundAssetPaths.Add(assetPath);
                }
                else
                {
                    notFoundNames.Add(name);
                }
            }
        }


        private string FindAssetFile(string fileName)
        {
            // 首先尝试在指定路径范围内搜索
            string result = FindAssetInPath(fileName);
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            // 如果指定路径搜索失败，尝试全局搜索
            return FindAssetGlobally(fileName);
        }

        // 在指定路径范围内搜索
        private string FindAssetInPath(string fileName)
        {
            // 检查搜索路径是否存在
            if (!System.IO.Directory.Exists(searchPath))
            {
                return null;
            }

            // 根据选择的格式构建搜索模式
            string[] extensions = GetExtensionsForFormat(selectedFormat);

            foreach (string extension in extensions)
            {
                string searchPattern = fileName + extension;

                // 首先尝试精确匹配
                string[] files = Directory.GetFiles(searchPath, searchPattern,
                    searchSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                if (files.Length > 0)
                {
                    return files[0].Replace('\\', '/');
                }

                // 如果精确匹配失败，根据LOD选项决定是否进行模糊匹配
                if (includeLod0 || includeLod1 || includeLod2)
                {
                    string[] allFiles = Directory.GetFiles(searchPath, "*" + extension,
                        searchSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                    foreach (string file in allFiles)
                    {
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);

                        // 检查是否包含基础文件名且匹配选中的LOD类型
                        if (fileNameWithoutExt.StartsWith(fileName, System.StringComparison.OrdinalIgnoreCase) &&
                            fileNameWithoutExt.Length > fileName.Length &&
                            fileNameWithoutExt[fileName.Length] == '_')
                        {
                            string suffix = fileNameWithoutExt.Substring(fileName.Length + 1).ToLower();

                            // 检查是否匹配选中的LOD类型（无视大小写）
                            if ((includeLod0 && suffix.StartsWith("lod0")) ||
                                (includeLod1 && suffix.StartsWith("lod1")) ||
                                (includeLod2 && suffix.StartsWith("lod2")))
                            {
                                return file.Replace('\\', '/');
                            }
                        }
                    }
                }
            }

            return null;
        }

        // 全局搜索（使用AssetDatabase）
        private string FindAssetGlobally(string fileName)
        {
            string[] extensions = GetExtensionsForFormat(selectedFormat);

            foreach (string extension in extensions)
            {
                string searchPattern = fileName + extension;

                // 使用AssetDatabase.FindAssets搜索
                string[] guids;
                if (extension == ".prefab")
                {
                    guids = AssetDatabase.FindAssets($"t:Prefab {fileName}");
                }
                else
                {
                    guids = AssetDatabase.FindAssets($"t:Model {fileName}");
                }

                // 首先尝试精确匹配
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.EndsWith(searchPattern, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return path;
                    }
                }

                // 如果精确匹配失败，根据LOD选项决定是否进行模糊匹配
                if (includeLod0 || includeLod1 || includeLod2)
                {
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);

                        // 检查是否包含基础文件名且匹配选中的LOD类型
                        if (fileNameWithoutExt.StartsWith(fileName, System.StringComparison.OrdinalIgnoreCase) &&
                            fileNameWithoutExt.Length > fileName.Length &&
                            fileNameWithoutExt[fileName.Length] == '_')
                        {
                            string suffix = fileNameWithoutExt.Substring(fileName.Length + 1).ToLower();

                            // 检查是否匹配选中的LOD类型（无视大小写）
                            if ((includeLod0 && suffix.StartsWith("lod0")) ||
                                (includeLod1 && suffix.StartsWith("lod1")) ||
                                (includeLod2 && suffix.StartsWith("lod2")))
                            {
                                return path;
                            }
                        }
                    }
                }
            }

            return null;
        }

        // 根据格式获取扩展名数组
        private string[] GetExtensionsForFormat(AssetFormat format)
        {
            switch (format)
            {
                case AssetFormat.FBX:
                    return new string[] { ".fbx" };
                case AssetFormat.Prefab:
                    return new string[] { ".prefab" };
                case AssetFormat.OBJ:
                    return new string[] { ".obj" };
                case AssetFormat.All:
                    return new string[] { ".fbx", ".prefab", ".obj" };
                default:
                    return new string[] { ".fbx" };
            }
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
