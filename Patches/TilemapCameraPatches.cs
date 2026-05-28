using UnityEngine.SceneManagement;
using WorldWeaver.Managers;

namespace WorldWeaver.Patches;

[HarmonyPatch(typeof(CameraController))]
internal static class TilemapCameraPatches
{
    static bool IsCustomTilemapScene => WeaverTilemapManager.IsCustomTilemapScene(SceneManager.GetActiveScene().name);

    // Override tilemap sizing cleanly
    [HarmonyPatch(nameof(CameraController.GetTilemapInfo))]
    [HarmonyPrefix]
    static bool OverrideTilemapInfo(CameraController __instance)
    {
        if (!IsCustomTilemapScene)
            return true;

        WeaverTilemapManager.UpdateSceneDimensions();

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

    // Fix bounds after leaving a cam lock
    [HarmonyPatch(nameof(CameraController.ReleaseLock))]
    [HarmonyPostfix]
    static void RefreshCameraLimits_ReleaseLock(CameraController __instance)
    {
        if (!IsCustomTilemapScene)
            return;

        ApplyCameraLimits(__instance);
    }

    // Fix bounds after entering a cam lock
    [HarmonyPatch(nameof(CameraController.LockToArea))]
    [HarmonyPostfix]
    static void RefreshCameraLimits_LockToArea(CameraController __instance)
    {
        if (!IsCustomTilemapScene)
            return;

        ApplyCameraLimits(__instance);
    }

    #pragma warning disable HARMONIZE004 // Undefined patch type
    private static void ApplyCameraLimits(CameraController cam)
    #pragma warning restore HARMONIZE004 // Undefined patch type
    {
        var gm = GameManager.instance;
        if (gm == null) return;

        float w = gm.sceneWidth;
        float h = gm.sceneHeight;

        cam.sceneWidth = (int)w;
        cam.sceneHeight = (int)h;

        cam.xLimit = w - 14.6f;
        cam.yLimit = h - 8.3f;
    }
}