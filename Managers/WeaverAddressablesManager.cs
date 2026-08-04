using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace WorldWeaver.Managers;

public static class WeaverAddressablesManager
{
    private static readonly Dictionary<string, string> rootMap = [];
    private static readonly List<string> catalogQueue = [];

    private static bool _registeredCatalogs = false;
    public static bool RegisteredCatalogs => _registeredCatalogs;

    internal static void Init()
    {
        Plugin.Instance.StartCoroutine(RegisterCatalogs());
    }

    private static IEnumerator RegisterCatalogs()
    {
        if (catalogQueue.Count == 0)
        {
            _registeredCatalogs = true;
            yield break;
        }

        Plugin.Instance.Logger.LogDebug($"[Addressables] Registering {catalogQueue.Count} catalog(s)");

        yield return Addressables.InitializeAsync();

        foreach (var catalogPath in catalogQueue)
        {
            var handle = Addressables.LoadContentCatalogAsync(new Uri(catalogPath).AbsoluteUri);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Plugin.Instance.Logger.LogError($"[Addressables] Catalog failed to load: {catalogPath} ({handle.Status})");
                continue;
            }

            Plugin.Instance.Logger.LogDebug($"[Addressables] Catalog loaded: {catalogPath}");
        }

        InjectAddressablesIds();
        _registeredCatalogs = true;
        WeaverMapManager.LoadSelectedMaps();
    }

    // Hopefully theres a better fix than this
    // Not even sure if we need this anymore
    private static void InjectAddressablesIds()
    {
        var previous = Addressables.InternalIdTransformFunc;

        Addressables.InternalIdTransformFunc = location =>
        {
            string id = previous?.Invoke(location) ?? location.InternalId;

            string normalized = id.Replace("\\", "/");
            foreach (var kvp in rootMap)
            {
                var root = kvp.Key;
                var pluginPath = kvp.Value;

                int index = normalized.IndexOf(root, StringComparison.OrdinalIgnoreCase);

                if (index < 0)
                    continue;

                string relativePath = normalized[(index + root.Length)..];
                string newId = Path.Combine(pluginPath, relativePath);
                newId = newId.Replace("\\", "/");

                Plugin.Instance.Logger.LogDebug($"[Addressables] Rewrote Addressables path: {id} -> {newId}");
                return newId;
            }
            
            return id;
        };
    }

    private static void RegisterAddressablesRoot(string rootId, string pluginFolder)
    {
        if (string.IsNullOrWhiteSpace(rootId))
            return;

        rootId = rootId.Replace("\\", "/");

        if (!rootId.EndsWith('/'))
            rootId += "/";

        pluginFolder = pluginFolder.Replace("\\", "/");

        foreach (var kvp in rootMap)
        {
            var existing = kvp.Key;

            if (existing.Contains(rootId, StringComparison.OrdinalIgnoreCase) || rootId.Contains(existing, StringComparison.OrdinalIgnoreCase))
            {
                Plugin.Instance.Logger.LogError($"[Addressables] Ambiguous root registration rejected.\n" + $"[Addressables] New: {rootId}\nExisting: {existing}");
                return;
            }
        }

        rootMap[rootId] = pluginFolder;
        Plugin.Instance.Logger.LogDebug($"[Addressables] Registered Addressables root: {rootId} -> {pluginFolder}");
    }

    public static void RegisterAddressablesCatalog(string windows64CatalogPath, string OSXCatalogPath, string linux64CatalogPath, string rootId, string pluginFolder)
    {
        if (_registeredCatalogs)
        {
            Plugin.Instance.Logger.LogWarning($"[Addressables] Catalog ignored (already loaded catalogs)");
            return;
        }

        string catalogPath = Application.platform switch
        {
            RuntimePlatform.WindowsPlayer => windows64CatalogPath,
            RuntimePlatform.OSXPlayer => OSXCatalogPath,
            RuntimePlatform.LinuxPlayer => linux64CatalogPath,
            _ => throw new PlatformNotSupportedException($"Unsupported platform: {Application.platform}")
        };

        if (string.IsNullOrWhiteSpace(catalogPath))
            return;

        catalogPath = catalogPath.Replace("\\", "/");

        if (!File.Exists(catalogPath))
        {
            Plugin.Instance.Logger.LogWarning($"[Addressables] Catalog path invalid ({catalogPath})");
            return;
        }

        if (!Path.IsPathRooted(catalogPath))
        {
            Plugin.Instance.Logger.LogWarning($"[Addressables] Catalog path is not rooted ({catalogPath})");
            return;
        }
        Plugin.Instance.Logger.LogDebug($"[Addressables] Catalog added to registration queue ({catalogPath})");
        RegisterAddressablesRoot(rootId, pluginFolder);
        catalogQueue.Add(catalogPath);
    }

    public static void RegisterAddressablesCatalog(string catalogFolderPath, string rootId, string pluginFolder)
    {
        if (_registeredCatalogs)
        {
            Plugin.Instance.Logger.LogWarning($"[Addressables] Catalog ignored (already loaded catalogs)");
            return;
        }

        if (!Directory.Exists(catalogFolderPath))
        {
            Plugin.Instance.Logger.LogWarning($"[Addressables] Catalog folder path invalid ({catalogFolderPath})");
            return;
        }

        var catalog = Application.platform switch
        {
            RuntimePlatform.WindowsPlayer => "catalog-StandaloneWindows64.bin",
            RuntimePlatform.LinuxPlayer => "catalog-StandaloneLinux64.bin",
            RuntimePlatform.OSXPlayer => "catalog-StandaloneOSX.bin",
            _ => throw new PlatformNotSupportedException($"Unsupported platform: {Application.platform}")
        };


        var catalogPath = Path.Combine(catalogFolderPath, catalog);

        if (!File.Exists(catalogPath))
        {
            Plugin.Instance.Logger.LogWarning($"[Addressables] Catalog path invalid ({catalogPath})");
            return;
        }

        catalogPath = catalogPath.Replace("\\", "/");

        if (!Path.IsPathRooted(catalogPath))
        {
            Plugin.Instance.Logger.LogWarning($"[Addressables] Catalog path is not rooted ({catalogPath})");
            return;
        }

        Plugin.Instance.Logger.LogDebug($"[Addressables] Catalog added to registration queue ({catalogPath})");
        RegisterAddressablesRoot(rootId, pluginFolder);
        catalogQueue.Add(catalogPath);
    }
}
// TODO: Add logging that doesnt show full game path/catalog path