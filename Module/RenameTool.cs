using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

namespace DYM.ToolBox
{


public class RenamePlus : EditorWindow
{
    private Vector2 scrollPosition;
    private string renameCSVPath = string.Empty;
    private string outputCSVPath = string.Empty;
    private string compareCSVPath = string.Empty;
    private string searchString = string.Empty;
    private string replacementString = string.Empty;
    private string searchPath = "";
    private bool caseSensitive = false;
    private int removeCharCount = 0;
    private string prefix = "";
    private string suffix = "";
    private string renamePattern = "";
    private int renameStartIndex = 0;
    private string[] excludeExtensions = { ".meta", ".dll", ".cs" };
    private string folderPath = "";
    private string assetPath = "";
    private string baseName = "";

    private string OriginCSVPath = "";


    [MenuItem("美术工具/重命名工具/Rename Plus")]
    public static void ShowWindow()
    {
        GetWindow<RenamePlus>("Rename Plus");
    }

    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        // CSV操作部分
        GUILayout.Space(20);
        GUILayout.Label("资产CSV操作", EditorStyles.boldLabel);

        folderPath = DrawFilePathField("目标文件夹", folderPath, "选择文件夹", "", true);
        outputCSVPath = DrawFilePathField("输出CSV路径", outputCSVPath, "选择CSV文件", "", true);

        if (GUILayout.Button("输出CSV"))
        {
            ExportAssetsCsv();
        }

        renameCSVPath = DrawFilePathField("重命名CSV路径", renameCSVPath, "选择重命名CSV文件", "csv", false);
        if (GUILayout.Button("重命名CSV"))
        {
            RenameAssetsByCsv();
        }

        OriginCSVPath = DrawFilePathField("输出CSV路径", OriginCSVPath, "选择CSV文件", "csv", false);
        compareCSVPath = DrawFilePathField("对照CSV路径", compareCSVPath, "选择对照CSV文件", "csv", false);

        if (GUILayout.Button("对比CSV并在csv中标记'未制作'"))
        {
            CompareAndMarkCSV();
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
            AddPrefixToSelectedAssets();
        }
        GUILayout.Label("---");
        if (GUILayout.Button("直接添加"))
        {
            AddSuffixToSelectedAssets();
        }
        GUILayout.Label("---");
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("批量重命名选中资源"))
        {
            BatchRenameAssets();
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
            FindAndReplaceInAssetNames(searchString, replacementString, new[] { searchPath }, caseSensitive);
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
            RemoveCharactersFromAssetNames(start: true);
        }
        if (GUILayout.Button("从后往前删除名称中的字符"))
        {
            RemoveCharactersFromAssetNames(start: false);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        assetPath = DrawFilePathField("处理路径", assetPath, "选择文件夹", "", true);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("基础名称", GUILayout.Width(100));
        baseName = GUILayout.TextField(baseName);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("资产名称重命名为基础名称（自动加编号，唯一）"))
        {
            RenameToUniqueName(assetPath, baseName);
        }
        // 其他命名功能

        GUILayout.Space(20);
        GUILayout.Label("其他命名功能", EditorStyles.boldLabel);

        if (GUILayout.Button("重命名场景中选中对象为顶级物体名称"))
        {
            RenameSelectedObjects();
        }
        if (GUILayout.Button("还原场景中选中对象为其引用prefab原始名称"))
        {
            RenameSelectedObjectsToPrefabName();
        }
        if (GUILayout.Button("重命名场景中选中对象为object并自动添加索引"))
        {
            RenameSelectedObjectsWithIndex();
        }

        GUILayout.Space(20);




        GUILayout.EndScrollView();
    }


    private void RemoveCharactersFromAssetNames(bool start)
    {
        var selectedObjects = Selection.objects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected.");
            return;
        }

        List<string> renameErrors = new List<string>();

        AssetDatabase.StartAssetEditing(); // 开始批量编辑
        foreach (var obj in selectedObjects)
        {
            var assetPath = AssetDatabase.GetAssetPath(obj);
            string directory = Path.GetDirectoryName(assetPath);
            string oldName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);

            if (string.IsNullOrEmpty(oldName))
            {
                Debug.LogError($"Failed to get file name without extension for asset at path: {assetPath}");
                continue;
            }

            string newName;
            if (start)
            {
                newName = oldName.Length <= removeCharCount ? "" : oldName.Substring(removeCharCount);
            }
            else
            {
                int charsToRemove = Mathf.Min(oldName.Length, removeCharCount);
                newName = oldName.Substring(0, oldName.Length - charsToRemove);
            }

            string newAssetPath = Path.Combine(directory, newName + extension);
            string error = AssetDatabase.RenameAsset(assetPath, newName);
            if (!string.IsNullOrEmpty(error))
            {
                renameErrors.Add($"Failed to rename asset at path: {assetPath}. Error: {error}");
            }
        }
        AssetDatabase.StopAssetEditing(); // 结束批量编辑

        AssetDatabase.Refresh();
        if (renameErrors.Count > 0)
        {
            Debug.LogError(string.Join("\n", renameErrors));
        }
        else
        {
            Debug.Log("Characters removed from selected assets' names.");
        }
    }




    //--------------------------------从CSV重命名--------------------------------
    private void RenameAssetsByCsv()
    {
        if (string.IsNullOrEmpty(renameCSVPath))
        {
            Debug.LogError("请指定重命名CSV文件的路径。");
            return;
        }

        try
        {
            // 解析CSV文件，获取GUID到新名称的映射
            var guidToNewName = ParseCsvForRename(renameCSVPath);

            // 解析CSV文件，获取路径到新名称的映射
            var pathToNewName = ParseCsvForRenameByPath(renameCSVPath);

            // 首先尝试通过GUID重命名（保持GUID不变）
            int guidRenameCount = ProcessRenameByGuid(guidToNewName);

            // 然后处理无法通过GUID重命名的资产
            int pathRenameCount = ProcessRenameByPath(pathToNewName);

            AssetDatabase.Refresh();
            Debug.Log($"已根据CSV文件重命名资源。通过GUID重命名: {guidRenameCount}, 通过路径重命名: {pathRenameCount}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"重命名资产时出错: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private Dictionary<string, string> ParseCsvForRename(string csvPath)
    {
        var guidToNewName = new Dictionary<string, string>();
        var lines = File.ReadAllLines(csvPath);
        lines.Skip(1)
            .Select(line => line.Split(','))
            .ToList()
            .ForEach(tokens =>
            {
                if (tokens.Length >= 3)
                {
                    var newName = tokens[0].Trim('"').Trim();
                    var guid = tokens[1].Trim('"').Trim();
                    if (!string.IsNullOrEmpty(newName) && !string.IsNullOrEmpty(guid))
                    {
                        guidToNewName[guid] = newName;
                    }
                }
            });
        return guidToNewName;
    }

    private Dictionary<string, string> ParseCsvForRenameByPath(string csvPath)
    {
        var pathToNewName = new Dictionary<string, string>();
        var lines = File.ReadAllLines(csvPath);
        lines.Skip(1)
            .Select(line => line.Split(','))
            .ToList()
            .ForEach(tokens =>
            {
                if (tokens.Length >= 3)
                {
                    var newName = tokens[0].Trim('"').Trim();
                    var path = tokens[2].Trim('"').Trim(); // 使用路径
                    if (!string.IsNullOrEmpty(newName) && !string.IsNullOrEmpty(path))
                    {
                        pathToNewName[path] = newName;
                    }
                }
            });
        return pathToNewName;
    }

    private int ProcessRenameByGuid(Dictionary<string, string> guidToNewName)
    {
        int successCount = 0;

        // 使用AssetDatabase.StartAssetEditing()和StopAssetEditing()可以提高批量操作的性能
        AssetDatabase.StartAssetEditing();

        foreach (var kvp in guidToNewName)
        {
            string guid = kvp.Key;
            string newName = kvp.Value;

            // 通过GUID获取资产路径
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // 如果资产路径为空，说明GUID不在项目中，跳过
            if (string.IsNullOrEmpty(assetPath))
            {
                continue;
            }

            // 检查是否是文件夹
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                continue;
            }

            // 使用AssetDatabase.RenameAsset来保持GUID不变
            string error = AssetDatabase.RenameAsset(assetPath, newName);

            if (string.IsNullOrEmpty(error))
            {
                successCount++;
                Debug.Log($"已重命名资产(GUID保持不变): {assetPath} -> {newName}");
            }
            else
            {
                Debug.LogError($"重命名资产失败: {assetPath}, 错误: {error}");
            }
        }

        AssetDatabase.StopAssetEditing();
        return successCount;
    }

    private int ProcessRenameByPath(Dictionary<string, string> pathToNewName)
    {
        int successCount = 0;

        foreach (var kvp in pathToNewName)
        {
            string path = kvp.Key;
            string newName = kvp.Value;

            // 检查是否是Unity项目内的路径
            bool isUnityPath = false;
            string unityPath = path;

            if (Path.IsPathRooted(path))
            {
                int assetsIndex = path.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                if (assetsIndex >= 0)
                {
                    unityPath = path.Substring(assetsIndex);
                    isUnityPath = true;
                }
            }
            else if (path.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
            {
                isUnityPath = true;
            }

            // 如果是Unity项目内的路径，尝试使用AssetDatabase.RenameAsset
            if (isUnityPath)
            {
                // 获取GUID
                string guid = AssetDatabase.AssetPathToGUID(unityPath);

                // 如果能获取到GUID，说明已经在ProcessRenameByGuid中处理过，跳过
                if (!string.IsNullOrEmpty(guid))
                {
                    continue;
                }
            }

            try
            {
                // 对于项目外的文件，使用File.Move
                if (File.Exists(path))
                {
                    string directory = Path.GetDirectoryName(path);
                    string extension = Path.GetExtension(path);
                    string newPath = Path.Combine(directory, newName + extension);

                    // 检查新路径是否已存在
                    if (File.Exists(newPath) && !string.Equals(path, newPath, StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogWarning($"无法重命名 {path} 到 {newPath}，目标文件已存在");
                        continue;
                    }

                    File.Move(path, newPath);
                    successCount++;
                    Debug.Log($"已重命名文件(可能会改变GUID): {path} -> {newPath}");
                }
                else
                {
                    Debug.LogWarning($"文件不存在: {path}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"重命名文件时出错: {path}, 错误: {ex.Message}");
            }
        }

        return successCount;
    }


    //--------------------------------比较标记csv--------------------------------
    private void CompareAndMarkCSV()
    {
        if (string.IsNullOrEmpty(compareCSVPath) || string.IsNullOrEmpty(outputCSVPath))
        {
            Debug.LogError("CSV文件路径不能为空。");
            return;
        }

        try
        {
            var outputFileNames = new HashSet<string>(File.ReadAllLines(outputCSVPath).Skip(1).Select(line => line.Split(',')[0].Trim('"')));
            var compareLines = File.ReadAllLines(compareCSVPath);

            var savePath = EditorUtility.SaveFilePanel("保存标记文件", "", "MarkedCompareCSV", "csv");
            if (!string.IsNullOrEmpty(savePath))
            {
                MarkCompareCsv(compareLines, outputFileNames, savePath);
                Debug.Log($"对比结果已保存到: {savePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("读取CSV文件时发生错误：" + e.Message);
        }
    }

    private void MarkCompareCsv(string[] compareLines, HashSet<string> outputFileNames, string outputPath)
    {
        var markedLines = new List<string> { compareLines[0] + ",SVN中是否已有资源" };

        foreach (string line in compareLines.Skip(1))
        {
            string trimmedLine = line.Trim().Trim('"');
            if (string.IsNullOrEmpty(trimmedLine))
                continue;

            bool existsInOutput = outputFileNames.Contains(trimmedLine);
            markedLines.Add(trimmedLine + (existsInOutput ? "" : ",未制作"));
        }

        try
        {
            File.WriteAllLines(outputPath, markedLines, new System.Text.UTF8Encoding(true));
        }
        catch (System.Exception e)
        {
            Debug.LogError("写文件时发生错误：" + e.Message);
        }
    }


    //--------------------------------获取资产信息--------------------------------
    private string CalculateMD5(string filename)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            using (var stream = File.OpenRead(filename))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    private List<string> GetAssetPaths(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("目标文件夹路径为空");
            return new List<string>();
        }

        var assetPaths = new List<string>();

        try
        {
            // 确保路径存在
            if (!Directory.Exists(folderPath))
            {
                Debug.LogError($"目录不存在: {folderPath}");
                return assetPaths;
            }

            Debug.Log($"使用IO直接搜索目录: {folderPath}");

            // 获取所有文件（包括子目录）
            var allFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
            Debug.Log($"找到 {allFiles.Length} 个文件");

            // 过滤掉不需要的扩展名
            foreach (var file in allFiles)
            {
                string extension = Path.GetExtension(file);
                if (!excludeExtensions.Contains(extension))
                {
                    assetPaths.Add(file);
                }
            }

            Debug.Log($"过滤后保留 {assetPaths.Count} 个资源路径");
        }
        catch (Exception ex)
        {
            Debug.LogError($"获取资源路径时出错: {ex.Message}");
        }

        return assetPaths;
    }


    //--------------------------------导出CSV--------------------------------
    private void ExportAssetsCsv()
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("请指定目标文件夹路径。");
            return;
        }

        try
        {
            // 如果只有目录路径，自动添加文件名
            if (string.IsNullOrEmpty(outputCSVPath) || Directory.Exists(outputCSVPath))
            {
                string directory = string.IsNullOrEmpty(outputCSVPath) ?
                    Path.GetDirectoryName(Application.dataPath) : outputCSVPath;
                outputCSVPath = Path.Combine(directory, "AssetsInfo.csv");
            }
            // 如果是路径但没有.csv后缀，添加后缀
            else if (!outputCSVPath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                outputCSVPath = outputCSVPath + ".csv";
            }

            Debug.Log($"将导出CSV到: {outputCSVPath}");

            // 确保目录存在
            string outputDirectory = Path.GetDirectoryName(outputCSVPath);
            if (!Directory.Exists(outputDirectory))
            {
                Debug.LogError($"输出目录不存在: {outputDirectory}");
                return;
            }

            var assetPaths = GetAssetPaths(folderPath);
            if (assetPaths.Count == 0)
            {
                Debug.LogWarning("没有找到符合条件的资源，CSV将只包含标题行");
            }

            WriteToCsvFile(outputCSVPath, assetPaths, "文件名,GUID,资源路径,资源类型,哈希值",
            assetPath =>
            {
                try
                {
                    // 尝试获取相对于项目的路径（用于GUID）
                    string unityPath = assetPath;
                    string guid = "";

                    // 如果是绝对路径，尝试转换为相对路径以获取GUID
                    if (Path.IsPathRooted(assetPath))
                    {
                        int assetsIndex = assetPath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
                        if (assetsIndex >= 0)
                        {
                            unityPath = assetPath.Substring(assetsIndex);
                            guid = AssetDatabase.AssetPathToGUID(unityPath);
                        }
                    }

                    // 如果无法获取GUID，生成一个基于路径的唯一标识符
                    if (string.IsNullOrEmpty(guid))
                    {
                        // 使用路径的哈希值作为替代GUID
                        guid = assetPath.GetHashCode().ToString("X8");
                    }

                    string hashValue = File.Exists(assetPath) ? CalculateMD5(assetPath) : string.Empty;

                    return new[]
                    {
                    $"\"{Path.GetFileNameWithoutExtension(assetPath)}\"",
                    $"\"{guid}\"",
                    $"\"{assetPath}\"",
                    $"\"{Path.GetExtension(assetPath).TrimStart('.')}\"",
                    $"\"{hashValue}\""
                    };
                }
                catch (Exception ex)
                {
                    Debug.LogError($"处理资源路径时出错: {assetPath}, 错误: {ex.Message}");
                    return new[] { "错误", "错误", "错误", "错误", "错误" };
                }
            });

            Debug.Log($"资源已导出到: {outputCSVPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"导出CSV时发生错误: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void WriteToCsvFile(string path, IEnumerable<string> assetPaths, string header, System.Func<string, IEnumerable<string>> lineSelector)
    {
        try
        {
            Debug.Log($"开始写入CSV文件: {path}");
            Debug.Log($"资源路径数量: {assetPaths.Count()}");

            using (var writer = new StreamWriter(path, false, new System.Text.UTF8Encoding(true)))
            {
                writer.WriteLine(header);

                int count = 0;
                foreach (var assetPath in assetPaths)
                {
                    try
                    {
                        var lineData = lineSelector(assetPath);
                        string line = string.Join(",", lineData);
                        writer.WriteLine(line);
                        count++;

                        if (count % 100 == 0)
                        {
                            Debug.Log($"已处理 {count} 个资源");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"处理资源时出错: {assetPath}, 错误: {ex.Message}");
                    }
                }

                Debug.Log($"CSV文件写入完成，共写入 {count} 条记录");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"写入CSV文件时出错: {ex.Message}\n{ex.StackTrace}");
        }
    }

    //--------------------------------重命名部分--------------------------------
    private void AddSuffixtoAssets(string prefix, string suffix)
    {
        var selectedAssets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets);
        if (selectedAssets.Length == 0)
        {
            Debug.LogWarning("未选择任何资产。");
            return;
        }

        int successCount = 0;
        AssetDatabase.StartAssetEditing(); // 批量操作开始，提高性能

        foreach (var obj in selectedAssets)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath) || AssetDatabase.IsValidFolder(assetPath))
                continue; // 跳过文件夹和无效路径

            string oldName = Path.GetFileNameWithoutExtension(assetPath);
            string extension;

            if (pathAndExtensionValid(assetPath, out extension))
            {
                string newName = $"{prefix}{oldName}{suffix}";

                // 如果新名称与旧名称相同，则跳过
                if (newName == oldName)
                    continue;

                string error = AssetDatabase.RenameAsset(assetPath, newName);
                if (string.IsNullOrEmpty(error))
                {
                    successCount++;
                }
                else
                {
                    Debug.LogError($"重命名资产 {assetPath} 失败: {error}");
                }
            }
        }

        AssetDatabase.StopAssetEditing(); // 批量操作结束
        AssetDatabase.Refresh();

        if (successCount > 0)
        {
            Debug.Log($"已成功为 {successCount} 个资产添加前缀/后缀。");
        }
        else
        {
            Debug.Log("没有资产需要重命名。");
        }
    }

    public void RenameSelectedObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        foreach (GameObject obj in selectedObjects)
        {
            Transform parentTransform = obj.transform;
            while (parentTransform.parent != null)
            {
                parentTransform = parentTransform.parent;
            }

            string parentName = parentTransform.name;
            int siblingIndex = System.Array.IndexOf(selectedObjects, obj) + 1;

            obj.name = parentName + "_" + siblingIndex;
        }
    }

    // 批量重命名资源
    private void BatchRenameAssets()
    {
        var selectedAssets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets);
        int index = renameStartIndex;
        foreach (var obj in selectedAssets)
        {
            var assetPath = AssetDatabase.GetAssetPath(obj);
            if (AssetDatabase.IsValidFolder(assetPath)) continue;

            string extension;
            if (pathAndExtensionValid(assetPath, out extension))
            {
                string newName = $"{prefix}{renamePattern}{index}{suffix}";
                AssetDatabase.RenameAsset(assetPath, newName);
            }
            index++;
        }
        AssetDatabase.Refresh();
        Debug.Log("已完成选中资源的批量重命名。");
    }

    // 重命名选中的对象为预制体名称
    private void RenameSelectedObjectsToPrefabName()
    {
        var renamedObjects = new Dictionary<string, int>();
        Selection.gameObjects
            .Where(go => PrefabUtility.GetPrefabInstanceStatus(go) == PrefabInstanceStatus.Connected)
            .ToList()
            .ForEach(go =>
            {
                var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(go);
                if (prefabAsset != null)
                {
                    string newName = GetUniqueNameForGameObject(prefabAsset.name, renamedObjects);
                    Undo.RecordObject(go, "Rename GameObject");
                    go.name = newName;
                    UpdateRenamedObjectsDictionary(renamedObjects, newName);
                }
            });
        Undo.FlushUndoRecordObjects();
    }

    // 获取游戏对象的唯一名称
    private string GetUniqueNameForGameObject(string originalName, Dictionary<string, int> renamedObjects)
    {
        int counter = 1;
        if (renamedObjects.TryGetValue(originalName, out counter))
        {
            string uniqueName = $"{originalName}_{counter}";
            while (GameObject.Find(uniqueName) != null)
            {
                counter++;
                uniqueName = $"{originalName}_{counter}";
            }
            renamedObjects[originalName] = counter + 1;
            return uniqueName;
        }
        return originalName;
    }

    // 更新重命名对象的字典
    private void UpdateRenamedObjectsDictionary(Dictionary<string, int> renamedObjects, string name)
    {
        if (renamedObjects.ContainsKey(name))
        {
            renamedObjects[name]++;
        }
        else
        {
            renamedObjects[name] = 1;
        }
    }

    // 在资产名称中查找和替换
    private static void FindAndReplaceInAssetNames(string searchString, string replacementString, string[] searchPaths, bool caseSensitive)
    {
        if (string.IsNullOrEmpty(searchString))
        {
            Debug.LogWarning("搜索字符串为空。");
            return;
        }

        List<string> assetGuids = new List<string>();

        // 检查搜索路径是否为空或者只包含空字符串
        bool hasValidSearchPath = searchPaths != null && searchPaths.Length > 0 &&
                                  searchPaths.Any(p => !string.IsNullOrWhiteSpace(p));

        if (!hasValidSearchPath)
        {
            // 如果没有提供有效的搜索路径，则仅对所选资产进行操作
            var selectedGuids = Selection.assetGUIDs;
            if (selectedGuids == null || selectedGuids.Length == 0)
            {
                Debug.LogWarning("未选择任何资产，且未提供有效的搜索路径。");
                return;
            }
            assetGuids.AddRange(selectedGuids);
            Debug.Log($"将对 {selectedGuids.Length} 个选中的资产进行查找替换操作。");
        }
        else
        {
            // 如果提供了搜索路径，则查找路径中的所有资产
            foreach (var path in searchPaths)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue; // 跳过空路径
                }

                // 检查路径是否存在
                if (!AssetDatabase.IsValidFolder(path))
                {
                    Debug.LogWarning($"搜索路径不存在或不是有效的文件夹: {path}");
                    continue;
                }

                var guids = AssetDatabase.FindAssets("", new[] { path });
                if (guids.Length > 0)
                {
                    assetGuids.AddRange(guids);
                    Debug.Log($"在路径 {path} 中找到 {guids.Length} 个资产。");
                }
                else
                {
                    Debug.LogWarning($"在路径 {path} 中未找到任何资产。");
                }
            }
        }

        if (assetGuids.Count == 0)
        {
            Debug.LogWarning("未找到任何资产进行处理。");
            return;
        }

        // 去除重复的GUID
        assetGuids = assetGuids.Distinct().ToList();
        Debug.Log($"总共将处理 {assetGuids.Count} 个唯一资产。");

        List<string> renameErrors = new List<string>();
        List<string> renamedAssets = new List<string>();

        AssetDatabase.StartAssetEditing();

        foreach (var guid in assetGuids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // 检查是否是有效的资产路径
            if (string.IsNullOrEmpty(assetPath) || !File.Exists(assetPath))
            {
                continue;
            }

            string oldName = Path.GetFileNameWithoutExtension(assetPath);

            if (string.IsNullOrEmpty(oldName))
            {
                Debug.LogError($"无法获取路径 {assetPath} 的文件名。");
                continue;
            }

            string comparisonStr = caseSensitive ? oldName : oldName.ToLower();
            string comparisonSearchStr = caseSensitive ? searchString : searchString.ToLower();

            if (comparisonStr.Contains(comparisonSearchStr))
            {
                string newName = caseSensitive ?
                    oldName.Replace(searchString, replacementString) :
                    ReplaceCaseInsensitive(oldName, searchString, replacementString);

                if (!string.Equals(oldName, newName, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                {
                    string error = AssetDatabase.RenameAsset(assetPath, newName);
                    if (string.IsNullOrEmpty(error))
                    {
                        renamedAssets.Add($"{assetPath} -> {newName}");
                    }
                    else
                    {
                        renameErrors.Add($"无法重命名路径 {assetPath} 的资产。错误: {error}");
                    }
                }
            }
        }

        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();

        // 输出结果
        if (renameErrors.Count > 0)
        {
            Debug.LogError($"重命名过程中发生 {renameErrors.Count} 个错误:\n{string.Join("\n", renameErrors)}");
        }

        if (renamedAssets.Count > 0)
        {
            Debug.Log($"成功重命名 {renamedAssets.Count} 个资产:\n{string.Join("\n", renamedAssets)}");
        }
        else if (renameErrors.Count == 0)
        {
            Debug.Log("未找到需要重命名的资产。");
        }
    }

    // 不区分大小写的替换
    private static string ReplaceCaseInsensitive(string input, string search, string replacement)
    {
        int index = input.ToLower().IndexOf(search.ToLower());
        if (index < 0) return input;

        return input.Remove(index, search.Length).Insert(index, replacement);
    }

    // 验证路径和扩展名
    private bool pathAndExtensionValid(string assetPath, out string extension)
    {
        extension = Path.GetExtension(assetPath);
        return !AssetDatabase.IsValidFolder(assetPath) && !string.IsNullOrEmpty(extension);
    }

    // 重命名资产
    private void RenameAsset(string assetPath, string newName)
    {
        AssetDatabase.RenameAsset(assetPath, newName);
        AssetDatabase.SaveAssets();
    }

    // 重命名为唯一名称
    private void RenameToUniqueName(string assetPath, string baseName)
    {
        int index = 0;
        string extension;
        if (pathAndExtensionValid(assetPath, out extension))
        {
            string newName = baseName + "_" + index;
            string newAssetPath = Path.GetDirectoryName(assetPath) + "/" + newName + extension;
            while (File.Exists(newAssetPath))
            {
                index++;
                newName = baseName + "_" + index;
                newAssetPath = Path.GetDirectoryName(assetPath) + "/" + newName + extension;
            }
            RenameAsset(assetPath, newName);
        }
    }

    // 重命名选中的对象并附加索引
    private void RenameSelectedObjectsWithIndex()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Rename Objects", "Please select at least one object.", "Ok");
            return;
        }

        System.Array.Sort(selectedObjects, (obj1, obj2) => obj1.transform.GetSiblingIndex().CompareTo(obj2.transform.GetSiblingIndex()));

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            Undo.RecordObject(selectedObjects[i], "Rename Objects");
            selectedObjects[i].name += "_" + (i + 1).ToString("D3");
        }
    }

    //--------------------------------前后缀--------------------------------
    private void AddPrefixToSelectedAssets()
    {
        AddSuffixtoAssets(prefix, "");
    }
    private void AddSuffixToSelectedAssets()
    {
        AddSuffixtoAssets("", suffix);
    }

    //--------------------------------UI面板组件--------------------------------

    private string DrawFilePathField(string label, string path, string panelTitle, string extension, bool isDirectory)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(100));
        path = EditorGUILayout.TextField(path);

        if (GUILayout.Button(isDirectory ? "浏览" : "选择", GUILayout.Width(60)))
        {
            string initialPath = string.IsNullOrEmpty(path) ? Application.dataPath : path;

            // 如果路径是相对于 Assets 的路径，转换为完整路径
            if (path.StartsWith("Assets/") || path.StartsWith("Assets\\"))
            {
                initialPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, path);
            }

            string selectedPath;
            if (isDirectory)
            {
                selectedPath = EditorUtility.OpenFolderPanel(panelTitle, initialPath, "");
            }
            else
            {
                // 如果是保存文件的场景
                if (panelTitle.Contains("选择") && string.IsNullOrEmpty(extension))
                {
                    selectedPath = EditorUtility.SaveFilePanel(panelTitle, Path.GetDirectoryName(initialPath),
                        Path.GetFileNameWithoutExtension(initialPath) ?? "output", "csv");
                }
                else
                {
                    selectedPath = EditorUtility.OpenFilePanel(panelTitle, Path.GetDirectoryName(initialPath), extension);
                }
            }

            if (!string.IsNullOrEmpty(selectedPath))
            {
                // 处理路径转换
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    // 转换为相对于项目的路径
                    path = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    Debug.Log($"转换为项目相对路径: {path}");
                }
                else
                {
                    path = selectedPath;
                    Debug.Log($"使用绝对路径: {path}");
                }
            }
        }

        EditorGUILayout.EndHorizontal();
        return path;
    }




}
}