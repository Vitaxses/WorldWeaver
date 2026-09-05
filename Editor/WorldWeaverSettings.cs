using System;
using UnityEditor;
using UnityEngine;
using WorldWeaver.Editor.Windows;

namespace WorldWeaver.Editor
{
    [Serializable]
    public class WorldWeaverSettings : ScriptableObject
    {
        private const string Path = "Assets/WorldWeaverData/WorldWeaverSettings.asset";
        
        private static WorldWeaverSettings? _instance;
        public static WorldWeaverSettings Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = CreateInstance<WorldWeaverSettings>();

                if (AssetDatabase.AssetPathExists(Path))
                    return _instance = AssetDatabase.LoadAssetAtPath<WorldWeaverSettings>(Path);

                if (!AssetDatabase.IsValidFolder("Assets/WorldWeaverData"))
                    AssetDatabase.CreateFolder("Assets", "WorldWeaverData");

                AssetDatabase.CreateAsset(_instance, Path);
                _instance = AssetDatabase.LoadAssetAtPath<WorldWeaverSettings>(Path);
                AssetDatabase.SaveAssets();

                return _instance;
            }
        }

        [Header("WorldWeaver Settings")]
        [SerializeField]
        public string ModIdDefault = string.Empty;

        [Space(4)]
        [Header("Play Bootstrap")]

        [SerializeField]
        public bool PlayBootstrapEnabled;
        
        [SerializeField]
        public string PlayBootstrapBootScene = "Menu_Title";

        [SerializeField]
        public GameObject? PlayBootstrapBootGameManagerPrefab;
        
        [SerializeField]
        public GameObject? PlayBootstrapBootUIManagerPrefab;
        
        [SerializeField]
        public GameObject? PlayBootstrapBootGameCamerasPrefab;

        [SerializeField]
        public int PlayBootstrapTimeoutFrames = 900;
        
        [SerializeField]
        public KeyCode PlayBootstrapNoClipKey = KeyCode.Comma;
        
        [SerializeField]
        public KeyCode PlayBootstrapInvincibilityKey = KeyCode.Period;

        [Space(4)]
        [Header("Scene Teleport Map")]

        [SerializeField]
        public bool SceneTeleportMapAutoGenerateWithImport;
        
        [SerializeField]
        public bool SceneTeleportMapIncludeNonAddressableScenes;
        
        [SerializeField]
        public bool SceneTeleportMapSearchSubfolders = true;

        [SerializeField]
        public bool SceneTeleportMapSkipHashCheck = false;
        
        [SerializeField]
        public int SceneTeleportMapLintVer;

        [SerializeField]
        public string SceneTeleportMapOutputPath = string.Empty;
        
        [SerializeField]
        public string SceneTeleportMapInputPath = string.Empty;
        
        [SerializeField]
        public SceneSelectMode SceneTeleportMapSceneSelectMode;
        
        [Space(4)]
        [Header("Scene Tools")]

        [SerializeField]
        public SceneSelectMode SceneToolsSceneSelectMode;
        
        [SerializeField]
        public string SceneToolsInputPath = string.Empty;
        
        [SerializeField]
        public bool SceneToolsIncludeSubfolders = true;
        
        [SerializeField]
        public bool SceneToolsSkipFsmActions = true;
    }   
}