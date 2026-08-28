using BepInEx.Bootstrap;

namespace WorldWeaver.Compatibility;

public static class ModCompatibility
{
    public const string Id = Plugin.Id + ".compatibility";
    private static Harmony Harmony = null!;
    
    public static void Init()
    {
        Harmony = new(Id);

        if (Chainloader.PluginInfos.TryGetValue(DebugModCompatibility.Id, out var debugInfo))
            DebugModCompatibility.Patch(debugInfo.Instance.GetType().Assembly, Harmony);

        if (Chainloader.PluginInfos.TryGetValue(QuickWarpCompatibility.Id, out var quickwarpInfo))
            QuickWarpCompatibility.Patch(quickwarpInfo.Instance.GetType().Assembly, Harmony);
    }
}