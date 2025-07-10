using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

namespace DYM.ToolBox
{

    public class FbxMatChanger : EditorWindow
    {
        private Vector2 scrollPosition;
        private Material selectedMaterial;

        string changeShaderName = "Universal Render Pipeline/Lit";
        string folderPath = "";
        string shaderName = "Universal Render Pipeline/Lit";


        [MenuItem("美术工具/材质修改工具/材质修改（批量）")]
        public static void ShowWindow()
        {
            GetWindow<FbxMatChanger>("FbxMatChanger");
        }

        private void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("FBX材质修改（支持批量）", EditorStyles.boldLabel);

            selectedMaterial = EditorGUILayout.ObjectField(selectedMaterial, typeof(Material), false) as Material;  // 控制对象字段的宽度

            // 如果用户点击了这个按钮，将触发更新.meta文件的操作
            if (GUILayout.Button("更新材质引用"))
            {
                if (selectedMaterial == null)
                {
                    EditorUtility.DisplayDialog("Error", "Please select a material to assign.", "OK");
                }
                else
                {
                    ModifySelectedMetaFiles(updateExternalObjects: true);
                }
            }

            if (GUILayout.Button("移除所有材质引用"))
            {
                if (EditorUtility.DisplayDialog("确认", "确定要移除所有选中FBX的材质引用吗？", "确定", "取消"))
                {
                    ModifySelectedMetaFiles(removeExternalObjects: true);
                }
            }

            GUILayout.Space(10); // 添加一些空隙
            GUILayout.Label("Shader更改", EditorStyles.boldLabel);


            changeShaderName = GUILayout.TextField(changeShaderName);
            if (GUILayout.Button("批量更改shader为指定名称shader"))
            {
                ChangeShader(changeShaderName);
            }

            GUILayout.Space(10); // 添加一些空隙

            GUILayout.Label("FBX材质生成（支持批量）", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            // GUILayout.Label("选择文件夹路径", GUILayout.Width(150)); // 调整标签宽度
            // folderPath = EditorGUILayout.TextField(folderPath);

            folderPath = DrawFilePathField("处理路径", folderPath, "选择文件夹", "", true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("材质球shader名称", GUILayout.Width(100)); // 调整标签宽度
            shaderName = EditorGUILayout.TextField(shaderName);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("对路径下fbx生成材质球"))
            {
                GenerateMaterials(folderPath, shaderName);
            }




            GUILayout.EndScrollView();
        }

        // private string ()
        // {}

        private void ModifySelectedMetaFiles(bool updateExternalObjects = false, bool removeExternalObjects = false)
        {
            foreach (UnityEngine.Object obj in Selection.objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                string metaPath = AssetDatabase.GetTextMetaFilePathFromAssetPath(assetPath);

                if (updateExternalObjects)
                {
                    UpdateExternalObjectsWithName(metaPath, obj);
                }

                if (removeExternalObjects)
                {
                    RemoveExternalObjects(metaPath);
                }
            }

            AssetDatabase.Refresh();

        }
        private void UpdateExternalObjectsWithName(string metaPath, UnityEngine.Object asset)
        {
            string content = File.ReadAllText(metaPath);
            Debug.Log("Processing meta file: " + metaPath);

            // 从选中的资产获取材质球名称
            string materialName = GetAssetMaterialName(asset);
            Debug.Log("Material name from asset: " + materialName);

            if (string.IsNullOrEmpty(materialName))
            {
                Debug.LogError("无法获取材质名称，跳过此文件");
                return;
            }

            // 获取选中的Material的GUID
            string materialGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(selectedMaterial));
            Debug.Log("Selected material GUID: " + materialGUID);

            if (string.IsNullOrEmpty(materialGUID))
            {
                Debug.LogError("无法获取材质GUID，请确保选择了有效的材质球");
                return;
            }

            // 准备新的材质引用内容
            string newMaterialReference = "  - first:\n" +
                                         "      type: UnityEngine:Material\n" +
                                         "      assembly: UnityEngine.CoreModule\n" +
                                         "      name: " + materialName + "\n" +
                                         "    second: {fileID: 2100000, guid: " + materialGUID + ", type: 2}";

            // 检查是否已有externalObjects字段
            if (content.Contains("externalObjects:"))
            {
                Debug.Log("Found existing externalObjects, updating...");

                // 检查externalObjects是否为空
                if (Regex.IsMatch(content, @"externalObjects:\s*\{\s*\}"))
                {
                    Debug.Log("ExternalObjects is empty, adding new reference...");
                    // 如果externalObjects为空，直接替换整个结构
                    string replacement = "externalObjects:\n" + newMaterialReference;
                    content = Regex.Replace(content, @"externalObjects:\s*\{\s*\}",
                        replacement, RegexOptions.Singleline);
                }
                else
                {
                    Debug.Log("ExternalObjects has content, replacing all material references...");
                    // 如果有内容，完全替换externalObjects块
                    // 找到externalObjects块的开始和结束
                    int startIndex = content.IndexOf("externalObjects:");
                    int braceCount = 0;
                    int endIndex = startIndex;
                    bool foundStart = false;

                    for (int i = startIndex; i < content.Length; i++)
                    {
                        if (content[i] == '{')
                        {
                            braceCount++;
                            foundStart = true;
                        }
                        else if (content[i] == '}')
                        {
                            braceCount--;
                            if (foundStart && braceCount == 0)
                            {
                                endIndex = i;
                                break;
                            }
                        }
                    }

                    if (endIndex > startIndex)
                    {
                        string beforeBlock = content.Substring(0, startIndex);
                        string afterBlock = content.Substring(endIndex + 1);
                        content = beforeBlock + "externalObjects:\n" + newMaterialReference + afterBlock;
                        Debug.Log("Replaced all material references with new one");
                    }
                }
            }
            else
            {
                Debug.Log("No existing externalObjects found, adding new one...");
                // 如果没有externalObjects字段，在materials部分前添加
                string replacement = "  externalObjects:\n" + newMaterialReference + "\n";

                // 找到materials部分的位置
                int materialsIndex = content.IndexOf("  materials:");
                if (materialsIndex != -1)
                {
                    content = content.Insert(materialsIndex, replacement);
                }
                else
                {
                    // 如果找不到materials部分，在文件末尾添加
                    content = content.TrimEnd() + "\n" + replacement;
                }
            }

            File.WriteAllText(metaPath, content);
            Debug.Log("Meta file updated successfully: " + metaPath);
        }
        private string GetAssetMaterialName(UnityEngine.Object asset)
        {
            var assetPath = AssetDatabase.GetAssetPath(asset);
            Debug.Log("Getting material name for asset: " + assetPath);

            var assetImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;

            if (assetImporter != null)
            {
                // Load the model prefab to access its materials
                var assetObj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                if (assetObj != null)
                {
                    var renderers = assetObj.GetComponentsInChildren<Renderer>();
                    Debug.Log("Found " + renderers.Length + " renderers in the asset");

                    foreach (var renderer in renderers)
                    {
                        if (renderer != null && renderer.sharedMaterials.Length > 0)
                        {
                            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                            {
                                var material = renderer.sharedMaterials[i];
                                if (material != null)
                                {
                                    Debug.Log("Found material: " + material.name + " at index " + i);
                                    return material.name;
                                }
                            }
                        }
                    }

                    // 如果没有找到材质，尝试从ModelImporter获取
                    var materialNames = assetImporter.GetExternalObjectMap();
                    foreach (var kvp in materialNames)
                    {
                        if (kvp.Key.type == typeof(Material))
                        {
                            Debug.Log("Found material from importer: " + kvp.Key.name);
                            return kvp.Key.name;
                        }
                    }
                }
            }

            Debug.LogWarning("No material found for asset: " + assetPath);
            return null;
        }


        private static void ChangeShader(string changeShaderName)
        {
            // Define the URP Lit Shader
            Shader urpLitShader = Shader.Find(changeShaderName);

            // Check that the URP Lit Shader exists
            if (urpLitShader == null)
            {
                Debug.LogError("无法查找到对应名称shader");
                return;
            }

            // Get the selected GameObjects in the Editor
            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("没有物体选中，选中物体后再进行修改");
                return;
            }

            int changedMaterialsCount = 0;

            // Loop through each selected GameObject
            foreach (GameObject obj in selectedObjects)
            {
                // Get all renderers in the GameObject (and its children)
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

                foreach (Renderer renderer in renderers)
                {
                    // Loop through all materials in the renderer
                    foreach (Material material in renderer.sharedMaterials)
                    {
                        if (material != null && material.shader != urpLitShader)
                        {
                            Undo.RecordObject(material, "Change Material Shader");
                            material.shader = urpLitShader;
                            changedMaterialsCount++;
                        }
                    }
                }
            }

            Debug.Log($"Changed shader to Universal Render Pipeline/Lit for {changedMaterialsCount} materials.");
        }




        public static void GenerateMaterials(string folderPath, string shaderName)
        {

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError("Folder path is not valid");
                return;
            }

            string[] fbxFiles = Directory.GetFiles(folderPath, "*.fbx", SearchOption.TopDirectoryOnly);

            foreach (string fbxFile in fbxFiles)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fbxFile);

                string baseMapPath = Path.Combine(folderPath, $"{fileNameWithoutExtension}_Abedel.png");
                string colorMaskPath = Path.Combine(folderPath, $"{fileNameWithoutExtension}_ColorMask.png");
                string emissionMaskPath = Path.Combine(folderPath, $"{fileNameWithoutExtension}_EmissionMask.png");

                Material material = new Material(Shader.Find(shaderName))
                {
                    name = fileNameWithoutExtension
                };

                if (File.Exists(baseMapPath))
                {
                    material.SetTexture("_BaseMap", LoadTexture(baseMapPath));
                }

                if (File.Exists(colorMaskPath))
                {
                    material.SetTexture("_ColorMask", LoadTexture(colorMaskPath));
                }

                if (File.Exists(emissionMaskPath))
                {
                    material.SetTexture("_EmissionMask", LoadTexture(emissionMaskPath));
                }

                string materialPath = Path.Combine(folderPath, $"{fileNameWithoutExtension}.mat");
                AssetDatabase.CreateAsset(material, materialPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Material generation finished.");
        }

        private static Texture LoadTexture(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Texture>(path);
        }







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

        private void RemoveExternalObjects(string metaPath)
        {
            Debug.Log("*** NEW VERSION OF RemoveExternalObjects IS RUNNING ***");

            try
            {
                string content = File.ReadAllText(metaPath);
                Debug.Log("File path: " + metaPath);
                Debug.Log("File content length: " + content.Length);
                Debug.Log("Contains externalObjects: " + content.Contains("externalObjects:"));

                // 查找externalObjects的位置
                int externalObjectsIndex = content.IndexOf("externalObjects:");
                Debug.Log("externalObjects index: " + externalObjectsIndex);

                if (externalObjectsIndex != -1)
                {
                    Debug.Log("Found externalObjects, proceeding with removal...");

                    // 简单的字符串替换方法
                    string newContent = content;

                    // 找到externalObjects块的开始和结束
                    int startIndex = externalObjectsIndex;
                    int braceCount = 0;
                    int endIndex = startIndex;
                    bool foundStart = false;

                    for (int i = startIndex; i < content.Length; i++)
                    {
                        if (content[i] == '{')
                        {
                            braceCount++;
                            foundStart = true;
                        }
                        else if (content[i] == '}')
                        {
                            braceCount--;
                            if (foundStart && braceCount == 0)
                            {
                                endIndex = i;
                                break;
                            }
                        }
                    }

                    Debug.Log("externalObjects block: " + startIndex + " to " + endIndex);

                    if (endIndex > startIndex)
                    {
                        string beforeBlock = content.Substring(0, startIndex);
                        string afterBlock = content.Substring(endIndex + 1);
                        newContent = beforeBlock + "externalObjects: {}" + afterBlock;

                        Debug.Log("Block replacement completed");
                    }

                    // 同时设置materialImportMode
                    newContent = Regex.Replace(newContent, "materialImportMode: \\d", "materialImportMode: 1");

                    File.WriteAllText(metaPath, newContent);
                    Debug.Log("File written successfully");

                    // 验证
                    string verifyContent = File.ReadAllText(metaPath);
                    Debug.Log("Verification - contains externalObjects: {}: " + verifyContent.Contains("externalObjects: {}"));
                }
                else
                {
                    Debug.Log("No externalObjects found in file");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error in RemoveExternalObjects: " + e.Message);
            }

            Debug.Log("=== RemoveExternalObjects END ===");
        }

    }
}