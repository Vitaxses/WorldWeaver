using WorldWeaver.Managers;

namespace WorldWeaver.Patches;

[HarmonyPatch(typeof(GameManager))]
internal static class TimePassesPatch
{
    [HarmonyPatch(nameof(GameManager.TimePasses))]
    [HarmonyPostfix]
    static void TimePasses(GameManager __instance)
    {
        WeaverDataManager.OnTimePassesMod(__instance, false);
    }
    
    [HarmonyPatch(nameof(GameManager.TimePassesElsewhere))]
    [HarmonyPostfix]
    static void TimePassesElsewhere(GameManager __instance)
    {
        WeaverDataManager.OnTimePassesMod(__instance, true);
    }
}