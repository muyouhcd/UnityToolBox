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

        // 添加按钮：移除文件名末尾下划线
        if (GUILayout.Button("移除文件名末尾的下划线"))
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

            RemoveTrailingUnderscores(folderPath);
        }

        // 添加按钮：将FBX后缀转为小写
        if (GUILayout.Button("将FBX后缀转为小写"))
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

            ConvertFbxExtensionToLowercase(folderPath);
        }

        // 添加按钮：替换#为下划线
        if (GUILayout.Button("替换资产名称中的 '#' 为 '_'"))
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

            ReplaceHashWithUnderscore(folderPath);
        }

        // 添加按钮：处理文件夹名称
        if (GUILayout.Button("处理文件夹名称（替换空格、破折号和连续下划线）"))
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

            ProcessFolderNames(folderPath);
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

        // 0. 首先处理文件夹名称 - 由于这会影响其他资产的路径，所以最先处理
        Debug.Log("步骤0: 处理文件夹名称...");
        int foldersRenamed = ProcessFolderNames(assetFolderPath, true);
        bool needsRefresh = foldersRenamed > 0;

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

        Regex regexMultipleUnderscores = new Regex("_{2,}"); // 匹配两个或更多连续的下划线

        // 1. 处理空格替换为下划线
        Debug.Log("步骤1: 替换空格为下划线...");
        foreach (string guid in allAssetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // 跳过目录
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);

            if (assetName.Contains(" "))
            {
                string newAssetName = assetName.Replace(" ", "_");
                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (string.IsNullOrEmpty(renameResult))
                {
                    spacesReplaced++;
                    needsRefresh = true;
                }
                else
                {
                    Debug.LogError($"资产重命名失败: {assetPath}. 错误: {renameResult}");
                }
            }
        }

        // 2. 处理破折号替换为下划线
        Debug.Log("步骤2: 替换破折号为下划线...");
        // 重新获取资产列表，因为可能有些资产已经重命名
        if (needsRefresh)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            allAssetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        }

        foreach (string guid in allAssetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // 跳过目录
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);

            // 检查名称中是否有破折号，使用更严格的检测
            if (assetName.IndexOf("-") >= 0)
            {
                // 确保替换所有破折号，而不仅仅是第一个
                string newAssetName = assetName.Replace("-", "_");

                // 验证新名称确实不包含破折号
                if (newAssetName.IndexOf("-") >= 0)
                {
                    Debug.LogError($"替换失败: {assetPath} 的名称中仍有破折号");
                    continue;
                }

                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (string.IsNullOrEmpty(renameResult))
                {
                    Debug.Log($"资产重命名（替换 '-' 为 '_'）: {newAssetName}", AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid)));
                    dashesReplaced++;
                    needsRefresh = true;
                }
                else
                {
                    Debug.LogError($"资产重命名失败: {assetPath}. 错误: {renameResult}");
                }
            }
        }

        // 3. 处理连续下划线替换为单个下划线
        Debug.Log("步骤3: 替换连续下划线为单个下划线...");
        // 重新获取资产列表，因为可能有些资产已经重命名
        if (needsRefresh)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            allAssetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        }

        foreach (string guid in allAssetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // 跳过目录
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);

            if (regexMultipleUnderscores.IsMatch(assetName))
            {
                string newAssetName = regexMultipleUnderscores.Replace(assetName, "_");
                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (string.IsNullOrEmpty(renameResult))
                {
                    underscoresReplaced++;
                    needsRefresh = true;
                }
                else
                {
                    Debug.LogError($"资产重命名失败: {assetPath}. 错误: {renameResult}");
                }
            }
        }

        // 4. 修改贴图名称为材质球名称
        Debug.Log("步骤4: 修改贴图名称为材质球名称...");
        // 重新获取资产列表和材质球-贴图关系，因为可能有些资产已经重命名
        if (needsRefresh)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 重新收集材质球和贴图关系
            textureUsage.Clear();
            materialGuids = AssetDatabase.FindAssets("t:Material", new[] { assetFolderPath });
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

                    // 忽略.vox文件
                    if (texturePath.EndsWith(".vox", System.StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.Log($"跳过.vox文件: {texturePath}");
                        continue;
                    }

                    if (!textureUsage.ContainsKey(texturePath))
                    {
                        textureUsage[texturePath] = new List<string>();
                    }
                    textureUsage[texturePath].Add(material.name);
                }
            }
        }

        foreach (var entry in textureUsage)
        {
            string texturePath = entry.Key;
            List<string> materialNames = entry.Value;
            string textureName = Path.GetFileNameWithoutExtension(texturePath);

            // 再次检查，忽略.vox文件（以防万一）
            if (texturePath.EndsWith(".vox", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

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
                Object textureAsset = AssetDatabase.LoadAssetAtPath<Object>(texturePath);
                Debug.LogWarning($"贴图被多个不同名称的材质球引用: {string.Join(", ", distinctMaterialNames)}", textureAsset);
            }
        }

        // 新增步骤: 移除文件名末尾的下划线
        Debug.Log("步骤3.5: 移除文件名末尾的下划线...");
        // 重新获取资产列表
        if (needsRefresh)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            allAssetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        }

        int trailingUnderscoresRemoved = 0;
        Regex regexTrailingUnderscore = new Regex("_+$"); // 匹配文件名末尾的一个或多个下划线

        foreach (string guid in allAssetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // 跳过目录
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);

            // 检测并移除文件名末尾的下划线
            if (regexTrailingUnderscore.IsMatch(assetName))
            {
                string newAssetName = regexTrailingUnderscore.Replace(assetName, "");

                // 确保重命名后的名称不为空
                if (string.IsNullOrEmpty(newAssetName))
                {
                    Debug.LogWarning($"移除末尾下划线后文件名为空: {assetPath}，跳过重命名");
                    continue;
                }

                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (string.IsNullOrEmpty(renameResult))
                {
                    trailingUnderscoresRemoved++;
                    needsRefresh = true;
                }
                else
                {
                    Debug.LogError($"资产重命名失败: {assetPath}. 错误: {renameResult}");
                }
            }
        }

        // 添加新步骤: 将FBX后缀转为小写
        Debug.Log("步骤4.5: 将FBX后缀转为小写...");
        // 重新获取资产列表
        if (needsRefresh)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        int fbxExtensionsConverted = ConvertFbxExtensionToLowercase(assetFolderPath, false);
        if (fbxExtensionsConverted > 0)
        {
            needsRefresh = true;
        }

        // 1.5. 替换#为下划线
        Debug.Log("步骤1.5: 替换#为下划线...");
        int hashesReplaced = ReplaceHashWithUnderscore(assetFolderPath, false);
        if (hashesReplaced > 0)
        {
            needsRefresh = true;
        }

        // 最后进行统一刷新
        if (needsRefresh)
        {
            Debug.Log("正在保存并刷新资源...");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // 5. 检测中文字符
        Debug.Log("步骤5: 检测中文字符...");
        // 收集中文字符的资产
        List<string> assetsWithChineseCharacters = new List<string>();
        Regex regexChineseCharacters = new Regex(@"[\u4e00-\u9fa5\u3000-\u303F\uFF00-\uFFEF]+"); // 匹配中文字符和中文标点

        // 重新获取最新的资产列表
        allAssetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });

        foreach (string guid in allAssetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // 跳过目录
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);

            // 检测中文字符
            if (regexChineseCharacters.IsMatch(assetName))
            {
                assetsWithChineseCharacters.Add(assetPath);
            }
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

        Debug.Log($"批处理完成！共重命名了 {foldersRenamed} 个文件夹，替换了 {spacesReplaced} 个资产的空格为'_'，替换了 {dashesReplaced} 个资产的'-'为'_'，处理了 {underscoresReplaced} 个资产的连续下划线，移除了 {trailingUnderscoresRemoved} 个资产名称末尾的下划线，将 {fbxExtensionsConverted} 个FBX后缀转为小写，重命名了 {texturesRenamed} 个贴图。");
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

                // 忽略.vox文件
                if (texturePath.EndsWith(".vox", System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"跳过.vox文件: {texturePath}");
                    continue;
                }

                if (!textureUsage.ContainsKey(texturePath))
                {
                    textureUsage[texturePath] = new List<string>();
                }
                textureUsage[texturePath].Add(material.name);
            }
        }

        int texturesRenamed = 0;

        foreach (var entry in textureUsage)
        {
            string texturePath = entry.Key;
            List<string> materialNames = entry.Value;
            string textureName = Path.GetFileNameWithoutExtension(texturePath);

            // 再次检查，忽略.vox文件（以防万一）
            if (texturePath.EndsWith(".vox", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

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
                Object textureAsset = AssetDatabase.LoadAssetAtPath<Object>(texturePath);
                Debug.LogWarning($"贴图被多个不同名称的材质球引用: {string.Join(", ", distinctMaterialNames)}", textureAsset);
            }
        }

        if (needsRefresh && refreshAssets)
        {
            Debug.Log("正在刷新资产数据库...");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"贴图重命名完成，共处理了 {texturesRenamed} 个贴图！");
        return texturesRenamed;
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

    // 添加一个单独的函数来去除文件名末尾的下划线
    private static int RemoveTrailingUnderscores(string assetFolderPath, bool refreshAssets = true)
    {
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        int renameCount = 0;
        bool needsRefresh = false;
        Regex regexTrailingUnderscore = new Regex("_+$"); // 匹配文件名末尾的一个或多个下划线

        Debug.Log($"开始检查文件名末尾下划线，共找到 {assetGuids.Length} 个资产");

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // 跳过目录
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);

            // 检测并移除文件名末尾的下划线
            if (regexTrailingUnderscore.IsMatch(assetName))
            {
                string newAssetName = regexTrailingUnderscore.Replace(assetName, "");

                // 确保重命名后的名称不为空
                if (string.IsNullOrEmpty(newAssetName))
                {
                    Debug.LogWarning($"移除末尾下划线后文件名为空: {assetPath}，跳过重命名");
                    continue;
                }

                string directory = Path.GetDirectoryName(assetPath);
                string newAssetPath = Path.Combine(directory, newAssetName + extension).Replace("\\", "/");

                Debug.Log($"检测到末尾下划线: {assetName}", AssetDatabase.LoadAssetAtPath<Object>(assetPath));

                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (string.IsNullOrEmpty(renameResult))
                {
                    Debug.Log($"资产重命名（移除末尾下划线）: {newAssetName}", AssetDatabase.LoadAssetAtPath<Object>(newAssetPath));
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

        Debug.Log($"移除末尾下划线处理完成，共处理了 {renameCount} 个资产名称！");
        return renameCount;
    }

    // 添加一个新的函数来将FBX后缀转为小写
    private static int ConvertFbxExtensionToLowercase(string assetFolderPath, bool refreshAssets = true)
    {
        string[] assetGuids = AssetDatabase.FindAssets("t:Model", new[] { assetFolderPath });
        int renameCount = 0;
        bool needsRefresh = false;

        Debug.Log($"开始处理FBX后缀大小写，共找到 {assetGuids.Length} 个模型资产");

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // 检查扩展名是否为.FBX (大写)
            if (Path.GetExtension(assetPath).Equals(".FBX", System.StringComparison.OrdinalIgnoreCase) &&
                !Path.GetExtension(assetPath).Equals(".fbx")) // 避免已经是小写的也处理
            {
                string directory = Path.GetDirectoryName(assetPath);
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                string newPath = Path.Combine(directory, fileName + ".fbx").Replace("\\", "/");

                // 由于Unity的AssetDatabase.RenameAsset不能更改扩展名，我们需要使用文件系统API
                // 但在Unity中，我们需要先检查资产数据库状态
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (asset != null)
                {
                    Debug.Log($"检测到大写FBX后缀: {assetPath}", asset);

                    try
                    {
                        // 使用AssetDatabase的方法修改文件名（包括扩展名）
                        // 这是一个两步过程：
                        // 1. 重命名为临时名称（加上时间戳避免冲突）
                        string tempName = fileName + "_temp_" + System.DateTime.Now.Ticks;
                        string tempResult = AssetDatabase.RenameAsset(assetPath, tempName);
                        if (!string.IsNullOrEmpty(tempResult))
                        {
                            Debug.LogError($"重命名到临时名称失败: {assetPath} -> {tempName}. 错误: {tempResult}");
                            continue;
                        }

                        string tempPath = Path.Combine(directory, tempName + Path.GetExtension(assetPath)).Replace("\\", "/");

                        // 2. 重命名为原名称加小写扩展名
                        string finalResult = AssetDatabase.RenameAsset(tempPath, fileName + ".fbx");
                        if (string.IsNullOrEmpty(finalResult))
                        {
                            Debug.Log($"FBX后缀转为小写: {fileName}.FBX -> {fileName}.fbx", AssetDatabase.LoadAssetAtPath<Object>(newPath));
                            renameCount++;
                            needsRefresh = true;
                        }
                        else
                        {
                            Debug.LogError($"重命名失败: {tempPath} -> {fileName}.fbx. 错误: {finalResult}");
                            // 尝试恢复原名
                            AssetDatabase.RenameAsset(tempPath, fileName + Path.GetExtension(assetPath));
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"处理文件时出错: {assetPath}. 错误: {ex.Message}");
                    }
                }
            }
        }

        if (needsRefresh && refreshAssets)
        {
            Debug.Log("正在刷新资产数据库...");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"FBX后缀转为小写处理完成，共处理了 {renameCount} 个资产！");
        return renameCount;
    }

    // 添加替换#为下划线的功能
    private static int ReplaceHashWithUnderscore(string assetFolderPath, bool refreshAssets = true)
    {
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        int renameCount = 0;
        bool needsRefresh = false;
        Regex regexMultipleUnderscores = new Regex("_{2,}"); // 匹配两个或更多连续的下划线

        Debug.Log($"开始替换#为下划线，共找到 {assetGuids.Length} 个资产");

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // 跳过目录
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);
            bool needsRename = false;
            string newAssetName = assetName;

            // 检测并替换 '#'
            if (assetName.Contains("#"))
            {
                newAssetName = assetName.Replace("#", "_");
                needsRename = true;
            }

            // 替换完#后立即检测并替换连续下划线
            if (needsRename && regexMultipleUnderscores.IsMatch(newAssetName))
            {
                newAssetName = regexMultipleUnderscores.Replace(newAssetName, "_");
            }

            // 如果需要重命名，执行重命名
            if (needsRename)
            {
                string directory = Path.GetDirectoryName(assetPath);
                string newAssetPath = Path.Combine(directory, newAssetName + extension).Replace("\\", "/");

                Debug.Log($"检测到含有#号: {assetName}", AssetDatabase.LoadAssetAtPath<Object>(assetPath));

                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (string.IsNullOrEmpty(renameResult))
                {
                    Debug.Log($"资产重命名（替换#为_）: {newAssetName}", AssetDatabase.LoadAssetAtPath<Object>(newAssetPath));
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

        Debug.Log($"替换#为_处理完成，共处理了 {renameCount} 个资产名称！");
        return renameCount;
    }

    // 修改ProcessFolderNames函数，添加替换#的功能
    private static int ProcessFolderNames(string assetFolderPath, bool refreshAssets = true)
    {
        int renamedCount = 0;
        bool needsRefresh = false;
        List<string> allFolders = new List<string>();
        Regex regexMultipleUnderscores = new Regex("_{2,}"); // 匹配两个或更多连续的下划线

        // 收集所有文件夹路径
        CollectAllFolders(assetFolderPath, allFolders);

        // 按路径长度排序，确保先处理较短的路径（父文件夹）
        // 使用降序排序，这样先处理深层目录，再处理上层目录
        allFolders.Sort((a, b) => b.Length.CompareTo(a.Length));

        Debug.Log($"开始处理文件夹名称，共找到 {allFolders.Count} 个文件夹");

        foreach (string folderPath in allFolders)
        {
            string folderName = Path.GetFileName(folderPath);
            string parentFolder = Path.GetDirectoryName(folderPath);
            bool needsRename = false;
            string newFolderName = folderName;

            // 替换空格为下划线
            if (newFolderName.Contains(" "))
            {
                newFolderName = newFolderName.Replace(" ", "_");
                needsRename = true;
            }

            // 替换破折号为下划线
            if (newFolderName.Contains("-"))
            {
                newFolderName = newFolderName.Replace("-", "_");
                needsRename = true;
            }

            // 替换#为下划线
            if (newFolderName.Contains("#"))
            {
                newFolderName = newFolderName.Replace("#", "_");
                needsRename = true;
            }

            // 替换连续下划线为单个下划线
            if (regexMultipleUnderscores.IsMatch(newFolderName))
            {
                newFolderName = regexMultipleUnderscores.Replace(newFolderName, "_");
                needsRename = true;
            }

            // 如果需要重命名，执行重命名
            if (needsRename)
            {
                string oldPath = folderPath;
                string newPath = Path.Combine(parentFolder, newFolderName).Replace("\\", "/");

                Debug.Log($"重命名文件夹: {oldPath} -> {newPath}");

                // 使用AssetDatabase API重命名文件夹
                string error = AssetDatabase.MoveAsset(oldPath, newPath);
                if (string.IsNullOrEmpty(error))
                {
                    renamedCount++;
                    needsRefresh = true;
                }
                else
                {
                    Debug.LogError($"重命名文件夹失败: {oldPath} -> {newPath}. 错误: {error}");
                }
            }
        }

        if (needsRefresh && refreshAssets)
        {
            Debug.Log("正在刷新资产数据库...");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"文件夹名称处理完成，共重命名了 {renamedCount} 个文件夹！");
        return renamedCount;
    }

    // 递归收集所有文件夹
    private static void CollectAllFolders(string rootFolder, List<string> allFolders)
    {
        if (!AssetDatabase.IsValidFolder(rootFolder))
            return;

        // 添加当前文件夹
        allFolders.Add(rootFolder);

        // 获取所有子文件夹
        string[] guids = AssetDatabase.FindAssets("", new[] { rootFolder });
        HashSet<string> subFolders = new HashSet<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path) && path != rootFolder)
            {
                string parentFolder = Path.GetDirectoryName(path).Replace("\\", "/");
                if (parentFolder == rootFolder)
                {
                    subFolders.Add(path);
                }
            }
        }

        // 递归处理每个子文件夹
        foreach (string subFolder in subFolders)
        {
            CollectAllFolders(subFolder, allFolders);
        }
    }
}