using UnityEngine.SceneManagement;
using WorldWeaver.Managers;

namespace WorldWeaver.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.RefreshTilemapInfo))]
internal static class RefreshTilemapInfoPatch
{
    [HarmonyPrefix]
    static bool DrawBlackBorders()
    {
        if (!WeaverTilemapManager.IsCustomTilemapScene(SceneManager.GetActiveScene().name))
            return true;

        WeaverTilemapManager.ConvertTk2dTilemaps();
        return false; // No more error spam
    }
}

// Ensure correct size before borders
[HarmonyPatch(typeof(CustomSceneManager), nameof(CustomSceneManager.DrawBlackBorders))]
internal static class DrawBlackBordersPatch
{
    [HarmonyPrefix]
    static void DrawBlackBorders()
    {
        if (!WeaverTilemapManager.IsCustomTilemapScene(SceneManager.GetActiveScene().name))
            return;

        WeaverTilemapManager.UpdateSceneDimensions();
    }
}