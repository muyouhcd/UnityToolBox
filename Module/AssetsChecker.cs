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
        bool needsRefresh = false;

        // ��¼���������ַ����ʲ�
        List<string> assetsWithChineseCharacters = new List<string>();

        Regex regexMultipleUnderscores = new Regex("_{2,}"); // ƥ������������������»���
        Regex regexChineseCharacters = new Regex(@"[\u4e00-\u9fa5\u3000-\u303F\uFF00-\uFFEF]+"); // ƥ�������ַ������ı��

        // 1. ������ͨ�ʲ�����
        Debug.Log("��ʼ�����ʲ�����...");
        foreach (string guid in allAssetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            // ����Ŀ¼
            if (Directory.Exists(assetPath) && !File.Exists(assetPath))
                continue;

            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath);
            bool needsRename = false;
            string newAssetName = assetName;

            // ��������ַ�
            if (regexChineseCharacters.IsMatch(assetName))
            {
                assetsWithChineseCharacters.Add(assetPath);
            }

            // ����ո�
            if (newAssetName.Contains(" "))
            {
                newAssetName = newAssetName.Replace(" ", "_");
                needsRename = true;
                spacesReplaced++;
            }

            // �������ۺ�
            if (newAssetName.Contains("-"))
            {
                newAssetName = newAssetName.Replace("-", "_");
                needsRename = true;
                dashesReplaced++;
            }

            // ���������»���
            if (regexMultipleUnderscores.IsMatch(newAssetName))
            {
                string beforeReplace = newAssetName;
                newAssetName = regexMultipleUnderscores.Replace(newAssetName, "_");
                Debug.Log($"�滻�����»���: {assetPath}, {beforeReplace} -> {newAssetName}");
                needsRename = true;
                underscoresReplaced++;
            }

            // �����Ҫ��������ִ��������
            if (needsRename)
            {
                string renameResult = AssetDatabase.RenameAsset(assetPath, newAssetName);
                if (!string.IsNullOrEmpty(renameResult))
                {
                    Debug.LogError($"�ʲ�������ʧ��: {assetPath} -> {newAssetName}. ����: {renameResult}");
                }
                needsRefresh = true;
            }
        }

        // 2. ������ͼ������
        Debug.Log("��ʼ������ͼ����...");
        foreach (var entry in textureUsage)
        {
            string texturePath = entry.Key;
            List<string> materialNames = entry.Value;
            string textureName = Path.GetFileNameWithoutExtension(texturePath);

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
                Debug.LogWarning($"��ͼ {texturePath} �������ͬ���ƵĲ���������: {string.Join(", ", distinctMaterialNames)}");
            }
        }

        // ͳһˢ��
        if (needsRefresh)
        {
            Debug.Log("���ڱ��沢ˢ����Դ...");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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

        Debug.Log($"��������ɣ����滻�� {spacesReplaced} ���ʲ��Ŀո�Ϊ'_'���滻�� {dashesReplaced} ���ʲ���'-'Ϊ'_'�������� {underscoresReplaced} ���ʲ��������»��ߣ��������� {texturesRenamed} ����ͼ��");
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

            // ������ͼ���������������õ����
            if (materialNames.Count == 1)
            {
                string newTextureName = materialNames[0]; // ʹ�ò����������

                if (textureName != newTextureName)
                {
                    RenameTexture(texturePath, newTextureName, ref renameCount, ref needsRefresh, false);
                }
            }
            // ������ͼ��������������õ����
            else
            {
                // ����������ø���ͼ�Ĳ����������Ƿ���ͬ
                var distinctMaterialNames = materialNames.Distinct().ToList();

                // ����������õĲ�����������ͬ
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
                    Debug.LogWarning($"��ͼ�������ͬ���ƵĲ���������: {string.Join(", ", distinctMaterialNames)}", textureAsset);
                }
            }
        }

        if (needsRefresh && refreshAssets)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"��ͼ��������ɣ��������� {renameCount} ����ͼ��");
        return renameCount;
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
}