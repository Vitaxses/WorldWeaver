using UnityEngine;

namespace WorldWeaver.Data.MonoBehaviours
{
    public class WeaverGeoRock : GeoRock
    {
        [Space(2)]
        [Header("WorldWeaver")]
        [SerializeField]
        private string ModId = "";
    }
}
