using UnityEngine.ResourceManagement.AsyncOperations;

namespace WorldWeaver.Managers;

public static class WeaverMapManager
{   
    /// <summary>
    /// Map with area buttons
    /// </summary>
    internal static MapHolder WideMap = new();

    /// <summary>
    /// Marker map
    /// </summary>
    internal static MapHolder GameMap = new();

    public static bool HasSelectedMaps { get; private set; }

    public static void SetWideMap(string addressablesKey, int priority)
    {
        if (HasSelectedMaps)
            return;
            
        if (addressablesKey.IsNullOrWhiteSpace())
            return;

        if (WideMap.Priority >= priority)
            return;

        WideMap.Priority = priority;
        WideMap.AddressablesKey = addressablesKey;
        WideMap.Prefab = null;
    }

    public static void SetWideMap(GameObject prefab, int priority)
    {
        if (HasSelectedMaps)
            return;
            
        if (prefab == null)
            return;

        if (WideMap.Priority >= priority)
            return;

        WideMap.Priority = priority;
        WideMap.Prefab = prefab;
        WideMap.AddressablesKey = string.Empty;
    }

    public static void SetGameMap(string addressablesKey, int priority)
    {
        if (HasSelectedMaps)
            return;
            
        if (addressablesKey.IsNullOrWhiteSpace())
            return;

        if (GameMap.Priority >= priority)
            return;

        GameMap.Priority = priority;
        GameMap.AddressablesKey = addressablesKey;
        GameMap.Prefab = null;
    }

    public static void SetGameMap(GameObject prefab, int priority)
    {
        if (HasSelectedMaps)
            return;

        if (prefab == null)
            return;

        if (GameMap.Priority >= priority)
            return;

        GameMap.Priority = priority;
        GameMap.Prefab = prefab;
        GameMap.AddressablesKey = string.Empty;
    }

    internal static void LoadSelectedMaps()
    {
        if (HasSelectedMaps)
            return;

        HasSelectedMaps = true;

        if (WideMap.Prefab == null && !WideMap.AddressablesKey.IsNullOrWhiteSpace())
        {
            Addressables.LoadAssetAsync<GameObject>(WideMap.AddressablesKey).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    WideMap.Prefab = handle.Result;
                else
                    Plugin.Instance.Logger.LogWarning($"Failed to load wide map '{WideMap.AddressablesKey}'.");
            };
        }

        
        if (GameMap.Prefab == null && !GameMap.AddressablesKey.IsNullOrWhiteSpace())
        {
            Addressables.LoadAssetAsync<GameObject>(GameMap.AddressablesKey).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    GameMap.Prefab = handle.Result;
                else
                    Plugin.Instance.Logger.LogWarning($"Failed to load game map '{GameMap.AddressablesKey}'.");
            };
        }
    }

    internal class MapHolder
    {
        public int Priority;
        public GameObject? Prefab;
        public string AddressablesKey;

        public MapHolder()
        {
            Priority = int.MinValue;
            AddressablesKey = "";
        }
    }
}
