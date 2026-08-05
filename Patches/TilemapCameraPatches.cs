using UnityEngine.SceneManagement;
using WorldWeaver.Managers;

namespace WorldWeaver.Patches;

[HarmonyPatch(typeof(CameraController))]
internal static class TilemapCameraPatches
{
    // Override tilemap sizing cleanly
    [HarmonyPatch(nameof(CameraController.GetTilemapInfo))]
    [HarmonyPrefix]
    static bool OverrideTilemapInfo(CameraController __instance)
    {
        if (!WeaverTilemapManager.IsCustomTilemapScene(SceneManager.GetActiveScene().name))
            return true;

        var gm = GameManager.instance;
        if (gm == null) return true;

        float w = gm.sceneWidth;
        float h = gm.sceneHeight;

        __instance.sceneWidth = (int)w;
        __instance.sceneHeight = (int)h;

        __instance.xLimit = w - 14.6f;
        __instance.yLimit = h - 8.3f;
        return false;
    }
}