using UnityEngine;
using UnityEngine.SceneManagement;
using WorldWeaver.Managers;

namespace WorldWeaver.Data.MonoBehaviours;

public class WeaverGeoRock : GeoRock
{
    [SerializeField]
    private string ModId;

    new void OnEnable()
    {
        SceneManager.activeSceneChanged += LevelActivated;
        gm = GameManager.instance;
        gm.SavePersistentObjects += SaveState;
    }

    new void OnDisable()
    {
        SceneManager.activeSceneChanged -= LevelActivated;
        if (gm != null)
            gm.SavePersistentObjects -= SaveState;
    }

    new void SaveState()
    {
        SetMyId();
        UpdateHitsLeftFromFsm();

        if (Plugin.Instance == null || !WeaverDataManager.TryGetWorldWeaverPlugin(ModId, out var plugin))
            return;

        var sceneData = plugin.GetSceneData();  

        if (sceneData == null)
        {
            Plugin.Instance.Logger.LogWarning($"SceneData from {ModId} is null while SaveState() on GeoRock");
            return;
        }
        
        Plugin.Instance.Logger.LogDebug($"Saving GeoRockState: Id={geoRockData.id}, Scene={geoRockData.sceneName}, HitsLeft={geoRockData.hitsLeft}, ModId={ModId}");
        sceneData.SaveMyState(geoRockData);
    }

    new void LevelActivated(Scene from, Scene to)
    {
        SetMyId();
        
        if (Plugin.Instance != null && WeaverDataManager.TryGetWorldWeaverPlugin(ModId, out var plugin))
        {
            var sceneData = plugin.GetSceneData();  

            if (sceneData == null)
            {
                Plugin.Instance.Logger.LogWarning($"SceneData from {ModId} is null while LevelActivated() on GeoRock");
                UpdateHitsLeftFromFsm();
                return;
            }
                
            GeoRockData? data = plugin.GetSceneData()?.FindMyState(geoRockData);
            if (data != null)
            {
                geoRockData.hitsLeft = data.hitsLeft;
                hitsInt.Value = data.hitsLeft;
                return;
            }
        }

        UpdateHitsLeftFromFsm();
    }
}
