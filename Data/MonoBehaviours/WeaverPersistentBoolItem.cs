using UnityEngine;
using WorldWeaver.Editor;

namespace WorldWeaver.Data.MonoBehaviours
{
    [AddComponentMenu("WorldWeaver/Weaver Persistent Bool Item")]
    public class WeaverPersistentBoolItem : PersistentBoolItem
    {
        [Space(2)]
        [Header("WorldWeaver")]
        public string ModId = "";

        void OnValidate()
        {
            if (string.IsNullOrEmpty(ModId))
                ModId = WorldWeaverSettings.Instance.ModIdDefault;
        }
    }
}
