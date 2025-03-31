//using UnityEngine;
//using UnityEngine.Timeline;
//using UnityEditor;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using Tools.Timeline;

//public class SubtitleEditorWindow : EditorWindow
//{
//    [Serializable]
//    public class SubtitleInfo
//    {
//        public int? startFrame;
//        public string text;
//    }

//    private List<SubtitleInfo> subtitles = new List<SubtitleInfo>();
//    private TimelineAsset timelineAsset;
//    private TextAsset csvFile;
//    private string csvFilePath;
//    private string timelineFolderPath;
//    private string csvFolderPath;
//    private int frameRate = 30;
//    private float timePerCharacter = 0.05f;

//    [MenuItem("Tools/��ĻTimelineClip������")]
//    public static void ShowWindow()
//    {
//        GetWindow<SubtitleEditorWindow>("��ĻTimelineClip������");
//    }

//    private void OnGUI()
//    {
//        GUILayout.Label("Batch Create Subtitle Clips from CSV", EditorStyles.boldLabel);

//        timelineAsset = (TimelineAsset)EditorGUILayout.ObjectField("Timeline Asset", timelineAsset, typeof(TimelineAsset), false);
//        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

//        if (csvFile != null)
//        {
//            csvFilePath = AssetDatabase.GetAssetPath(csvFile);
//            if (!csvFilePath.EndsWith(".csv"))
//            {
//                Debug.LogWarning("The selected file is not a CSV. Please select a valid CSV file.");
//                csvFilePath = null;
//            }
//        }

//        frameRate = EditorGUILayout.IntField("Frame Rate", frameRate);
//        timePerCharacter = EditorGUILayout.FloatField("Time Per Character", timePerCharacter);

//        EditorGUI.BeginDisabledGroup(timelineAsset == null || string.IsNullOrEmpty(csvFilePath));
//        if (GUILayout.Button("Create Subtitle Clips"))
//        {
//            List<SubtitleInfo> list = ReadSubtitlesFromCSV(csvFilePath);
//            if (list.Count > 0)
//            {
//                CreateSubtitleClips(timelineAsset, list, frameRate);
//            }
//            else
//            {
//                Debug.LogWarning("No valid subtitle info found in the selected CSV file.");
//            }
//        }
//        EditorGUI.EndDisabledGroup();

//        GUILayout.Space(20);
//        GUILayout.Label("Batch Process", EditorStyles.boldLabel);

//        timelineFolderPath = EditorGUILayout.TextField("Timeline Assets Folder Path", timelineFolderPath);
//        csvFolderPath = EditorGUILayout.TextField("CSV Files Folder Path", csvFolderPath);

//        if (GUILayout.Button("Batch Create Subtitle Clips"))
//        {
//            BatchCreateSubtitles();
//        }
//    }

//    private List<SubtitleInfo> ReadSubtitlesFromCSV(string filePath)
//    {
//        List<SubtitleInfo> subtitles = new List<SubtitleInfo>();
//        try
//        {
//            using (StreamReader reader = new StreamReader(filePath, System.Text.Encoding.GetEncoding("GBK")))
//            {
//                string line;
//                while ((line = reader.ReadLine()) != null)
//                {
//                    if (string.IsNullOrWhiteSpace(line)) continue;
//                    var parts = line.Split(',');
//                    if (parts.Length < 1) continue;

//                    var text = parts[0].Trim();
//                    int startFrame;
//                    if (parts.Length < 2 || !int.TryParse(parts[1].Trim(), out startFrame))
//                    {
//                        startFrame = -1;
//                        Debug.LogWarning($"No start frame specified for line: {line}. It will be calculated automatically.");
//                    }

//                    subtitles.Add(new SubtitleInfo { startFrame = startFrame == -1 ? (int?)null : startFrame, text = text });
//                }
//            }
//        }
//        catch (Exception e)
//        {
//            Debug.LogError($"Failed to read CSV file at {filePath}: {e.Message}");
//        }
//        return subtitles;
//    }

//    private void CreateSubtitleClips(TimelineAsset timelineAsset, List<SubtitleInfo> subtitles, int frameRate)
//    {
//        string trackName = "AUTO_Subtitle";
//        List<TrackAsset> tracksToDelete = new List<TrackAsset>();

//        // Collect tracks to delete
//        foreach (var track in timelineAsset.GetOutputTracks())
//        {
//            if (track.name == trackName)
//            {
//                tracksToDelete.Add(track);
//            }
//        }

//        // Safely delete collected tracks
//        if (tracksToDelete.Count > 0)
//        {
//            Undo.RecordObject(timelineAsset, "Delete Subtitle Track");
//            foreach (var track in tracksToDelete)
//            {
//                timelineAsset.DeleteTrack(track);
//            }
//            Debug.Log($"Deleted {tracksToDelete.Count} tracks named '{trackName}'.");
//        }

//        // Create new track and add subtitles
//        CutsceneFunctionTrack newTrack = timelineAsset.CreateTrack<CutsceneFunctionTrack>(null, trackName);
//        double currentTime = 0.0;

//        foreach (var subtitle in subtitles)
//        {
//            SubTitleAsset asset = ScriptableObject.CreateInstance<SubTitleAsset>();
//            asset.subTitle = subtitle.text;

//            TimelineClip clip = newTrack.CreateDefaultClip();
//            clip.displayName = subtitle.text;
//            clip.asset = asset;

//            if (subtitle.startFrame.HasValue)
//            {
//                currentTime = (double)subtitle.startFrame.Value / frameRate;
//            }

//            clip.start = currentTime;
//            clip.duration = CalculateDurationFromText(subtitle.text);

//            currentTime += clip.duration;
//        }

//        EditorUtility.SetDirty(timelineAsset);
//        AssetDatabase.SaveAssets();

//        Debug.Log($"Subtitle clips created for {timelineAsset.name} successfully from CSV.");
//    }

//    private void BatchCreateSubtitles()
//    {
//        if (string.IsNullOrEmpty(timelineFolderPath) || string.IsNullOrEmpty(csvFolderPath))
//        {
//            Debug.LogWarning("Please specify both the timeline assets folder path and the CSV files folder path.");
//            return;
//        }

//        string[] timelinePaths = Directory.GetFiles(timelineFolderPath, "*.playable");
//        string[] csvPaths = Directory.GetFiles(csvFolderPath, "*.csv");

//        foreach (string timelinePath in timelinePaths)
//        {
//            string timelineName = Path.GetFileNameWithoutExtension(timelinePath);
//            string matchingCsvPath = Array.Find(csvPaths, csvPath =>
//                Path.GetFileNameWithoutExtension(csvPath).Equals(timelineName, StringComparison.OrdinalIgnoreCase));

//            if (!string.IsNullOrEmpty(matchingCsvPath))
//            {
//                TimelineAsset timelineAsset = AssetDatabase.LoadAssetAtPath<TimelineAsset>(timelinePath);
//                List<SubtitleInfo> subtitles = ReadSubtitlesFromCSV(matchingCsvPath);
//                if (timelineAsset != null && subtitles.Count > 0)
//                {
//                    CreateSubtitleClips(timelineAsset, subtitles, frameRate);
//                }
//            }
//        }

//        Debug.Log("Batch subtitle creation completed.");
//    }

//    private double CalculateDurationFromText(string text)
//    {
//        int characterCount = text.Length;
//        double duration = characterCount * timePerCharacter;
//        return duration;
//    }
//}