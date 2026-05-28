using System.IO;
using UnityEngine;
using WorldWeaver.Managers;

namespace WorldWeaver.Patches;

[HarmonyPatch]
internal static class SceneTeleportMapLoadPatch
{
    [HarmonyPatch(typeof(SceneTeleportMap), nameof(SceneTeleportMap.Load))]
    [HarmonyPostfix]
    static void Load(SceneTeleportMap __instance)
    {
        #if DEBUG
        string json = JsonUtility.ToJson(Resources.Load<SceneTeleportMap>("SceneTeleportMap"), true);

        File.WriteAllText(Path.Combine(Paths.PluginPath, "SceneTeleportMap.json"), json);
        #endif
        
        foreach (var source in WeaverSceneMapManager.registeredSceneTpMaps)
        {
            SceneTeleportMap? map = WeaverSceneMapManager.LoadMap(source);

            if (map == null)
                continue;

            WeaverSceneMapManager.MergeMap(map);
        }
    }

}
