using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace DYM.ToolBox
{
public class ImprovedTimelineCheckerWindow : OdinEditorWindow
{
    [PropertyOrder(0)]
    [Title("Timeline Checker")]
    private string trackKeyword = "Timeline Event Track"; // Track keyword

    [PropertyOrder(1)]
    private string clipKeyword = "TimelineEndAsset"; // Clip keyword

    [PropertyOrder(2)]
    private string targetScriptName = "AnimationEvents"; // Target script name

    [PropertyOrder(3)]
    [AssetsOnly]
    [ListDrawerSettings(Expanded = true, CustomAddFunction = "AddPrefabFromDragAndDrop", OnTitleBarGUI = "DrawClearButton")]
    public List<GameObject> prefabs = new List<GameObject>();

    [MenuItem("美术工具/检查工具/Timeline完整性检查工具")]
    public static void ShowWindow()
    {
        GetWindow<ImprovedTimelineCheckerWindow>("Improved Timeline Checker");
    }

    [PropertyOrder(4)]
    [Button("检查所有预制件")]
    private void CheckTimelines()
    {
        CheckTimelinesInPrefabs();
    }

    private void DrawClearButton()
    {
        if (GUILayout.Button("清空列表", EditorStyles.toolbarButton))
        {
            ClearList();
        }
    }

    private void AddPrefabFromDragAndDrop()
    {
        Event currentEvent = Event.current;

        if (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (currentEvent.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (obj is GameObject go && !prefabs.Contains(go))
                    {
                        prefabs.Add(go);
                    }
                }
            }

            Event.current.Use();
        }
    }

    private void CheckTimelinesInPrefabs()
    {
        List<GameObject> prefabsToRemove = new List<GameObject>();

        foreach (var prefab in prefabs)
        {
            if (prefab == null)
            {
                Debug.Log("检测到空Prefab对象");
                continue;
            }

            List<string> results = new List<string> { $"检查Prefab '{prefab.name}':" };
            bool issuesFound = false;

            PlayableDirector[] directors = prefab.GetComponentsInChildren<PlayableDirector>();

            if (directors.Length == 0)
            {
                results.Add("未找到PlayableDirector组件");
                issuesFound = true;
            }
            else
            {
                foreach (var director in directors)
                {
                    if (director == null) continue;

                    bool hasTargetScript = director.GetComponent(targetScriptName) != null;

                    if (!hasTargetScript)
                    {
                        results.Add($"- PlayableDirector '{director.name}' 所在的 GameObject 没有附加脚本 '{targetScriptName}'");
                        issuesFound = true;
                    }

                    TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;
                    if (timelineAsset == null)
                    {
                        results.Add($"PlayableDirector '{director.name}' 没有关联TimelineAsset");
                        issuesFound = true;
                        continue;
                    }

                    bool trackWithKeywordExists = false;

                    foreach (var track in timelineAsset.GetOutputTracks())
                    {
                        if (track.name.Contains(trackKeyword))
                        {
                            trackWithKeywordExists = true;

                            Object trackBinding = director.GetGenericBinding(track);
                            if (trackBinding == null)
                            {
                                results.Add($"- Track '{track.name}' 没有绑定任何对象");
                                issuesFound = true;
                            }

                            bool contentFound = false;
                            foreach (var clip in track.GetClips())
                            {
                                if (clip.displayName.Contains(clipKeyword))
                                {
                                    contentFound = true;
                                    break;
                                }
                            }

                            if (!contentFound)
                            {
                                results.Add($"- Track '{track.name}' 缺少包含 '{clipKeyword}' 的片段");
                                issuesFound = true;
                            }
                        }
                    }

                    if (!trackWithKeywordExists)
                    {
                        results.Add($"未找到包含关键字 '{trackKeyword}' 的轨道");
                        issuesFound = true;
                    }
                }
            }

            if (issuesFound)
            {
                string logMessage = string.Join("\n", results);
                Debug.Log(logMessage);
            }
            else
            {
                // 如果没有问题，将Prefab添加到待移除列表
                prefabsToRemove.Add(prefab);
            }
        }

        // 移除检查合格的Prefab
        foreach (var prefab in prefabsToRemove)
        {
            prefabs.Remove(prefab);
        }
    }

    private void ClearList()
    {
        prefabs.Clear();
    }
}
}