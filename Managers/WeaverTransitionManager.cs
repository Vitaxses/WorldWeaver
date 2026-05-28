using System.IO;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

namespace WorldWeaver.Managers;

public static class WeaverTransitionManager
{
    private static TransitionOverride? transitionOverride;
    private static SceneTransitionGroup? currentSceneGroup;

    public static void AddToTransitionRegistry(string json, bool isPath)
    {
        var data = JsonConvert.DeserializeObject<TransitionOverride>(isPath ? File.ReadAllText(json) : json);
        if (data == null)
            return;

        AddToTransitionRegistry(data);
    }

    public static void AddToTransitionRegistry(TransitionOverride data)
    {
        transitionOverride ??= new()
        {
            overwritePriority = data.overwritePriority,
            scenes = data.scenes
        };

        transitionOverride.scenesDict ??= new();

        if (data.overwritePriority < transitionOverride.overwritePriority)
            return;

        transitionOverride.overwritePriority = data.overwritePriority;

        foreach (var scene in data.scenes)
        {
            if (string.IsNullOrEmpty(scene.sceneName))
                continue;

            scene.transitionsDict = new();

            foreach (var transitionData in scene.transitions)
            {
                if (!string.IsNullOrEmpty(transitionData.gateName))
                    scene.transitionsDict[transitionData.gateName] = transitionData;
            }

            transitionOverride.scenesDict[scene.sceneName] = scene;
        }
    }

    public static SceneTransitionGroup? GetGroup(string sceneName)
    {
        if (transitionOverride == null)
            return null;

        transitionOverride.scenesDict.TryGetValue(sceneName, out var group);
        return group;
    }

    public static SceneTransitionGroup? GetCurrentGroup()
    {
        var currentScene = SceneManager.GetActiveScene().name;

        if (currentSceneGroup == null || currentSceneGroup.sceneName != currentScene)
            currentSceneGroup = GetGroup(currentScene);

        return currentSceneGroup;
    }

    [Serializable]
    public class TransitionOverride
    {
        public int overwritePriority = -1;

        public SceneTransitionGroup[] scenes = [];

        [NonSerialized]
        public Dictionary<string, SceneTransitionGroup> scenesDict = new();
    }

    [Serializable]
    public class SceneTransitionGroup
    {
        public string? sceneName;

        public TransitionData[] transitions = [];

        [NonSerialized]
        public Dictionary<string, TransitionData> transitionsDict = new();

        public TransitionData? GetTransitionData(string gateName)
        {
            if (string.IsNullOrEmpty(gateName))
                return null;

            transitionsDict.TryGetValue(gateName, out var data);
            return data;
        }
    }

    [Serializable]
    public class TransitionData
    {
        public string gateName = "";
        
        public string destinationScene = "";
        
        public string destinationGate = "";
    }
}
