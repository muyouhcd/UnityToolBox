using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using DYM.ToolBox;


namespace DYM.ToolBox
{
    public class ToolboxManager : EditorWindow
    {
        private enum TabType
        {
            Transform,
            Materials,
            FBX,
            Assets,
            Navigation,
            Cleanup,
            Misc
        }



        //检查工具变量

        





        private TabType selectedTab = TabType.Transform;
        private Vector2 scrollPosition;
        private GUIStyle headerStyle;
        private GUIStyle tabStyle;
        private GUIStyle selectedTabStyle;
        private Color defaultBackgroundColor;

        // 变换工具变量
        private float offsetX = 0f;
        private float offsetY = 0f;
        private float offsetZ = 0f;
        private float roundPrecision = 0.25f;

        // 材质工具变量
        private Material sourceMat;
        private Material targetMat;
        private string materialName = "NewMaterial";
        private string savePath = "";
        private string shaderName = "";

        // FBX工具变量
        private string checkDirectory = "";
        private string sourceFbxDirectory = "";
        private string outputPrefabDirectory = "";
        private bool preserveMaterials = true;

        // 资产工具变量
        private string prefix = "";
        private string suffix = "";
        private string renamePattern = "";
        private int renameStartIndex = 1;
        private string searchString = "";
        private string replacementString = "";
        private string searchPath = "";
        private bool caseSensitive = false;
        private int removeCharCount = 0;
        private string assetPath = "";
        private string baseName = "Asset";

        // 导航工具变量
        private float agentRadius = 0.5f;
        private float agentHeight = 2f;
        private float maxSlope = 45f;
        private float stepHeight = 0.4f;

        // 清理工具变量
        private string cleanupFolderPath = "";
        private bool cleanUnusedAssets = true;
        private bool cleanEmptyDirectories = true;
        private string scriptCheckDirectory = "";
        private bool includeMetaFiles = true;
        private bool includeSubfolders = true;
        private bool deleteEmptyFolders = true;

        // 其他工具变量
        private GameObject trackPrefab;
        private float trackLength = 10f;
        private float trackSpacing = 1f;
        private UnityEngine.Object timelineAsset;

        // Misc工具变量
        private string findFileName = "";
        private string findPath = "";
        private bool findInScenes = true;
        private bool findInPrefabs = true;
        private bool findInAssets = true;
        private bool exactMatch = true;
        private string screenshotPath = "";
        private float screenshotScale = 1f;
        private bool selectByTag = false;
        private string selectedTag = "";
        private bool selectByLayer = false;
        private int selectedLayer = 0;
        private bool selectByName = false;
        private string nameContains = "";
        private bool selectByComponent = false;
        private string componentType = "";

        // 导航工具变量
        private string bookmarkName = "";
        private List<string> bookmarks = new List<string>();

        [MenuItem("DYM/工具箱 %#t")]  // Ctrl+Alt+T 快捷键

        public static void ShowWindow()
        {
            GetWindow<ToolboxManager>("DYM工具箱");
        }

        private void OnEnable()
        {
            defaultBackgroundColor = GUI.backgroundColor;
        }

        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    margin = new RectOffset(4, 4, 8, 8)
                };
            }

            if (tabStyle == null)
            {
                tabStyle = new GUIStyle(EditorStyles.toolbarButton)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Normal,
                    fixedHeight = 25
                };
            }

            if (selectedTabStyle == null)
            {
                selectedTabStyle = new GUIStyle(tabStyle)
                {
                    fontStyle = FontStyle.Bold,
                    normal = { background = EditorGUIUtility.whiteTexture }
                };
                selectedTabStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            }
        }

        private void OnGUI()
        {
            InitializeStyles();

            EditorGUILayout.BeginVertical();

            // 绘制标签页
            DrawTabs();

            EditorGUILayout.Space(5);

            // 绘制分割线
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // 内容区域开始滚动视图
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 根据选择的标签页显示不同的内容
            switch (selectedTab)
            {
                case TabType.Transform:
                    DrawTransformTab();
                    break;
                case TabType.Materials:
                    DrawMaterialsTab();
                    break;
                case TabType.FBX:
                    DrawFBXTab();
                    break;
                case TabType.Assets:
                    DrawAssetsTab();
                    break;
                case TabType.Navigation:
                    DrawNavigationTab();
                    break;
                case TabType.Cleanup:
                    DrawCleanupTab();
                    break;
                case TabType.Misc:
                    DrawMiscTab();
                    break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal();
            
            foreach (TabType tabType in Enum.GetValues(typeof(TabType)))
            {
                GUIStyle style = selectedTab == tabType ? selectedTabStyle : tabStyle;
                string tabName = GetTabName(tabType);
                
                if (GUILayout.Button(tabName, style))
                {
                    selectedTab = tabType;
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private string GetTabName(TabType tabType)
        {
            switch (tabType)
            {
                case TabType.Transform: return "变换工具";
                case TabType.Materials: return "材质工具";
                case TabType.FBX: return "FBX工具";
                case TabType.Assets: return "资源工具";
                case TabType.Navigation: return "导航工具";
                case TabType.Cleanup: return "清理工具";
                case TabType.Misc: return "其他工具";
                default: return tabType.ToString();
            }
        }

        private void DrawTransformTab()
        {
            EditorGUILayout.LabelField("变换工具", headerStyle);
            
            if (GUILayout.Button("变换管理器", GUILayout.Height(30)))
            {
                // 使用完全限定类型名称
                EditorWindow.GetWindow(Type.GetType("DYM.ToolBox.TransformManager, Assembly-CSharp-Editor"), false, "变换管理器");
            }

            // 变换管理器UI
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("变换操作工具", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("此工具可以帮助您对场景中的对象进行批量变换操作。", MessageType.Info);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("批量坐标调整", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("X偏移量:", GUILayout.Width(70));
            offsetX = EditorGUILayout.FloatField(offsetX);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Y偏移量:", GUILayout.Width(70));
            offsetY = EditorGUILayout.FloatField(offsetY);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Z偏移量:", GUILayout.Width(70));
            offsetZ = EditorGUILayout.FloatField(offsetZ);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("应用偏移到选中对象"))
            {
                Debug.Log($"应用偏移: X={offsetX}, Y={offsetY}, Z={offsetZ}");
            }

            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("重置选中对象旋转"))
            {
                Debug.Log("重置选中对象旋转");
            }
            
            if (GUILayout.Button("重置选中对象缩放"))
            {
                Debug.Log("重置选中对象缩放");
            }

            if (GUILayout.Button("将位置四舍五入到0.25", GUILayout.Height(30)))
            {
                // 使用完全限定类型名称
                EditorWindow.GetWindow(Type.GetType("DYM.ToolBox.RoundPositionToQuarter, Assembly-CSharp-Editor"), false, "位置四舍五入");
            }

            // 位置四舍五入UI
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("位置四舍五入工具", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("此工具可以将选中对象的位置值四舍五入到指定精度。", MessageType.Info);

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("四舍五入精度:", GUILayout.Width(100));
            roundPrecision = EditorGUILayout.FloatField(roundPrecision);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("应用四舍五入到选中对象"))
            {
                Debug.Log($"应用四舍五入，精度为: {roundPrecision}");
            }

            EditorGUILayout.Space(10);
        }

        private void DrawMaterialsTab()
        {
            EditorGUILayout.LabelField("材质工具", headerStyle);

            EditorGUILayout.Space();
            
            // 材质工具面板
            EditorGUILayout.BeginVertical("Box");
            
            // 标题
            GUILayout.Label("材质批量修改工具", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            // 选择材质
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("源材质:", GUILayout.Width(60));
            sourceMat = (Material)EditorGUILayout.ObjectField(sourceMat, typeof(Material), false);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("目标材质:", GUILayout.Width(60));
            targetMat = (Material)EditorGUILayout.ObjectField(targetMat, typeof(Material), false);
            EditorGUILayout.EndHorizontal();
            
            // 操作按钮
            if (GUILayout.Button("替换选中对象的材质"))
            {
                ReplaceSelectedObjectsMaterial();
            }
            
            if (GUILayout.Button("替换所有使用源材质的对象"))
            {
                ReplaceAllMaterials();
            }
            
            EditorGUILayout.Space();
            
            // 批量创建材质
            GUILayout.Label("批量创建材质", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("材质名称:", GUILayout.Width(60));
            materialName = EditorGUILayout.TextField(materialName);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("保存路径:", GUILayout.Width(60));
            savePath = EditorGUILayout.TextField(savePath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择保存路径", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        path = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    savePath = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("创建材质"))
            {
                CreateMaterial();
            }
            
            EditorGUILayout.Space();
            
            // 批量修改Shader
            GUILayout.Label("批量修改Shader", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Shader名称:", GUILayout.Width(80));
            shaderName = EditorGUILayout.TextField(shaderName);
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("修改选中材质的Shader"))
            {
                ChangeSelectedMaterialsShader();
            }
            
            EditorGUILayout.EndVertical();
            
            // 工具按钮
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("材质替换", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.MaterialChanger>("材质替换");
            }
            
            if (GUILayout.Button("材质修改器", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.MaterialModifier>("材质修改器");
            }
            
            if (GUILayout.Button("白色替换", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.WhiteReplacer>("白色替换");
            }
        }
        
        private void ReplaceSelectedObjectsMaterial()
        {
            if (sourceMat == null || targetMat == null)
            {
                Debug.LogError("请选择源材质和目标材质");
                return;
            }
            
            Debug.Log($"替换选中对象的材质: {sourceMat.name} -> {targetMat.name}");
        }
        
        private void ReplaceAllMaterials()
        {
            if (sourceMat == null || targetMat == null)
            {
                Debug.LogError("请选择源材质和目标材质");
                return;
            }
            
            Debug.Log($"替换所有使用源材质的对象: {sourceMat.name} -> {targetMat.name}");
        }
        
        private void CreateMaterial()
        {
            if (string.IsNullOrEmpty(materialName))
            {
                Debug.LogError("请输入材质名称");
                return;
            }
            
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = "Assets";
            }
            
            Debug.Log($"创建材质: {materialName}, 保存路径: {savePath}");
        }
        
        private void ChangeSelectedMaterialsShader()
        {
            if (string.IsNullOrEmpty(shaderName))
            {
                Debug.LogError("请输入Shader名称");
                return;
            }
            
            Debug.Log($"修改选中材质的Shader: {shaderName}");
        }

        private void DrawFBXTab()
        {
            EditorGUILayout.LabelField("FBX工具", headerStyle);

            // 从FBXChecker.cs中的OnGUI方法复制过来的
            GUILayout.Label("FBX检查工具", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("检查目录", GUILayout.Width(80));
            checkDirectory = EditorGUILayout.TextField(checkDirectory);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    checkDirectory = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("检查FBX"))
            {
                Debug.Log("检查FBX: " + checkDirectory);
            }
            
            EditorGUILayout.Space();
            
            // 显示检查结果（模拟）
            GUILayout.Label("检查结果", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("导出结果路径", GUILayout.Width(80));
            fbxExportPath = EditorGUILayout.TextField(fbxExportPath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.SaveFilePanel("保存CSV文件", Application.dataPath, "FBXCheckResult.csv", "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    fbxExportPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("导出到CSV"))
            {
                Debug.Log("导出到CSV: " + fbxExportPath);
            }
            
            EditorGUILayout.Space();
            
            // 没有Root层级FBX列表导出功能
            string noRootCSVPath = "";
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("导出无Root列表路径", GUILayout.Width(120));
            noRootCSVPath = EditorGUILayout.TextField(noRootCSVPath);
            if (GUILayout.Button("浏览...", GUILayout.Width(60)))
            {
                string defaultPath = !string.IsNullOrEmpty(noRootCSVPath) 
                    ? System.IO.Path.GetDirectoryName(noRootCSVPath) 
                    : Application.dataPath;
                    
                string path = EditorUtility.SaveFilePanel(
                    "保存CSV文件",
                    defaultPath,
                    "NoRootFBXList.csv",
                    "csv");
                
                if (!string.IsNullOrEmpty(path))
                {
                    noRootCSVPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            // 导出按钮固定在底部
            if (GUILayout.Button("导出无Root层级的FBX列表", GUILayout.Height(30)))
            {
                // 调用FBXChecker.ExportNoRootFBXToCSV
                Debug.Log("导出无Root层级的FBX列表: " + noRootCSVPath);
            }

            // 工具窗口按钮
            EditorGUILayout.Space(10);
            if (GUILayout.Button("检查FBX", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.FBXChecker>("检查FBX");
            }

            if (GUILayout.Button("FBX转预制体", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.FBX2Prefab>("FBX转预制体");
            }

            // 从FBX2Prefab.cs中的OnGUI方法复制过来的
            EditorGUILayout.Space();
            GUILayout.Label("FBX转预制体工具", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("FBX目录", GUILayout.Width(80));
            sourceFbxDirectory = EditorGUILayout.TextField(sourceFbxDirectory);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择FBX文件夹", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        path = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    sourceFbxDirectory = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("预制体目录", GUILayout.Width(80));
            outputPrefabDirectory = EditorGUILayout.TextField(outputPrefabDirectory);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择预制体文件夹", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        path = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    outputPrefabDirectory = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            preserveMaterials = EditorGUILayout.Toggle("保留材质", preserveMaterials);

            if (GUILayout.Button("转换"))
            {
                // 调用FBX2Prefab.Convert
                Debug.Log($"转换FBX到预制体: 源={sourceFbxDirectory}, 目标={outputPrefabDirectory}, 保留材质={preserveMaterials}");
            }
        }

        private void DrawAssetsTab()
        {
            EditorGUILayout.LabelField("资源工具", headerStyle);
            
            if (GUILayout.Button("重命名工具", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.RenameTool>("重命名工具");
            }

            // 以下是从RenamePlus.cs中的OnGUI方法复制过来的
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            // CSV操作部分
            GUILayout.Space(20);
            GUILayout.Label("资产CSV操作", EditorStyles.boldLabel);

            string folderPath = DrawFilePathField("目标文件夹", "", "选择文件夹", "", true);
            string outputCSVPath = DrawFilePathField("输出CSV路径", "", "选择CSV文件", "", true);

            if (GUILayout.Button("输出CSV"))
            {
                // 调用ExportAssetsCsv
                Debug.Log("输出CSV");
            }

            string renameCSVPath = DrawFilePathField("重命名CSV路径", "", "选择重命名CSV文件", "csv", false);
            if (GUILayout.Button("重命名CSV"))
            {
                // 调用RenameAssetsByCsv
                Debug.Log("重命名CSV");
            }

            string OriginCSVPath = DrawFilePathField("输出CSV路径", "", "选择CSV文件", "csv", false);
            string compareCSVPath = DrawFilePathField("对照CSV路径", "", "选择对照CSV文件", "csv", false);

            if (GUILayout.Button("对比CSV并在csv中标记'未制作'"))
            {
                // 调用CompareAndMarkCSV
                Debug.Log("对比CSV");
            }

            GUILayout.Space(20);
            GUILayout.Label("批量重命名选项", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("前缀");
            GUILayout.Label("重命名内容");
            GUILayout.Label("后缀");
            GUILayout.Label("起始索引");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            prefix = EditorGUILayout.TextField(prefix);
            renamePattern = EditorGUILayout.TextField(renamePattern);
            suffix = EditorGUILayout.TextField(suffix);
            renameStartIndex = EditorGUILayout.IntField(renameStartIndex);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("直接添加"))
            {
                // 调用AddPrefixToSelectedAssets
                Debug.Log("添加前缀:" + prefix);
            }
            GUILayout.Label("---");
            if (GUILayout.Button("直接添加"))
            {
                // 调用AddSuffixToSelectedAssets
                Debug.Log("添加后缀:" + suffix);
            }
            GUILayout.Label("---");
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("批量重命名选中资源"))
            {
                // 调用BatchRenameAssets
                Debug.Log("批量重命名");
            }

            // 资产名称查找替换
            GUILayout.Space(20);

            GUILayout.Label("资产名称查找替换", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("将字符：", GUILayout.Width(100));
            searchString = EditorGUILayout.TextField(searchString);
            EditorGUILayout.LabelField("替换为：", GUILayout.Width(100));
            replacementString = EditorGUILayout.TextField(replacementString);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("批量替换路径：", GUILayout.Width(100));
            searchPath = EditorGUILayout.TextField(searchPath);
            EditorGUILayout.LabelField("区分大小写", GUILayout.Width(100));
            caseSensitive = EditorGUILayout.Toggle(caseSensitive);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("查找替换"))
            {
                // 调用FindAndReplaceInAssetNames
                Debug.Log("查找替换:" + searchString + " -> " + replacementString);
            }

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("删除字符数量:", GUILayout.Width(100));
            removeCharCount = EditorGUILayout.IntField(removeCharCount);
            removeCharCount = Mathf.Max(0, removeCharCount);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("从前往后删除名称中的字符"))
            {
                // 调用RemoveCharactersFromAssetNames(true)
                Debug.Log("从前往后删除字符");
            }
            if (GUILayout.Button("从后往前删除名称中的字符"))
            {
                // 调用RemoveCharactersFromAssetNames(false)
                Debug.Log("从后往前删除字符");
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            assetPath = DrawFilePathField("处理路径", assetPath, "选择文件夹", "", true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("基础名称", GUILayout.Width(100));
            baseName = EditorGUILayout.TextField(baseName);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("资产名称重命名为基础名称（自动加编号，唯一）"))
            {
                // 调用RenameToUniqueName
                Debug.Log("重命名为基础名称:" + baseName);
            }

            GUILayout.Space(20);
            GUILayout.Label("其他命名功能", EditorStyles.boldLabel);

            if (GUILayout.Button("重命名场景中选中对象为顶级物体名称"))
            {
                // 调用RenameSelectedObjects
                Debug.Log("重命名为顶级物体名称");
            }
            if (GUILayout.Button("还原场景中选中对象为其引用prefab原始名称"))
            {
                // 调用RenameSelectedObjectsToPrefabName
                Debug.Log("还原为prefab原始名称");
            }
            if (GUILayout.Button("重命名场景中选中对象为object并自动添加索引"))
            {
                // 调用RenameSelectedObjectsWithIndex
                Debug.Log("重命名为object加索引");
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button("替换器", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.Replacer>("替换器");
            }

            if (GUILayout.Button("纹理导出器", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.TextureExporter>("纹理导出器");
            }

            if (GUILayout.Button("地形导出", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.terrainexport>("地形导出");
            }

            EditorGUILayout.Space(10);
        }

        private void DrawNavigationTab()
        {
            EditorGUILayout.LabelField("导航工具", headerStyle);

            // 场景导航
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("场景导航", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("顶视图"))
            {
                // 调用SceneView.lastActiveSceneView.FrameSelected()
                Debug.Log("切换到顶视图");
            }
            
            if (GUILayout.Button("前视图"))
            {
                Debug.Log("切换到前视图");
            }
            
            if (GUILayout.Button("侧视图"))
            {
                Debug.Log("切换到侧视图");
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("聚焦所选对象"))
            {
                Debug.Log("聚焦所选对象");
            }
            
            EditorGUILayout.EndVertical();
            
            // 场景书签
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("场景书签", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("书签名称", GUILayout.Width(80));
            bookmarkName = EditorGUILayout.TextField(bookmarkName);
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("保存当前视图"))
            {
                Debug.Log($"保存当前视图: {bookmarkName}");
            }
            
            if (bookmarks != null && bookmarks.Count > 0)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
                for (int i = 0; i < bookmarks.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(bookmarks[i], GUILayout.Width(150)))
                    {
                        Debug.Log($"跳转到书签: {bookmarks[i]}");
                    }
                    
                    if (GUILayout.Button("X", GUILayout.Width(30)))
                    {
                        Debug.Log($"删除书签: {bookmarks[i]}");
                        bookmarks.RemoveAt(i);
                        i--;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox("没有保存的视图书签", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
            
            // 层级导航
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("层级导航", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("展开所有"))
            {
                Debug.Log("展开所有层级");
            }
            
            if (GUILayout.Button("折叠所有"))
            {
                Debug.Log("折叠所有层级");
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("仅显示选中对象"))
            {
                Debug.Log("仅显示选中对象");
            }
            
            EditorGUILayout.EndVertical();
            
            // 打开工具窗口
            EditorGUILayout.Space(10);
            if (GUILayout.Button("场景导航器", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.SceneNavigator>("场景导航器");
            }
            
            if (GUILayout.Button("快速跳转", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.QuickJump>("快速跳转");
            }
        }

        private void DrawCleanupTab()
        {
            EditorGUILayout.LabelField("清理工具", headerStyle);

            // 以下是从CleanupTool.cs的OnGUI方法复制过来的
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("清理选项", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("目标路径", GUILayout.Width(80));
            cleanupFolderPath = GUILayout.TextField(cleanupFolderPath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    cleanupFolderPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            includeMetaFiles = EditorGUILayout.Toggle("包含Meta文件", includeMetaFiles);
            includeSubfolders = EditorGUILayout.Toggle("包含子文件夹", includeSubfolders);
            deleteEmptyFolders = EditorGUILayout.Toggle("删除空文件夹", deleteEmptyFolders);
            
            EditorGUILayout.Space();
            
            GUILayout.Label("选择要清理的文件类型：");
            EditorGUILayout.BeginHorizontal();
            bool cleanupImages = EditorGUILayout.Toggle("图片", false);
            bool cleanupModels = EditorGUILayout.Toggle("模型", false);
            bool cleanupMaterials = EditorGUILayout.Toggle("材质", false);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            bool cleanupPrefabs = EditorGUILayout.Toggle("预制体", false);
            bool cleanupScenes = EditorGUILayout.Toggle("场景", false);
            bool cleanupScripts = EditorGUILayout.Toggle("脚本", false);
            EditorGUILayout.EndHorizontal();
            
            string customFileExtension = EditorGUILayout.TextField("自定义文件扩展名 (用;分隔)", "");
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("分析未引用资源"))
            {
                AnalyzeUnusedAssets();
            }
            
            EditorGUILayout.Space();
            
            if (unusedAssets != null && unusedAssets.Count > 0)
            {
                EditorGUILayout.LabelField($"找到 {unusedAssets.Count} 个未引用资源", EditorStyles.boldLabel);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
                
                foreach (var asset in unusedAssets)
                {
                    EditorGUILayout.BeginHorizontal();
                    bool isSelected = selectedAssets.Contains(asset);
                    bool newSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
                    
                    if (newSelected != isSelected)
                    {
                        if (newSelected)
                        {
                            selectedAssets.Add(asset);
                        }
                        else
                        {
                            selectedAssets.Remove(asset);
                        }
                    }
                    
                    EditorGUILayout.LabelField(asset);
                    
                    if (GUILayout.Button("查看", GUILayout.Width(60)))
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(asset);
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("全选"))
                {
                    selectedAssets = new List<string>(unusedAssets);
                }
                
                if (GUILayout.Button("全不选"))
                {
                    selectedAssets.Clear();
                }
                
                if (GUILayout.Button("反选"))
                {
                    List<string> newSelection = new List<string>();
                    foreach (var asset in unusedAssets)
                    {
                        if (!selectedAssets.Contains(asset))
                        {
                            newSelection.Add(asset);
                        }
                    }
                    selectedAssets = newSelection;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginHorizontal();
                backupBeforeDelete = EditorGUILayout.Toggle("删除前备份", backupBeforeDelete);
                EditorGUILayout.EndHorizontal();
                
                if (backupBeforeDelete)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("备份路径", GUILayout.Width(80));
                    backupFolderPath = GUILayout.TextField(backupFolderPath);
                    if (GUILayout.Button("浏览", GUILayout.Width(60)))
                    {
                        string path = EditorUtility.OpenFolderPanel("选择备份文件夹", Application.dataPath, "");
                        if (!string.IsNullOrEmpty(path))
                        {
                            backupFolderPath = path;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                GUIStyle deleteButtonStyle = new GUIStyle(GUI.skin.button);
                deleteButtonStyle.normal.textColor = Color.red;
                
                if (GUILayout.Button("删除选中的未引用资源", deleteButtonStyle))
                {
                    DeleteSelectedAssets();
                }
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("资源清理", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.CleanupTool>("资源清理");
            }
            
            if (GUILayout.Button("无用模型删除", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.UnusedMeshRemover>("无用模型删除");
            }
        }
        
        private void AnalyzeUnusedAssets()
        {
            Debug.Log("分析未引用资源");
            // 这里会调用CleanupTool中的实际分析逻辑
            unusedAssets = new List<string>();
            selectedAssets = new List<string>();
            
            // 模拟添加一些未引用资源用于展示
            unusedAssets.Add("Assets/Unused/Image1.png");
            unusedAssets.Add("Assets/Unused/Model1.fbx");
            unusedAssets.Add("Assets/Unused/Material1.mat");
        }
        
        private void DeleteSelectedAssets()
        {
            Debug.Log($"删除{selectedAssets.Count}个未引用资源");
            if (backupBeforeDelete)
            {
                Debug.Log($"备份到: {backupFolderPath}");
            }
            // 这里会调用CleanupTool中的实际删除逻辑
        }

        private void DrawMiscTab()
        {
            EditorGUILayout.LabelField("杂项工具", headerStyle);

            // 资源查找工具
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("资源查找工具", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("查找文件名", GUILayout.Width(80));
            findFileName = EditorGUILayout.TextField(findFileName);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("查找路径", GUILayout.Width(80));
            findPath = EditorGUILayout.TextField(findPath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    // 转换为相对于项目的路径
                    if (path.StartsWith(Application.dataPath))
                    {
                        path = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                    findPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            findInScenes = EditorGUILayout.Toggle("在场景中查找", findInScenes);
            findInPrefabs = EditorGUILayout.Toggle("在预制体中查找", findInPrefabs);
            findInAssets = EditorGUILayout.Toggle("在资源中查找", findInAssets);
            exactMatch = EditorGUILayout.Toggle("精确匹配", exactMatch);
            caseSensitive = EditorGUILayout.Toggle("区分大小写", caseSensitive);
            
            if (GUILayout.Button("查找"))
            {
                Debug.Log($"查找文件: {findFileName}, 路径: {findPath}");
                // 这里调用MiscTools中的实际查找逻辑
            }
            
            EditorGUILayout.EndVertical();
            
            // 截图工具
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("截图工具", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("截图路径", GUILayout.Width(80));
            screenshotPath = EditorGUILayout.TextField(screenshotPath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    screenshotPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            screenshotScale = EditorGUILayout.Slider("截图缩放", screenshotScale, 0.1f, 5f);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("截取游戏视图"))
            {
                Debug.Log($"截取游戏视图: 路径={screenshotPath}, 缩放={screenshotScale}");
                // 这里调用MiscTools中的实际截图逻辑
            }
            
            if (GUILayout.Button("截取场景视图"))
            {
                Debug.Log($"截取场景视图: 路径={screenshotPath}, 缩放={screenshotScale}");
                // 这里调用MiscTools中的实际截图逻辑
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            // 对象选择器
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("对象选择器", EditorStyles.boldLabel);
            
            selectByTag = EditorGUILayout.Toggle("按标签选择", selectByTag);
            if (selectByTag)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("标签", GUILayout.Width(80));
                selectedTag = EditorGUILayout.TagField(selectedTag);
                EditorGUILayout.EndHorizontal();
                
                if (GUILayout.Button("选择所有带此标签的对象"))
                {
                    Debug.Log($"选择标签: {selectedTag}");
                    // 这里调用MiscTools中的实际选择逻辑
                }
            }
            
            selectByLayer = EditorGUILayout.Toggle("按层选择", selectByLayer);
            if (selectByLayer)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("层", GUILayout.Width(80));
                selectedLayer = EditorGUILayout.LayerField(selectedLayer);
                EditorGUILayout.EndHorizontal();
                
                if (GUILayout.Button("选择所有在此层的对象"))
                {
                    Debug.Log($"选择层: {selectedLayer}");
                    // 这里调用MiscTools中的实际选择逻辑
                }
            }
            
            selectByName = EditorGUILayout.Toggle("按名称选择", selectByName);
            if (selectByName)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("名称包含", GUILayout.Width(80));
                nameContains = EditorGUILayout.TextField(nameContains);
                EditorGUILayout.EndHorizontal();
                
                if (GUILayout.Button("选择所有名称包含此字符串的对象"))
                {
                    Debug.Log($"选择名称包含: {nameContains}");
                    // 这里调用MiscTools中的实际选择逻辑
                }
            }
            
            selectByComponent = EditorGUILayout.Toggle("按组件选择", selectByComponent);
            if (selectByComponent)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("组件类型", GUILayout.Width(80));
                componentType = EditorGUILayout.TextField(componentType);
                EditorGUILayout.EndHorizontal();
                
                if (GUILayout.Button("选择所有带此组件的对象"))
                {
                    Debug.Log($"选择组件: {componentType}");
                    // 这里调用MiscTools中的实际选择逻辑
                }
            }
            
            EditorGUILayout.EndVertical();

            // 工具按钮
            EditorGUILayout.Space(10);
            if (GUILayout.Button("网格工具", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.MeshTools>("网格工具");
            }
            
            if (GUILayout.Button("杂项工具", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.MiscTools>("杂项工具");
            }
            
            if (GUILayout.Button("批量缩放", GUILayout.Height(30)))
            {
                GetWindow<DYM.ToolBox.BatchScaleTool>("批量缩放");
            }
        }
    }
} 