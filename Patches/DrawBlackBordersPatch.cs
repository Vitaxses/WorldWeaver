using UnityEngine.SceneManagement;
using WorldWeaver.Managers;

namespace WorldWeaver.Patches;

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