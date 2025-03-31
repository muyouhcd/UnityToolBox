// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using UnityEditor;
// using UnityEngine;
// using System;
// using System.Text.RegularExpressions;
// using UnityEditor.ShortcutManagement;
// using Unity.AI.Navigation; // ȷ��������ȷ�������ռ�
// using UnityEditor.Experimental.SceneManagement;
// using UnityEngine.SceneManagement;
// using UnityEditor.SceneManagement;
// using UnityEngine.UI;
// using System.Security.Cryptography;

// public class ToolBox : EditorWindow
// {

//     private Vector2 scrollPosition = Vector2.zero;
//     private string prefix = "";
//     private string suffix = "";
//     private string renamePattern = "NewName_";
//     private int renameStartIndex = 0;
//     private string folderPath = "Assets";
//     private string outputPath = "";
//     private string renameCSVPath = "";
//     private string[] excludeExtensions = { ".meta", ".dll", ".cs" };
//     private string outputCSVPath = "";
//     private string compareCSVPath = "";
//     private int removeCharCount = 0;
//     private string prefabPath = "Assets/Prefabs";
//     private Dictionary<GameObject, GameObject> prefabSourceToPrefabMap = new Dictionary<GameObject, GameObject>();
//     private Vector3 copiedPosition;
//     private Quaternion copiedRotation;
//     private Material selectedMaterial;
//     private static readonly string materialImportModePattern = @"materialImportMode: \d";
//     private static readonly string externalObjectsPattern = @"externalObjects:\s*\{\s*\}";
//     private string searchString = "";
//     private string replaceString = "";
//     private float xSpacing = 1f;
//     private float zSpacing = 1f;
//     private GameObject targetPrefab;  // Ҫ���ҵ�Ƕ��Ԥ����
//     private GameObject replacementPrefab;  // Ҫ�滻��Ԥ����
//     private string pathA;
//     private string pathB;
//     public GameObject assetToReplace; // Ҫ�滻���ʲ�
//     private static List<Vector3> recordedPositions = new List<Vector3>();
//     private string directoryPath = "Assets";
//     //Բ������
//     private GameObject centerObject;
//     private GameObject objectToDuplicate;
//     private int count = 8;
//     private float radius = 5f;
//     private bool autoCalculateRadius = true; // �����Զ�����뾶ѡ��
//     private bool lookAtCenter = true;
//     private string sourceDirectory = "";

//     private int numberOfCopies = 5;
//     private float distance = 1.0f;
//     private bool xAxis = true;
//     private bool yAxis = false;
//     private bool zAxis = false;

//     private const float ScaleFactor = 0.01f;




//     //UI������ã�

//     //����ui��������
//     private Texture2D MakeTex(int width, int height, Color col)
//     {
//         Color[] pix = new Color[width * height];
//         for (int i = 0; i < pix.Length; i++)
//         {
//             pix[i] = col;
//         }
//         Texture2D result = new Texture2D(width, height);
//         result.SetPixels(pix);
//         result.Apply();
//         return result;
//     }

//     private GUIStyle yellowStyle;
//     private GUIStyle greenStyle;
//     private GUIStyle blueStyle;


//     [MenuItem("��������/MiaoTools")]
//     public static void ShowWindow()
//     {
//         GetWindow<ToolBox>("MiaoTools");
//     }

//     private void OnGUI()
//     {
//         //������ͬ����ɫ���
//         if (yellowStyle == null)
//         {
//             yellowStyle = new GUIStyle(GUI.skin.box);
//             yellowStyle.normal.background = MakeTex(2, 2, new Color(1f, 1f, 0f, 1f));  // ����ɫ����
//         }

//         if (greenStyle == null)
//         {
//             greenStyle = new GUIStyle(GUI.skin.box);
//             greenStyle.normal.background = MakeTex(2, 2, new Color(0f, 1f, 0f, 1f));  // ����ɫ����
//         }

//         if (blueStyle == null)
//         {
//             blueStyle = new GUIStyle(GUI.skin.box);
//             blueStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 1f, 1f));  // ����ɫ����
//         }

//         scrollPosition = GUILayout.BeginScrollView(scrollPosition);
//         //��ʼ����--------------------------------------------------


//         GUILayout.Label("������������", EditorStyles.boldLabel);


//         {
//             RenameSelectedObjectsWithIndex();
//         }
//         if (GUILayout.Button("����������������"))
//         {
//             SortGameObjectsByNumber();
//         }

//         //---------------------------------------------------������ʼ

//         // GUILayout.Label("���Ʊ任", EditorStyles.boldLabel);


//         // GUILayout.Space(10); // ���һЩ��϶

//         // GUILayout.Label("Prefab����", EditorStyles.boldLabel);

//         // EditorGUILayout.BeginHorizontal();



//         // prefabPath = EditorGUILayout.TextField("Prefab���·��", prefabPath);
//         // if (GUILayout.Button("���", GUILayout.MaxWidth(100)))
//         // {
//         //     // ���ļ���ѡ��Ի��򣬲���ѡ���·����ֵ��prefabPath
//         //     string path = EditorUtility.OpenFolderPanel("ѡ�����Ŀ¼", prefabPath, "");
//         //     if (!string.IsNullOrEmpty(path))
//         //     {
//         //         // ����ѡ����Ŀ¼��û�е��ȡ����ʱ�Ÿ���·��
//         //         prefabPath = Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "Assets");
//         //     }
//         // }
//         // // EditorGUILayout.EndHorizontal();
//         // if (GUILayout.Button("ת��ΪPrefab"))
//         // {
//         //     GeneratePrefabs();
//         // }


//         //---------------------------------------------------��������
//         GUILayout.EndVertical();

//         //---------------------------------------------------��������


//         GUILayout.Space(10); // ���һЩ��϶

//         GUILayout.Label("Prefab���ڵ������޸�", EditorStyles.boldLabel);
//         GUILayout.BeginHorizontal();
//         GUILayout.Label("����·��", GUILayout.Width(100));
//         folderPath = EditorGUILayout.TextField(folderPath);
//         if (GUILayout.Button("���", GUILayout.Width(100)))
//         {
//             string path = EditorUtility.OpenFolderPanel("Select Folder", folderPath, "");
//             if (!string.IsNullOrEmpty(path))
//             {
//                 folderPath = path.Substring(path.IndexOf("Assets"));
//                 GUI.FocusControl(null);
//             }
//         }
//         // GUILayout.EndHorizontal();


//         EditorGUILayout.BeginHorizontal();
//         if (GUILayout.Button("�滻Root����"))
//         {
//             CleanAndReplacePrefabRoots(folderPath);
//         }
//         if (GUILayout.Button("�滻mesh����"))
//         {
//             ReplaceMeshWithPrefab(folderPath);
//         }
//         EditorGUILayout.EndHorizontal();

//         GUILayout.Space(10); // ���һЩ��϶

//         GUILayout.Label("��������", EditorStyles.boldLabel);
//         EditorGUILayout.BeginHorizontal();
//         // if (GUILayout.Button("������ѡ�ʲ���ͼ"))
//         // {
//         //     ExportTexturesFromSelectedObjects();
//         // }
//         // if (GUILayout.Button("�Զ�����ͬ����ͼ"))
//         // {
//         //     AutolinkSelectedMaterials();
//         // }
//         // if (GUILayout.Button("ɾ����ѡ�ʲ��е�ռλfbx"))
//         // {
//         //     RemoveDuplicatesfbx();
//         // }
//         EditorGUILayout.EndHorizontal();

//         GUILayout.Space(10); // ���һЩ��϶

//         GUILayout.Label("�㼶����", EditorStyles.boldLabel);


//         EditorGUILayout.BeginHorizontal();
//         if (GUILayout.Button("�Ƴ�ѡ���ص���Ʒ"))
//         {
//             RemoveDuplicateObjects();
//         }
//         if (GUILayout.Button("��ȡ�Ӽ�"))
//         {
//             FlattenAndRemoveParent();
//         }
//         if (GUILayout.Button("�Ƴ�����������"))
//         {
//             MoveChildren();
//         }



//         EditorGUILayout.EndHorizontal();

//         EditorGUILayout.BeginHorizontal();
//         if (GUILayout.Button("�Ƴ�����������"))
//         {
//             RemoveEmpty();
//         }
//         if (GUILayout.Button("��������"))
//         {
//             ResetSelectedObjectsParent();
//         }
//         // if (GUILayout.Button("��������"))
//         // {
//         //     DropSelectedObjectsToMesh();
//         // }
//         EditorGUILayout.EndHorizontal();

//         EditorGUILayout.BeginHorizontal();
//         if (GUILayout.Button("����������prefab�ʲ�ԭ�����ײ�����"))
//         {
//             AdjustPivot();
//         }
//         if (GUILayout.Button("ѡ�еײ����Ĵ�������"))
//         {
//             CreateParentAtCenter(Selection.gameObjects);
//         }
//         if (GUILayout.Button("ѡ�еײ����Ĵ�������(�̳����������ת)"))
//         {
//             if (Selection.gameObjects.Length > 0)
//             {
//                 CreateParentWithMaxRotation(Selection.gameObjects);
//             }
//             else
//             {
//                 // No GameObjects are selected
//                 Debug.LogError("You must select at least one GameObject.");
//             }
//         }
//         EditorGUILayout.EndHorizontal();

//         EditorGUILayout.BeginHorizontal();
//         // if (GUILayout.Button("�決��ѡԤ����NavMesh"))
//         // {
//         //     AddNavMeshSurfaceToSelectedPrefabs();

//         // }
//         // if (GUILayout.Button("��������shaderΪURPLit"))
//         // {
//         //     ChangeShader();

//         // }
//         EditorGUILayout.EndHorizontal();

//         GUILayout.Space(10); // ���һЩ��϶

//         // GUILayout.Label("���������ʲ�����", EditorStyles.boldLabel);
//         // EditorGUILayout.BeginHorizontal();
//         // �ṩһ���û����棬�����û�ѡ��һ��Material����
//         // EditorGUILayout.LabelField("�����������", GUILayout.Width(100));  // ���Ʊ�ǩ�Ŀ��
//         // selectedMaterial = EditorGUILayout.ObjectField(selectedMaterial, typeof(Material), false) as Material;  // ���ƶ����ֶεĿ��
//         // // ����û�����������ť...
//         // if (GUILayout.Button("1.���ĵ��뷽ʽ", GUILayout.Width(100)))
//         // {
//         //     ModifySelectedMetaFiles(changeMaterialImportMode: true);
//         // }
//         // // ����û�����������ť������������.meta�ļ��Ĳ���
//         // if (GUILayout.Button("2.���²�������", GUILayout.Width(100)))
//         // {
//         //     if (selectedMaterial == null)
//         //     {
//         //         EditorUtility.DisplayDialog("Error", "Please select a material to assign.", "OK");
//         //     }
//         //     else
//         //     {
//         //         ModifySelectedMetaFiles(updateExternalObjects: true);
//         //     }
//         // }
//         // EditorGUILayout.EndHorizontal();

//         // GUILayout.Space(10); // ���һЩ��϶

//         // GUILayout.Label("������ļ���", EditorStyles.boldLabel);

//         // EditorGUILayout.BeginHorizontal();

//         // EditorGUILayout.LabelField("����·��", GUILayout.Width(100));
//         // directoryPath = EditorGUILayout.TextField(directoryPath);

//         // if (GUILayout.Button("���", GUILayout.Width(100)))
//         // {
//         //     string selectedPath = EditorUtility.OpenFolderPanel("ѡ��·��", directoryPath, "");
//         //     if (!string.IsNullOrEmpty(selectedPath))
//         //     {
//         //         directoryPath = selectedPath.Replace("/", "\\");
//         //     }
//         // }
//         // if (GUILayout.Button("�����ļ���", GUILayout.Width(100)))
//         // {
//         //     Clean(directoryPath);
//         // }
//         // EditorGUILayout.EndHorizontal();

//         // GUILayout.Space(10); // ���һЩ��϶

//         GUILayout.Label("ƽ���ļ��ṹ", EditorStyles.boldLabel);
//         // ��ʼһ��ˮƽ������
//         EditorGUILayout.BeginHorizontal();
//         // ��ǩ���ı��ֶ�������һ����
//         EditorGUILayout.LabelField("Ŀ��Ŀ¼", GUILayout.Width(100));
//         sourceDirectory = EditorGUILayout.TextField(sourceDirectory);
//         // ��������ť
//         if (GUILayout.Button("���", GUILayout.Width(100)))
//         {
//             string selectedPath = EditorUtility.OpenFolderPanel("ѡ��·��", sourceDirectory, "");
//             if (!string.IsNullOrEmpty(selectedPath))
//             {
//                 sourceDirectory = selectedPath.Replace("/", "\\");
//             }
//         }
//         if (GUILayout.Button("ִ��ƽ��", GUILayout.Width(100)))
//         {
//             FlattenDirectory(sourceDirectory);
//         }
//         // ����ˮƽ������
//         EditorGUILayout.EndHorizontal();

//         //�����ظ��ļ�

//         GUILayout.Label("�Ƴ��ظ��ʲ�", EditorStyles.boldLabel);

//         directoryPath = EditorGUILayout.TextField("����·��", directoryPath);

//         if (GUILayout.Button("��ʼ�Ƴ��ظ��ļ�"))
//         {
//             RemoveDuplicates();
//         }




//         //����������ͼ
//         GUILayout.EndScrollView();


//     }

//     private string DrawCsvFilePath(string label, string path)
//     {
//         EditorGUILayout.BeginHorizontal();
//         EditorGUILayout.LabelField(label, GUILayout.Width(100));
//         path = EditorGUILayout.TextField(path);
//         if (GUILayout.Button("���", GUILayout.Width(100)))
//         {
//             path = EditorUtility.OpenFilePanel("ѡ��CSV�ļ�", "", "csv");
//         }
//         EditorGUILayout.EndHorizontal();

//         return path;
//     }

//     private string CalculateMD5(string filename)
//     {
//         using (var md5 = System.Security.Cryptography.MD5.Create())
//         {
//             using (var stream = File.OpenRead(filename))
//             {
//                 var hash = md5.ComputeHash(stream);
//                 return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
//             }
//         }
//     }
//     private void DrawFolderInput()
//     {
//         folderPath = DrawDirectoryField("Ŀ���ļ���", folderPath);
//     }

//     private void DrawOutputInput()
//     {
//         outputPath = DrawFilePath("���CSV·��:", outputPath, "����Excel�ļ�", "AssetsInfo", "csv");
//     }

//     private void DrawRenameCSVInput()
//     {
//         EditorGUILayout.BeginHorizontal();
//         EditorGUILayout.LabelField("������CSV", GUILayout.Width(100));
//         renameCSVPath = EditorGUILayout.TextField(renameCSVPath);
//         if (GUILayout.Button("���", GUILayout.Width(100)))
//         {
//             renameCSVPath = EditorUtility.OpenFilePanel("ѡ��CSV�ļ�", "", "csv");
//         }
//         EditorGUILayout.EndHorizontal();
//         if (GUILayout.Button("��CSV�ļ�������"))
//         {
//             RenameAssets();
//         }
//     }

//     private void DrawPrefixSuffixInput()

//     {
//         prefix = DrawPrefixSuffixOption("ǰ׺:", prefix, AddPrefixToSelectedAssets);
//         suffix = DrawPrefixSuffixOption("��׺:", suffix, AddSuffixToSelectedAssets);
//     }

//     private string DrawPrefixSuffixOption(string name, string fieldValue, System.Action action)
//     {
//         EditorGUILayout.BeginHorizontal();
//         EditorGUILayout.LabelField(name, GUILayout.Width(100));
//         fieldValue = EditorGUILayout.TextField(fieldValue);
//         if (GUILayout.Button("���", GUILayout.Width(100)))
//         {
//             action.Invoke();
//         }
//         EditorGUILayout.EndHorizontal();
//         return fieldValue;
//     }

//     private void DrawRenameOptions()
//     {

//         EditorGUILayout.BeginHorizontal();
//         renamePattern = DrawSimpleLabeledTextField("����������:", renamePattern);
//         EditorGUILayout.LabelField("��ʼ����:", GUILayout.Width(100));  // ���Ʊ�ǩ�Ŀ��
//         renameStartIndex = EditorGUILayout.IntField(renameStartIndex);
//         EditorGUILayout.EndHorizontal();
//     }

//     private string DrawDirectoryField(string label, string path)
//     {
//         EditorGUILayout.BeginHorizontal();
//         EditorGUILayout.LabelField(label, GUILayout.Width(100));
//         path = EditorGUILayout.TextField(path);
//         if (GUILayout.Button("���", GUILayout.Width(100)))
//         {
//             path = EditorUtility.OpenFolderPanel("ѡ���ļ���", Application.dataPath, "");
//             if (!string.IsNullOrEmpty(path))
//             {
//                 path = DataPathToAssetPath(path);
//             }
//         }
//         EditorGUILayout.EndHorizontal();
//         return path;
//     }

//     private string DrawFilePath(string label, string path, string title, string defaultName, string extension)
//     {
//         EditorGUILayout.BeginHorizontal();
//         EditorGUILayout.LabelField(label, GUILayout.Width(100));
//         path = EditorGUILayout.TextField(path);
//         if (GUILayout.Button("���", GUILayout.Width(100)))
//         {
//             path = EditorUtility.SaveFilePanel(title, "", defaultName, extension);
//         }
//         EditorGUILayout.EndHorizontal();
//         return path;
//     }

//     private static string DataPathToAssetPath(string path)
//     {
//         return "Assets" + path.Substring(Application.dataPath.Length);
//     }

//     private void RenameSelectedAssets(string prefix, string suffix, int index)
//     {
//         var selectedAssets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets);
//         foreach (var obj in selectedAssets)
//         {
//             string assetPath = AssetDatabase.GetAssetPath(obj);
//             if (AssetDatabase.IsValidFolder(assetPath)) continue;

//             string directory = Path.GetDirectoryName(assetPath);
//             string oldName = Path.GetFileNameWithoutExtension(assetPath);
//             string extension;

//             if (pathAndExtensionValid(assetPath, out extension))
//             {
//                 string newName = $"{prefix}{oldName}{suffix}";
//                 RenameAsset(assetPath, newName);
//             }
//         }
//         AssetDatabase.Refresh();
//         Debug.Log("�Ѹ�ѡ����Դ���ǰ׺/��׺��");
//     }

//     private void AddPrefixToSelectedAssets()
//     {
//         RenameSelectedAssets(prefix, "", renameStartIndex);
//     }

//     private void AddSuffixToSelectedAssets()
//     {
//         RenameSelectedAssets("", suffix, renameStartIndex);
//     }

//     private void Export()
//     {
//         if (string.IsNullOrEmpty(outputPath))
//         {
//             Debug.LogError("��ָ�����·����");
//             return;
//         }

//         var assetPaths = GetAssetPaths(folderPath);
//         WriteToCsvFile(outputPath, assetPaths, "�ļ���,GUID,��Դ·��,��Դ����,��ϣֵ",
//          assetPath =>
//          {
//              string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
//              string hashValue = File.Exists(fullPath) ? CalculateMD5(fullPath) : string.Empty;
//              return new[]
//              {
//                 Path.GetFileNameWithoutExtension(assetPath),
//                 AssetDatabase.AssetPathToGUID(assetPath),
//                 assetPath,
//                 Path.GetExtension(assetPath).TrimStart('.'),
//                 hashValue
//              };
//          });

//         Debug.Log($"��Դ�ѵ�����: {outputPath}");
//     }

//     private List<string> GetAssetPaths(string folderPath)
//     {
//         var assetPaths = AssetDatabase.FindAssets(string.Empty, new[] { folderPath })
//             .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
//             .Where(assetPath => !excludeExtensions.Contains(Path.GetExtension(assetPath)) && File.Exists(assetPath))
//             .ToList();

//         return assetPaths;
//     }

//     //---------------------------------------Rename-------------------------------------------------------------

//     private void ReplaceTemplate()
//     {
//         // Check if any GameObjects are selected
//         if (Selection.gameObjects.Length == 0)
//         {
//             Debug.LogWarning("No GameObjects selected. Please select one or more GameObjects.");
//             return;
//         }

//         foreach (GameObject selectedObj in Selection.gameObjects)
//         {
//             string newName = selectedObj.name;
//             if (newName.Contains("TEMPLATE"))
//             {
//                 Transform topLevelParent = selectedObj.transform;

//                 // Find the top-level parent (the root in the hierarchy)
//                 while (topLevelParent.parent != null)
//                 {
//                     topLevelParent = topLevelParent.parent;
//                 }

//                 // Replace "TEMPLATE" with the top-level parent's name
//                 newName = newName.Replace("TEMPLATE", topLevelParent.name);

//                 // Rename the selected object
//                 selectedObj.name = newName;
//                 Debug.Log($"Renamed {selectedObj.name} to {newName}");
//             }
//         }
//     }

//     [SerializeField] private string baseName = "BaseName";

//     public void RenameSelectedObjects()
//     {
//         GameObject[] selectedObjects = Selection.gameObjects;

//         foreach (GameObject obj in selectedObjects)
//         {
//             Transform parentTransform = obj.transform;
//             while (parentTransform.parent != null)
//             {
//                 parentTransform = parentTransform.parent;
//             }

//             string parentName = parentTransform.name;
//             int siblingIndex = System.Array.IndexOf(selectedObjects, obj) + 1;

//             obj.name = parentName + "_" + siblingIndex;
//         }
//     }

//     private void RenameAssets()
//     {
//         if (string.IsNullOrEmpty(renameCSVPath))
//         {
//             Debug.LogError("��ָ��������CSV�ļ���·����");
//             return;
//         }

//         var guidToNewName = ParseCsvForRename(renameCSVPath);
//         ProcessRename(guidToNewName);
//         AssetDatabase.Refresh();
//         Debug.Log("�Ѹ���CSV�ļ���������Դ��");
//     }

//     private void BatchRenameAssets()
//     {
//         var selectedAssets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets);
//         int index = renameStartIndex;
//         foreach (var obj in selectedAssets)
//         {
//             var assetPath = AssetDatabase.GetAssetPath(obj);
//             if (AssetDatabase.IsValidFolder(assetPath)) continue;

//             string extension;
//             if (pathAndExtensionValid(assetPath, out extension))
//             {
//                 string newName = $"{prefix}{renamePattern}{index}{suffix}";
//                 string newAssetPath = AssetDatabase.RenameAsset(assetPath, newName);
//             }
//             index++;
//         }
//         AssetDatabase.Refresh();
//         Debug.Log("�����ѡ����Դ��������������");
//     }

//     private void RenameToGrandparentFolderName()
//     {
//         var selectedAssets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets);
//         foreach (var obj in selectedAssets)
//         {
//             var assetPath = AssetDatabase.GetAssetPath(obj);
//             if (AssetDatabase.IsValidFolder(assetPath)) continue;

//             var grandparentFolderName = GetGrandparentFolderName(assetPath);
//             if (grandparentFolderName != null)
//             {
//                 RenameToUniqueName(assetPath, grandparentFolderName);
//             }
//         }
//         AssetDatabase.Refresh();
//         Debug.Log("�ѽ���ѡ�ʲ�����Ϊ�����ϼ��ļ��е�����");
//     }

//     private string GetGrandparentFolderName(string assetPath)
//     {
//         var directoryInfo = new DirectoryInfo(assetPath).Parent?.Parent;
//         return directoryInfo?.Name;
//     }

//     private void RenameSelectedObjectsToPrefabName()
//     {
//         var renamedObjects = new Dictionary<string, int>();
//         Selection.gameObjects
//             .Where(go => PrefabUtility.GetPrefabInstanceStatus(go) == PrefabInstanceStatus.Connected)
//             .ToList()
//             .ForEach(go =>
//             {
//                 var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(go);
//                 if (prefabAsset != null)
//                 {
//                     string newName = GetUniqueNameForGameObject(prefabAsset.name, renamedObjects);
//                     Undo.RecordObject(go, "Rename GameObject");
//                     go.name = newName;
//                     UpdateRenamedObjectsDictionary(renamedObjects, newName);
//                 }
//             });
//         Undo.FlushUndoRecordObjects();
//     }

//     private string GetUniqueNameForGameObject(string originalName, Dictionary<string, int> renamedObjects)
//     {
//         int counter = 1;
//         if (renamedObjects.TryGetValue(originalName, out counter))
//         {
//             string uniqueName = $"{originalName}_{counter}";
//             while (GameObject.Find(uniqueName) != null)
//             {
//                 counter++;
//                 uniqueName = $"{originalName}_{counter}";
//             }
//             renamedObjects[originalName] = counter + 1;
//             return uniqueName;
//         }
//         return originalName;
//     }

//     private void UpdateRenamedObjectsDictionary(Dictionary<string, int> renamedObjects, string name)
//     {
//         if (renamedObjects.ContainsKey(name))
//         {
//             renamedObjects[name]++;
//         }
//         else
//         {
//             renamedObjects[name] = 1;
//         }
//     }

//     private static void FindAndReplaceInAssetNames(string searchString, string replacementString = "")
//     {
//         var selectedObjects = Selection.objects;
//         if (selectedObjects.Length == 0)
//         {
//             Debug.LogWarning("No objects selected.");
//             return;
//         }

//         if (string.IsNullOrEmpty(searchString))
//         {
//             Debug.LogWarning("Search string is empty.");
//             return;
//         }

//         List<string> renameErrors = new List<string>();

//         AssetDatabase.StartAssetEditing();
//         foreach (var obj in selectedObjects)
//         {
//             var assetPath = AssetDatabase.GetAssetPath(obj);
//             string directory = Path.GetDirectoryName(assetPath);
//             string oldName = Path.GetFileNameWithoutExtension(assetPath);
//             string extension = Path.GetExtension(assetPath);

//             if (string.IsNullOrEmpty(oldName))
//             {
//                 Debug.LogError($"Failed to get file name without extension for asset at path: {assetPath}");
//                 continue;
//             }

//             string newName = oldName.Replace(searchString, replacementString);
//             string error = AssetDatabase.RenameAsset(assetPath, newName);
//             if (!string.IsNullOrEmpty(error))
//             {
//                 renameErrors.Add($"Failed to rename asset at path: {assetPath}. Error: {error}");
//             }
//         }
//         AssetDatabase.StopAssetEditing();

//         AssetDatabase.Refresh();
//         if (renameErrors.Count > 0)
//         {
//             Debug.LogError(string.Join("\n", renameErrors));
//         }
//         else
//         {
//             Debug.Log("Find and replace completed for selected assets.");
//         }
//     }


//     //---------------------------------------Rename Done-------------------------------------------------------------

//     private void CompareAndMarkCSV()
//     {
//         if (string.IsNullOrEmpty(compareCSVPath) || string.IsNullOrEmpty(outputCSVPath))
//         {
//             Debug.LogError("CSV�ļ�·������Ϊ�ա�");
//             return;
//         }

//         try
//         {
//             // ��ȡ���CSV
//             var outputFileNames = new HashSet<string>(File.ReadAllLines(outputCSVPath).Skip(1).Select(line => line.Split(',')[0].Trim('"')));

//             // ��ȡ����CSV
//             var compareLines = File.ReadAllLines(compareCSVPath);

//             // ѯ���û����·��
//             var savePath = EditorUtility.SaveFilePanel("�������ļ�", "", "MarkedCompareCSV", "csv");
//             if (!string.IsNullOrEmpty(savePath))
//             {
//                 // ���ú����Ƚ�����CSV�ļ��������ǹ��Ķ���CSV�ļ���ѡ��·��
//                 MarkCompareCsv(compareLines, outputFileNames, savePath);
//                 Debug.Log($"�ԱȽ���ѱ��浽: {savePath}");
//             }
//         }
//         catch (Exception e)
//         {
//             Debug.LogError("��ȡCSV�ļ�ʱ��������" + e.Message);
//         }
//     }

//     private void MarkCompareCsv(string[] compareLines, HashSet<string> outputFileNames, string outputPath)
//     {
//         // ����б���
//         var markedLines = new List<string> { compareLines[0] + ",SVN���Ƿ�������Դ" };

//         // ����CSV��
//         foreach (string line in compareLines.Skip(1)) // ����������
//         {
//             // �Ƴ����е�˫���źͿհף���������Ķ����Ƴ�
//             string trimmedLine = line.Trim().Trim('"');
//             // ��������
//             if (string.IsNullOrEmpty(trimmedLine))
//                 continue;

//             // ������CSV�е��ļ����Ƿ�������ļ���
//             bool existsInOutput = outputFileNames.Contains(trimmedLine);
//             // ���ļ�����ӵ����У�����ĩβ����ʵ��ı��
//             markedLines.Add(trimmedLine + (existsInOutput ? "" : ",δ����"));
//         }

//         // ʹ�ô���BOM��UTF-8����д���ļ���ע�⴦���쳣
//         try
//         {
//             File.WriteAllLines(outputPath, markedLines, new System.Text.UTF8Encoding(true));
//         }
//         catch (Exception e)
//         {
//             Debug.LogError("д�ļ�ʱ��������" + e.Message);
//         }
//     }

//     private bool pathAndExtensionValid(string assetPath, out string extension)
//     {
//         extension = Path.GetExtension(assetPath);
//         return !AssetDatabase.IsValidFolder(assetPath) && !string.IsNullOrEmpty(extension);
//     }

//     private void RenameAsset(string assetPath, string newName)
//     {
//         AssetDatabase.RenameAsset(assetPath, newName);
//         AssetDatabase.SaveAssets();
//     }

//     private void RenameToUniqueName(string assetPath, string baseName)
//     {
//         int index = 0;
//         string extension;
//         if (pathAndExtensionValid(assetPath, out extension))
//         {
//             string newName = baseName + "_" + index;
//             string newAssetPath = Path.GetDirectoryName(assetPath) + "/" + newName + extension;
//             while (File.Exists(newAssetPath))
//             {
//                 index++;
//                 newName = baseName + "_" + index;
//                 newAssetPath = Path.GetDirectoryName(assetPath) + "/" + newName + extension;
//             }
//             RenameAsset(assetPath, newName);
//         }
//     }

//     private Dictionary<string, string> ParseCsvForRename(string csvPath)
//     {
//         var guidToNewName = new Dictionary<string, string>();
//         var lines = File.ReadAllLines(csvPath);
//         lines.Skip(1) // Skip header
//             .Select(line => line.Split(','))
//             .ToList()
//             .ForEach(tokens =>
//             {
//                 if (tokens.Length >= 3)
//                 {
//                     var newName = tokens[0].Trim('"').Trim();
//                     var guid = tokens[1].Trim('"').Trim();
//                     if (!string.IsNullOrEmpty(newName) && !string.IsNullOrEmpty(guid))
//                     {
//                         guidToNewName[guid] = newName;
//                     }
//                 }
//             });
//         return guidToNewName;
//     }

//     private void ProcessRename(Dictionary<string, string> guidToNewName)
//     {
//         foreach (var kvp in guidToNewName)
//         {
//             var guid = kvp.Key;
//             var newName = kvp.Value;
//             var assetPath = AssetDatabase.GUIDToAssetPath(guid);
//             if (!AssetDatabase.IsValidFolder(assetPath))
//             {
//                 RenameAsset(assetPath, newName);
//             }
//         }
//     }

//     private void WriteToCsvFile(string path, IEnumerable<string> assetPaths, string header, Func<string, IEnumerable<string>> lineSelector)
//     {
//         using (var writer = new StreamWriter(path, false, new System.Text.UTF8Encoding(true)))
//         {
//             writer.WriteLine(header);
//             assetPaths.ToList().ForEach(assetPath => writer.WriteLine(string.Join(",", lineSelector(assetPath))));
//         }
//     }

//     private void CompareCsvFileAndMark(List<string> compareLines, HashSet<string> outputFileNames)
//     {
//         //int artColumnIndex = -1;
//         var resultLines = new List<string> { compareLines[0] + ",״̬" };
//         compareLines.Skip(1).ToList().ForEach(line =>
//         {
//             var tokens = line.Split(',');
//             string fileName = tokens.Length > 0 ? tokens[0].Trim('"') : "None";
//             string mark = outputFileNames.Contains(fileName) ? "" : "δ����";
//             resultLines.Add(line + $",{mark}");
//         });
//         File.WriteAllLines(Path.ChangeExtension(compareCSVPath, "_marked.csv"), resultLines);
//     }

//     private string DrawSimpleLabeledTextField(string label, string value)
//     {
//         EditorGUILayout.BeginHorizontal();
//         EditorGUILayout.LabelField(label, GUILayout.Width(100));
//         value = EditorGUILayout.TextField(value);
//         EditorGUILayout.EndHorizontal();
//         return value;
//     }
//     private void RenameSelectedObjectsWithIndex()
//     {
//         GameObject[] selectedObjects = Selection.gameObjects;

//         if (selectedObjects.Length == 0)
//         {
//             EditorUtility.DisplayDialog("Rename Objects", "Please select at least one object.", "Ok");
//             return;
//         }

//         System.Array.Sort(selectedObjects, (obj1, obj2) => obj1.transform.GetSiblingIndex().CompareTo(obj2.transform.GetSiblingIndex()));

//         for (int i = 0; i < selectedObjects.Length; i++)
//         {
//             Undo.RecordObject(selectedObjects[i], "Rename Objects");
//             selectedObjects[i].name += "_" + (i + 1).ToString("D3");
//         }
//     }

//     private static void RemoveDuplicateObjects()
//     {
//         GameObject[] selectedObjects = Selection.gameObjects;
//         HashSet<GameObject> uniqueObjects = new HashSet<GameObject>();
//         Dictionary<string, GameObject> objectLookup = new Dictionary<string, GameObject>();

//         foreach (GameObject selected in selectedObjects)
//         {
//             string combinedInfo = $"{PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(selected)}_{selected.transform.position}";
//             if (!objectLookup.ContainsKey(combinedInfo))
//             {
//                 objectLookup.Add(combinedInfo, selected);
//                 uniqueObjects.Add(selected);
//             }
//             else
//             {
//                 DestroyImmediate(selected);
//             }
//         }

//         Debug.Log(uniqueObjects.Count + " unique objects kept.");
//         Debug.Log((selectedObjects.Length - uniqueObjects.Count) + " duplicate objects removed.");
//     }

//     private static void CheckForDuplicates()
//     {
//         GameObject[] selectedObjects = Selection.gameObjects;
//         HashSet<string> objectInfo = new HashSet<string>();

//         foreach (GameObject selected in selectedObjects)
//         {
//             string combinedInfo = $"{PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(selected)}_{selected.transform.position}";
//             if (objectInfo.Contains(combinedInfo))
//             {
//                 Debug.LogWarning("Duplicates detected!");
//                 return;
//             }
//             else
//             {
//                 objectInfo.Add(combinedInfo);
//             }
//         }

//         Debug.Log("No duplicates found.");
//     }

//     private int ExtractNumber(string name)
//     {
//         var match = Regex.Match(name, @"\d+");
//         return match.Success ? int.Parse(match.Value) : 0;
//     }

//     private void CopyTransform()
//     {
//         if (Selection.activeTransform != null)
//         {
//             copiedPosition = Selection.activeTransform.position; // ��ȷ�ġ�λ�ø�ֵ�� Vector3
//             copiedRotation = Selection.activeTransform.rotation; // ��ȷ�ġ���ת��ֵ�� Quaternion
//             Debug.Log("World Position & Rotation Copied");
//         }
//         else
//         {
//             Debug.Log("No object selected to copy transform");
//         }
//     }

//     private void PasteTransform()
//     {
//         if (Selection.activeTransform != null)
//         {
//             Undo.RecordObject(Selection.activeTransform, "Paste World Position & Rotation");

//             Selection.activeTransform.position = copiedPosition; // ��ȷ�ġ�Vector3 ��ֵ��λ��
//             Selection.activeTransform.rotation = copiedRotation; // ��ȷ�ġ�Quaternion ��ֵ����ת
//             Debug.Log("World Position & Rotation Pasted to selected object");
//         }
//         else
//         {
//             Debug.Log("No object selected to paste transform");
//         }
//     }

//     public class NaturalComparer : IComparer<string>
//     {
//         private static readonly Regex _regex = new Regex(@"(\d+)|(\D+)", RegexOptions.Compiled);

//         public int Compare(string x, string y)
//         {
//             // If both strings are identical or either is null, no need to compare further.
//             if (string.Equals(x, y, StringComparison.OrdinalIgnoreCase))
//                 return 0;
//             if (x == null) return -1;
//             if (y == null) return 1;

//             // Compare each numerical or textual chunk one by one from both strings.
//             var xMatches = _regex.Matches(x);
//             var yMatches = _regex.Matches(y);
//             for (int i = 0; i < Math.Min(xMatches.Count, yMatches.Count); i++)
//             {
//                 var xPart = xMatches[i].Value;
//                 var yPart = yMatches[i].Value;

//                 // If both are numeric, compare numerically.
//                 if (int.TryParse(xPart, out int xNum) && int.TryParse(yPart, out int yNum))
//                 {
//                     int numCompare = xNum.CompareTo(yNum);
//                     if (numCompare != 0)
//                         return numCompare;
//                 }
//                 else // If any or both are non-numeric, compare as text.
//                 {
//                     int stringCompare = string.Compare(xPart, yPart, StringComparison.OrdinalIgnoreCase);
//                     if (stringCompare != 0)
//                         return stringCompare;
//                 }
//             }

//             // If all parts matched but one string has additional chunks, the shorter string goes first.
//             return xMatches.Count.CompareTo(yMatches.Count);
//         }
//     }

//     void SortGameObjectsByNumber()
//     {
//         if (Selection.transforms.Length > 0)
//         {
//             NaturalComparer nc = new NaturalComparer();

//             // Sorting Transforms directly by comparing their names using the NaturalComparer
//             var sortedTransforms = Selection.transforms
//                 .OrderBy(t => t.gameObject.name, nc)
//                 .ToArray();

//             for (int i = 0; i < sortedTransforms.Length; i++)
//             {
//                 sortedTransforms[i].SetSiblingIndex(i);
//             }

//             Debug.Log("Game Objects sorted by natural order in their names.");
//         }
//         else
//         {
//             Debug.Log("No game objects selected to sort.");
//         }
//     }

//     private void DrawRemoveCharactersOptions()
//     {
//         GUILayout.BeginHorizontal();
//         GUILayout.Label("�ַ�����:", GUILayout.Width(100));
//         int charCount = EditorGUILayout.IntField(removeCharCount);
//         GUILayout.EndHorizontal();

//         // Update field after validation
//         removeCharCount = Mathf.Max(0, charCount); // Ensure non-negative character count
//     }

//     private void RemoveCharactersFromAssetNames(bool start)
//     {
//         var selectedObjects = Selection.objects;
//         if (selectedObjects.Length == 0)
//         {
//             Debug.LogWarning("No objects selected.");
//             return;
//         }

//         List<string> renameErrors = new List<string>();

//         AssetDatabase.StartAssetEditing(); // ��ʼ�����༭
//         foreach (var obj in selectedObjects)
//         {
//             var assetPath = AssetDatabase.GetAssetPath(obj);
//             string directory = Path.GetDirectoryName(assetPath);
//             string oldName = Path.GetFileNameWithoutExtension(assetPath);
//             string extension = Path.GetExtension(assetPath);

//             if (string.IsNullOrEmpty(oldName))
//             {
//                 Debug.LogError($"Failed to get file name without extension for asset at path: {assetPath}");
//                 continue;
//             }

//             string newName;
//             if (start)
//             {
//                 newName = oldName.Length <= removeCharCount ? "" : oldName.Substring(removeCharCount);
//             }
//             else
//             {
//                 int charsToRemove = Mathf.Min(oldName.Length, removeCharCount);
//                 newName = oldName.Substring(0, oldName.Length - charsToRemove);
//             }

//             string newAssetPath = Path.Combine(directory, newName + extension);
//             string error = AssetDatabase.RenameAsset(assetPath, newName);
//             if (!string.IsNullOrEmpty(error))
//             {
//                 renameErrors.Add($"Failed to rename asset at path: {assetPath}. Error: {error}");
//             }
//         }
//         AssetDatabase.StopAssetEditing(); // ���������༭

//         AssetDatabase.Refresh();
//         if (renameErrors.Count > 0)
//         {
//             Debug.LogError(string.Join("\n", renameErrors));
//         }
//         else
//         {
//             Debug.Log("Characters removed from selected assets' names.");
//         }
//     }

//     private static void ResetSelectedObjectsParent()
//     {
//         foreach (GameObject selectedObj in Selection.gameObjects)
//         {
//             Transform parentTransform = selectedObj.transform.parent;

//             // Continue only if there is a parent
//             if (parentTransform != null)
//             {
//                 // Keep track of all siblings including the selected object
//                 int siblingCount = parentTransform.childCount;
//                 Transform[] siblings = new Transform[siblingCount];
//                 Vector3[] siblingsWorldPosition = new Vector3[siblingCount];
//                 Quaternion[] siblingsWorldRotation = new Quaternion[siblingCount];

//                 // Store siblings and their current world position and rotation
//                 for (int i = 0; i < siblingCount; i++)
//                 {
//                     siblings[i] = parentTransform.GetChild(i);
//                     siblingsWorldPosition[i] = siblings[i].position;
//                     siblingsWorldRotation[i] = siblings[i].rotation;
//                 }

//                 // Detach all the siblings from the parent
//                 parentTransform.DetachChildren();

//                 // Reset the parent's transform
//                 parentTransform.position = Vector3.zero;
//                 parentTransform.rotation = Quaternion.identity;

//                 // Re-parent the siblings and restore their world position and rotation
//                 for (int i = 0; i < siblings.Length; i++)
//                 {
//                     siblings[i].SetParent(parentTransform);
//                     siblings[i].position = siblingsWorldPosition[i];
//                     siblings[i].rotation = siblingsWorldRotation[i];
//                 }

//                 // Record the operation for Undo system
//                 Undo.RegisterCompleteObjectUndo(parentTransform.gameObject, "Reset Parent Transform");
//             }
//         }
//     }

//     private static void ResetParentAndMoveToChildrenBottomCenter()
//     {
//         // �洢���ж�����������
//         HashSet<Transform> topParentTransforms = new HashSet<Transform>();

//         // ��������ѡ�е���Ϸ����
//         foreach (GameObject selectedObj in Selection.gameObjects)
//         {
//             Transform topParentTransform = GetTopParentTransform(selectedObj.transform);
//             topParentTransforms.Add(topParentTransform);
//         }

//         // ͳһ����ÿһ��������������
//         foreach (Transform topParentTransform in topParentTransforms)
//         {
//             ProcessParentTransform(topParentTransform);
//         }
//     }

//     private static void ProcessParentTransform(Transform topParentTransform)
//     {
//         // Get all child Transforms
//         Transform[] allChildren = topParentTransform.GetComponentsInChildren<Transform>();
//         Vector3[] childrenWorldPositions = new Vector3[allChildren.Length];
//         Quaternion[] childrenWorldRotations = new Quaternion[allChildren.Length];

//         // Store the children's world position and rotation
//         for (int i = 0; i < allChildren.Length; i++)
//         {
//             childrenWorldPositions[i] = allChildren[i].position;
//             childrenWorldRotations[i] = allChildren[i].rotation;
//         }

//         // Calculate the bounding box of all the children
//         Bounds bounds = CalculateBounds(allChildren);

//         // Detach children
//         topParentTransform.DetachChildren();

//         // Move the top parent to the bottom center of the bounding box
//         Vector3 newParentPosition = bounds.min;
//         newParentPosition.y = bounds.min.y;
//         topParentTransform.position = newParentPosition;
//         topParentTransform.rotation = Quaternion.identity;

//         // Re-parent the children and restore their world position and rotation
//         for (int i = 0; i < allChildren.Length; i++)
//         {
//             allChildren[i].SetParent(topParentTransform);
//             allChildren[i].position = childrenWorldPositions[i];
//             allChildren[i].rotation = childrenWorldRotations[i];
//         }

//         // Register the operation for Undo system
//         Undo.RegisterCompleteObjectUndo(topParentTransform.gameObject, "Move Parent to Children Bottom Center");
//     }

//     private static Transform GetTopParentTransform(Transform transform)
//     {
//         while (transform.parent != null)
//         {
//             transform = transform.parent;
//         }
//         return transform;
//     }

//     private static Bounds CalculateBounds(Transform[] transforms)
//     {
//         Bounds bounds = new Bounds(transforms[0].position, Vector3.zero);
//         foreach (Transform trans in transforms)
//         {
//             bounds.Encapsulate(trans.position);
//         }
//         return bounds;
//     }

//     // private static void ExportTexturesFromSelectedObjects()
//     // {
//     //     string exportPath = EditorUtility.SaveFolderPanel("Export Textures", "", "");

//     //     if (string.IsNullOrEmpty(exportPath))
//     //     {
//     //         return;
//     //     }

//     //     HashSet<Texture2D> exportedTextures = new HashSet<Texture2D>();

//     //     UnityEngine.Object[] selectedAssets = Selection.objects;

//     //     foreach (UnityEngine.Object obj in selectedAssets)
//     //     {
//     //         if (obj is GameObject go)
//     //         {
//     //             string assetPath = AssetDatabase.GetAssetPath(go);

//     //             if (string.IsNullOrEmpty(assetPath))
//     //             {
//     //                 ExportTexturesFromSceneObject(go, exportPath, exportedTextures);
//     //             }
//     //             else if (AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) is GameObject prefab)
//     //             {
//     //                 ExportTexturesFromPrefab(prefab, exportPath, exportedTextures);
//     //             }
//     //         }
//     //     }

//     //     AssetDatabase.Refresh();
//     //     Debug.Log("Texture export completed.");
//     // }

//     // private static void ExportTexturesFromPrefab(GameObject prefab, string exportPath, HashSet<Texture2D> exportedTextures)
//     // {
//     //     Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();

//     //     foreach (Renderer renderer in renderers)
//     //     {
//     //         Material[] materials = renderer.sharedMaterials;

//     //         foreach (Material material in materials)
//     //         {
//     //             if (material.mainTexture is Texture2D texture && !exportedTextures.Contains(texture))
//     //             {
//     //                 ProcessTextureForExport(texture, exportPath, renderer.gameObject.name, exportedTextures);
//     //             }
//     //         }
//     //     }
//     // }

//     // private static void ExportTexturesFromSceneObject(GameObject sceneObject, string exportPath, HashSet<Texture2D> exportedTextures)
//     // {
//     //     Renderer[] renderers = sceneObject.GetComponentsInChildren<Renderer>();

//     //     foreach (Renderer renderer in renderers)
//     //     {
//     //         Material[] materials = renderer.sharedMaterials;

//     //         foreach (Material material in materials)
//     //         {
//     //             if (material.mainTexture is Texture2D texture && !exportedTextures.Contains(texture))
//     //             {
//     //                 ProcessTextureForExport(texture, exportPath, renderer.gameObject.name, exportedTextures);
//     //             }
//     //         }
//     //     }
//     // }

//     // private static void ProcessTextureForExport(Texture2D texture, string exportPath, string objectName, HashSet<Texture2D> exportedTextures)
//     // {
//     //     string texturePath = AssetDatabase.GetAssetPath(texture);
//     //     if (texturePath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
//     //     {
//     //         // If the texture is already a PNG, copy the file directly
//     //         string fileName = $"{objectName}_{Path.GetFileName(texturePath)}";
//     //         string exportFilePath = Path.Combine(exportPath, fileName);
//     //         File.Copy(texturePath, exportFilePath, overwrite: true);
//     //         Debug.Log($"Copied original PNG texture {texture.name} to {exportFilePath}");
//     //     }
//     //     else
//     //     {
//     //         TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;

//     //         if (importer != null)
//     //         {
//     //             bool wasReadable = importer.isReadable;

//     //             TextureImporterPlatformSettings platformSettings = importer.GetDefaultPlatformTextureSettings();
//     //             TextureImporterFormat originalFormat = platformSettings.format;
//     //             TextureImporterCompression originalCompression = importer.textureCompression;

//     //             // Set texture as readable and uncompressed (RGBA32) for export
//     //             importer.isReadable = true;
//     //             platformSettings.format = TextureImporterFormat.RGBA32;
//     //             importer.SetPlatformTextureSettings(platformSettings);

//     //             importer.textureCompression = TextureImporterCompression.Uncompressed;
//     //             AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);

//     //             ExportTexture(texture, exportPath, objectName);
//     //             exportedTextures.Add(texture);

//     //             // Restore original settings
//     //             importer.isReadable = wasReadable;
//     //             platformSettings.format = originalFormat;
//     //             importer.SetPlatformTextureSettings(platformSettings);

//     //             importer.textureCompression = originalCompression;
//     //             AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
//     //         }
//     //     }
//     // }

//     // private static void ExportTexture(Texture2D texture, string exportPath, string objectName)
//     // {
//     //     string fileName = $"{objectName}_{texture.name}.png";
//     //     string exportFilePath = Path.Combine(exportPath, fileName);

//     //     byte[] textureData = texture.EncodeToPNG();
//     //     if (textureData != null)
//     //     {
//     //         File.WriteAllBytes(exportFilePath, textureData);
//     //         Debug.Log($"Exported texture {texture.name} to {exportFilePath}");
//     //     }
//     //     else
//     //     {
//     //         Debug.LogError($"Failed to encode texture {texture.name} to PNG format.");
//     //     }
//     // }

//     void MoveChildren()
//     {
//         foreach (GameObject parent in Selection.gameObjects)
//         {
//             // Check if the object has any children
//             if (parent.transform.childCount == 0)
//             {
//                 Debug.LogWarning(parent.name + " has no children to move.");
//                 continue;
//             }

//             // Making changes in the hierarchy so start a new undo group
//             Undo.SetCurrentGroupName("Move Children And Remove Parent");

//             int group = Undo.GetCurrentGroup();

//             // Record the parent before the operation to allow undo correctly
//             Undo.RegisterFullObjectHierarchyUndo(parent, "Register Parent");

//             while (parent.transform.childCount > 0)
//             {
//                 Transform child = parent.transform.GetChild(0);

//                 // Set the child's new parent to be the parent's parent (null if parent is a root object)
//                 // This also keeps the child's world position unchanged
//                 Undo.SetTransformParent(child, parent.transform.parent, "Reparenting child");

//                 // Record the new parent setting operation within the undo group
//                 Undo.RegisterCreatedObjectUndo(child.gameObject, "Reparent Child");
//             }

//             // Delete the now empty parent object
//             Undo.DestroyObjectImmediate(parent);

//             // Set the Undo group, this enables us to undo the whole action with one Ctrl+Z
//             Undo.CollapseUndoOperations(group);
//         }

//         // Outside of the loop, we mark the end of our action here
//         Undo.FlushUndoRecordObjects();
//     }

//     // [Shortcut("Custom Tools/Drop Objects to Mesh", KeyCode.D, ShortcutModifiers.Shift | ShortcutModifiers.Control)]
//     public static void DropSelectedObjectsToMesh()
//     {
//         var selectedObjects = Selection.gameObjects;

//         if (selectedObjects.Length == 0)
//         {
//             Debug.Log("No objects selected. Please select at least one GameObject.");
//             return;
//         }

//         List<Collider> addedColliders = new List<Collider>();
//         EnsureCollidersExistAndGather(ref addedColliders);

//         DropObjectsToMesh(selectedObjects, ref addedColliders);

//         CleanupAddedColliders(addedColliders);
//     }

//     private static void EnsureCollidersExistAndGather(ref List<Collider> addedColliders)
//     {
//         foreach (GameObject obj in FindObjectsOfType<GameObject>())
//         {
//             if (obj.GetComponent<MeshRenderer>() != null && obj.GetComponent<Collider>() == null)
//             {
//                 MeshCollider newCollider = obj.AddComponent<MeshCollider>();
//                 addedColliders.Add(newCollider);
//             }
//         }
//     }

//     private static void DropObjectsToMesh(GameObject[] selectedObjects, ref List<Collider> addedColliders)
//     {
//         foreach (GameObject obj in selectedObjects)
//         {
//             Collider collider = obj.GetComponent<Collider>();
//             if (collider == null)
//             {
//                 collider = obj.AddComponent<BoxCollider>();
//                 addedColliders.Add(collider);
//             }

//             RaycastHit hit;
//             Vector3 colliderBottom = GetColliderBottomPoint(collider);

//             if (Physics.Raycast(colliderBottom, Vector3.down, out hit))
//             {
//                 obj.transform.position = new Vector3(obj.transform.position.x, hit.point.y, obj.transform.position.z);
//                 Debug.Log(obj.name + " dropped to mesh at " + hit.point);
//             }
//             else
//             {
//                 Debug.LogWarning("No mesh found under " + obj.name + ". Object has not been moved.");
//             }
//         }
//     }

//     private static void CleanupAddedColliders(List<Collider> addedColliders)
//     {
//         foreach (Collider addedCollider in addedColliders)
//         {
//             if (addedCollider != null)
//             {
//                 DestroyImmediate(addedCollider);
//             }
//         }
//     }

//     private static Vector3 GetColliderBottomPoint(Collider collider)
//     {
//         Vector3 bottomCenterPoint = collider.bounds.center;
//         bottomCenterPoint.y -= collider.bounds.extents.y;
//         return bottomCenterPoint;
//     }

//     private static void RemoveEmpty()
//     {
//         // Start an undo group so the actions can be undone in one step.
//         Undo.SetCurrentGroupName("Remove Empty Objects");

//         // Work on a copy of the selection array because the selection will change during the iteration.
//         GameObject[] selection = Selection.gameObjects;

//         foreach (GameObject obj in selection)
//         {
//             // If object has no child and no component (other than Transform), delete it.
//             if (obj.transform.childCount == 0 && obj.GetComponents<Component>().Length <= 1)
//             {
//                 // Record changes for undo.
//                 Undo.DestroyObjectImmediate(obj);
//             }
//         }
//         // Close the undo group.
//         Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
//     }


//     private static void CreateParentAtCenter(GameObject[] objects)
//     {
//         // Calculating the weighted average position of all selected objects
//         Vector3 center = GetCenter(objects);

//         // Setting y to be the lowest position among selected objects
//         float minY = objects.Min(obj => obj.transform.position.y);
//         Vector3 parentPosition = new Vector3(center.x, minY, center.z);

//         // Creating the new parent GameObject
//         GameObject parentObject = new GameObject("ParentObject");
//         Undo.RegisterCreatedObjectUndo(parentObject, "Created Parent Object");
//         parentObject.transform.position = parentPosition;

//         // Parenting the selected objects to the new parent
//         foreach (GameObject obj in objects)
//         {
//             Undo.SetTransformParent(obj.transform, parentObject.transform, "Parent " + obj.name);
//         }

//         // Select the new parent object in the Editor
//         Selection.activeGameObject = parentObject;
//     }

//     private static Vector3 GetCenter(GameObject[] objects)
//     {
//         Bounds bounds = new Bounds(objects[0].transform.position, Vector3.zero);
//         foreach (GameObject obj in objects.Skip(1))
//         {
//             bounds.Encapsulate(obj.transform.position);
//         }
//         return bounds.center;
//     }

//     private static Quaternion GetMaxRotationFromPrefabsAndObjects(GameObject[] objects)
//     {
//         GameObject maxSizeObject = null;
//         float maxSize = float.MinValue;

//         foreach (var obj in objects)
//         {
//             GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(obj);
//             if (root != null)
//             {
//                 // ����� Prefab ʵ��,������ Prefab ʵ����Ϊһ������
//                 float size = CalculateBoundsSize(root);
//                 if (size > maxSize)
//                 {
//                     maxSizeObject = root;
//                     maxSize = size;
//                 }
//             }
//             else
//             {
//                 // ������� Prefab ʵ��,�򵥶����Ǹö���
//                 float size = CalculateBoundsSize(obj);
//                 if (size > maxSize)
//                 {
//                     maxSizeObject = obj;
//                     maxSize = size;
//                 }
//             }
//         }

//         return maxSizeObject != null ? maxSizeObject.transform.rotation : Quaternion.identity;
//     }

//     private static float CalculateBoundsSize(GameObject obj)
//     {
//         Renderer renderer = obj.GetComponentInChildren<Renderer>();
//         if (renderer != null)
//         {
//             return renderer.bounds.size.magnitude;
//         }
//         else
//         {
//             return 0f;
//         }
//     }

//     private static void CreateParentWithMaxRotation(GameObject[] objects)
//     {
//         if (objects.Length == 0)
//             return;

//         // ��������λ�ú���СY����
//         Vector3 center = GetCenter(objects);
//         float minY = objects.Min(obj => obj.transform.position.y);
//         Vector3 parentPosition = new Vector3(center.x, minY, center.z);

//         // ��ȡ�����תֵ
//         Quaternion maxRotation = GetMaxRotationFromPrefabsAndObjects(objects);

//         // �����µĸ�����
//         GameObject parentObject = new GameObject("ParentObject");
//         Undo.RegisterCreatedObjectUndo(parentObject, "Created Parent Object");
//         parentObject.transform.position = parentPosition;
//         parentObject.transform.rotation = Quaternion.Euler(0, maxRotation.eulerAngles.y, 0);

//         // ��ѡ���Ķ����Ƶ��¸�������
//         foreach (GameObject obj in objects)
//         {
//             Undo.SetTransformParent(obj.transform, parentObject.transform, "Parent " + obj.name);
//         }

//         // ѡ���´����ĸ�����
//         Selection.activeGameObject = parentObject;
//     }

//     static Quaternion GetMaxRotation(GameObject[] objects)
//     {
//         return objects.OrderBy(obj => obj.transform.eulerAngles.y).Last().transform.rotation;
//     }

//     public static void FlattenAndRemoveParent()
//     {
//         Undo.IncrementCurrentGroup();
//         int undoGroup = Undo.GetCurrentGroup();

//         foreach (GameObject parent in Selection.gameObjects)
//         {
//             // Check if the current context is a prefab stage
//             PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
//             bool isInPrefabMode = prefabStage != null && prefabStage.stageHandle.IsValid();

//             int childrenCount = parent.transform.childCount;

//             // Use a reverse loop as you're modifying the hierarchy
//             for (int i = childrenCount - 1; i >= 0; i--)
//             {
//                 Transform child = parent.transform.GetChild(i);

//                 // Record the child transform for undo
//                 Undo.RecordObject(child, "Flatten Hierarchy And Remove Parent");

//                 if (isInPrefabMode)
//                 {
//                     // Parent it under the root of the prefab stage
//                     child.SetParent(prefabStage.prefabContentsRoot.transform, true);
//                 }
//                 else
//                 {
//                     // Unparent it by setting the parent to null
//                     child.SetParent(null, true);
//                 }
//             }

//             // Record the parent GameObject for undo and delete it
//             Undo.DestroyObjectImmediate(parent);
//         }

//         // Set the Undo name for the group action
//         Undo.CollapseUndoOperations(undoGroup);
//         Undo.SetCurrentGroupName("Flatten Hierarchy And Remove Parent");
//     }

//     void GeneratePrefabs()
//     {
//         // ���ӳ���
//         prefabSourceToPrefabMap.Clear();

//         // Ԥ�ȼ�Ᵽ��Ŀ¼
//         if (!System.IO.Directory.Exists(prefabPath))
//         {
//             System.IO.Directory.CreateDirectory(prefabPath);
//             Debug.Log($"Created directory at {prefabPath}");
//         }

//         // ɨ��Ŀ��Ŀ¼������Prefab������ԴPrefabӳ���
//         string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { prefabPath });
//         foreach (var guid in prefabGUIDs)
//         {
//             string prefabAssetPath = AssetDatabase.GUIDToAssetPath(guid);
//             GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
//             GameObject sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(prefabAsset);

//             if (sourcePrefab != null && !prefabSourceToPrefabMap.ContainsKey(sourcePrefab))
//             {
//                 prefabSourceToPrefabMap.Add(sourcePrefab, prefabAsset);
//             }
//         }

//         foreach (GameObject obj in Selection.gameObjects)
//         {

//             GameObject sourcePrefab = PrefabUtility.GetCorrespondingObjectFromSource(obj);

//             // ��鳡�������ԴPrefab�Ƿ���ӳ�����
//             if (sourcePrefab != null && prefabSourceToPrefabMap.ContainsKey(sourcePrefab))
//             {
//                 // ʹ��ӳ����е�Prefab�滻�����е�����
//                 ReplaceSceneObjectWithPrefab(obj, prefabSourceToPrefabMap[sourcePrefab]);
//                 continue;
//             }

//             // ����Prefab�ı���·��
//             string localPath = $"{prefabPath}/{obj.name}.prefab";
//             if (System.IO.File.Exists(localPath))
//             {
//                 // �����·��������Prefab������ӳ����滻��������
//                 GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(localPath);
//                 prefabSourceToPrefabMap.Add(obj, existingPrefab);
//                 ReplaceSceneObjectWithPrefab(obj, existingPrefab);
//                 continue;
//             }

//             // �����µ�Prefab
//             GameObject prefabInstance = Instantiate(obj);
//             prefabInstance.name = obj.name;
//             prefabInstance.transform.SetParent(null);

//             GameObject newPrefab = PrefabUtility.SaveAsPrefabAsset(prefabInstance, localPath);
//             if (newPrefab != null)
//             {
//                 Debug.Log($"Prefab created: {localPath}");
//                 prefabSourceToPrefabMap.Add(sourcePrefab, newPrefab);
//                 ReplaceSceneObjectWithPrefab(obj, newPrefab);
//             }
//             else
//             {
//                 Debug.LogError($"Failed to create prefab: {localPath}");
//             }

//             // ɾ����ʱ��Prefab����
//             DestroyImmediate(prefabInstance);
//         }

//         AssetDatabase.SaveAssets();
//         AssetDatabase.Refresh();
//     }

//     private void ReplaceSceneObjectWithPrefab(GameObject originalObject, GameObject prefab)
//     {
//         // ͨ��InstantiatePrefabʵ�����µ�Prefab
//         GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

//         // �����滻����ĸ�����ͱ任��Ϣ
//         prefabInstance.transform.SetParent(originalObject.transform.parent);
//         prefabInstance.transform.position = originalObject.transform.position;
//         prefabInstance.transform.rotation = originalObject.transform.rotation;
//         prefabInstance.transform.localScale = originalObject.transform.localScale;
//         Selection.activeGameObject = prefabInstance;

//         // �������ԭ�����е�����
//         DestroyImmediate(originalObject);
//     }

//     public void CleanAndReplacePrefabRoots(string PrefabfolderPath)
//     {
//         string[] prefabFiles = Directory.GetFiles(PrefabfolderPath, "*.prefab", SearchOption.AllDirectories);
//         foreach (string prefabFile in prefabFiles)
//         {
//             string prefabPath = prefabFile.Replace("\\", "/").Replace(Application.dataPath, "Assets");
//             ProcessPrefab(prefabPath);
//             //ReplaceMeshWithPrefab(prefabPath);
//         }

//         AssetDatabase.SaveAssets();
//         AssetDatabase.Refresh();
//         Debug.Log($"Processed prefabs under '{PrefabfolderPath}'.");
//     }

//     private void ProcessPrefab(string prefabPath)
//     {
//         GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
//         if (prefabAsset == null)
//             return;

//         GameObject rootPrefab = PrefabUtility.LoadPrefabContents(prefabPath);

//         MeshFilter meshFilter = rootPrefab.GetComponent<MeshFilter>();
//         MeshRenderer meshRenderer = rootPrefab.GetComponent<MeshRenderer>();

//         if (meshFilter && meshFilter.sharedMesh)
//         {
//             string meshAssetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
//             GameObject rootAssetGO = AssetDatabase.LoadAssetAtPath<GameObject>(meshAssetPath);

//             if (rootAssetGO)
//             {
//                 GameObject replacementInstance = PrefabUtility.InstantiatePrefab(rootAssetGO, rootPrefab.transform) as GameObject;

//                 ResetTransform(replacementInstance.transform);
//                 ResetTransform(rootPrefab.transform);

//                 UnityEngine.Object.DestroyImmediate(meshFilter, true);
//                 if (meshRenderer)
//                 {
//                     UnityEngine.Object.DestroyImmediate(meshRenderer, true);
//                 }

//                 PrefabUtility.SaveAsPrefabAsset(rootPrefab, prefabPath);
//             }
//         }

//         PrefabUtility.UnloadPrefabContents(rootPrefab);
//     }

//     // Transfers transform data from one transform to another
//     private void TransferTransform(Transform source, Transform destination)
//     {
//         destination.localPosition = new Vector3(source.localPosition.x, source.localPosition.y, source.localPosition.z);
//         destination.localRotation = new Quaternion(source.localRotation.x, source.localRotation.y, source.localRotation.z, source.localRotation.w);
//         destination.localScale = new Vector3(source.localScale.x, source.localScale.y, source.localScale.z);
//     }

//     // Resets the transform properties to their defaults
//     private void ResetTransform(Transform transform)
//     {
//         transform.localPosition = Vector3.zero;
//         transform.localRotation = Quaternion.identity;
//         transform.localScale = Vector3.one;
//     }

//     private void ReplaceMeshWithPrefab(string prefabPath)
//     {
//         GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
//         if (prefabAsset == null)
//         {
//             Debug.LogWarning($"Prefab at path '{prefabPath}' could not be loaded.");
//             return;
//         }

//         GameObject rootPrefab = PrefabUtility.LoadPrefabContents(prefabPath);

//         MeshFilter[] meshFilters = rootPrefab.GetComponentsInChildren<MeshFilter>();

//         foreach (MeshFilter meshFilter in meshFilters)
//         {
//             if (meshFilter.sharedMesh)
//             {
//                 string meshAssetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
//                 Debug.Log($"Mesh asset path: {meshAssetPath}");

//                 GameObject meshPrefab = FindParentPrefabOfMesh(meshAssetPath);

//                 if (meshPrefab)
//                 {
//                     GameObject replacementInstance = PrefabUtility.InstantiatePrefab(meshPrefab, meshFilter.transform.parent) as GameObject;

//                     TransferTransform(meshFilter.transform, replacementInstance.transform);

//                     UnityEngine.Object.DestroyImmediate(meshFilter.gameObject, true);
//                 }
//                 else
//                 {
//                     Debug.LogWarning($"No suitable prefab found for mesh '{meshFilter.sharedMesh.name}' in prefab at path '{prefabPath}'.");
//                 }
//             }
//         }

//         PrefabUtility.SaveAsPrefabAsset(rootPrefab, prefabPath);
//         PrefabUtility.UnloadPrefabContents(rootPrefab);
//     }

//     private GameObject FindParentPrefabOfMesh(string meshAssetPath)
//     {
//         string[] prefabPaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
//         foreach (string path in prefabPaths)
//         {
//             GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
//             if (prefab == null) continue;

//             MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
//             foreach (MeshFilter mf in meshFilters)
//             {
//                 if (AssetDatabase.GetAssetPath(mf.sharedMesh) == meshAssetPath)
//                 {
//                     Debug.Log($"Found parent prefab '{path}' for mesh asset '{meshAssetPath}'.");
//                     return prefab;
//                 }
//             }
//         }
//         return null;
//     }




//     private void ModifySelectedMetaFiles(bool changeMaterialImportMode = false, bool updateExternalObjects = false)
//     {
//         foreach (UnityEngine.Object obj in Selection.objects)
//         {
//             string assetPath = AssetDatabase.GetAssetPath(obj);
//             string metaPath = AssetDatabase.GetTextMetaFilePathFromAssetPath(assetPath);

//             if (changeMaterialImportMode)
//             {
//                 ModifyMaterialImportMode(metaPath);
//             }

//             if (updateExternalObjects)
//             {
//                 UpdateExternalObjectsWithName(metaPath, obj);
//             }
//         }

//         AssetDatabase.Refresh();

//     }

//     private void ModifyMaterialImportMode(string metaPath)
//     {
//         string metaContent = File.ReadAllText(metaPath);
//         metaContent = Regex.Replace(metaContent, materialImportModePattern, "materialImportMode: 1");
//         File.WriteAllText(metaPath, metaContent);
//     }

//     private void UpdateExternalObjectsWithName(string metaPath, UnityEngine.Object asset)
//     {
//         string content = File.ReadAllText(metaPath);

//         // ��ѡ�е��ʲ���ȡ����������
//         string materialName = GetAssetMaterialName(asset);

//         // ��ȡѡ�е�Material��GUID
//         string materialGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(selectedMaterial));

//         // ׼���滻������
//         string replacement = $"externalObjects:\n" +
//                              "  - first:\n" +
//                              "      type: UnityEngine:Material\n" +
//                              "      assembly: UnityEngine.CoreModule\n" +
//                              $"      name: {materialName}\n" +
//                              "    second: {fileID: 2100000, guid: " + materialGUID + ", type: 2}"; // ��������ֱ�ӽ�fileID��ֵǶ�뵽�ַ�����

//         // �滻externalObjects�ֶ�
//         content = Regex.Replace(content, externalObjectsPattern, replacement, RegexOptions.Singleline);
//         File.WriteAllText(metaPath, content);
//     }

//     private string GetAssetMaterialName(UnityEngine.Object asset)
//     {
//         var assetPath = AssetDatabase.GetAssetPath(asset);
//         var assetImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;

//         if (assetImporter != null)
//         {
//             // Load the model prefab to access its materials
//             var assetObj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

//             if (assetObj != null)
//             {
//                 var renderer = assetObj.GetComponentInChildren<Renderer>();
//                 if (renderer != null && renderer.sharedMaterials.Length > 0 && renderer.sharedMaterials[0] != null)
//                 {
//                     return renderer.sharedMaterials[0].name;
//                 }
//             }
//         }
//         return null;
//     }

//     void ArrangeSelectedObjects()
//     {
//         // ��ȡ��ǰѡ�е�������Ϸ����
//         GameObject[] selectedObjects = Selection.gameObjects;

//         // ���û��ѡ���κζ�����ʾһ������Ի����˳�����
//         if (selectedObjects.Length == 0)
//         {
//             EditorUtility.DisplayDialog("Error", "No objects selected!", "OK");
//             return;
//         }

//         // �������ƽ������򣬰�����Ȼ��˳��
//         Array.Sort(selectedObjects, (x, y) => NaturalCompare(x.name, y.name));

//         // ����ѡ�еĶ���������������Ĵ�С
//         int gridSize = Mathf.CeilToInt(Mathf.Sqrt(selectedObjects.Length));

//         // ��������ѡ�еĶ�������������������
//         for (int i = 0; i < selectedObjects.Length; i++)
//         {
//             int row = i / gridSize; // ���㵱ǰ����Ӧ���ڵڼ���
//             int col = i % gridSize; // ���㵱ǰ����Ӧ���ڵڼ���
//             Vector3 newPosition = new Vector3(col * xSpacing, selectedObjects[i].transform.position.y, row * zSpacing); // �趨��λ��
//             selectedObjects[i].transform.position = newPosition; // Ӧ����λ��
//         }
//     }

//     private int NaturalCompare(string a, string b)
//     {
//         if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
//             return string.Compare(a, b);

//         int i1 = 0, i2 = 0, r = 0;
//         while ((i1 < a.Length || i2 < b.Length) && r == 0)
//         {
//             if (i1 < a.Length && char.IsDigit(a[i1]) && i2 < b.Length && char.IsDigit(b[i2]))
//             {
//                 int j1 = i1, j2 = i2;
//                 while (j1 < a.Length && char.IsDigit(a[j1])) j1++;
//                 while (j2 < b.Length && char.IsDigit(b[j2])) j2++;
//                 string s1 = a.Substring(i1, j1 - i1), s2 = b.Substring(i2, j2 - i2);
//                 int n1 = int.Parse(s1), n2 = int.Parse(s2);
//                 r = n1.CompareTo(n2);
//                 i1 = j1;
//                 i2 = j2;
//             }
//             else
//             {
//                 if (i1 < a.Length && i2 < b.Length)
//                 {
//                     r = char.ToLower(a[i1]).CompareTo(char.ToLower(b[i2]));
//                     if (r == 0) r = a[i1++].CompareTo(b[i2++]);
//                 }
//                 else if (i1 < a.Length) r = 1;
//                 else if (i2 < b.Length) r = -1;
//             }
//         }
//         return r;
//     }


//     private void ReplaceNestedPrefabsInSelectedPrefabs(GameObject targetPrefab, GameObject replacementGO)
//     {
//         GameObject[] selectedPrefabs = Selection.gameObjects;

//         if (selectedPrefabs.Length == 0)
//         {
//             EditorUtility.DisplayDialog("Error", "Please select at least one prefab in the Project window", "OK");
//             return;
//         }

//         foreach (GameObject prefab in selectedPrefabs)
//         {
//             string pathToPrefab = AssetDatabase.GetAssetPath(prefab);
//             if (!pathToPrefab.EndsWith(".prefab"))
//             {
//                 Debug.LogWarning("Selected object is not a prefab: " + prefab.name);
//                 continue;
//             }

//             GameObject prefabContents = PrefabUtility.LoadPrefabContents(pathToPrefab);
//             if (prefabContents == null)
//             {
//                 Debug.LogError("Failed to load the prefab: " + prefab.name);
//                 continue;
//             }

//             List<Transform> nestedPrefabsToReplace = new List<Transform>();
//             GetNestedPrefabsToReplace(prefabContents.transform, nestedPrefabsToReplace, targetPrefab);

//             foreach (var nestedPrefabTransform in nestedPrefabsToReplace)
//             {
//                 GameObject newGOInstance =
//                     (GameObject)PrefabUtility.InstantiatePrefab(replacementGO, nestedPrefabTransform.parent);
//                 newGOInstance.name = replacementGO.name;
//                 newGOInstance.transform.SetSiblingIndex(nestedPrefabTransform.GetSiblingIndex());

//                 // ���ƾֲ��任
//                 newGOInstance.transform.localPosition = nestedPrefabTransform.localPosition;
//                 newGOInstance.transform.localRotation = nestedPrefabTransform.localRotation;
//                 newGOInstance.transform.localScale = nestedPrefabTransform.localScale;

//                 DestroyImmediate(nestedPrefabTransform.gameObject);
//             }

//             PrefabUtility.SaveAsPrefabAsset(prefabContents, pathToPrefab);
//             PrefabUtility.UnloadPrefabContents(prefabContents);
//         }

//         Debug.Log("Nested prefab replacement completed successfully.");
//     }

//     private void GetNestedPrefabsToReplace(Transform parent, List<Transform> nestedPrefabsToReplace, GameObject targetPrefab)
//     {
//         foreach (Transform child in parent)
//         {
//             // Check if the child is an instance of the target prefab
//             if (PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject) == targetPrefab)
//             {
//                 nestedPrefabsToReplace.Add(child);
//             }
//             else
//             {
//                 // �ݹ������������е�Ƕ��Ԥ����
//                 GetNestedPrefabsToReplace(child, nestedPrefabsToReplace, targetPrefab);
//             }
//         }
//     }


//     void BatchReplacePrefabs(string pathA, string pathB)
//     {
//         // �ݹ��ȡ·���µ������ļ�
//         string[] filesA = Directory.GetFiles(pathA, "*.*", SearchOption.AllDirectories);
//         string[] filesB = Directory.GetFiles(pathB, "*.*", SearchOption.AllDirectories);

//         // ʹ��Dictionary����ļ����Ͷ�Ӧ�ļ�·����ͬʱ��¼�ļ��㼶
//         Dictionary<string, (string filePath, int depth)> nameToFileB = new Dictionary<string, (string, int)>();

//         foreach (string fileB in filesB)
//         {
//             string fileName = Path.GetFileName(fileB);
//             int depth = GetFileDepth(fileB, pathB);

//             // ���ֵ�����ӻ�����ļ���ȷ������������ϲ���ļ�
//             if (!nameToFileB.ContainsKey(fileName) || nameToFileB[fileName].depth > depth)
//             {
//                 nameToFileB[fileName] = (fileB, depth);
//             }
//         }

//         foreach (string fileA in filesA)
//         {
//             string fileName = Path.GetFileName(fileA);

//             if (nameToFileB.TryGetValue(fileName, out var fileBData))
//             {
//                 GameObject targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fileA);
//                 GameObject replacementPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fileBData.filePath);

//                 if (targetPrefab != null && replacementPrefab != null)
//                 {
//                     ReplaceNestedPrefabsInSelectedPrefabs(targetPrefab, replacementPrefab);
//                 }
//             }
//         }

//         Debug.Log("Batch prefab replacement completed successfully.");
//     }

//     // ��ȡ�ļ��Ĳ㼶���
//     int GetFileDepth(string filePath, string basePath)
//     {
//         return filePath.Substring(basePath.Length).Split(Path.DirectorySeparatorChar).Length;
//     }

//     private UnityEngine.Object[] prefabsToModify;

//     private void AddNavMeshSurfaceToSelectedPrefabs()
//     {
//         // ��ȡ��ǰѡ��� Prefabs �б�
//         prefabsToModify = Selection.objects
//                             .Where(o => AssetDatabase.GetAssetPath(o).EndsWith(".prefab"))
//                             .ToArray();

//         if (prefabsToModify.Length == 0)
//         {
//             Debug.LogWarning("No prefabs selected.");
//             return;
//         }

//         foreach (UnityEngine.Object prefab in prefabsToModify)
//         {
//             string prefabPath = AssetDatabase.GetAssetPath(prefab);
//             GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

//             if (prefabRoot != null)
//             {
//                 // �����ڵ��Ƿ����� NavMeshSurface ���
//                 NavMeshSurface navMeshSurface = prefabRoot.GetComponent<NavMeshSurface>();
//                 if (navMeshSurface == null)
//                 {
//                     navMeshSurface = prefabRoot.AddComponent<NavMeshSurface>();
//                     Debug.Log($"Added NavMeshSurface to {prefab.name}");
//                 }
//                 else
//                 {
//                     Debug.Log($"Prefab {prefab.name} already has NavMeshSurface component");
//                 }

//                 // �Զ��決 NavMesh
//                 navMeshSurface.BuildNavMesh();

//                 // ���� NavMesh ����
//                 if (navMeshSurface.navMeshData != null)
//                 {
//                     string navMeshDataPath = System.IO.Path.Combine(
//                         System.IO.Path.GetDirectoryName(prefabPath),
//                         prefab.name + "_NavMeshData.asset"
//                     );

//                     AssetDatabase.CreateAsset(navMeshSurface.navMeshData, navMeshDataPath);
//                     AssetDatabase.SaveAssets();
//                     Debug.Log($"NavMesh data for {prefab.name} saved to {navMeshDataPath}");
//                 }
//                 else
//                 {
//                     Debug.LogWarning($"Failed to build NavMesh for {prefab.name}");
//                 }

//                 PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
//                 PrefabUtility.UnloadPrefabContents(prefabRoot);
//             }
//             else
//             {
//                 Debug.LogWarning($"Failed to load prefab: {prefab.name}");
//             }
//         }
//     }

//     // private static void ChangeShader()
//     // {
//     //     // Define the URP Lit Shader
//     //     Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");

//     //     // Check that the URP Lit Shader exists
//     //     if (urpLitShader == null)
//     //     {
//     //         Debug.LogError("Universal Render Pipeline/Lit shader not found in the project.");
//     //         return;
//     //     }

//     //     // Get the selected GameObjects in the Editor
//     //     GameObject[] selectedObjects = Selection.gameObjects;

//     //     if (selectedObjects.Length == 0)
//     //     {
//     //         Debug.LogWarning("No GameObjects selected. Please select GameObjects to change their shaders.");
//     //         return;
//     //     }

//     //     int changedMaterialsCount = 0;

//     //     // Loop through each selected GameObject
//     //     foreach (GameObject obj in selectedObjects)
//     //     {
//     //         // Get all renderers in the GameObject (and its children)
//     //         Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

//     //         foreach (Renderer renderer in renderers)
//     //         {
//     //             // Loop through all materials in the renderer
//     //             foreach (Material material in renderer.sharedMaterials)
//     //             {
//     //                 if (material != null && material.shader != urpLitShader)
//     //                 {
//     //                     Undo.RecordObject(material, "Change Material Shader");
//     //                     material.shader = urpLitShader;
//     //                     changedMaterialsCount++;
//     //                 }
//     //             }
//     //         }
//     //     }

//     //     Debug.Log($"Changed shader to Universal Render Pipeline/Lit for {changedMaterialsCount} materials.");
//     // }
//     private void Replace()
//     {
//         if (assetToReplace == null)
//         {
//             Debug.LogError("Please assign an asset to replace with.");
//             return;
//         }

//         Transform[] selectedTransforms = Selection.transforms;

//         if (selectedTransforms.Length == 0)
//         {
//             Debug.LogError("Please select at least one object to replace.");
//             return;
//         }

//         foreach (Transform selectedTransform in selectedTransforms)
//         {
//             GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(assetToReplace);

//             if (newObject != null)
//             {
//                 Undo.RegisterCreatedObjectUndo(newObject, "Replace With Asset");
//                 newObject.transform.position = selectedTransform.position;
//                 newObject.transform.rotation = selectedTransform.rotation;
//                 newObject.transform.localScale = selectedTransform.localScale;

//                 Undo.DestroyObjectImmediate(selectedTransform.gameObject); // ɾ��ԭʼ����
//             }
//             else
//             {
//                 Debug.LogError("Failed to instantiate the specified asset.");
//             }
//         }
//     }

//     private void RecordPositions()
//     {
//         if (Selection.activeGameObject != null)
//         {
//             GameObject selectedObject = Selection.activeGameObject;
//             targetPrefab = PrefabUtility.GetCorrespondingObjectFromSource(selectedObject) ?? selectedObject;
//             recordedPositions.Clear();

//             foreach (GameObject obj in FindObjectsOfType<GameObject>())
//             {
//                 if (PrefabUtility.GetCorrespondingObjectFromSource(obj) == targetPrefab)
//                 {
//                     Vector3 bottomCenter = GetBottomCenterPosition(obj.transform);
//                     recordedPositions.Add(bottomCenter);
//                 }
//             }

//             Debug.Log($"Recorded {recordedPositions.Count} positions for prefab: {targetPrefab.name}");
//         }
//         else
//         {
//             Debug.LogWarning("No object selected!");
//         }
//     }

//     private void PastePositions()
//     {
//         if (targetPrefab == null)
//         {
//             Debug.LogWarning("No positions recorded yet!");
//             return;
//         }

//         GameObject[] instances = FindObjectsOfType<GameObject>();
//         int index = 0;

//         foreach (GameObject obj in instances)
//         {
//             if (PrefabUtility.GetCorrespondingObjectFromSource(obj) == targetPrefab && index < recordedPositions.Count)
//             {
//                 Vector3 position = recordedPositions[index];
//                 obj.transform.position = position;
//                 index++;
//             }
//         }

//         if (index < recordedPositions.Count)
//         {
//             Debug.LogWarning("Not all recorded positions were used!");
//         }
//         else
//         {
//             Debug.Log($"Pasted {index} positions to instances of prefab: {targetPrefab.name}");
//         }
//     }

//     private Vector3 GetBottomCenterPosition(Transform objTransform)
//     {
//         Renderer renderer = objTransform.GetComponent<Renderer>();

//         if (renderer != null)
//         {
//             Bounds bounds = renderer.bounds;
//             Vector3 bottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
//             return bottomCenter;
//         }
//         else
//         {
//             Debug.LogWarning("Selected object does not have a Renderer component!");
//             return Vector3.zero;
//         }
//     }

//     private int CleanEmptyDirectories(string dir)
//     {
//         int deletedCount = 0;

//         try
//         {
//             // �ݹ��������ļ���
//             foreach (var subdir in Directory.GetDirectories(dir))
//             {
//                 deletedCount += CleanEmptyDirectories(subdir);
//             }

//             // ����ǿ��ļ��У�ɾ����
//             if (Directory.GetFileSystemEntries(dir).Length == 0 &&
//                 (dir != Application.dataPath))
//             {
//                 Directory.Delete(dir);
//                 File.Delete(dir + ".meta");  // ɾ��.meta�ļ�
//                 Debug.Log("Deleted empty folder: " + dir);
//                 deletedCount++;
//             }
//         }
//         catch (System.Exception ex)
//         {
//             Debug.LogError("Error cleaning directories: " + ex.Message);
//         }

//         return deletedCount;
//     }

//     private void Clean(string path)
//     {
//         if (string.IsNullOrEmpty(path))
//         {
//             Debug.LogError("·������Ϊ��");
//             return;
//         }

//         if (!Directory.Exists(path))
//         {
//             Debug.LogError("ָ����Ŀ¼������: " + path);
//             return;
//         }

//         int deletedFolders = CleanEmptyDirectories(path);
//         Debug.Log($"������ɣ���ɾ���� {deletedFolders} �����ļ��С�");
//         AssetDatabase.Refresh();
//     }

//     static void AdjustPivot()
//     {
//         GameObject selectedObject = Selection.activeGameObject;

//         if (selectedObject == null)
//         {
//             Debug.LogError("No GameObject selected.");
//             return;
//         }

//         MeshFilter meshFilter = selectedObject.GetComponent<MeshFilter>();
//         if (meshFilter == null)
//         {
//             Debug.LogError("Selected GameObject does not have a MeshFilter.");
//             return;
//         }

//         Mesh mesh = meshFilter.sharedMesh;
//         if (mesh == null)
//         {
//             Debug.LogError("Selected GameObject does not have a SharedMesh.");
//             return;
//         }

//         // Calculate bounds and offset to bottom-center
//         Bounds bounds = mesh.bounds;
//         Vector3 offset = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

//         // Use PrefabUtility to handle Prefab asset
//         string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(selectedObject);
//         if (string.IsNullOrEmpty(prefabPath))
//         {
//             Debug.LogError("Selected GameObject is not a valid prefab.");
//             return;
//         }

//         // Load the prefab at the asset path
//         GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
//         if (prefab == null)
//         {
//             Debug.LogError("Could not load prefab from path: " + prefabPath);
//             return;
//         }

//         // Create a new empty GameObject as the new root in the prefab
//         GameObject newParent = new GameObject(prefab.name + "_PivotAdjusted");

//         // Save the original transform properties of the prefab root
//         newParent.transform.position = prefab.transform.position;
//         newParent.transform.rotation = prefab.transform.rotation;
//         newParent.transform.localScale = prefab.transform.localScale;

//         // Set the prefab root as child of the new parent and adjust its local position
//         prefab.transform.SetParent(newParent.transform, false);
//         prefab.transform.localPosition -= offset;

//         // Apply changes and save the modified prefab
//         PrefabUtility.SaveAsPrefabAsset(newParent, prefabPath);

//         // Clean up newParent from the scene
//         UnityEngine.Object.DestroyImmediate(newParent);

//         // Apply changes to prefab instances in the scene
//         PrefabUtility.UnloadPrefabContents(prefab);

//         // Update the selected GameObject position to maintain its world position
//         Vector3 worldPositionOffset = selectedObject.transform.TransformVector(offset);
//         selectedObject.transform.position += worldPositionOffset;

//         Debug.Log("Mesh pivot adjusted and prefab updated in place.");
//     }

//     //Բ�����й���
//     private void GenerateCircleArray()
//     {
//         // �ȼ����ÿ�������λ�ú���ת
//         Vector3[] positions = new Vector3[count];
//         Quaternion[] rotations = new Quaternion[count];

//         float angleStep = 360f / count;
//         for (int i = 0; i < count; i++)
//         {
//             float angle = i * angleStep;
//             positions[i] = CalculatePosition(angle);
//             rotations[i] = CalculateRotation(angle);
//         }

//         // Ȼ���������ʵ������������λ�ú���ת
//         for (int i = 0; i < count; i++)
//         {
//             GameObject instance = InstantiateObject(objectToDuplicate);
//             instance.transform.position = positions[i];
//             instance.transform.rotation = rotations[i];

//             if (lookAtCenter && centerObject != null)
//             {
//                 instance.transform.LookAt(centerObject.transform);
//             }

//             Undo.RegisterCreatedObjectUndo(instance, "Create Object Array");
//         }
//     }

//     private GameObject InstantiateObject(GameObject original)
//     {
//         GameObject instance;
//         if (PrefabUtility.IsPartOfPrefabAsset(original))
//         {
//             instance = (GameObject)PrefabUtility.InstantiatePrefab(original);
//         }
//         else
//         {
//             instance = Instantiate(original);
//         }
//         instance.name = original.name + " (Instance)";
//         return instance;
//     }

//     private Vector3 CalculatePosition(float angle)
//     {
//         float radians = angle * Mathf.Deg2Rad;
//         float x = centerObject != null ? centerObject.transform.position.x + radius * Mathf.Cos(radians) : 0;
//         float z = centerObject != null ? centerObject.transform.position.z + radius * Mathf.Sin(radians) : 0;
//         return new Vector3(x, centerObject != null ? centerObject.transform.position.y : 0, z);
//     }

//     private Quaternion CalculateRotation(float angle)
//     {
//         return Quaternion.Euler(0, angle, 0);
//     }

//     // private static void ProcessSelectedMaterials()
//     // {
//     //     foreach (var obj in Selection.objects)
//     //     {
//     //         if (obj is Material material)
//     //         {
//     //             ProcessMaterial(material);
//     //         }
//     //     }
//     // }

//     //�Զ�����ͬ������ͼ
//     // private static void AutolinkSelectedMaterials()
//     // {
//     //     foreach (var obj in Selection.objects)
//     //     {
//     //         if (obj is Material material)
//     //         {
//     //             ProcessMaterial(material);
//     //         }
//     //     }
//     // }

//     // private static void ProcessMaterial(Material material)
//     // {
//     //     // ʹ�� Basemap ���Դ��� BaseColor
//     //     string texturePropertyName = "_BaseMap"; // ����Ϊ Basemap ��������

//     //     // ����Ƿ���Basemap��ͼ����
//     //     if (!material.HasProperty(texturePropertyName))
//     //     {
//     //         Debug.LogWarning($"Material {material.name} does not have a Basemap property.");
//     //         return;
//     //     }

//     //     // ���Basemap��ͼ�Ѿ����ڣ���������
//     //     if (material.GetTexture(texturePropertyName) != null)
//     //     {
//     //         Debug.Log($"Material {material.name} already has a Basemap texture.");
//     //         return;
//     //     }

//     //     // ��ȡ�������·��
//     //     string materialPath = AssetDatabase.GetAssetPath(material);
//     //     string materialDirectory = Path.GetDirectoryName(materialPath);

//     //     // ��ȡ���������Ƶ�ͨ�ò��֣�ȥ��_LOD��_mat����
//     //     string pattern = @"(.+)_LOD\d+_mat";
//     //     Match match = Regex.Match(material.name, pattern);
//     //     if (!match.Success)
//     //     {
//     //         Debug.LogWarning($"Material name {material.name} does not match the expected naming convention.");
//     //         return;
//     //     }

//     //     string baseName = match.Groups[1].Value;

//     //     // ��ͬĿ¼�в����������Ƶ���ͼ
//     //     string[] allFiles = Directory.GetFiles(materialDirectory, "*.png"); // ������ͼ��ʽΪPNG
//     //     foreach (var file in allFiles)
//     //     {
//     //         string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
//     //         if (Regex.IsMatch(fileNameWithoutExtension, $@"{baseName}_LOD\d+_tex"))
//     //         {
//     //             Texture2D baseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
//     //             if (baseTexture != null)
//     //             {
//     //                 material.SetTexture(texturePropertyName, baseTexture);
//     //                 Debug.Log($"Applied texture {file} to material {material.name}");
//     //             }
//     //             else
//     //             {
//     //                 Debug.LogWarning($"Failed to load texture at path: {file}");
//     //             }
//     //             break;
//     //         }
//     //     }
//     // }

//     //ɾ����fbxռλ�ʲ��ʲ�
//     private static void RemoveDuplicatesfbx()
//     {
//         // ��ȡ��ѡĿ¼
//         string[] selectedGUIds = Selection.assetGUIDs;
//         HashSet<string> filesToDelete = new HashSet<string>();

//         foreach (string guid in selectedGUIds)
//         {
//             string path = AssetDatabase.GUIDToAssetPath(guid);
//             Debug.Log($"Selected path: {path}");
//             ProcessDirectory(path, filesToDelete);
//         }

//         // ��ӡ��ɾ����ɾ���б��е��ļ�
//         foreach (string file in filesToDelete)
//         {
//             Debug.Log($"Marking for deletion: {file}");
//         }

//         foreach (string file in filesToDelete)
//         {
//             Debug.Log($"Deleting: {file}");
//             bool deleteResult = AssetDatabase.DeleteAsset(file);
//             if (!deleteResult)
//             {
//                 Debug.LogError($"Failed to delete: {file}");
//             }
//             else
//             {
//                 Debug.Log($"Successfully deleted: {file}");
//             }
//         }

//         AssetDatabase.Refresh();
//         Debug.Log("Duplicate files removed successfully.");
//     }

//     // ����Ŀ¼�ĺ���
//     private static void ProcessDirectory(string path, HashSet<string> filesToDelete)
//     {
//         string[] guids = AssetDatabase.FindAssets("*", new[] { path });
//         Dictionary<string, List<string>> fileGroups = new Dictionary<string, List<string>>();

//         foreach (string guid in guids)
//         {
//             string filePath = AssetDatabase.GUIDToAssetPath(guid);
//             Debug.Log($"Processing file: {filePath}");
//             string baseName = GetBaseName(Path.GetFileNameWithoutExtension(filePath));

//             if (!fileGroups.ContainsKey(baseName))
//             {
//                 fileGroups[baseName] = new List<string>();
//             }
//             fileGroups[baseName].Add(filePath);
//         }

//         foreach (var entry in fileGroups)
//         {
//             string baseName = entry.Key;
//             List<string> files = entry.Value;

//             bool hasVoxOrPrefab = files.Exists(file =>
//             {
//                 string extension = Path.GetExtension(file).ToLower();
//                 return extension == ".vox" || extension == ".prefab";
//             });

//             if (hasVoxOrPrefab)
//             {
//                 MarkFilesForDeletion(files, baseName, filesToDelete);
//             }
//         }
//     }

//     // �����Ҫɾ�����ļ�
//     private static void MarkFilesForDeletion(List<string> files, string baseName, HashSet<string> filesToDelete)
//     {
//         foreach (string file in files)
//         {
//             string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
//             string extension = Path.GetExtension(file).ToLower();

//             // �����Ƿ�����������ͣ�ͳһɾ�� .fbx �� .png �ļ�
//             if (fileNameWithoutExtension.StartsWith(baseName) &&
//                 (extension == ".fbx" || extension == ".png"))
//             {
//                 filesToDelete.Add(file);
//                 Debug.Log($"Marked for deletion: {file}");
//             }
//         }
//     }

//     // ��ȡ��������
//     private static string GetBaseName(string fileName)
//     {
//         int index = fileName.IndexOf("_LOD");
//         return index >= 0 ? fileName.Substring(0, index) : fileName;
//     }

//     //ƽ���ļ���
//     private void FlattenDirectory(string sourceDir)
//     {
//         if (!Directory.Exists(sourceDir))
//         {
//             Debug.LogError("Source directory does not exist.");
//             return;
//         }

//         var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
//         Dictionary<string, int> pathCounter = new Dictionary<string, int>();

//         foreach (var file in files)
//         {
//             string relativePath = Path.GetDirectoryName(file).Replace(sourceDir, "").TrimStart('\\');
//             if (!pathCounter.ContainsKey(relativePath))
//             {
//                 pathCounter[relativePath] = 1;
//             }

//             string destPath = Path.Combine(sourceDir, Path.GetFileName(file));
//             if (file == destPath)
//             {
//                 continue; // Skip files that are already at the top level
//             }

//             int counter = pathCounter[relativePath];
//             string baseName = Path.GetFileNameWithoutExtension(destPath);
//             string extension = Path.GetExtension(destPath);
//             string basePath = Path.Combine(sourceDir, baseName + "_" + counter);

//             while (File.Exists(destPath))
//             {
//                 counter++;
//                 basePath = Path.Combine(sourceDir, baseName + "_" + counter);
//                 pathCounter[relativePath] = counter;
//                 destPath = basePath + extension;
//             }

//             string metaFile = file + ".meta";
//             string destMetaFile = destPath + ".meta";

//             try
//             {
//                 File.Move(file, destPath);
//                 // ����.meta�ļ�
//                 if (File.Exists(metaFile))
//                 {
//                     File.Move(metaFile, destMetaFile);
//                 }
//                 else
//                 {
//                     Debug.LogWarning($".meta file not found for: {file}");
//                 }

//                 Debug.Log($"Moved: {file} -> {destPath}");
//             }
//             catch (IOException e)
//             {
//                 Debug.LogError($"Failed to move {file} or its .meta file. Error: {e.Message}");
//             }

//             pathCounter[relativePath]++; // Increment counter for next potential conflict in the same directory
//         }

//         AssetDatabase.Refresh();
//     }
//     //��ֱ��������ѡ����
//     private void ArrayDuplicate()
//     {
//         if (Selection.transforms.Length == 0)
//         {
//             Debug.LogWarning("No objects selected. Please select objects to array.");
//             return;
//         }

//         Vector3 axis = Vector3.zero;
//         if (xAxis) axis += Vector3.right;
//         if (yAxis) axis += Vector3.up;
//         if (zAxis) axis += Vector3.forward;

//         if (axis == Vector3.zero)
//         {
//             Debug.LogWarning("No axis selected. Please select at least one axis.");
//             return;
//         }

//         foreach (Transform original in Selection.transforms)
//         {
//             for (int i = 1; i <= numberOfCopies; i++)
//             {
//                 GameObject clone;
//                 if (PrefabUtility.IsPartOfPrefabInstance(original.gameObject))
//                 {
//                     clone = (GameObject)PrefabUtility.InstantiatePrefab(PrefabUtility.GetCorrespondingObjectFromSource(original.gameObject), original.parent);
//                 }
//                 else
//                 {
//                     clone = Instantiate(original.gameObject, original.parent);
//                 }

//                 Undo.RegisterCreatedObjectUndo(clone, "Array Duplicate");
//                 clone.transform.localPosition = original.localPosition + original.rotation * (axis.normalized * distance * i);
//                 clone.transform.localRotation = original.localRotation;
//             }
//         }
//     }

//     public static void SwapTransforms()
//     {
//         GameObject[] selectedObjects = Selection.gameObjects;
//         if (selectedObjects.Length != 2)
//         {
//             EditorUtility.DisplayDialog("Invalid Selection", "Please select exactly two objects to swap their transforms.", "OK");
//             return;
//         }

//         GameObject obj1 = selectedObjects[0];
//         GameObject obj2 = selectedObjects[1];

//         // Store the transform data
//         Vector3 position1 = obj1.transform.position;
//         Vector3 rotation1 = obj1.transform.eulerAngles; // Use eulerAngles for simplicity

//         // Swap positions
//         obj1.transform.position = obj2.transform.position;
//         obj2.transform.position = position1;

//         // Swap rotations
//         obj1.transform.eulerAngles = obj2.transform.eulerAngles;
//         obj2.transform.eulerAngles = rotation1;

//         EditorUtility.SetDirty(obj1);
//         EditorUtility.SetDirty(obj2);
//     }

//     private static void ResizeSelectedObject()
//     {
//         // ��ȡ��ǰѡ�е���Ϸ����
//         GameObject[] selectedObjects = Selection.gameObjects;

//         if (selectedObjects.Length > 0)
//         {
//             Undo.RecordObjects(selectedObjects, "Resize Selected Object");

//             foreach (GameObject obj in selectedObjects)
//             {
//                 obj.transform.localScale *= ScaleFactor;
//             }
//         }
//         else
//         {
//             Debug.LogWarning("û��ѡ���κ����壡");
//         }
//     }

//     //���������ظ��ļ�

//     private void RemoveDuplicates()
//     {
//         if (string.IsNullOrEmpty(directoryPath))
//         {
//             EditorUtility.DisplayDialog("Error", "Please enter a directory path.", "OK");
//             return;
//         }

//         if (!Directory.Exists(directoryPath))
//         {
//             EditorUtility.DisplayDialog("Error", "The specified path is invalid.", "OK");
//             return;
//         }

//         FindAndRemoveDuplicateFiles(directoryPath);
//     }

//     private void FindAndRemoveDuplicateFiles(string directory)
//     {
//         Dictionary<string, string> hashMap = new Dictionary<string, string>();

//         foreach (var filePath in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
//         {
//             try
//             {
//                 string fileHash = CalculateFileHash(filePath);

//                 if (hashMap.ContainsKey(fileHash))
//                 {
//                     Debug.Log($"Deleting duplicate file: {filePath}");
//                     File.Delete(filePath);
//                 }
//                 else
//                 {
//                     hashMap[fileHash] = filePath;
//                 }
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError($"Error processing file: {filePath}, Error: {e.Message}");
//             }
//         }

//         EditorUtility.DisplayDialog("Operation Completed", "Duplicate files removed.", "OK");
//     }

//     private string CalculateFileHash(string filePath)
//     {
//         using (var md5 = MD5.Create())
//         {
//             using (var stream = File.OpenRead(filePath))
//             {
//                 byte[] hash = md5.ComputeHash(stream);
//                 return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
//             }
//         }
//     }







// }