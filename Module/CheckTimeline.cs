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

    [MenuItem("��������/��鹤��/Timeline�����Լ�鹤��")]
    public static void ShowWindow()
    {
        GetWindow<ImprovedTimelineCheckerWindow>("Improved Timeline Checker");
    }

    [PropertyOrder(4)]
    [Button("�������Ԥ�Ƽ�")]
    private void CheckTimelines()
    {
        CheckTimelinesInPrefabs();
    }

    private void DrawClearButton()
    {
        if (GUILayout.Button("����б�", EditorStyles.toolbarButton))
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
                Debug.Log("��⵽��Prefab����");
                continue;
            }

            List<string> results = new List<string> { $"���Prefab '{prefab.name}':" };
            bool issuesFound = false;

            PlayableDirector[] directors = prefab.GetComponentsInChildren<PlayableDirector>();

            if (directors.Length == 0)
            {
                results.Add("δ�ҵ�PlayableDirector���");
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
                        results.Add($"- PlayableDirector '{director.name}' ���ڵ� GameObject û�и��ӽű� '{targetScriptName}'");
                        issuesFound = true;
                    }

                    TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;
                    if (timelineAsset == null)
                    {
                        results.Add($"PlayableDirector '{director.name}' û�й���TimelineAsset");
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
                                results.Add($"- Track '{track.name}' û�а��κζ���");
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
                                results.Add($"- Track '{track.name}' ȱ�ٰ��� '{clipKeyword}' ��Ƭ��");
                                issuesFound = true;
                            }
                        }
                    }

                    if (!trackWithKeywordExists)
                    {
                        results.Add($"δ�ҵ������ؼ��� '{trackKeyword}' �Ĺ��");
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
                // ���û�����⣬��Prefab��ӵ����Ƴ��б�
                prefabsToRemove.Add(prefab);
            }
        }

        // �Ƴ����ϸ��Prefab
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