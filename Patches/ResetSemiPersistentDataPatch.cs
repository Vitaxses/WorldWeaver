using WorldWeaver.Managers;

namespace WorldWeaver.Patches;

[HarmonyPatch(typeof(SceneData), nameof(SceneData.ResetSemiPersistentItems))]
internal static class ResetSemiPersistentDataPatch
{
    [HarmonyPostfix]
    static void ResetSemiPersistentItems(SceneData __instance)
    {
        WeaverDataManager.ResetSemiPersistentItems();
    }
}
