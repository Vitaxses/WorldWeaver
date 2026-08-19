using GlobalEnums;
using TeamCherry.Localization;
using WorldWeaver.Data;

namespace WorldWeaver.Managers;

public static class WeaverAchievementManager
{
    public const string ACHIEVEMENTS_SHEET = "Achievements";
    private static readonly List<Achievement> customAchievements = new();
    
    public static void AddAchievement(WeaverAchievement achievement)
    {
        if (customAchievements.FirstOrDefault(a => a.PlatformKey == achievement.PlatformKey) != null)
        {
            Plugin.Instance.Logger.LogWarning($"[AchievementManager] Achievement with id {achievement.PlatformKey} already exists");
            return;
        }

        customAchievements.Add(achievement);
        Plugin.Instance.Logger.LogDebug($"[AchievementManager] Registered achievement: {achievement.PlatformKey}");
    }
    
    public static void AddAchievement(Achievement achievement)
    {
        AddAchievement(new WeaverAchievement(achievement));
    }

    public static void AddAchievement(string id, LocalisedString title, LocalisedString description, AchievementType type, Sprite icon) => AddAchievement(new WeaverAchievement()
    {
        NormalTitle = title,
        NormalDescription = description,

        PlatformKey = id,
        Type = type,
        Icon = icon
    });

    public static void AddAchievementsList(AchievementsList list)
    {
        Plugin.Instance.Logger.LogDebug($"[AchievementManager] Registering achievements list: {list.name}");

        foreach (var achievement in list.Achievements)
            AddAchievement(achievement);
    }
    
    public static void AddAchievementsList(WeaverAchievementsList list)
    {
        Plugin.Instance.Logger.LogDebug($"[AchievementManager] Registering achievements list: {list.name}");
        Plugin.Instance.Logger.LogDebug(list.Achievements == null); // This for some reason returns true?
        foreach (var achievement in list.Achievements)
            AddAchievement(achievement);
    }

    public static AchievementsList GetAchievementsList()
    {
        var list = GameManager.instance.achievementHandler.AchievementsList;
        foreach (var customAchievement in customAchievements)
        {
            list.achievements.Insert(0, customAchievement);
        }
        
        return list;
    }
}