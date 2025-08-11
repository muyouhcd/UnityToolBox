using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class FbxAdderTest : MonoBehaviour
{
    [MenuItem("美术工具/测试/检查FbxAdder状态")]
    public static void CheckFbxAdderStatus()
    {
        Debug.Log("=== FbxAdder 状态检查 ===");
        
        // 检查类是否存在
        try
        {
            var editorType = typeof(FbxAdder.FbxAdderEditor);
            Debug.Log($"✓ FbxAdderEditor 类存在: {editorType.FullName}");
            
            // 检查枚举是否存在
            var enumType = typeof(FbxAdder.FbxAdderEditor.AssetFormat);
            Debug.Log($"✓ AssetFormat 枚举存在: {enumType.FullName}");
            
            // 检查方法是否存在
            var findMethod = editorType.GetMethod("FindAssetFiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (findMethod != null)
            {
                Debug.Log("✓ FindAssetFiles 方法存在");
            }
            else
            {
                Debug.LogError("✗ FindAssetFiles 方法不存在");
            }
            
            // 测试枚举值
            var testFormat = FbxAdder.FbxAdderEditor.AssetFormat.FBX;
            Debug.Log($"✓ 枚举值测试成功: {testFormat}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"检查失败: {e.Message}");
        }
    }
    
    [MenuItem("美术工具/测试/强制刷新FbxAdder")]
    public static void ForceRefreshFbxAdder()
    {
        Debug.Log("=== 强制刷新 FbxAdder ===");
        
        // 关闭所有FbxAdder窗口
        var windows = Resources.FindObjectsOfTypeAll<FbxAdder.FbxAdderEditor>();
        foreach (var window in windows)
        {
            window.Close();
        }
        
        // 重新打开
        FbxAdder.FbxAdderEditor.ShowWindow();
        
        Debug.Log("FbxAdder窗口已刷新，请检查按钮是否显示");
    }
    
    [MenuItem("美术工具/测试/检查编译状态")]
    public static void CheckCompilation()
    {
        Debug.Log("=== 编译状态检查 ===");
        Debug.Log("如果看到这条消息，说明脚本编译正常");
        
        // 检查是否有编译错误
        if (EditorUtility.scriptCompilationFailed)
        {
            Debug.LogError("✗ 脚本编译失败");
        }
        else
        {
            Debug.Log("✓ 脚本编译成功");
        }
    }
    
    [MenuItem("美术工具/测试/测试枚举访问")]
    public static void TestEnumAccess()
    {
        Debug.Log("=== 测试枚举访问 ===");
        
        try
        {
            // 测试所有枚举值
            var fbxFormat = FbxAdder.FbxAdderEditor.AssetFormat.FBX;
            var prefabFormat = FbxAdder.FbxAdderEditor.AssetFormat.Prefab;
            var objFormat = FbxAdder.FbxAdderEditor.AssetFormat.OBJ;
            var allFormat = FbxAdder.FbxAdderEditor.AssetFormat.All;
            
            Debug.Log($"✓ FBX: {fbxFormat}");
            Debug.Log($"✓ Prefab: {prefabFormat}");
            Debug.Log($"✓ OBJ: {objFormat}");
            Debug.Log($"✓ All: {allFormat}");
            
            Debug.Log("所有枚举值访问成功！");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"枚举访问失败: {e.Message}");
        }
    }
}
#endif
