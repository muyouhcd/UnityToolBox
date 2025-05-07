using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;

public class AssetNameCheckerWindow : EditorWindow
{
    private string folderPath = "Assets"; // Ĭ�ϼ��·��

    [MenuItem("��������/��鹤��/�ʲ����Ƽ�鹤��")]
    public static void ShowWindow()
    {
        // ����һ������
        GetWindow<AssetNameCheckerWindow>("Asset Name Checker");
    }

    private void OnGUI()
    {
        GUILayout.Label("����ʲ��������򹤾�", EditorStyles.boldLabel);

        // ��������û�����·��
        GUILayout.Label("���·���������Assets�ļ��У���");
        folderPath = EditorGUILayout.TextField(folderPath);

        // ��ť�������
        if (GUILayout.Button("��������������"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("·������Ϊ�գ�");
                return;
            }

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"·����Ч: {folderPath}. ��ȷ���������һ����Ч���ļ���·����");
                return;
            }

            CheckAssetNaming(folderPath);
        }

        // ��ť�����滻�ո�Ϊ_
        if (GUILayout.Button("�滻�ʲ������еĿո�Ϊ'_'"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("·������Ϊ�գ�");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"·����Ч: {folderPath}. ��ȷ���������һ����Ч���ļ���·����");
                return;
            }

            ReplaceSpacesWithUnderscores(folderPath);
        }

        // ��ť������ - �滻Ϊ _
        if (GUILayout.Button("���ʲ������е� '-' �滻Ϊ '_'"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("·������Ϊ�գ�");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"·����Ч: {folderPath}. ��ȷ���������һ����Ч���ļ���·����");
                return;
            }

            ReplaceDashWithUnderscoreInAssetNames(folderPath);
        }

        // ��ť������������� _ �滻Ϊ���� _
        if (GUILayout.Button("��������� '_' �滻Ϊ���� '_'"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("·������Ϊ�գ�");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"·����Ч: {folderPath}. ��ȷ���������һ����Ч���ļ���·����");
                return;
            }

            ReplaceMultipleUnderscoresWithSingle(folderPath);
        }

        // ��ť������ͼ�����޸�
        if (GUILayout.Button("����ͼ�����޸�Ϊ�������Ĳ���������"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("·������Ϊ�գ�");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"·����Ч: {folderPath}. ��ȷ���������һ����Ч���ļ���·����");
                return;
            }

            RenameTexturesBasedOnMaterial(folderPath);
        }

        // ��������ַ�
        if (GUILayout.Button("����ʲ������е������ַ�"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("·������Ϊ�գ�");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"·����Ч: {folderPath}. ��ȷ���������һ����Ч���ļ���·����");
                return;
            }

            CheckChineseCharactersInAssetNames(folderPath);
        }

        // ��Ӱ�ť���Ƴ��ļ���ĩβ�»���
        if (GUILayout.Button("�Ƴ��ļ���ĩβ���»���"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("·������Ϊ�գ�");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"·����Ч: {folderPath}. ��ȷ���������һ����Ч���ļ���·����");
                return;
            }

            RemoveTrailingUnderscores(folderPath);
        }

        // ��Ӱ�ť����FBX��׺תΪСд
        if (GUILayout.Button("��FBX��׺תΪСд"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("·������Ϊ�գ�");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"·����Ч: {folderPath}. ��ȷ���������һ����Ч���ļ���·����");
                return;
            }

            ConvertFbxExtensionToLowercase(folderPath);
        }

        // ��Ӱ�ť���滻#Ϊ�»���
        if (GUILayout.Button("�滻�ʲ������е� '#' Ϊ '_'"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("·������Ϊ�գ�");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"·����Ч: {folderPath}. ��ȷ���������һ����Ч���ļ���·����");
                return;
            }

            ReplaceHashWithUnderscore(folderPath);
        }

        // ��Ӱ�ť�������ļ�������
        if (GUILayout.Button("�����ļ������ƣ��滻�ո����ۺź������»��ߣ�"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("·������Ϊ�գ�");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"·����Ч: {folderPath}. ��ȷ���������һ����Ч���ļ���·����");
                return;
            }

            ProcessFolderNames(folderPath);
        }

        // ���Ϲ��ܰ�ť - ����ִ�����в����������ͳһˢ��
        if (GUILayout.Button("һ���Ż��ʲ�������������ģʽ��"))
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("·������Ϊ�գ�");
                return;
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"·����Ч: {folderPath}. ��ȷ���������һ����Ч���ļ���·����");
                return;
            }

            BatchProcessAssetNames(folderPath);
        }
    }

    private static void BatchProcessAssetNames(string assetFolderPath)
    {
        Debug.Log("��ʼ�������ʲ������Ż�...");

        // 0. ���ȴ����ļ������� - �������Ӱ�������ʲ���·�����������ȴ���
        Debug.Log("����0: �����ļ�������...");
        int foldersRenamed = ProcessFolderNames(assetFolderPath, true);
        bool needsRefresh = foldersRenamed > 0;

        // �Ѽ������ʲ���Ϣ
        Debug.Log("�����ռ��ʲ���Ϣ...");
        string[] allAssetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        Dictionary<string, List<string>> textureUsage = new Dictionary<string, List<string>>();

        // �ռ����������ͼ�Ĺ�ϵ��Ϣ
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

        // ��������������
        int spacesReplaced = 0;
        int dashesReplaced = 0;
        int underscoresReplaced = 0;
        int texturesRenamed = 0;

        Regex regexMultipleUnderscores = new Regex("_{2,}"); // ƥ������������������»���

        // 1. ����ո��滻Ϊ�»���
        Debug.Log("����1: �滻�ո�Ϊ�»���...");
        foreach (string guid in allAssetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // ����Ŀ¼
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
                    Debug.LogError($"�ʲ�������ʧ��: {assetPath}. ����: {renameResult}");
                }
            }
        }

        // 2. �������ۺ��滻Ϊ�»���
        Debug.Log("����2: �滻���ۺ�Ϊ�»���...");
        // ���»�ȡ�ʲ��б���Ϊ������Щ�ʲ��Ѿ�������
        if (needsRefresh)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            allAssetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        }

        foreach (string guid in allAssetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // ����Ŀ¼
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);

            // ����������Ƿ������ۺţ�ʹ�ø��ϸ�ļ��
            if (assetName.IndexOf("-") >= 0)
            {
                // ȷ���滻�������ۺţ����������ǵ�һ��
                string newAssetName = assetName.Replace("-", "_");

                // ��֤������ȷʵ���������ۺ�
                if (newAssetName.IndexOf("-") >= 0)
                {
                    Debug.LogError($"�滻ʧ��: {assetPath} ���������������ۺ�");
                    continue;
                }

                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (string.IsNullOrEmpty(renameResult))
                {
                    Debug.Log($"�ʲ����������滻 '-' Ϊ '_'��: {newAssetName}", AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid)));
                    dashesReplaced++;
                    needsRefresh = true;
                }
                else
                {
                    Debug.LogError($"�ʲ�������ʧ��: {assetPath}. ����: {renameResult}");
                }
            }
        }

        // 3. ���������»����滻Ϊ�����»���
        Debug.Log("����3: �滻�����»���Ϊ�����»���...");
        // ���»�ȡ�ʲ��б���Ϊ������Щ�ʲ��Ѿ�������
        if (needsRefresh)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            allAssetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        }

        foreach (string guid in allAssetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // ����Ŀ¼
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
                    Debug.LogError($"�ʲ�������ʧ��: {assetPath}. ����: {renameResult}");
                }
            }
        }

        // 4. �޸���ͼ����Ϊ����������
        Debug.Log("����4: �޸���ͼ����Ϊ����������...");
        // ���»�ȡ�ʲ��б�Ͳ�����-��ͼ��ϵ����Ϊ������Щ�ʲ��Ѿ�������
        if (needsRefresh)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // �����ռ����������ͼ��ϵ
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

                    // ����.vox�ļ�
                    if (texturePath.EndsWith(".vox", System.StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.Log($"����.vox�ļ�: {texturePath}");
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

            // �ٴμ�飬����.vox�ļ����Է���һ��
            if (texturePath.EndsWith(".vox", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // ȷ����ͼ�����ǵ�Ŀ���ļ�����
            if (!texturePath.StartsWith(assetFolderPath))
                continue;

            // ������ͼ���������������û�����ͬ���Ʋ��������õ����
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
                        Debug.LogError($"��ͼ������ʧ��: {texturePath}. ����: {renameResult}");
                    }
                }
            }
            else
            {
                Object textureAsset = AssetDatabase.LoadAssetAtPath<Object>(texturePath);
                Debug.LogWarning($"��ͼ�������ͬ���ƵĲ���������: {string.Join(", ", distinctMaterialNames)}", textureAsset);
            }
        }

        // ��������: �Ƴ��ļ���ĩβ���»���
        Debug.Log("����3.5: �Ƴ��ļ���ĩβ���»���...");
        // ���»�ȡ�ʲ��б�
        if (needsRefresh)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            allAssetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        }

        int trailingUnderscoresRemoved = 0;
        Regex regexTrailingUnderscore = new Regex("_+$"); // ƥ���ļ���ĩβ��һ�������»���

        foreach (string guid in allAssetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // ����Ŀ¼
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);

            // ��Ⲣ�Ƴ��ļ���ĩβ���»���
            if (regexTrailingUnderscore.IsMatch(assetName))
            {
                string newAssetName = regexTrailingUnderscore.Replace(assetName, "");

                // ȷ��������������Ʋ�Ϊ��
                if (string.IsNullOrEmpty(newAssetName))
                {
                    Debug.LogWarning($"�Ƴ�ĩβ�»��ߺ��ļ���Ϊ��: {assetPath}������������");
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
                    Debug.LogError($"�ʲ�������ʧ��: {assetPath}. ����: {renameResult}");
                }
            }
        }

        // ����²���: ��FBX��׺תΪСд
        Debug.Log("����4.5: ��FBX��׺תΪСд...");
        // ���»�ȡ�ʲ��б�
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

        // 1.5. �滻#Ϊ�»���
        Debug.Log("����1.5: �滻#Ϊ�»���...");
        int hashesReplaced = ReplaceHashWithUnderscore(assetFolderPath, false);
        if (hashesReplaced > 0)
        {
            needsRefresh = true;
        }

        // ������ͳһˢ��
        if (needsRefresh)
        {
            Debug.Log("���ڱ��沢ˢ����Դ...");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // 5. ��������ַ�
        Debug.Log("����5: ��������ַ�...");
        // �ռ������ַ����ʲ�
        List<string> assetsWithChineseCharacters = new List<string>();
        Regex regexChineseCharacters = new Regex(@"[\u4e00-\u9fa5\u3000-\u303F\uFF00-\uFFEF]+"); // ƥ�������ַ������ı��

        // ���»�ȡ���µ��ʲ��б�
        allAssetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });

        foreach (string guid in allAssetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // ����Ŀ¼
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);

            // ��������ַ�
            if (regexChineseCharacters.IsMatch(assetName))
            {
                assetsWithChineseCharacters.Add(assetPath);
            }
        }

        // ������������ַ����ʲ��б�
        if (assetsWithChineseCharacters.Count > 0)
        {
            Debug.LogWarning($"���� {assetsWithChineseCharacters.Count} ���ʲ����ư��������ַ�:");
            foreach (var assetPath in assetsWithChineseCharacters)
            {
                // ʹ�ÿɵ�����ʲ�·��
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (asset != null)
                {
                    Debug.LogWarning($"���������ַ�: {Path.GetFileNameWithoutExtension(assetPath)}", asset);
                }
                else
                {
                    Debug.LogWarning($"���������ַ�: {assetPath}");
                }
            }
        }

        Debug.Log($"��������ɣ����������� {foldersRenamed} ���ļ��У��滻�� {spacesReplaced} ���ʲ��Ŀո�Ϊ'_'���滻�� {dashesReplaced} ���ʲ���'-'Ϊ'_'�������� {underscoresReplaced} ���ʲ��������»��ߣ��Ƴ��� {trailingUnderscoresRemoved} ���ʲ�����ĩβ���»��ߣ��� {fbxExtensionsConverted} ��FBX��׺תΪСд���������� {texturesRenamed} ����ͼ��");
    }

    private static void CheckAssetNaming(string assetFolderPath)
    {
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        int issueCount = 0;

        Debug.Log($"��ʼ����ʲ���������...");

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

            // ����ʲ������Ƿ�����ո�
            if (assetName.Contains(" "))
            {
                Debug.LogWarning($"���ư����ո�: {assetName}", asset);
                issueCount++;
            }

            // ��������ͼ�����Ƿ�һ��
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
                            // �����������ƺ���ͼ�����Ƿ�һ��
                            // ���������ͼ��������������ͼ�ȣ�
                            if (texturePropertyName == "_MainTex" && texture.name != material.name)
                            {
                                string texturePath = AssetDatabase.GetAssetPath(texture);
                                Debug.LogWarning($"���� '{material.name}' ������ͼ���Ʋ�һ��: ��ͼ '{texture.name}'", material);
                                issueCount++;
                            }
                        }
                    }
                }
            }
        }

        if (issueCount == 0)
        {
            Debug.Log("�����ʲ��������������Ҫ��");
        }
        else
        {
            Debug.Log($"�����ɣ������� {issueCount} ���������⣡");
        }
    }

    private static void CheckChineseCharactersInAssetNames(string assetFolderPath)
    {
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        List<string> assetsWithChineseCharacters = new List<string>();
        Regex regexChineseCharacters = new Regex(@"[\u4e00-\u9fa5\u3000-\u303F\uFF00-\uFFEF]+"); // ƥ�������ַ������ı��

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
            Debug.Log("δ���ְ��������ַ����ʲ����ƣ�");
        }
        else
        {
            Debug.LogWarning($"���� {assetsWithChineseCharacters.Count} ���ʲ����ư��������ַ�:");
            foreach (var assetPath in assetsWithChineseCharacters)
            {
                string assetName = Path.GetFileNameWithoutExtension(assetPath);
                // ��ȡ�������ַ����ֲ���ʾ
                var matches = regexChineseCharacters.Matches(assetName);
                StringBuilder chineseChars = new StringBuilder();
                foreach (Match match in matches)
                {
                    chineseChars.Append(match.Value);
                }

                // ʹ�ø�ʽ����־��ȷ��·���ɵ��
                Debug.LogWarning($"���������ַ�: \"{chineseChars}\"", AssetDatabase.LoadAssetAtPath<Object>(assetPath));
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
                    // ʹ�ÿɵ�����ʲ�·��
                    Debug.Log($"�ʲ����������滻�ո�Ϊ'_'��: {newAssetName}", AssetDatabase.LoadAssetAtPath<Object>(newAssetPath));
                    renameCount++;
                    needsRefresh = true;
                }
                else
                {
                    Debug.LogError($"�ʲ�������ʧ��: {assetPath}. ����: {renameResult}");
                }
            }
        }

        if (needsRefresh && refreshAssets)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"�滻�ո�Ϊ'_'������ɣ��������� {renameCount} ���ʲ����ƣ�");
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

            // ��Ⲣ�滻 '-'
            if (assetName.Contains("-"))
            {
                string newAssetName = assetName.Replace("-", "_");
                string directory = Path.GetDirectoryName(assetPath);
                string newAssetPath = Path.Combine(directory, newAssetName + extension).Replace("\\", "/");

                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (string.IsNullOrEmpty(renameResult))
                {
                    // ʹ�ÿɵ�����ʲ�·��
                    Debug.Log($"�ʲ����������滻 '-' Ϊ '_'��: {newAssetName}", AssetDatabase.LoadAssetAtPath<Object>(newAssetPath));
                    renameCount++;
                    needsRefresh = true;
                }
                else
                {
                    Debug.LogError($"�ʲ�������ʧ��: {assetPath}. ����: {renameResult}");
                }
            }
        }

        if (needsRefresh && refreshAssets)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"�滻 '-' Ϊ '_' ������ɣ��������� {renameCount} ���ʲ����ƣ�");
        return renameCount;
    }

    private static int ReplaceMultipleUnderscoresWithSingle(string assetFolderPath, bool refreshAssets = true)
    {
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        int renameCount = 0;
        bool needsRefresh = false;
        Regex regex = new Regex("_{2,}"); // ƥ������������������»���

        Debug.Log($"��ʼ��������»��ߣ����ҵ� {assetGuids.Length} ���ʲ�");

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // ����Ŀ¼
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);

            // ��Ⲣ�滻��������»���
            if (regex.IsMatch(assetName))
            {
                string newAssetName = regex.Replace(assetName, "_");
                string directory = Path.GetDirectoryName(assetPath);
                string newAssetPath = Path.Combine(directory, newAssetName + extension).Replace("\\", "/");

                // ʹ�ÿɵ�����ʲ�·��
                Debug.Log($"��⵽�����»���: {assetName}", AssetDatabase.LoadAssetAtPath<Object>(assetPath));

                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (string.IsNullOrEmpty(renameResult))
                {
                    // ��������ʹ����·��
                    Debug.Log($"�ʲ����������滻��� '_' Ϊ���� '_'��: {newAssetName}", AssetDatabase.LoadAssetAtPath<Object>(newAssetPath));
                    renameCount++;
                    needsRefresh = true;
                }
                else
                {
                    Debug.LogError($"�ʲ�������ʧ��: {assetPath}. ����: {renameResult}");
                }
            }
        }

        if (needsRefresh && refreshAssets)
        {
            Debug.Log("����ˢ���ʲ����ݿ�...");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"�滻��� '_' Ϊ���� '_' ������ɣ��������� {renameCount} ���ʲ����ƣ�");
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

                // ����.vox�ļ�
                if (texturePath.EndsWith(".vox", System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"����.vox�ļ�: {texturePath}");
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

            // �ٴμ�飬����.vox�ļ����Է���һ��
            if (texturePath.EndsWith(".vox", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // ȷ����ͼ�����ǵ�Ŀ���ļ�����
            if (!texturePath.StartsWith(assetFolderPath))
                continue;

            // ������ͼ���������������û�����ͬ���Ʋ��������õ����
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
                        Debug.LogError($"��ͼ������ʧ��: {texturePath}. ����: {renameResult}");
                    }
                }
            }
            else
            {
                Object textureAsset = AssetDatabase.LoadAssetAtPath<Object>(texturePath);
                Debug.LogWarning($"��ͼ�������ͬ���ƵĲ���������: {string.Join(", ", distinctMaterialNames)}", textureAsset);
            }
        }

        if (needsRefresh && refreshAssets)
        {
            Debug.Log("����ˢ���ʲ����ݿ�...");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"��ͼ��������ɣ��������� {texturesRenamed} ����ͼ��");
        return texturesRenamed;
    }

    // ��װ��ͼ���������߼�Ϊһ�������ķ���
    private static void RenameTexture(string texturePath, string newTextureName, ref int renameCount, ref bool needsRefresh, bool printLog = true)
    {
        string extension = Path.GetExtension(texturePath);
        string directory = Path.GetDirectoryName(texturePath);
        string newTexturePath = Path.Combine(directory, newTextureName + extension).Replace("\\", "/");

        string renameResult = AssetDatabase.RenameAsset(texturePath, newTextureName);
        if (string.IsNullOrEmpty(renameResult))
        {
            // ������Ҫʱ��ӡ��־
            if (printLog)
            {
                // ʹ�ÿɵ�����ʲ�·��
                Debug.Log($"��ͼ������: {newTextureName}", AssetDatabase.LoadAssetAtPath<Object>(newTexturePath));
            }
            renameCount++;
            needsRefresh = true;
        }
        else
        {
            Debug.LogError($"��ͼ������ʧ��: {texturePath}. ����: {renameResult}");
        }
    }

    // ���һ�������ĺ�����ȥ���ļ���ĩβ���»���
    private static int RemoveTrailingUnderscores(string assetFolderPath, bool refreshAssets = true)
    {
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        int renameCount = 0;
        bool needsRefresh = false;
        Regex regexTrailingUnderscore = new Regex("_+$"); // ƥ���ļ���ĩβ��һ�������»���

        Debug.Log($"��ʼ����ļ���ĩβ�»��ߣ����ҵ� {assetGuids.Length} ���ʲ�");

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // ����Ŀ¼
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);

            // ��Ⲣ�Ƴ��ļ���ĩβ���»���
            if (regexTrailingUnderscore.IsMatch(assetName))
            {
                string newAssetName = regexTrailingUnderscore.Replace(assetName, "");

                // ȷ��������������Ʋ�Ϊ��
                if (string.IsNullOrEmpty(newAssetName))
                {
                    Debug.LogWarning($"�Ƴ�ĩβ�»��ߺ��ļ���Ϊ��: {assetPath}������������");
                    continue;
                }

                string directory = Path.GetDirectoryName(assetPath);
                string newAssetPath = Path.Combine(directory, newAssetName + extension).Replace("\\", "/");

                Debug.Log($"��⵽ĩβ�»���: {assetName}", AssetDatabase.LoadAssetAtPath<Object>(assetPath));

                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (string.IsNullOrEmpty(renameResult))
                {
                    Debug.Log($"�ʲ����������Ƴ�ĩβ�»��ߣ�: {newAssetName}", AssetDatabase.LoadAssetAtPath<Object>(newAssetPath));
                    renameCount++;
                    needsRefresh = true;
                }
                else
                {
                    Debug.LogError($"�ʲ�������ʧ��: {assetPath}. ����: {renameResult}");
                }
            }
        }

        if (needsRefresh && refreshAssets)
        {
            Debug.Log("����ˢ���ʲ����ݿ�...");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"�Ƴ�ĩβ�»��ߴ�����ɣ��������� {renameCount} ���ʲ����ƣ�");
        return renameCount;
    }

    // ���һ���µĺ�������FBX��׺תΪСд
    private static int ConvertFbxExtensionToLowercase(string assetFolderPath, bool refreshAssets = true)
    {
        string[] assetGuids = AssetDatabase.FindAssets("t:Model", new[] { assetFolderPath });
        int renameCount = 0;
        bool needsRefresh = false;

        Debug.Log($"��ʼ����FBX��׺��Сд�����ҵ� {assetGuids.Length} ��ģ���ʲ�");

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // �����չ���Ƿ�Ϊ.FBX (��д)
            if (Path.GetExtension(assetPath).Equals(".FBX", System.StringComparison.OrdinalIgnoreCase) &&
                !Path.GetExtension(assetPath).Equals(".fbx")) // �����Ѿ���Сд��Ҳ����
            {
                string directory = Path.GetDirectoryName(assetPath);
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                string newPath = Path.Combine(directory, fileName + ".fbx").Replace("\\", "/");

                // ����Unity��AssetDatabase.RenameAsset���ܸ�����չ����������Ҫʹ���ļ�ϵͳAPI
                // ����Unity�У�������Ҫ�ȼ���ʲ����ݿ�״̬
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (asset != null)
                {
                    Debug.Log($"��⵽��дFBX��׺: {assetPath}", asset);

                    try
                    {
                        // ʹ��AssetDatabase�ķ����޸��ļ�����������չ����
                        // ����һ���������̣�
                        // 1. ������Ϊ��ʱ���ƣ�����ʱ��������ͻ��
                        string tempName = fileName + "_temp_" + System.DateTime.Now.Ticks;
                        string tempResult = AssetDatabase.RenameAsset(assetPath, tempName);
                        if (!string.IsNullOrEmpty(tempResult))
                        {
                            Debug.LogError($"����������ʱ����ʧ��: {assetPath} -> {tempName}. ����: {tempResult}");
                            continue;
                        }

                        string tempPath = Path.Combine(directory, tempName + Path.GetExtension(assetPath)).Replace("\\", "/");

                        // 2. ������Ϊԭ���Ƽ�Сд��չ��
                        string finalResult = AssetDatabase.RenameAsset(tempPath, fileName + ".fbx");
                        if (string.IsNullOrEmpty(finalResult))
                        {
                            Debug.Log($"FBX��׺תΪСд: {fileName}.FBX -> {fileName}.fbx", AssetDatabase.LoadAssetAtPath<Object>(newPath));
                            renameCount++;
                            needsRefresh = true;
                        }
                        else
                        {
                            Debug.LogError($"������ʧ��: {tempPath} -> {fileName}.fbx. ����: {finalResult}");
                            // ���Իָ�ԭ��
                            AssetDatabase.RenameAsset(tempPath, fileName + Path.GetExtension(assetPath));
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"�����ļ�ʱ����: {assetPath}. ����: {ex.Message}");
                    }
                }
            }
        }

        if (needsRefresh && refreshAssets)
        {
            Debug.Log("����ˢ���ʲ����ݿ�...");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"FBX��׺תΪСд������ɣ��������� {renameCount} ���ʲ���");
        return renameCount;
    }

    // ����滻#Ϊ�»��ߵĹ���
    private static int ReplaceHashWithUnderscore(string assetFolderPath, bool refreshAssets = true)
    {
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { assetFolderPath });
        int renameCount = 0;
        bool needsRefresh = false;
        Regex regexMultipleUnderscores = new Regex("_{2,}"); // ƥ������������������»���

        Debug.Log($"��ʼ�滻#Ϊ�»��ߣ����ҵ� {assetGuids.Length} ���ʲ�");

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // ����Ŀ¼
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);
            bool needsRename = false;
            string newAssetName = assetName;

            // ��Ⲣ�滻 '#'
            if (assetName.Contains("#"))
            {
                newAssetName = assetName.Replace("#", "_");
                needsRename = true;
            }

            // �滻��#��������Ⲣ�滻�����»���
            if (needsRename && regexMultipleUnderscores.IsMatch(newAssetName))
            {
                newAssetName = regexMultipleUnderscores.Replace(newAssetName, "_");
            }

            // �����Ҫ��������ִ��������
            if (needsRename)
            {
                string directory = Path.GetDirectoryName(assetPath);
                string newAssetPath = Path.Combine(directory, newAssetName + extension).Replace("\\", "/");

                Debug.Log($"��⵽����#��: {assetName}", AssetDatabase.LoadAssetAtPath<Object>(assetPath));

                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (string.IsNullOrEmpty(renameResult))
                {
                    Debug.Log($"�ʲ����������滻#Ϊ_��: {newAssetName}", AssetDatabase.LoadAssetAtPath<Object>(newAssetPath));
                    renameCount++;
                    needsRefresh = true;
                }
                else
                {
                    Debug.LogError($"�ʲ�������ʧ��: {assetPath}. ����: {renameResult}");
                }
            }
        }

        if (needsRefresh && refreshAssets)
        {
            Debug.Log("����ˢ���ʲ����ݿ�...");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"�滻#Ϊ_������ɣ��������� {renameCount} ���ʲ����ƣ�");
        return renameCount;
    }

    // �޸�ProcessFolderNames����������滻#�Ĺ���
    private static int ProcessFolderNames(string assetFolderPath, bool refreshAssets = true)
    {
        int renamedCount = 0;
        bool needsRefresh = false;
        List<string> allFolders = new List<string>();
        Regex regexMultipleUnderscores = new Regex("_{2,}"); // ƥ������������������»���

        // �ռ������ļ���·��
        CollectAllFolders(assetFolderPath, allFolders);

        // ��·����������ȷ���ȴ���϶̵�·�������ļ��У�
        // ʹ�ý������������ȴ������Ŀ¼���ٴ����ϲ�Ŀ¼
        allFolders.Sort((a, b) => b.Length.CompareTo(a.Length));

        Debug.Log($"��ʼ�����ļ������ƣ����ҵ� {allFolders.Count} ���ļ���");

        foreach (string folderPath in allFolders)
        {
            string folderName = Path.GetFileName(folderPath);
            string parentFolder = Path.GetDirectoryName(folderPath);
            bool needsRename = false;
            string newFolderName = folderName;

            // �滻�ո�Ϊ�»���
            if (newFolderName.Contains(" "))
            {
                newFolderName = newFolderName.Replace(" ", "_");
                needsRename = true;
            }

            // �滻���ۺ�Ϊ�»���
            if (newFolderName.Contains("-"))
            {
                newFolderName = newFolderName.Replace("-", "_");
                needsRename = true;
            }

            // �滻#Ϊ�»���
            if (newFolderName.Contains("#"))
            {
                newFolderName = newFolderName.Replace("#", "_");
                needsRename = true;
            }

            // �滻�����»���Ϊ�����»���
            if (regexMultipleUnderscores.IsMatch(newFolderName))
            {
                newFolderName = regexMultipleUnderscores.Replace(newFolderName, "_");
                needsRename = true;
            }

            // �����Ҫ��������ִ��������
            if (needsRename)
            {
                string oldPath = folderPath;
                string newPath = Path.Combine(parentFolder, newFolderName).Replace("\\", "/");

                Debug.Log($"�������ļ���: {oldPath} -> {newPath}");

                // ʹ��AssetDatabase API�������ļ���
                string error = AssetDatabase.MoveAsset(oldPath, newPath);
                if (string.IsNullOrEmpty(error))
                {
                    renamedCount++;
                    needsRefresh = true;
                }
                else
                {
                    Debug.LogError($"�������ļ���ʧ��: {oldPath} -> {newPath}. ����: {error}");
                }
            }
        }

        if (needsRefresh && refreshAssets)
        {
            Debug.Log("����ˢ���ʲ����ݿ�...");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"�ļ������ƴ�����ɣ����������� {renamedCount} ���ļ��У�");
        return renamedCount;
    }

    // �ݹ��ռ������ļ���
    private static void CollectAllFolders(string rootFolder, List<string> allFolders)
    {
        if (!AssetDatabase.IsValidFolder(rootFolder))
            return;

        // ��ӵ�ǰ�ļ���
        allFolders.Add(rootFolder);

        // ��ȡ�������ļ���
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

        // �ݹ鴦��ÿ�����ļ���
        foreach (string subFolder in subFolders)
        {
            CollectAllFolders(subFolder, allFolders);
        }
    }
}