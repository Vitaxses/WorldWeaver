using WorldWeaver.Managers;

namespace WorldWeaver.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.RefreshTilemapInfo))]
internal static class RefreshTilemapInfoPatch
{
    [HarmonyPrefix]
    static bool RefreshTilemapInfo()
    {
        if (!WeaverTilemapManager.IsCustomTilemapScene())
            return true;
        
        WeaverTilemapManager.UpdateSceneDimensions();
        return false; // This should stop the tilemap not found error spam
    }
}