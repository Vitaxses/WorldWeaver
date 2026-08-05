using UnityEngine.SceneManagement;
using WorldWeaver.Managers;

namespace WorldWeaver.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.RefreshTilemapInfo))]
internal static class RefreshTilemapInfoPatch
{
    [HarmonyPrefix]
    static void DrawBlackBorders()
    {
        if (!WeaverTilemapManager.IsCustomTilemapScene(SceneManager.GetActiveScene().name))
            return;

        WeaverTilemapManager.UpdateSceneDimensions();
    }
}