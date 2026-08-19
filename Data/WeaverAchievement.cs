using GlobalEnums;
using TeamCherry.Localization;
using static WorldWeaver.Managers.WeaverAchievementManager;

namespace WorldWeaver.Data
{   
    [Serializable]
    public class WeaverAchievement : Achievement
    {
        public LocalisedString Title
        {
            get
            {
                if (Awarded)
                    return AwardedTitle;
                
                if (Type == AchievementType.Normal)
                    return NormalTitle;
                else
                    return HiddenTitle;
            }
        }

        public LocalisedString Description
        {
            get
            {
                if (Awarded)
                    return AwardedDescription;
                
                if (Type == AchievementType.Normal)
                    return NormalDescription;
                else
                    return HiddenDescription;
            }
        }

        public Sprite? Sprite
        {
            get
            {
                if (Awarded)
                    return AwardedIcon;
                
                if (Type == AchievementType.Normal)
                    return Icon;
                else
                    return HiddenIcon;
            }
        }

        public bool Awarded
        {
            get => GameManager.SilentInstance != null && GameManager.instance.IsAchievementAwarded(PlatformKey);
        }

        public Sprite? HiddenIcon; // If null will select default
        public Sprite? AwardedIcon; // If null will use Icon

        public LocalisedString NormalTitle;
        public LocalisedString NormalDescription;
        
        public LocalisedString HiddenTitle;
        public LocalisedString HiddenDescription;

        public LocalisedString AwardedTitle;
        public LocalisedString AwardedDescription;

        public WeaverAchievement()
        {
            if (NormalTitle.IsEmpty)
                NormalTitle = new(sheet: ACHIEVEMENTS_SHEET, key: TitleCell);
            
            if (NormalDescription.IsEmpty)
                NormalDescription = new(sheet: ACHIEVEMENTS_SHEET, key: DescriptionCell);

            if (HiddenTitle.IsEmpty)
                HiddenTitle = new(sheet: ACHIEVEMENTS_SHEET, key: "HIDDEN_ACHIEVEMENT_TITLE");
            
            if (HiddenDescription.IsEmpty)
                HiddenDescription = new(sheet: ACHIEVEMENTS_SHEET, key: "HIDDEN_ACHIEVEMENT");

            if (AwardedTitle.IsEmpty)
                AwardedTitle = NormalTitle;

            if (AwardedDescription.IsEmpty)
                AwardedDescription = NormalDescription;

            if (AwardedIcon == null)
                AwardedIcon = Icon;
        }
        
        public WeaverAchievement(Achievement achievement) : this()
        {
            Type = achievement.Type;
            PlatformKey = achievement.PlatformKey;
            Icon = achievement.Icon;
            
            if (AwardedIcon == null)
                AwardedIcon = Icon;
        }

    }

    [Serializable]
    [CreateAssetMenu(menuName = "WorldWeaver/Achievements List")]
    public class WeaverAchievementsList : ScriptableObject
    {
        public List<WeaverAchievement> Achievements => achievements;

        public WeaverAchievement FindAchievement(string key)
        {
            foreach (var achievement in achievements)
                if (achievement.PlatformKey == key)
                    return achievement;

            return null!;
        }

        public bool AchievementExists(string key) => achievements.Any(a => a.PlatformKey == key);

        [SerializeField]
        private List<WeaverAchievement> achievements;
    }

}