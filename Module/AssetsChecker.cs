using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

public class AssetNameCheckerWindow : EditorWindow
{
    private string folderPath = "Assets"; // 默认检查路径

    [MenuItem("美术工具/检查工具/资产名称检查工具")]
    public static void ShowWindow()
    {
        // 创建一个窗口
        GetWindow<AssetNameCheckerWindow>("Asset Name Checker");
    }

    private void OnGUI()
    {
        GUILayout.Label("检查资产命名规则工具", EditorStyles.boldLabel);

        // 输入框让用户输入路径
        GUILayout.Label("检查路径（相对于Assets文件夹）：");
        folderPath = EditorGUILayout.TextField(folderPath);

        // 按钮触发检查
        if (GUILayout.Button("运行命名规则检查"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("路径不能为空！");
                return;
            }

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"路径无效: {folderPath}. 请确保输入的是一个有效的文件夹路径！");
                return;
            }

            CheckAssetNaming(folderPath);
        }

        // 按钮触发替换空格为_
        if (GUILayout.Button("替换资产名称中的空格为'_'"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("路径不能为空！");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"路径无效: {folderPath}. 请确保输入的是一个有效的文件夹路径！");
                return;
            }

            ReplaceSpacesWithUnderscores(folderPath);
        }

        // 按钮触发将 - 替换为 _
        if (GUILayout.Button("将资产名称中的 '-' 替换为 '_'"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("路径不能为空！");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"路径无效: {folderPath}. 请确保输入的是一个有效的文件夹路径！");
                return;
            }

            ReplaceDashWithUnderscoreInAssetNames(folderPath);
        }

        // 按钮触发将连续多个 _ 替换为单个 _
        if (GUILayout.Button("将连续多个 '_' 替换为单个 '_'"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("路径不能为空！");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"路径无效: {folderPath}. 请确保输入的是一个有效的文件夹路径！");
                return;
            }

            ReplaceMultipleUnderscoresWithSingle(folderPath);
        }

        // 按钮触发贴图名称修改
        if (GUILayout.Button("将贴图名称修改为引用它的材质球名称"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("路径不能为空！");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"路径无效: {folderPath}. 请确保输入的是一个有效的文件夹路径！");
                return;
            }

            RenameTexturesBasedOnMaterial(folderPath);
        }

        // 检测中文字符
        if (GUILayout.Button("检测资产名称中的中文字符"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("路径不能为空！");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"路径无效: {folderPath}. 请确保输入的是一个有效的文件夹路径！");
                return;
            }

            CheckChineseCharactersInAssetNames(folderPath);
        }

        // 整合功能按钮 - 依次执行所有操作并在最后统一刷新
        if (GUILayout.Button("一键优化资产命名（批处理模式）"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("路径不能为空！");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"路径无效: {folderPath}. 请确保输入的是一个有效的文件夹路径！");
                return;
            }

            BatchProcessAssetNames(folderPath);
        }
    }

    private static void BatchProcessAssetNames(string assetFolderPath)
    {
        Debug.Log("开始批处理资产命名优化...");

        // 搜集所有资产信息
        Debug.Log("正在收集资产信息...");
        string[] allAssetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        Dictionary<string, List<string>> textureUsage = new Dictionary<string, List<string>>();

        // 收集材质球和贴图的关系信息
        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { assetFolderPath });
        foreach (string guid in materialGuids)
        {
            string materialPath = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
                continue;

            foreach (var texturePropertyName in material.GetTexturePropertyNames())
            {
                Texture texture = material.GetTexture(texturePropertyName);
                if (texture == null)
                    continue;

                string texturePath = AssetDatabase.GetAssetPath(texture);
                if (!textureUsage.ContainsKey(texturePath))
                {
                    textureUsage[texturePath] = new List<string>();
                }
                textureUsage[texturePath].Add(material.name);
            }
        }

        // 跟踪重命名计数
        int spacesReplaced = 0;
        int dashesReplaced = 0;
        int underscoresReplaced = 0;
        int texturesRenamed = 0;
        bool needsRefresh = false;

        // 记录包含中文字符的资产
        List<string> assetsWithChineseCharacters = new List<string>();

        Regex regexMultipleUnderscores = new Regex("_{2,}"); // 匹配两个或更多连续的下划线
        Regex regexChineseCharacters = new Regex(@"[\u4e00-\u9fa5\u3000-\u303F\uFF00-\uFFEF]+"); // 匹配中文字符和中文标点

        // 1. 处理普通资产名称
        Debug.Log("开始处理资产名称...");
        foreach (string guid in allAssetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // 跳过目录
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);
            bool needsRename = false;
            string newAssetName = assetName;

            // 检测中文字符
            if (regexChineseCharacters.IsMatch(assetName))
            {
                assetsWithChineseCharacters.Add(assetPath);
            }

            // 处理空格
            if (newAssetName.Contains(" "))
            {
                newAssetName = newAssetName.Replace(" ", "_");
                needsRename = true;
                spacesReplaced++;
            }

            // 处理破折号
            if (newAssetName.Contains("-"))
            {
                newAssetName = newAssetName.Replace("-", "_");
                needsRename = true;
                dashesReplaced++;
            }

            // 处理连续下划线
            if (regexMultipleUnderscores.IsMatch(newAssetName))
            {
                string beforeReplace = newAssetName;
                newAssetName = regexMultipleUnderscores.Replace(newAssetName, "_");
                Debug.Log($"替换连续下划线: {assetPath}, {beforeReplace} -> {newAssetName}");
                needsRename = true;
                underscoresReplaced++;
            }

            // 如果需要重命名，执行重命名
            if (needsRename)
            {
                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (!string.IsNullOrEmpty(renameResult))
                {
                    Debug.LogError($"资产重命名失败: {assetPath} -> {newAssetName}. 错误: {renameResult}");
                }
                needsRefresh = true;
            }
        }

        // 2. 处理贴图重命名
        Debug.Log("开始处理贴图名称...");
        foreach (var entry in textureUsage)
        {
            string texturePath = entry.Key;
            List<string> materialNames = entry.Value;
            string textureName = Path.GetFileNameWithoutExtension(texturePath);

            // 确保贴图在我们的目标文件夹下
            if (!texturePath.StartsWith(assetFolderPath))
                continue;

            // 处理贴图被单个材质球引用或多个相同名称材质球引用的情况
            var distinctMaterialNames = materialNames.Distinct().ToList();
            if (distinctMaterialNames.Count == 1)
            {
                string newTextureName = distinctMaterialNames[0];

                if (textureName != newTextureName)
                {
                    string renameResult = AssetDatabase.RenameAsset(texturePath, newTextureName);
                    if (string.IsNullOrEmpty(renameResult))
                    {
                        texturesRenamed++;
                        needsRefresh = true;
                    }
                    else
                    {
                        Debug.LogError($"贴图重命名失败: {texturePath}. 错误: {renameResult}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"贴图 {texturePath} 被多个不同名称的材质球引用: {string.Join(", ", distinctMaterialNames)}");
            }
        }

        // 统一刷新
        if (needsRefresh)
        {
            Debug.Log("正在保存并刷新资源...");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // 输出包含中文字符的资产列表
        if (assetsWithChineseCharacters.Count > 0)
        {
            Debug.LogWarning($"发现 {assetsWithChineseCharacters.Count} 个资产名称包含中文字符:");
            foreach (var assetPath in assetsWithChineseCharacters)
            {
                // 使用可点击的资产路径
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (asset != null)
                {
                    Debug.LogWarning($"包含中文字符: {Path.GetFileNameWithoutExtension(assetPath)}", asset);
                }
                else
                {
                    Debug.LogWarning($"包含中文字符: {assetPath}");
                }
            }
        }

        Debug.Log($"批处理完成！共替换了 {spacesReplaced} 个资产的空格为'_'，替换了 {dashesReplaced} 个资产的'-'为'_'，处理了 {underscoresReplaced} 个资产的连续下划线，重命名了 {texturesRenamed} 个贴图。");
    }

    private static void CheckAssetNaming(string assetFolderPath)
    {
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        int issueCount = 0;

        Debug.Log($"开始检查资产命名规则...");

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

            // 检测资产名称是否包含空格
            if (assetName.Contains(" "))
            {
                Debug.LogWarning($"名称包含空格: {assetName}", asset);
                issueCount++;
            }

            // 检测材质贴图名称是否一致
            if (Path.GetExtension(assetPath).Equals(".mat", System.StringComparison.OrdinalIgnoreCase))
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (material != null)
                {
                    foreach (var texturePropertyName in material.GetTexturePropertyNames())
                    {
                        Texture texture = material.GetTexture(texturePropertyName);
                        if (texture != null)
                        {
                            // 检查材质球名称和贴图名称是否一致
                            // 仅检查主贴图（不包括法线贴图等）
                            if (texturePropertyName == "_MainTex" && texture.name != material.name)
                            {
                                string texturePath = AssetDatabase.GetAssetPath(texture);
                                Debug.LogWarning($"材质 '{material.name}' 的主贴图名称不一致: 贴图 '{texture.name}'", material);
                                issueCount++;
                            }
                        }
                    }
                }
            }
        }

        if (issueCount == 0)
        {
            Debug.Log("所有资产命名规则均符合要求！");
        }
        else
        {
            Debug.Log($"检查完成，共发现 {issueCount} 个命名问题！");
        }
    }

    private static void CheckChineseCharactersInAssetNames(string assetFolderPath)
    {
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        List<string> assetsWithChineseCharacters = new List<string>();
        Regex regexChineseCharacters = new Regex(@"[\u4e00-\u9fa5\u3000-\u303F\uFF00-\uFFEF]+"); // 匹配中文字符和中文标点

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string assetName = Path.GetFileNameWithoutExtension(assetPath);

            if (regexChineseCharacters.IsMatch(assetName))
            {
                assetsWithChineseCharacters.Add(assetPath);
            }
        }

        if (assetsWithChineseCharacters.Count == 0)
        {
            Debug.Log("未发现包含中文字符的资产名称！");
        }
        else
        {
            Debug.LogWarning($"发现 {assetsWithChineseCharacters.Count} 个资产名称包含中文字符:");
            foreach (var assetPath in assetsWithChineseCharacters)
            {
                string assetName = Path.GetFileNameWithoutExtension(assetPath);
                // 提取出中文字符部分并显示
                var matches = regexChineseCharacters.Matches(assetName);
                StringBuilder chineseChars = new StringBuilder();
                foreach (Match match in matches)
                {
                    chineseChars.Append(match.Value);
                }

                // 使用格式化日志，确保路径可点击
                Debug.LogWarning($"包含中文字符: \"{chineseChars}\"", AssetDatabase.LoadAssetAtPath<Object>(assetPath));
            }
        }
    }

    private static int ReplaceSpacesWithUnderscores(string assetFolderPath, bool refreshAssets = true)
    {
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        int renameCount = 0;
        bool needsRefresh = false;

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);

            if (assetName.Contains(" "))
            {
                string newAssetName = assetName.Replace(" ", "_");
                string directory = Path.GetDirectoryName(assetPath);
                string newAssetPath = Path.Combine(directory, newAssetName + extension).Replace("\\", "/");

                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (string.IsNullOrEmpty(renameResult))
                {
                    // 使用可点击的资产路径
                    Debug.Log($"资产重命名（替换空格为'_'）: {newAssetName}", AssetDatabase.LoadAssetAtPath<Object>(newAssetPath));
                    renameCount++;
                    needsRefresh = true;
                }
                else
                {
                    Debug.LogError($"资产重命名失败: {assetPath}. 错误: {renameResult}");
                }
            }
        }

        if (needsRefresh && refreshAssets)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"替换空格为'_'处理完成，共处理了 {renameCount} 个资产名称！");
        return renameCount;
    }

    private static int ReplaceDashWithUnderscoreInAssetNames(string assetFolderPath, bool refreshAssets = true)
    {
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        int renameCount = 0;
        bool needsRefresh = false;

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);

            // 检测并替换 '-'
            if (assetName.Contains("-"))
            {
                string newAssetName = assetName.Replace("-", "_");
                string directory = Path.GetDirectoryName(assetPath);
                string newAssetPath = Path.Combine(directory, newAssetName + extension).Replace("\\", "/");

                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (string.IsNullOrEmpty(renameResult))
                {
                    // 使用可点击的资产路径
                    Debug.Log($"资产重命名（替换 '-' 为 '_'）: {newAssetName}", AssetDatabase.LoadAssetAtPath<Object>(newAssetPath));
                    renameCount++;
                    needsRefresh = true;
                }
                else
                {
                    Debug.LogError($"资产重命名失败: {assetPath}. 错误: {renameResult}");
                }
            }
        }

        if (needsRefresh && refreshAssets)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"替换 '-' 为 '_' 处理完成，共处理了 {renameCount} 个资产名称！");
        return renameCount;
    }

    private static int ReplaceMultipleUnderscoresWithSingle(string assetFolderPath, bool refreshAssets = true)
    {
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        int renameCount = 0;
        bool needsRefresh = false;
        Regex regex = new Regex("_{2,}"); // 匹配两个或更多连续的下划线

        Debug.Log($"开始检查连续下划线，共找到 {assetGuids.Length} 个资产");

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // 跳过目录
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);

            // 检测并替换多个连续下划线
            if (regex.IsMatch(assetName))
            {
                string newAssetName = regex.Replace(assetName, "_");
                string directory = Path.GetDirectoryName(assetPath);
                string newAssetPath = Path.Combine(directory, newAssetName + extension).Replace("\\", "/");

                // 使用可点击的资产路径
                Debug.Log($"检测到连续下划线: {assetName}", AssetDatabase.LoadAssetAtPath<Object>(assetPath));

                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (string.IsNullOrEmpty(renameResult))
                {
                    // 重命名后使用新路径
                    Debug.Log($"资产重命名（替换多个 '_' 为单个 '_'）: {newAssetName}", AssetDatabase.LoadAssetAtPath<Object>(newAssetPath));
                    renameCount++;
                    needsRefresh = true;
                }
                else
                {
                    Debug.LogError($"资产重命名失败: {assetPath}. 错误: {renameResult}");
                }
            }
        }

        if (needsRefresh && refreshAssets)
        {
            Debug.Log("正在刷新资产数据库...");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"替换多个 '_' 为单个 '_' 处理完成，共处理了 {renameCount} 个资产名称！");
        return renameCount;
    }

    private static int RenameTexturesBasedOnMaterial(string assetFolderPath, bool refreshAssets = true)
    {
        Dictionary<string, List<string>> textureUsage = new Dictionary<string, List<string>>();
        string[] assetGuids = AssetDatabase.FindAssets("t:Material", new[] { assetFolderPath });
        bool needsRefresh = false;

        foreach (string guid in assetGuids)
        {
            string materialPath = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
                continue;

            foreach (var texturePropertyName in material.GetTexturePropertyNames())
            {
                Texture texture = material.GetTexture(texturePropertyName);
                if (texture == null)
                    continue;

                string texturePath = AssetDatabase.GetAssetPath(texture);
                if (!textureUsage.ContainsKey(texturePath))
                {
                    textureUsage[texturePath] = new List<string>();
                }
                textureUsage[texturePath].Add(material.name);
            }
        }

        int renameCount = 0;

        foreach (var entry in textureUsage)
        {
            string texturePath = entry.Key;
            List<string> materialNames = entry.Value;
            string textureName = Path.GetFileNameWithoutExtension(texturePath);

            // 处理贴图被单个材质球引用的情况
            if (materialNames.Count == 1)
            {
                string newTextureName = materialNames[0]; // 使用材质球的名称

                if (textureName != newTextureName)
                {
                    RenameTexture(texturePath, newTextureName, ref renameCount, ref needsRefresh, false);
                }
            }
            // 处理贴图被多个材质球引用的情况
            else
            {
                // 检查所有引用该贴图的材质球名称是否相同
                var distinctMaterialNames = materialNames.Distinct().ToList();

                // 如果所有引用的材质球名称相同
                if (distinctMaterialNames.Count == 1)
                {
                    string newTextureName = distinctMaterialNames[0];

                    if (textureName != newTextureName)
                    {
                        RenameTexture(texturePath, newTextureName, ref renameCount, ref needsRefresh, false);
                    }
                }
                else
                {
                    Object textureAsset = AssetDatabase.LoadAssetAtPath<Object>(texturePath);
                    Debug.LogWarning($"贴图被多个不同名称的材质球引用: {string.Join(", ", distinctMaterialNames)}", textureAsset);
                }
            }
        }

        if (needsRefresh && refreshAssets)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"贴图重命名完成，共处理了 {renameCount} 个贴图！");
        return renameCount;
    }

    // 封装贴图重命名的逻辑为一个单独的方法
    private static void RenameTexture(string texturePath, string newTextureName, ref int renameCount, ref bool needsRefresh, bool printLog = true)
    {
        string extension = Path.GetExtension(texturePath);
        string directory = Path.GetDirectoryName(texturePath);
        string newTexturePath = Path.Combine(directory, newTextureName + extension).Replace("\\", "/");

        string renameResult = AssetDatabase.RenameAsset(texturePath, newTextureName);
        if (string.IsNullOrEmpty(renameResult))
        {
            // 仅在需要时打印日志
            if (printLog)
            {
                // 使用可点击的资产路径
                Debug.Log($"贴图重命名: {newTextureName}", AssetDatabase.LoadAssetAtPath<Object>(newTexturePath));
            }
            renameCount++;
            needsRefresh = true;
        }
        else
        {
            Debug.LogError($"贴图重命名失败: {texturePath}. 错误: {renameResult}");
        }
    }
}