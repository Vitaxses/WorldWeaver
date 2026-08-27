using BepInEx.Logging;
using WorldWeaver.Compatibility;
using WorldWeaver.Managers;

namespace WorldWeaver;

[BepInDependency("io.github.hk-speedrunning.debugmod", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("io.github.hk-speedrunning.quickwarp", BepInDependency.DependencyFlags.SoftDependency)]
[BepInAutoPlugin(id: "io.github.vitaxses.worldweaver")]
public partial class WorldWeaverPlugin : BaseUnityPlugin
{
    internal static Plugin Instance { get; private set; }

    internal new ManualLogSource Logger { get; private set; }

    void Awake()
    {
        Instance = this;
        Logger = base.Logger;
        
        Logger.LogInfo($"Plugin {Name} ({Id}) v{Version} has loaded!");

        new Harmony(Id).PatchAll();
        ModCompatibility.Init();

        WeaverDataManager.Init();
    }

    void Start()
    {
        WeaverAddressablesManager.Init();
    }
}
