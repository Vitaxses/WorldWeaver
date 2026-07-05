using WorldWeaver.Managers;

namespace WorldWeaver.Patches;

[HarmonyPatch(typeof(InventoryMapManager))]
public static class InventoryMapManagerPatch
{
    [HarmonyPatch(nameof(InventoryMapManager.EnsureWideMapSpawned))]
    [HarmonyPrefix]
    public static void SetWideMapPrefab(InventoryMapManager __instance)
    {
        if (WeaverMapManager.WideMap.prefab == null)
            return;

        __instance.wideMapPrefab = WeaverMapManager.WideMap.prefab.GetComponent<InventoryWideMap>();
    }
    
    [HarmonyPatch(nameof(InventoryMapManager.EnsureGameMapSpawned))]
    [HarmonyPrefix]
    public static void SetGameMapPrefab(InventoryMapManager __instance)
    {
        if (WeaverMapManager.GameMap.prefab == null)
            return;

        __instance.gameMapPrefab = WeaverMapManager.GameMap.prefab.GetComponent<GameMap>();
    }
    
}
