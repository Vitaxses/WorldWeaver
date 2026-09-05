using System;
using System.IO;
using System.Reflection;
using HutongGames.PlayMaker;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WorldWeaver.Data.Actions;
using WorldWeaver.Data.MonoBehaviours;

namespace WorldWeaver.Editor.Windows
{
    public class SceneToolsWindow : EditorWindow
    {
        [MenuItem("WorldWeaver/Scene Tools")]
        public static void Open()
        {
            GetWindow<SceneToolsWindow>("WorldWeaver - Scene Tools");
        }

        private static SceneSelectMode SceneMode { get => WorldWeaverSettings.Instance.SceneToolsSceneSelectMode; set { WorldWeaverSettings.Instance.SceneToolsSceneSelectMode = value; EditorUtility.SetDirty(WorldWeaverSettings.Instance); }}
        private static bool IncludeSubfolders { get => WorldWeaverSettings.Instance.SceneToolsIncludeSubfolders; set { WorldWeaverSettings.Instance.SceneToolsIncludeSubfolders = value; EditorUtility.SetDirty(WorldWeaverSettings.Instance); }}
        private static bool SkipFsmActions { get => WorldWeaverSettings.Instance.SceneToolsSkipFsmActions; set { WorldWeaverSettings.Instance.SceneToolsSkipFsmActions = value; EditorUtility.SetDirty(WorldWeaverSettings.Instance); }}
        private static string SelectedInputPath { get => WorldWeaverSettings.Instance.SceneToolsInputPath; set { WorldWeaverSettings.Instance.SceneToolsInputPath = value; EditorUtility.SetDirty(WorldWeaverSettings.Instance); }}

        private enum LoopScenesType
        {
            SetModId,
            Replace,
            PlayMakerGUI
        }

        void OnGUI()
        {
            DrawComponentHelperSection();
        }

        static void DrawComponentHelperSection()
        {
            GUILayout.Label("Component Helper", EditorStyles.boldLabel);
            GUILayout.Space(4);

            DrawComponentHelperSource();

            if (SceneMode == SceneSelectMode.Folder ? !Directory.Exists(GetFullInputPath()) : !File.Exists(GetFullInputPath()))
                return;

            DrawGenerate();
        }

        static void LoopScenes(LoopScenesType type)
        {
            var defaultModId = WorldWeaverSettings.Instance.ModIdDefault;
            string[] scenes = [];
            if (SceneMode == SceneSelectMode.Folder)
            {
                scenes = Directory.GetFiles(GetFullInputPath(), "*.unity", IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            } else
            {
                scenes = [GetFullInputPath()];
            }

            if (scenes.Length == 0)
                return;
 
            var title = "WorldWeaver - " + type switch
            {
                LoopScenesType.SetModId => "Setting ModIds",
                LoopScenesType.Replace => "Replacing Components",
                LoopScenesType.PlayMakerGUI => "Fixing PlayMakerGUI",

                _ => "Scene Tools"
            };
            
            string oldScene = SceneManager.GetActiveScene().path;
            int counter = 0;

            if (!EditorUtility.DisplayDialog(title, $"This will modify {scenes.Length} scene(s). This cannot be undone.\n\nContinue?", "Continue", "Cancel"))
            {
                return;
            }

            if (!SkipFsmActions && !EditorUtility.DisplayDialog(title, $"'Skip WorldWeaver PlayMaker Actions' is disabled.\n\nPlayMaker actions will be modified.\n\nContinue?", "Continue", "Cancel"))
            {
                return;
            }

            try
            {
                for (int i = 0; i < scenes.Length; i++)
                {
                    var scenePath = scenes[i];
                    var progress = i / (float)scenes.Length;

                    EditorUtility.DisplayProgressBar(title, $"Processing {Path.GetFileName(scenePath)}", progress);
                    var scene = EditorSceneManager.OpenScene(scenePath);

                    if (type == LoopScenesType.Replace)
                        Replace(scene, ref counter, defaultModId, progress, title);
                    else if (type == LoopScenesType.SetModId)
                        SetDefaultModIds(scene, ref counter, defaultModId, progress, title);
                    else if (type == LoopScenesType.PlayMakerGUI)
                        HandlePlayMakerGUI(scene, ref counter, defaultModId, progress, title);
                    
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            } 
            
            finally
            
            {
                EditorSceneManager.OpenScene(oldScene);
                EditorUtility.ClearProgressBar();
                Debug.Log($"[WorldWeaver] {title} completed. Processed {scenes.Length} scene(s). Modified {counter} component(s).");
            }
        }

        static void HandlePlayMakerGUI(Scene scene, ref int counter, string defaultModId, float progress, string title)
        {
            foreach (var gui in FindObjectsByType<PlayMakerGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!gui.controlMouseCursor)
                    continue;

                gui.controlMouseCursor = false;

                Debug.Log($"[WorldWeaver] Disabled controlMouseCursor on PlayMakerGUI: {gui.name} in {Path.GetFileName(scene.path)}");
                counter++;
            }
        }

        static void SetDefaultModIds(Scene scene, ref int counter, string modId, float progress, string title)
        {
            string sceneName = Path.GetFileName(scene.path);
            
            var geoRocks = FindObjectsByType<WeaverGeoRock>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (geoRocks.Length > 0)
                EditorUtility.DisplayProgressBar(title, $"Setting ModId on WeaverGeoRock Component in {sceneName}", progress);
            
            foreach (var weaverComponent in geoRocks)
            {
                if (weaverComponent.ModId != modId)
                {
                    weaverComponent.ModId = modId;
                    counter++;
                    Debug.Log($"[WorldWeaver] Set ModId to '{modId}' on WeaverGeoRock '{weaverComponent.name}' in scene '{sceneName}'.");
                }
            }
            
            var boolItems = FindObjectsByType<WeaverPersistentBoolItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (boolItems.Length > 0)
                EditorUtility.DisplayProgressBar(title, $"Setting ModId on WeaverPersistentBoolItem Component in {sceneName}", progress);

            foreach (var weaverComponent in boolItems)
            {
                if (weaverComponent.ModId != modId)
                {
                    weaverComponent.ModId = modId;
                    counter++;
                    Debug.Log($"[WorldWeaver] Set ModId to '{modId}' on WeaverPersistentBoolItem '{weaverComponent.name}' in scene '{sceneName}'.");
                }
            }
            
            var intItems = FindObjectsByType<WeaverPersistentIntItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (intItems.Length > 0)
                EditorUtility.DisplayProgressBar(title, $"Setting ModId on WeaverPersistentIntItem Component in {sceneName}", progress);
            
            foreach (var weaverComponent in intItems)
            {
                if (weaverComponent.ModId != modId)
                {
                    weaverComponent.ModId = modId;
                    counter++;
                    Debug.Log($"[WorldWeaver] Set ModId to '{modId}' on WeaverPersistentIntItem '{weaverComponent.name}' in scene '{sceneName}'.");
                }
            }

            if (SkipFsmActions)
                return;
            
            EditorUtility.DisplayProgressBar(title, $"Setting ModId on WorldWeaver PlayMaker Action in {sceneName}", progress);
            foreach (var fsm in FindObjectsByType<PlayMakerFSM>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                foreach (var state in fsm.FsmStates)
                {
                    foreach (var action in state.Actions)
                    {
                        var type = action.GetType();
                        if (type.Namespace != typeof(WeaverDataBoolTest).Namespace)
                            continue;

                        var field = type.GetField("ModId", BindingFlags.Instance | BindingFlags.Public);
                        if (field == null || field.FieldType != typeof(FsmString))
                            continue;

                        if (field.GetValue(action) is not FsmString fsmString)
                            continue;

                        if (fsmString.value == modId)
                            continue;

                        fsmString.Value = modId;
                        counter++;
                        Debug.Log($"Set ModId on action: {type.Name} on state: {state.Name} on PlayMakerFSM: (gameobject: {fsm.gameObject.name} FsmName: {fsm.FsmName}) in scene: {sceneName}");   
                    }
                }
            }
        }

        static void Replace(Scene scene, ref int counter, string modId, float progress, string title)
        {
            string sceneName = Path.GetFileName(scene.path);

            var intItems = FindObjectsByType<PersistentIntItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (intItems.Length > 0)
                EditorUtility.DisplayProgressBar(title, $"Replacing PersistentIntItem Component in {sceneName}", progress);

            foreach (var persistent in intItems)
            {
                if (persistent is WeaverPersistentIntItem)
                    continue;

                var itemData = persistent.ItemData;
                var go = persistent.gameObject;

                ReplaceComponent<PersistentIntItem, WeaverPersistentIntItem>(persistent, weaverComponent =>
                {
                    weaverComponent.itemData = (PersistentIntItem.PersistentIntData)itemData;
                    weaverComponent.ModId = modId;
                });

                counter++;
                Debug.Log($"[WorldWeaver] Replaced PersistentIntItem with WeaverPersistentIntItem on GameObject '{go.name}' in scene {sceneName}");
            }

            var boolItems = FindObjectsByType<PersistentBoolItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (boolItems.Length > 0)
                EditorUtility.DisplayProgressBar(title, $"Replacing PersistentBoolItem Component in {sceneName}", progress);

            foreach (var persistent in boolItems)
            {
                if (persistent is WeaverPersistentBoolItem)
                    continue;

                var itemData = persistent.ItemData;
                var go = persistent.gameObject;

                ReplaceComponent<PersistentBoolItem, WeaverPersistentBoolItem>(persistent, weaverComponent =>
                {
                    weaverComponent.itemData = (PersistentBoolItem.PersistentBoolData)itemData;
                    weaverComponent.ModId = modId;
                });

                counter++;
                Debug.Log($"[WorldWeaver] Replaced PersistentBoolItem with WeaverPersistentBoolItem on GameObject '{go.name}' in scene {sceneName}");
            }
            
            var geoRocks = FindObjectsByType<GeoRock>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (geoRocks.Length > 0)
                EditorUtility.DisplayProgressBar(title, $"Replacing GeoRock Component in {sceneName}", progress);

            foreach (var geoRock in FindObjectsByType<GeoRock>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (geoRock is WeaverGeoRock)
                    continue;

                var geoRockData = geoRock.geoRockData;
                var go = geoRock.gameObject;

                ReplaceComponent<GeoRock, WeaverGeoRock>(geoRock, weaverComponent =>
                {
                    weaverComponent.geoRockData = geoRockData;
                    weaverComponent.ModId = modId;
                });

                counter++;
                Debug.Log($"[WorldWeaver] Replaced GeoRock with WeaverGeoRock on GameObject '{go.name}' in scene {sceneName}");
            }
        }


        static TNew ReplaceComponent<TOld, TNew>(TOld oldComp, Action<TNew> setData) where TOld : Component where TNew : Component
        {
            var go = oldComp.gameObject;
            var newComp = go.AddComponent<TNew>();

            setData(newComp);
            ReplaceReferences(oldComp, newComp);
            DestroyImmediate(oldComp);
            
            EditorUtility.SetDirty(newComp);
            return newComp;
        }

        static void ReplaceReferences(Component oldComp, Component newComp)
        {
            foreach (var comp in FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (comp == null || comp == oldComp)
                    continue;

                SerializedObject serializedObject = new(comp);
                SerializedProperty property = serializedObject.GetIterator();

                bool changed = false;

                while (property.NextVisible(true))
                {
                    if (property.propertyType != SerializedPropertyType.ObjectReference || property.objectReferenceValue != oldComp)
                        continue;

                    property.objectReferenceValue = newComp;
                    changed = true;
                }

                if (!changed)
                    continue;

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(comp);
            }
        }

        static void DrawComponentHelperSource()
        {
            SceneSelectMode newMode = (SceneSelectMode)EditorGUILayout.EnumPopup("Scene Source", SceneMode);
            if (newMode != SceneMode)
            {
                SceneMode = newMode;
                SelectedInputPath = string.Empty;
            }

            if (SceneMode == SceneSelectMode.Folder)
            {
                string buttonLabel = string.IsNullOrEmpty(SelectedInputPath) ? "Select Scene Folder" : $"Select Scene Folder ({Directory.GetFiles(GetFullInputPath(), "*.unity", IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Length} scenes in {GetProjectPath()})";

                if (GUILayout.Button(buttonLabel))
                {
                    var newPath = EditorUtility.OpenFolderPanel("Select Scene Folder", string.IsNullOrEmpty(SelectedInputPath) ? Application.dataPath : GetFullInputPath(), string.Empty);
                    if (!string.IsNullOrEmpty(newPath))
                        SelectedInputPath = newPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");
                }
                
                IncludeSubfolders = EditorGUILayout.ToggleLeft("Include Subfolders", IncludeSubfolders);
            } 
            
            else if (GUILayout.Button(string.IsNullOrEmpty(SelectedInputPath) ? "Select Scene" : $"Select Scene ({GetProjectPath()})"))
            {
                var newPath =  EditorUtility.OpenFilePanel("Select Scene", string.IsNullOrEmpty(SelectedInputPath) ? Application.dataPath : GetFullInputPath(), "unity");
                if (!string.IsNullOrEmpty(newPath))
                    SelectedInputPath = newPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");
            }
        }

        static void DrawGenerate()
        {
            GUILayout.Space(2);

            SkipFsmActions = EditorGUILayout.ToggleLeft("Skip WorldWeaver PlayMaker Actions", SkipFsmActions);
            if (GUILayout.Button($"Set All ModIds To Default ({WorldWeaverSettings.Instance.ModIdDefault})"))
            {
                LoopScenes(LoopScenesType.SetModId);
            }
            else if (GUILayout.Button($"Convert Components To WorldWeaver Components"))
            {
                LoopScenes(LoopScenesType.Replace);
            } else if (GUILayout.Button("Fix PlayMakerGUI"))
            {
                LoopScenes(LoopScenesType.PlayMakerGUI);
            }
            
            EditorGUILayout.LabelField("Scene Source:", GetProjectPath());
        }

        static string GetProjectPath() => SelectedInputPath;
        static string GetFullInputPath() => Path.Combine(Application.dataPath, SelectedInputPath.Replace("Assets/", ""));
    }
}