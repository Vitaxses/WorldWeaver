using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
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

        Plugin.Instance.Logger.LogDebug($"[Adressables] Registering {catalogQueue.Count} catalog(s)");

        yield return Addressables.InitializeAsync();

        foreach (var catalogPath in catalogQueue)
        {
            var handle = Addressables.LoadContentCatalogAsync(new Uri(catalogPath).AbsoluteUri);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Plugin.Instance.Logger.LogError($"[Adressables] Catalog failed to load: {catalogPath} ({handle.Status})");
                continue;
            }

            Plugin.Instance.Logger.LogDebug($"[Adressables] Catalog loaded: {catalogPath}");
        }

        InjectAddressablesIds();
        _registeredCatalogs = true;
        WeaverMapManager.LoadSelectedMaps();
    }

    // Hopefully theres a better fix than this
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

                Plugin.Instance.Logger.LogDebug($"[Adressables] Rewrote Addressables path: {id} -> {newId}");
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
                Plugin.Instance.Logger.LogError($"[Adressables] Ambiguous root registration rejected.\n" + $"[Adressables] New: {rootId}\nExisting: {existing}");
                return;
            }
        }

        rootMap[rootId] = pluginFolder;
        Plugin.Instance.Logger.LogDebug($"[Adressables] Registered Addressables root: {rootId} -> {pluginFolder}");
    }

    public static void RegisterAddressablesCatalog(string catalogPath, string rootId, string pluginFolder)
    {
        if (_registeredCatalogs)
        {
            Plugin.Instance.Logger.LogWarning($"[Adressables] Catalog ignored (already loaded catalogs): {catalogPath}");
            return;
        }

        if (string.IsNullOrWhiteSpace(catalogPath))
            return;

        //I pray this'll make this stable on other platforms
        catalogPath = catalogPath.Replace("\\", "/");

        string catalogFolder = Path.GetDirectoryName(catalogPath)!;

        catalogPath = Path.Combine(catalogFolder, GetCatalogName())
            .Replace("\\", "/");

        if (!Path.IsPathRooted(catalogPath))
        {
            Plugin.Instance.Logger.LogWarning($"[Adressables] Catalog path is not rooted ({catalogPath})");
            return;
        }

        Plugin.Instance.Logger.LogDebug($"[Adressables] Catalog added to registration queue ({catalogPath})");
        RegisterAddressablesRoot(rootId, pluginFolder);
        catalogQueue.Add(catalogPath);
    }

    private static string GetCatalogName()
    {
        string catalog = Application.platform switch
        {
            RuntimePlatform.WindowsPlayer => "catalog-StandaloneWindows64.bin",
            RuntimePlatform.LinuxPlayer => "catalog-StandaloneLinux64.bin",
            RuntimePlatform.OSXPlayer => "catalog-StandaloneOSX.bin",
            _ => throw new PlatformNotSupportedException(
                $"Unsupported platform: {Application.platform}")
        };

        Plugin.Instance.Logger.LogDebug(
            $"[Addressables] Platform: {Application.platform}, using {catalog}");

        return catalog;
    }
}
// TODO: Add logging that doesnt show full game path/catalog path