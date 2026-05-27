using UnityEngine;

namespace WorldWeaver.Data.MonoBehaviours
{
    public class WeaverPersistentIntItem : PersistentIntItem
    {
        [Space(2)]
        [Header("WorldWeaver")]
        [SerializeField]
        private string ModId = "";
    }
}
