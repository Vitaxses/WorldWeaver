using System.IO;
using UnityEngine;

namespace WorldWeaver.Managers;

public static class WeaverSceneMapManager
{
    internal static readonly List<SceneTeleportMapSource> registeredSceneTpMaps = [];

    #region Registration

    public static void Register(SceneTeleportMapSource source)
    {
        if (source == null)
        {
            Plugin.Instance.Logger.LogWarning("Null SceneTeleportMapSource ignored");
            return;
        }

        registeredSceneTpMaps.Add(source);
    }

    public static void RegisterJsonString(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Plugin.Instance.Logger.LogWarning("Empty JSON ignored");
            return;
        }

        Register(new JsonStringMapSource
        {
            Json = json
        });
    }

    public static void RegisterJsonFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            Plugin.Instance.Logger.LogWarning("Empty JSON path ignored");
            return;
        }

        Register(new JsonFileMapSource
        {
            Path = path
        });
    }

    public static void RegisterAssetBundle(string path, string assetName, bool unloadBundle = false)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            Plugin.Instance.Logger.LogWarning("Empty AssetBundle path ignored");
            return;
        }

        if (string.IsNullOrWhiteSpace(assetName))
        {
            Plugin.Instance.Logger.LogWarning("Empty AssetBundle asset name ignored");
            return;
        }

        Register(new AssetBundleFileMapSource
        {
            Path = path,
            AssetName = assetName,
            UnloadBundle = unloadBundle
        });
    }

    #endregion

    #region Loading

    public static SceneTeleportMap? LoadMap(SceneTeleportMapSource source)
    {
        return source switch
        {
            JsonStringMapSource js => LoadFromJson(js.Json),
            JsonFileMapSource jf => LoadJsonFile(jf.Path),
            AssetBundleFileMapSource bf => LoadFromBundleFile(bf),
            AssetBundleEmbeddedMapSource be => LoadFromBundle(be.Bundle, be.AssetName),
            _ => throw new ArgumentException($"{nameof(source)}: {source}")
        };
    }

    private static SceneTeleportMap? LoadJsonFile(string path)
    {
        if (!File.Exists(path))
        {
            Plugin.Instance.Logger.LogWarning($"SceneTeleportMap json missing: {path}");
            return null;
        }

        string json = File.ReadAllText(path);
        return LoadFromJson(json);
    }

    private static SceneTeleportMap LoadFromJson(string json)
    {
        var map = ScriptableObject.CreateInstance<SceneTeleportMap>();
        JsonUtility.FromJsonOverwrite(json, map);
        return map;
    }

    private static SceneTeleportMap? LoadFromBundleFile(AssetBundleFileMapSource source)
    {
        if (!File.Exists(source.Path))
        {
            Plugin.Instance.Logger.LogWarning($"AssetBundle missing: {source.Path}");
            return null;
        }

        var bundle = AssetBundle.LoadFromFile(source.Path);

        if (bundle == null)
        {
            Plugin.Instance.Logger.LogWarning($"Failed loading AssetBundle: {source.Path}");
            return null;
        }

        try
        {
            return LoadFromBundle(bundle, source.AssetName);
        }
        finally
        {
            if (source.UnloadBundle)
                bundle.Unload(false);
        }
    }

    private static SceneTeleportMap? LoadFromBundle(AssetBundle bundle, string assetName)
    {
        var map = bundle.LoadAsset<SceneTeleportMap>(assetName);

        if (map == null)
        {
            Plugin.Instance.Logger.LogWarning($"Missing SceneTeleportMap asset '{assetName}' in bundle");
        }

        return map;
    }

    #endregion

    #region Merge

    internal static void MergeMap(SceneTeleportMap? map)
    {
        if (map == null || map.sceneList == null)
            return;

        var teleportMap = map.sceneList.GetAllSceneInfo();
        if (teleportMap == null)
            return;

        foreach (var (sceneName, info) in teleportMap)
        {
            if (string.IsNullOrWhiteSpace(sceneName) || info == null)
                continue;

            if (!string.IsNullOrWhiteSpace(info.SceneFileHash))
                SceneTeleportMap.RecordHash(sceneName, info.SceneFileHash); // Not actually sure what SceneFileHash is used for

            SceneTeleportMap.AddMapZone(sceneName, info.MapZone);

            AddList(sceneName, info.TransitionGates, SceneTeleportMap.AddTransitionGate);
            AddList(sceneName, info.RespawnPoints, SceneTeleportMap.AddRespawnPoint);
        }
    }

    private static void AddList(string scene, IEnumerable<string>? values, Action<string, string> addAction)
    {
        if (values == null) return;

        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
                addAction(scene, v);
        }
    }

    #endregion

    #region Source Types
    
    public abstract class SceneTeleportMapSource { }

    internal sealed class JsonStringMapSource : SceneTeleportMapSource
    {
        public string Json = string.Empty;
    }

    internal sealed class JsonFileMapSource : SceneTeleportMapSource
    {
        public string Path = string.Empty;
    }

    internal sealed class AssetBundleFileMapSource : SceneTeleportMapSource
    {
        public string Path = string.Empty;
        public string AssetName = string.Empty;
        public bool UnloadBundle;
    }

    internal sealed class AssetBundleEmbeddedMapSource : SceneTeleportMapSource
    {
        public AssetBundle Bundle = null!;
        public string AssetName = string.Empty;
    }

    #endregion
}