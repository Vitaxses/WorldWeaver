using UnityEngine;
using WorldWeaver.Editor;

namespace WorldWeaver.Data.MonoBehaviours
{
    [AddComponentMenu("WorldWeaver/Weaver Geo Rock")]
    public class WeaverGeoRock : GeoRock
    {
        [Space(2)]
        [Header("WorldWeaver")]
        [SerializeField]
        public string ModId = "";

        void OnValidate()
        {
            if (string.IsNullOrEmpty(ModId))
                ModId = WorldWeaverSettings.Instance.ModIdDefault;
        }
    }
}
