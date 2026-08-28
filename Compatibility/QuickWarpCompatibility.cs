using System.Reflection;
using GlobalEnums;
using TeamCherry.Localization;

namespace WorldWeaver.Compatibility;

public static class QuickWarpCompatibility
{
    public const string Id = "io.github.hk-speedrunning.quickwarp";

    static Type? warpType;
    static Type? quickWarpGuiType;

    static FieldInfo? scenesField;
    static FieldInfo? scenesByAreaField;

    static object? gui;

    public static void Patch(Assembly assembly, Harmony harmony)
    {
        warpType = assembly.GetType("QuickWarp.Warp");
        quickWarpGuiType = assembly.GetType("QuickWarp.QuickWarpGUI");

        scenesField = warpType.GetField("_scenes", BindingFlags.Static | BindingFlags.NonPublic);
        scenesByAreaField = warpType.GetField("_scenes_by_area", BindingFlags.Static | BindingFlags.NonPublic);

        harmony.Patch(original: AccessTools.Method(warpType, "BuildRefs"), postfix: new HarmonyMethod(typeof(QuickWarpCompatibility), nameof(Merge)));
    }

    public static void Merge()
    {
        Dictionary<string, SceneTeleportMap.SceneInfo> scenes = (Dictionary<string, SceneTeleportMap.SceneInfo>)scenesField!.GetValue(null);
        Dictionary<string, List<string>> scenesByArea = (Dictionary<string, List<string>>)scenesByAreaField!.GetValue(null);

        Dictionary<MapZone, List<string>> scenesByZone = new();

        foreach (var (scene, data) in scenes)
        {
            var mapZone = data.MapZone;

            if (!scenesByZone.TryGetValue(mapZone, out var sceneList))
            {
                sceneList = [];
                scenesByZone.Add(mapZone, sceneList);
            }

            sceneList.Add(scene);
        }

        foreach (KeyValuePair<MapZone, List<string>> pair in scenesByZone)
        {
            MapZone zone = pair.Key;
            var scenesForZone = pair.Value;

            string area = GetMapZoneString(zone);

            if (!scenesByArea.TryGetValue(area, out List<string>? existingScenes))
            {
                existingScenes = [];
                scenesByArea[area] = existingScenes;
            }

            foreach (string scene in scenesForZone)
            {
                if (!existingScenes.Contains(scene))
                    existingScenes.Add(scene);
            }
        }

        if (gui == null)
            gui = UObject.FindFirstObjectByType(quickWarpGuiType, FindObjectsInactive.Include);

        if (gui == null)
            return;

        AccessTools.Method(quickWarpGuiType, "Awake")?.Invoke(gui, []);
    }

    static string GetMapZoneString(MapZone mapZone)
    {
        return mapZone switch
        {
            MapZone.ABYSS => "Abyss",
            MapZone.BELLTOWN => "Bellhart",

            MapZone.SWAMP => "Bilewater",
            MapZone.GLOOM => "Bilewater",

            MapZone.JUDGE_STEPS => "Blasted Steps",
            MapZone.BONETOWN => "Bone Bottom",
            MapZone.CITY_OF_SONG => "Choral Chambers",
            MapZone.COG_CORE => "Cogwork Core",
            MapZone.CRADLE => "The Cradle",
            MapZone.DOCKS => "Deep Docks",
            MapZone.WILDS => "Far Fields",
            MapZone.FRONT_GATE => "Grand Gate",
            MapZone.GREYMOOR => "Greymoor",
            MapZone.HANG => "High Halls",
            MapZone.HUNTERS_NEST => "Hunter's March",
            MapZone.PATH_OF_BONE => "The Marrow",
            MapZone.TEST_AREA => "Test Area",
            MapZone.DUST_MAZE => "The Mist",

            MapZone.MOSSTOWN => "Moss Grotto",
            MapZone.MOSS_CAVE => "Moss Grotto",

            MapZone.PEAK => "Mount Fay",
            MapZone.AQUEDUCT => "Putrified Ducts",
            MapZone.CORAL_CAVERNS => "Sands of Karak",
            MapZone.SHELLWOOD_THICKET => "Shellwood",
            MapZone.DUSTPENS => "Sinner's Road",
            MapZone.THE_SLAB => "The Slab",
            MapZone.SURFACE => "The Surface",
            MapZone.UNDERSTORE => "Underworks",
            MapZone.CLOVER => "Verdania",
            MapZone.WEAVER_SHRINE => "Weavenest Atla",
            MapZone.LIBRARY => "Whispering Vaults",
            MapZone.WARD => "Whiteward",
            MapZone.WISP => "Wisp Thicket",
            MapZone.CRAWLSPACE => "Wormways",
            MapZone.RED_CORAL_GORGE => "Red Coral Gorge",
            MapZone.PHARLOOM_BAY => "Pharloom Bay",
            MapZone.BONECHURCH => "Ruined Chapel",
            MapZone.ARBORIUM => "Memorium",
            MapZone.PILGRIMS_REST => "Pilgrim's rest",
            MapZone.HALFWAY_HOUSE => "Halfway Home",
            MapZone.MEMORY => "Memory",

            _ => Language.Get(mapZone.ToString(), "Map Zones")
        };
    }
}
