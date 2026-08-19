using TeamCherry.Localization;
using WorldWeaver.Data;
using WorldWeaver.Managers;

namespace WorldWeaver.Patches;

[HarmonyPatch(typeof(MenuAchievementsList), nameof(MenuAchievementsList.PreInit))]
public static class MenuAchievementsListPatch
{
    [HarmonyPrefix]
    public static void Prefix()
    {
        GameManager.instance.achievementHandler.achievementsList = WeaverAchievementManager.GetAchievementsList();
    }
}

[HarmonyPatch(typeof(MenuAchievement))]
public static class MenuAchievementPatch
{
    [HarmonyPatch(nameof(MenuAchievement.Refresh))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPostfix]
    public static void Postfix(MenuAchievement __instance)
    {
        if (__instance.achievement is not WeaverAchievement achievement)
            return;

        __instance.icon.sprite = achievement.Sprite != null ? achievement.Sprite : __instance.icon.sprite;
        __instance.title.text = Language.Get(achievement.Title.Key, achievement.Title.Sheet);
        __instance.text.text = Language.Get(achievement.Description.Key, achievement.Description.Sheet);
    }
}

[HarmonyPatch(typeof(DesktopPlatform), nameof(DesktopPlatform.IsAchievementUnlocked))] // Remove funny exception
public static class IsAchievementUnlockedPatch
{
    [HarmonyPrefix]
    public static bool Prefix(DesktopPlatform __instance, string achievementId, ref bool? __result)
    {
        if (GameManager.instance.achievementHandler.achievementsList.FindAchievement(achievementId) is not WeaverAchievement)
            return true;

        __result = __instance.RoamingSharedData.GetBool(achievementId, false);
        return false;
    }
}