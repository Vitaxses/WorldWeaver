using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.SceneManagement;
using UnityEngine;

// Some very cursed window code

namespace WorldWeaver.Editor.Windows
{
    public class SceneTeleportMapWindow : EditorWindow
    {
        [MenuItem("WorldWeaver/SceneTeleportMap")]
        public static void Open()
        {
            GetWindow<SceneTeleportMapWindow>("WorldWeaver - SceneTeleportMap");
        }

        public static bool AutoGenerateTPMWithImport { get => WorldWeaverSettings.Instance.SceneTeleportMapAutoGenerateWithImport; set { WorldWeaverSettings.Instance.SceneTeleportMapAutoGenerateWithImport = value; EditorUtility.SetDirty(WorldWeaverSettings.Instance); }}
        public static bool IncludeScenesNotAddressable { get => WorldWeaverSettings.Instance.SceneTeleportMapIncludeNonAddressableScenes; set { WorldWeaverSettings.Instance.SceneTeleportMapIncludeNonAddressableScenes = value; EditorUtility.SetDirty(WorldWeaverSettings.Instance); }}

        private static bool SearchSubfolders { get => WorldWeaverSettings.Instance.SceneTeleportMapSearchSubfolders; set { WorldWeaverSettings.Instance.SceneTeleportMapSearchSubfolders = value; EditorUtility.SetDirty(WorldWeaverSettings.Instance); }}
        private static bool SkipHashCheck { get => WorldWeaverSettings.Instance.SceneTeleportMapSkipHashCheck; set { WorldWeaverSettings.Instance.SceneTeleportMapSkipHashCheck = value; EditorUtility.SetDirty(WorldWeaverSettings.Instance); }}
        private static int LintVer { get => WorldWeaverSettings.Instance.SceneTeleportMapLintVer; set { WorldWeaverSettings.Instance.SceneTeleportMapLintVer = value; EditorUtility.SetDirty(WorldWeaverSettings.Instance); }}
        private static string SelectedOutputPath { get => WorldWeaverSettings.Instance.SceneTeleportMapOutputPath; set { WorldWeaverSettings.Instance.SceneTeleportMapOutputPath = value; EditorUtility.SetDirty(WorldWeaverSettings.Instance); }}
        private static string SelectedInputPath { get => WorldWeaverSettings.Instance.SceneTeleportMapInputPath; set { WorldWeaverSettings.Instance.SceneTeleportMapInputPath = value; EditorUtility.SetDirty(WorldWeaverSettings.Instance); }}

        private static SceneSelectMode SceneSelectMode { get => WorldWeaverSettings.Instance.SceneTeleportMapSceneSelectMode; set { WorldWeaverSettings.Instance.SceneTeleportMapSceneSelectMode = value; EditorUtility.SetDirty(WorldWeaverSettings.Instance); }}

        private void OnGUI()
        {
            GUILayout.Label("SceneTeleportMap", EditorStyles.boldLabel);
            GUILayout.Space(4);

            DrawOptions();
            DrawInputSelection();
            DrawOutputSelection();
            DrawGenerateButton();
        }

        static void DrawOptions()
        {
            AutoGenerateTPMWithImport = EditorGUILayout.ToggleLeft("Auto-Generate on Asset Import", AutoGenerateTPMWithImport);
            
            GUILayout.Space(2);
            if (SceneSelectMode == SceneSelectMode.Folder)
                IncludeScenesNotAddressable = EditorGUILayout.ToggleLeft("Include Non-Addressable Scenes", IncludeScenesNotAddressable);
            
            LintVer = EditorGUILayout.IntField("Version", LintVer);

            GUILayout.Space(2);

            SceneSelectMode newMode = (SceneSelectMode)EditorGUILayout.EnumPopup("Scene Source", SceneSelectMode);
            if (newMode != SceneSelectMode)
            {
                SceneSelectMode = newMode;
                SelectedInputPath = string.Empty;
            }
        }

        static void DrawInputSelection()
        {
            if (SceneSelectMode == SceneSelectMode.Folder)
            {
                SearchSubfolders = EditorGUILayout.ToggleLeft("Search Subfolders", SearchSubfolders);

                string buttonLabel = string.IsNullOrEmpty(SelectedInputPath) ? "Select Scene Folder" : $"Select Scene Folder ({SelectedInputPath})";

                if (GUILayout.Button(buttonLabel))
                {
                    var newPathh = EditorUtility.OpenFolderPanel("Select Scene Folder", Application.dataPath, string.Empty);
                    if (!string.IsNullOrEmpty(newPathh))
                        SelectedInputPath = newPathh.Replace(Application.dataPath, "Assets").Replace("\\", "/");
                }

                return;
            }

            if (!GUILayout.Button(string.IsNullOrEmpty(SelectedInputPath) ? "Select Scene" : $"Select Scene ({SelectedInputPath})"))
                return;

            var newPath = EditorUtility.OpenFilePanel("Select Scene", Application.dataPath, "unity");
            if (!string.IsNullOrEmpty(newPath))
                SelectedInputPath = newPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");
        }

        static void DrawOutputSelection()
        {
            string buttonLabel = string.IsNullOrEmpty(SelectedOutputPath) ? "Select Output Folder" : $"Select Output Folder ({SelectedOutputPath})";

            if (GUILayout.Button(buttonLabel))
            {
                var newPath = EditorUtility.OpenFolderPanel("Select Output Folder", string.IsNullOrEmpty(SelectedOutputPath) ? Application.dataPath : GetFullPath(true), string.Empty);
                if (!string.IsNullOrEmpty(newPath))
                    SelectedOutputPath = newPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");
            }

            EditorGUILayout.LabelField("Scene Source: ", SelectedInputPath);
        }

        static void DrawGenerateButton()
        {
            if (!IsValidSelection())
                return;

            int count = SceneSelectMode == SceneSelectMode.Folder ? GenerateFromFolder(generate: false) : 1;

            SkipHashCheck = EditorGUILayout.ToggleLeft("Skip Hash Check", SkipHashCheck);

            if (!GUILayout.Button($"Generate SceneTeleportMap from " + (SceneSelectMode == SceneSelectMode.Folder ? count + (count == 1 ? " scene" : " scenes") : Path.GetFileName(GetFullPath(false)))))
                return;

            Generate();
        }

        public static void Generate()
        {
            if (string.IsNullOrEmpty(SelectedInputPath) || string.IsNullOrEmpty(SelectedOutputPath))
            {
                return;
            }

            SelectedInputPath = SelectedInputPath.Replace("\\", "/");

            if (SceneSelectMode == SceneSelectMode.SingleScene)
            {
                Generate([GetFullPath(false)]);
                return;
            }

            GenerateFromFolder();
        }

        static bool IsValidSelection()
        {
            if (string.IsNullOrEmpty(SelectedInputPath) || string.IsNullOrEmpty(SelectedOutputPath))
                return false;

            return (SceneSelectMode == SceneSelectMode.Folder ? Directory.Exists(GetFullPath(false)) : File.Exists(GetFullPath(false))) && Directory.Exists(GetFullPath(true));
        }


        static void Generate(string[] scenes)
        {
            if (scenes == null || scenes.Length == 0)
                return;

            if (!IncludeScenesNotAddressable && SceneSelectMode == SceneSelectMode.Folder)
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;

                if (settings == null)
                {
                    Debug.LogError("[WorldWeaver] Addressables settings null");
                    return;
                }

                bool cancelled = false;

                scenes = scenes.Where(path =>
                {
                    if (cancelled)
                    {
                        return false;
                    }
                
                    cancelled = EditorUtility.DisplayCancelableProgressBar("Generating SceneTeleportMap", $"", 0.15f);

                    if (string.IsNullOrEmpty(path))
                        return false;

                    string guid = AssetDatabase.AssetPathToGUID(FileUtil.GetProjectRelativePath(path));
                    var entry = settings.FindAssetEntry(guid);

                    if (entry == null || entry.parentGroup == null)
                        return false;

                    var schema = entry.parentGroup.GetSchema<BundledAssetGroupSchema>();

                    return schema != null && schema.IncludeInBuild;
                }).ToArray();

                if (cancelled)
                {
                    EditorUtility.ClearProgressBar();
                    Debug.Log("[WorldWeaver] Cancelled SceneTeleportMap creation");
                }
            }

            string previousScenePath = EditorSceneManager.GetActiveScene().path;
            bool previousSceneValid = EditorSceneManager.GetActiveScene().IsValid();
            var sceneInfos = new Dictionary<string, SceneTeleportMap.SceneInfo>();

            string relativeOutputPath = Path.Combine(SelectedOutputPath, "SceneTeleportMap.asset");

            SceneTeleportMap? oldTeleportMap = null;
            if (AssetDatabase.AssetPathExists(relativeOutputPath) && !SkipHashCheck)
            {
                oldTeleportMap = AssetDatabase.LoadAssetAtPath<SceneTeleportMap>(relativeOutputPath);
            }
            
            try
            {
                int totalScenes = scenes.Length;

                for (int i = 0; i < totalScenes; i++)
                {
                    string scenePath = scenes[i];
                    float progress = (i + 1) / (float)totalScenes;
                    bool cancelled = EditorUtility.DisplayCancelableProgressBar("Generating SceneTeleportMap", $"Processing scene {i + 1}/{totalScenes}: {Path.GetFileName(scenePath)}", progress);

                    if (cancelled)
                    {
                        Debug.Log("[WorldWeaver] SceneTeleportMap creation cancelled");
                        return;
                    }

                    if (oldTeleportMap != null && oldTeleportMap.lintVer == LintVer)
                    {
                        var oldData = oldTeleportMap.sceneList.GetSceneInfo(Path.GetFileNameWithoutExtension(scenePath));
                        if (!string.IsNullOrEmpty(oldData.SceneFileHash) && oldData.SceneFileHash == GetFileHash(scenePath))
                        {
                            sceneInfos[scenePath] = oldData;
                            continue;
                        }
                    }

                    SceneTeleportMap.SceneInfo? data = GetData(scenePath);

                    if (data == null)
                        continue;

                    sceneInfos[scenePath] = data;
                }

                EditorUtility.DisplayProgressBar("Generating SceneTeleportMap", $"Finished processing {totalScenes} scenes", 0.5f);
            }
            finally
            {
                if (previousSceneValid)
                    EditorSceneManager.OpenScene(previousScenePath);
                
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayProgressBar("Generating SceneTeleportMap", "Writing file...", 0.8f);

            if (oldTeleportMap != null)
            {
                EditorUtility.DisplayProgressBar("Generating SceneTeleportMap", "Deleting old file...", 0.9f);
                AssetDatabase.DeleteAsset(relativeOutputPath);
                AssetDatabase.Refresh();
            }

            SceneTeleportMap map = CreateInstance<SceneTeleportMap>();
            map.sceneList = new();
            map.lintVer = LintVer;

            foreach (var (scenePath, data) in sceneInfos)
            {
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);
                map.sceneList.SetData(sceneName, data);
            }

            EditorUtility.DisplayProgressBar("Generating SceneTeleportMap", "Creating asset...", 1f);

            AssetDatabase.CreateAsset(map, relativeOutputPath);
            
            string json = JsonUtility.ToJson(map, true);
            File.WriteAllText(Path.Combine(GetFullPath(true), "SceneTeleportMap.json"), json);

            EditorUtility.ClearProgressBar();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = map;
        }

        static int GenerateFromFolder(bool generate = true)
        {
            string[] scenes = Directory.GetFiles(GetFullPath(false), "*.unity", SearchSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            if (generate)
                Generate(scenes);

            return scenes.Length;
        }

        static SceneTeleportMap.SceneInfo? GetData(string scenePath)
        {
            Debug.Log($"Selected scene: {scenePath}");

            EditorSceneManager.OpenScene(scenePath);
            CustomSceneManager sceneManager = FindAnyObjectByType<CustomSceneManager>(FindObjectsInactive.Include);

            if (sceneManager == null)
            {
                Debug.LogWarning($"Scene '{scenePath}' does not have any CustomSceneManager component");
                return null;
            }

            var data = new SceneTeleportMap.SceneInfo
            {
                MapZone = sceneManager.mapZone,
                SceneFileHash = GetFileHash(scenePath)
            };

            var transitions = FindObjectsByType<TransitionPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < transitions.Length; i++)
            {
                var transition = transitions[i];

                if (!data.RespawnPoints.Contains(transition.name))
                    data.RespawnPoints.Add(transition.name);
            }

            var respawns = FindObjectsByType<RespawnMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < respawns.Length; i++)
            {
                var respawn = respawns[i];

                if (!data.RespawnPoints.Contains(respawn.name))
                    data.RespawnPoints.Add(respawn.name);
            }
            
            var benches = FindObjectsByType<RestBench>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < benches.Length; i++)
            {
                var respawn = benches[i];

                if (!data.RespawnPoints.Contains(respawn.name))
                    data.RespawnPoints.Add(respawn.name);
            }

            return data;
        }

        static string GetFileHash(string scenePath) => Hash128.Compute(File.ReadAllBytes(scenePath)).ToString();
        static string GetFullPath(bool isOutput) => Path.Combine(Application.dataPath, (isOutput ? SelectedOutputPath : SelectedInputPath).Replace("Assets/", ""));
    }
}