using UnityEngine;

namespace WorldWeaver.Data.MonoBehaviours
{
    [AddComponentMenu("WorldWeaver/Geo Rock")]
    public class WeaverGeoRock : GeoRock
    {
        [Space(2)]
        [Header("WorldWeaver")]
        [SerializeField]
        private string ModId = "";
    }
}
