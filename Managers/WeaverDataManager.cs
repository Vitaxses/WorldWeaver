using WorldWeaver.Data;

namespace WorldWeaver.Managers;

public static class WeaverDataManager 
{
    private static readonly List<WeaverSceneDataHandler> registeredDataHandlers = new();

    private static Action<GameManager, bool> OnTimePasses = null!;

    internal static void Init()
    {
    }

    public static bool TryGetWorldWeaverPlugin(string id, out WeaverSceneDataHandler plugin)
    {
        plugin = registeredDataHandlers.FirstOrDefault(p => p.ModIdentifier == id);
        
        if (plugin == null)
        {
            Plugin.Instance.Logger.LogWarning($"PluginID ({id}) does not match any registered registered DataHandlers");
            return false;
        }
        
        return true; // Maybe cache it in the component instead??
    }

    public static bool TryAddDataHandler(WeaverSceneDataHandler dataHandler)
    {
        if (registeredDataHandlers.Any(h => h.ModIdentifier == dataHandler.ModIdentifier))
        {
            Plugin.Instance.Logger.LogWarning($"RegisteredDataHandlers already contains ModIdentfier ({dataHandler.ModIdentifier})");
            return false;
        }

        registeredDataHandlers.Add(dataHandler);
        Plugin.Instance.Logger.LogInfo($"Added WeaverDataHandler ({dataHandler.ModIdentifier})");

        return true;
    }

    public static void AddToOnTimePasses(Action<GameManager, bool> action)
    {
        if (action == null)
            return;

        OnTimePasses += action;
    }

    public static void OnTimePassesMod(GameManager gm, bool isElsewhere)
    {
        OnTimePasses?.Invoke(gm, isElsewhere);
    }

    public static void ResetSemiPersistentItems()
    {
        foreach (var dataHandler in registeredDataHandlers)
        {
            if (dataHandler.GetSceneData() == null)
            {
                Plugin.Instance.Logger.LogDebug($"Mod: {dataHandler.ModIdentifier} SceneData is null");
                continue;
            }
            
            dataHandler.GetSceneData()!.ResetPersistentItems(true);
        }
    }
}
