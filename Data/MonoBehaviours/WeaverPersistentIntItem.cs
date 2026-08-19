using UnityEngine;

namespace WorldWeaver.Data.MonoBehaviours
{
    [AddComponentMenu("WorldWeaver/Persistent Int Item")]
    public class WeaverPersistentIntItem : PersistentIntItem
    {
        [Space(2)]
        [Header("WorldWeaver")]
        [SerializeField]
        private string ModId = "";
    }
}
