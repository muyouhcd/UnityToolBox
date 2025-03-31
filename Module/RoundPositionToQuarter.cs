using UnityEditor;
using UnityEngine;

namespace DYM.ToolBox
{

public class RoundPositionEditor : EditorWindow
{
    [MenuItem("美术工具/地编工具/选取物体位置取整0.25")]
    static void RoundPositionToQuarter()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RecordObject(obj.transform, "Round Position to Quarter");
            Vector3 currentPosition = obj.transform.position;
            Vector3 roundedPosition = new Vector3(
                RoundToNearestQuarter(currentPosition.x),
                RoundToNearestQuarter(currentPosition.y),
                RoundToNearestQuarter(currentPosition.z)
            );
            obj.transform.position = roundedPosition;
        }
    }

    static float RoundToNearestQuarter(float value)
    {
        return Mathf.Round(value * 4f) / 4f;
    }
}
}