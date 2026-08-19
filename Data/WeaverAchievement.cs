using System;
using System.Collections.Generic;
using TeamCherry.Localization;
using UnityEngine;

namespace WorldWeaver.Data
{
    [Serializable]
    public class WeaverAchievement : Achievement
    {
        public Sprite? HiddenIcon; // If null will select default
        public Sprite? AwardedIcon; // If null will use Icon

        public LocalisedString NormalTitle;
        public LocalisedString NormalDescription;
        
        public LocalisedString HiddenTitle;
        public LocalisedString HiddenDescription;

        public LocalisedString AwardedTitle;
        public LocalisedString AwardedDescription;
    }

    [Serializable]
    [CreateAssetMenu(menuName = "WorldWeaver/Achievements List")]
    public class WeaverAchievementsList : ScriptableObject
    {
        public List<WeaverAchievement> Achievements => achievements;

        public WeaverAchievement FindAchievement(string key) => null!;

        public bool AchievementExists(string key) => false;

        [SerializeField]
        private List<WeaverAchievement> achievements;
    }
}