using WorldWeaver.Managers;

namespace WorldWeaver.Patches;

[HarmonyPatch(typeof(InventoryMapManager))]
public static class InventoryMapManagerPatch
{
    [HarmonyPatch(nameof(InventoryMapManager.EnsureWideMapSpawned))]
    [HarmonyPrefix]
    public static void SetWideMapPrefab(InventoryMapManager __instance)
    {
        if (WeaverMapManager.WideMap.Prefab == null)
            return;

        __instance.wideMapPrefab = WeaverMapManager.WideMap.Prefab.GetComponent<InventoryWideMap>();
    }
    
    [HarmonyPatch(nameof(InventoryMapManager.EnsureGameMapSpawned))]
    [HarmonyPrefix]
    public static void SetGameMapPrefab(InventoryMapManager __instance)
    {
        if (WeaverMapManager.GameMap.Prefab == null)
            return;

        __instance.gameMapPrefab = WeaverMapManager.GameMap.Prefab.GetComponent<GameMap>();
    }
    
}
