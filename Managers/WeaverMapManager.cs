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

    public static void SetWideMap(string AddressablesKey, int priority)
    {
        if (HasSelectedMaps)
            return;
            
        if (AddressablesKey.IsNullOrWhiteSpace())
            return;

        if (WideMap.priority >= priority)
            return;

        WideMap.priority = priority;
        WideMap.AddressablesKey = AddressablesKey;
        WideMap.prefab = null;
    }

    public static void SetWideMap(GameObject prefab, int priority)
    {
        if (HasSelectedMaps)
            return;
            
        if (prefab == null)
            return;

        if (WideMap.priority >= priority)
            return;

        WideMap.priority = priority;
        WideMap.prefab = prefab;
        WideMap.AddressablesKey = string.Empty;
    }

    public static void SetGameMap(string AddressablesKey, int priority)
    {
        if (HasSelectedMaps)
            return;
            
        if (AddressablesKey.IsNullOrWhiteSpace())
            return;

        if (GameMap.priority >= priority)
            return;

        GameMap.priority = priority;
        GameMap.AddressablesKey = AddressablesKey;
        GameMap.prefab = null;
    }

    public static void SetGameMap(GameObject prefab, int priority)
    {
        if (HasSelectedMaps)
            return;

        if (prefab == null)
            return;

        if (GameMap.priority >= priority)
            return;

        GameMap.priority = priority;
        GameMap.prefab = prefab;
        GameMap.AddressablesKey = string.Empty;
    }

    internal static void LoadSelectedMaps()
    {
        if (HasSelectedMaps)
            return;

        HasSelectedMaps = true;

        if (WideMap.prefab == null && !WideMap.AddressablesKey.IsNullOrWhiteSpace())
        {
            Addressables.LoadAssetAsync<GameObject>(WideMap.AddressablesKey).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    WideMap.prefab = handle.Result;
                else
                    Plugin.Instance.Logger.LogWarning($"Failed to load wide map '{WideMap.AddressablesKey}'.");
            };
        }

        
        if (GameMap.prefab == null && !GameMap.AddressablesKey.IsNullOrWhiteSpace())
        {
            Addressables.LoadAssetAsync<GameObject>(GameMap.AddressablesKey).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    GameMap.prefab = handle.Result;
                else
                    Plugin.Instance.Logger.LogWarning($"Failed to load game map '{GameMap.AddressablesKey}'.");
            };
        }
    }

    internal class MapHolder
    {
        public int priority;
        public GameObject? prefab;
        public string AddressablesKey;

        public MapHolder()
        {
            priority = int.MinValue;
            AddressablesKey = "";
        }
    }
}
