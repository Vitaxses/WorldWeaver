using UnityEngine;

namespace WorldWeaver.Data.MonoBehaviours
{
    [AddComponentMenu("WorldWeaver/Persistent Bool Item")]
    public class WeaverPersistentBoolItem : PersistentBoolItem
    {
        [Space(2)]
        [Header("WorldWeaver")]
        [SerializeField]
        private string ModId = "";
    }
}
