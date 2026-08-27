using BepInEx.Bootstrap;

namespace WorldWeaver.Compatibility;

public static class ModCompatibility
{
    public const string Id = Plugin.Id + ".compatibility";
    private static Harmony Harmony = null!;
    
    public static void Init()
    {
        Harmony = new(Id);

        if (Chainloader.PluginInfos.TryGetValue("io.github.hk-speedrunning.debugmod", out var debugInfo))
            DebugModCompatibility.Patch(debugInfo.Instance.GetType().Assembly, Harmony);

        if (Chainloader.PluginInfos.TryGetValue("io.github.hk-speedrunning.quickwarp", out var quickwarpInfo))
            QuickWarpCompatibility.Patch(quickwarpInfo.Instance.GetType().Assembly, Harmony);
    }
}