namespace WorldWeaver.Managers;

public static class WeaverSceneMapManager
{
    internal static readonly List<SceneTeleportMapSource> registeredSceneTpMaps = [];

    public static void AddSceneTeleportMap(SceneTeleportMap map)
    {
        registeredSceneTpMaps.Add(new SceneTeleportMapSource(map));
    }

    public static void AddSceneTeleportMap(string addressablesKey)
    {
        registeredSceneTpMaps.Add(new SceneTeleportMapSource(addressablesKey));
    }

    internal static void MergeMap(SceneTeleportMap? map)
    {
        if (map == null || map.sceneList == null)
            return;

        var teleportMap = map.sceneList.GetAllSceneInfo();
        if (teleportMap == null)
            return;

        foreach (var (sceneName, info) in teleportMap)
        {
            if (string.IsNullOrWhiteSpace(sceneName) || info == null)
                continue;

            if (!string.IsNullOrWhiteSpace(info.SceneFileHash))
                SceneTeleportMap.RecordHash(sceneName, info.SceneFileHash); // Not actually used ingame, only used to find out when to regenerate it

            SceneTeleportMap.AddMapZone(sceneName, info.MapZone);
            
            foreach (var gate in info.TransitionGates)
                SceneTeleportMap.AddTransitionGate(sceneName, gate);

            foreach (var respawn in info.RespawnPoints)
                SceneTeleportMap.AddRespawnPoint(sceneName, respawn);
        }
    }

    internal class SceneTeleportMapSource
    {
        public string? AddressablesKey { get; set; }
        public SceneTeleportMap? Map { get; set; }

        public bool IsAddressable => !string.IsNullOrEmpty(AddressablesKey);

        public SceneTeleportMapSource(string addressablesKey)
        {
            AddressablesKey = addressablesKey;
        }
        
        public SceneTeleportMapSource(SceneTeleportMap map)
        {
            Map = map;
        }
    }
}