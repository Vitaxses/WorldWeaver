using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GlobalEnums;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WorldWeaver.Data.MonoBehaviours
{
    // Editor-testing port of WorldWeaver's WeaverSceneMapManager.MergeMap and WeaverTransitionManager
    // + TransitionPointStartPatch, driven as a runtime MonoBehaviour so no base-game scripts change.
    //
    // Loads two JSON data files from Assets/SilksoulData:
    //  - SceneTeleportMap.json  -> merged into the base SceneTeleportMap instance so custom
    //                              "_Silksoul" scenes get MapZone / TransitionGates / RespawnPoints.
    //  - TransitionFixer.json   -> rewires TransitionPoints whose gate name matches an override, so
    //                              custom scenes route to the right destination scene + door.
    //
    // Drop this on any object in the scene you are testing. It persists across scene loads.

    public sealed class SceneDataFixer : MonoBehaviour
    {
        [Tooltip("Folder (relative to Assets/) containing SceneTeleportMap.json and TransitionFixer.json.")]
        [SerializeField] private string dataFolder = "SilksoulData";

        private bool teleportMapLoaded;
        private bool transitionsLoaded;

        private readonly Dictionary<string, Dictionary<string, TransitionData>> transitionGroups =
            new Dictionary<string, Dictionary<string, TransitionData>>();

        [Serializable]
        private sealed class TransitionData
        {
            public string gateName = "";
            public string destinationScene = "";
            public string destinationGate = "";
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            if (!teleportMapLoaded)
            {
                teleportMapLoaded = true;
                LoadSceneTeleportMap();
            }
            if (!transitionsLoaded)
            {
                transitionsLoaded = true;
                LoadTransitionFixer();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(ApplyTransitionsAfterLoad());
        }

        private IEnumerator ApplyTransitionsAfterLoad()
        {
            yield return null;
            yield return null;

            string activeScene = SceneManager.GetActiveScene().name;
            if (!transitionGroups.ContainsKey(activeScene))
            {
                yield break;
            }

            Dictionary<string, TransitionData> gates = transitionGroups[activeScene];

            TransitionPoint[] points = UnityEngine.Object.FindObjectsByType<TransitionPoint>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            int applied = 0;
            for (int i = 0; i < points.Length; i++)
            {
                TransitionPoint tp = points[i];
                if (tp.gameObject.scene.name != activeScene)
                {
                    continue;
                }

                if (!gates.TryGetValue(tp.name, out TransitionData data))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(data.destinationGate))
                {
                    tp.SetTargetDoor(data.destinationGate);
                }

                if (!string.IsNullOrEmpty(data.destinationScene))
                {
                    tp.SetTargetScene(data.destinationScene);
                }

                applied++;
            }

            if (applied > 0)
            {
                Debug.Log("[SilkroadSceneDataFixer] Applied transition overrides to " + applied +
                        " TransitionPoint(s) in scene '" + activeScene + "'.");
            }
        }

        private void LoadSceneTeleportMap()
        {
            string path = Path.Combine(Application.dataPath, dataFolder, "SceneTeleportMap.json");
            if (!File.Exists(path))
            {
                Debug.LogWarning("[SilkroadSceneDataFixer] SceneTeleportMap.json not found at " + path);
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                SceneTeleportMapJson doc = JsonConvert.DeserializeObject<SceneTeleportMapJson>(json);
                if (doc == null || doc.sceneList == null || doc.sceneList.savedData == null)
                {
                    Debug.LogWarning("[SilkroadSceneDataFixer] SceneTeleportMap.json is empty or malformed.");
                    return;
                }

                int added = 0;
                foreach (SceneTeleportMapEntry entry in doc.sceneList.savedData)
                {
                    if (string.IsNullOrWhiteSpace(entry.Name) || entry.Data == null)
                    {
                        continue;
                    }

                    string sceneName = entry.Name;
                    SceneTeleportMapSceneData data = entry.Data;

                    SceneTeleportMap.RecordHash(sceneName, data.SceneFileHash);
                    SceneTeleportMap.AddMapZone(sceneName, (MapZone)data.MapZone);

                    if (data.TransitionGates != null)
                    {
                        for (int g = 0; g < data.TransitionGates.Count; g++)
                        {
                            SceneTeleportMap.AddTransitionGate(sceneName, data.TransitionGates[g]);
                        }
                    }

                    if (data.RespawnPoints != null)
                    {
                        for (int r = 0; r < data.RespawnPoints.Count; r++)
                        {
                            SceneTeleportMap.AddRespawnPoint(sceneName, data.RespawnPoints[r]);
                        }
                    }

                    added++;
                }

                Debug.Log("[SilkroadSceneDataFixer] Merged " + added +
                        " scene(s) from SceneTeleportMap.json into SceneTeleportMap.");
            }
            catch (Exception e)
            {
                Debug.LogError("[SilkroadSceneDataFixer] Failed to load SceneTeleportMap.json: " + e);
            }
        }

        private void LoadTransitionFixer()
        {
            string path = Path.Combine(Application.dataPath, dataFolder, "TransitionFixer.json");
            if (!File.Exists(path))
            {
                Debug.LogWarning("[SilkroadSceneDataFixer] TransitionFixer.json not found at " + path);
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                TransitionOverride doc = JsonConvert.DeserializeObject<TransitionOverride>(json);
                if (doc == null || doc.scenes == null)
                {
                    Debug.LogWarning("[SilkroadSceneDataFixer] TransitionFixer.json is empty or malformed.");
                    return;
                }

                for (int s = 0; s < doc.scenes.Length; s++)
                {
                    SceneTransitionGroup group = doc.scenes[s];
                    if (string.IsNullOrEmpty(group.sceneName) || group.transitions == null)
                    {
                        continue;
                    }

                    Dictionary<string, TransitionData> gateMap = new Dictionary<string, TransitionData>();
                    for (int t = 0; t < group.transitions.Length; t++)
                    {
                        TransitionData data = group.transitions[t];
                        if (!string.IsNullOrEmpty(data.gateName))
                        {
                            gateMap[data.gateName] = data;
                        }
                    }
                    transitionGroups[group.sceneName] = gateMap;
                }

                Debug.Log("[SilkroadSceneDataFixer] Loaded transition overrides for " +
                        transitionGroups.Count + " scene(s) from TransitionFixer.json.");
            }
            catch (Exception e)
            {
                Debug.LogError("[SilkroadSceneDataFixer] Failed to load TransitionFixer.json: " + e);
            }
        }

        [Serializable]
        private sealed class SceneTeleportMapJson
        {
            public int lintVer;
            public SceneTeleportMapSceneList sceneList;
        }

        [Serializable]
        private sealed class SceneTeleportMapSceneList
        {
            public List<SceneTeleportMapEntry> savedData;
        }

        [Serializable]
        private sealed class SceneTeleportMapEntry
        {
            public string Name;
            public SceneTeleportMapSceneData Data;
        }

        [Serializable]
        private sealed class SceneTeleportMapSceneData
        {
            public string SceneFileHash;
            public int MapZone;
            public List<string> TransitionGates;
            public List<string> RespawnPoints;
        }

        [Serializable]
        private sealed class TransitionOverride
        {
            public int overwritePriority = -1;
            public SceneTransitionGroup[] scenes;
        }

        [Serializable]
        private sealed class SceneTransitionGroup
        {
            public string sceneName;
            public TransitionData[] transitions;
        }
    }   
}