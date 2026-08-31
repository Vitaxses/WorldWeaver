using System.IO;
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

        File.WriteAllText(Path.Combine(Path.GetDirectoryName(Plugin.Instance.Info.Location), "SceneTeleportMap.json"), json);
        #endif
        
        foreach (var source in WeaverSceneMapManager.registeredSceneTpMaps)
        {
            if (source.IsAddressable)
            {
                var key = source.AddressablesKey;
                Addressables.LoadAssetAsync<SceneTeleportMap>(source.AddressablesKey).Completed += (handle) =>
                {
                    if (handle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {
                        Plugin.Instance.Logger.LogError($"Failed to load SceneTeleportMap: {handle.OperationException}");
                        return;
                    }

                    WeaverSceneMapManager.MergeMap(handle.Result);
                };
                continue;
            }

            SceneTeleportMap? map = source.Map;

            if (map != null)
                WeaverSceneMapManager.MergeMap(map);
        }
    }

}
