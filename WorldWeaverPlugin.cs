using BepInEx.Logging;
using WorldWeaver.Managers;

namespace WorldWeaver;

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

        WeaverDataManager.Init();
    }

    void Start()
    {
        WeaverAddressablesManager.Init();
    }
}
