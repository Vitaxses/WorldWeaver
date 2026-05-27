using UnityEngine;

namespace WorldWeaver.Data.MonoBehaviours
{
    public class WeaverPersistentBoolItem : PersistentBoolItem
    {
        [Space(2)]
        [Header("WorldWeaver")]
        [SerializeField]
        private string ModId = "";
    }
}
