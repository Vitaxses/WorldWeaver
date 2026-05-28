using System.Collections;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace WorldWeaver.Managers;

public static class WeaverAddressablesManager
{
    private static readonly Dictionary<string, string> rootMap = [];
    private static readonly List<string> catalogQueue = [];

    private static bool registeredCatalogs = false;

    internal static void Init()
    {
        Plugin.Instance.StartCoroutine(RegisterCatalogs());
        InjectAddressablesIds();
    }

    private static IEnumerator RegisterCatalogs()
    {
        if (catalogQueue.Count == 0)
            yield break;

        Plugin.Instance.Logger.LogDebug($"Registering {catalogQueue.Count} catalog(s)");

        yield return Addressables.InitializeAsync();

        foreach (var relative in catalogQueue)
        {
            string normalized = relative.Replace("\\", "/");

            int slash = normalized.IndexOf('/');
            if (slash <= 0)
            {
                Plugin.Instance.Logger.LogWarning($"Invalid catalog path: {relative}");
                continue;
            }

            string root = normalized.Substring(0, slash + 1);

            string pluginFolder = Path.GetFullPath(Path.Combine(Paths.PluginPath, root));

            RegisterRoot(root, pluginFolder);

            string fullPath = Path.GetFullPath(Path.Combine(Paths.PluginPath, normalized));
            string uri = new Uri(fullPath).AbsoluteUri;

            Plugin.Instance.Logger.LogDebug($"Loading catalog: {normalized}");

            var handle = Addressables.LoadContentCatalogAsync(uri);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
                Plugin.Instance.Logger.LogDebug($"Catalog loaded: {normalized}");
            else
                Plugin.Instance.Logger.LogWarning($"Failed to load catalog: {normalized}");
        }

        registeredCatalogs = true;
    }

    private static void InjectAddressablesIds()
    {
        var previous = Addressables.InternalIdTransformFunc;

        Addressables.InternalIdTransformFunc = location =>
        {
            string id = previous?.Invoke(location) ?? location.InternalId;
            string normalized = id.Replace("\\", "/");

            foreach (var kvp in rootMap)
            {
                string root = kvp.Key;
                string basePath = kvp.Value;

                if (!normalized.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    continue;

                string relative = normalized.Substring(root.Length);

                string newId = Path.Combine(basePath, relative).Replace("\\", "/");

                #if DEBUG
                Plugin.Instance.Logger.LogDebug($"Rewrote Addressables path: {id} -> {newId}");
                #endif

                return newId;
            }

            return id;
        };
    }

    public static void RegisterRoot(string root, string pluginFolder)
    {
        if (string.IsNullOrWhiteSpace(root))
            return;

        root = root.Replace("\\", "/");

        if (!root.EndsWith("/"))
            root += "/";

        pluginFolder = pluginFolder.Replace("\\", "/");

        if (rootMap.ContainsKey(root))
        {
            Plugin.Instance.Logger.LogWarning($"Addressables root already registered: {root}");
            return;
        }

        rootMap[root] = pluginFolder;

        Plugin.Instance.Logger.LogDebug($"Registered Addressables root: {root} -> {pluginFolder}");
    }

    public static void RegisterAddressablesCatalog(string catalogPath)
    {
        if (registeredCatalogs)
        {
            Plugin.Instance.Logger.LogWarning($"Catalog ignored (already loaded): {catalogPath}");
            return;
        }

        if (string.IsNullOrWhiteSpace(catalogPath))
            return;

        catalogPath = catalogPath.Replace("\\", "/");

        if (Path.IsPathRooted(catalogPath))
        {
            string pluginsPath = Path.GetFullPath(Paths.PluginPath).Replace("\\", "/");
            string full = Path.GetFullPath(catalogPath).Replace("\\", "/");

            if (!full.StartsWith(pluginsPath, StringComparison.OrdinalIgnoreCase))
            {
                Plugin.Instance.Logger.LogWarning($"Catalog outside plugins folder: {catalogPath}");
                return;
            }

            catalogPath = Path.GetRelativePath(pluginsPath, full).Replace("\\", "/");
        }
        
        Plugin.Instance.Logger.LogDebug($"Catalog added to registration queue ({catalogPath})");
        catalogQueue.Add(catalogPath);
    }
}