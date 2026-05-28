namespace WorldWeaver.Extensions;

public static class SceneDataExtensions
{
    public static void ResetPersistentItems(this SceneData sceneData, bool onlySemiPersistent = true)
    {
        if (!onlySemiPersistent)
        {
            sceneData.SetupNewSceneData();
            return;
        }

        sceneData.persistentBools.ResetSemiPersistent();
        sceneData.persistentInts.ResetSemiPersistent();
        sceneData.geoRocks.ResetSemiPersistent();
    }
    
    public static void SetValue(this SceneData sceneData, PersistentItemData<bool> value)
        => sceneData.persistentBools.SetValue(value);
    public static void SetValue(this SceneData sceneData, PersistentItemData<int> value)
        => sceneData.persistentInts.SetValue(value);
        
    public static bool TryGetValue(this SceneData sceneData, string sceneName, string ID, out PersistentItemData<int> itemData)
        => sceneData.persistentInts.TryGetValue(sceneName, ID, out itemData);
    public static bool TryGetValue(this SceneData sceneData, string sceneName, string id, out PersistentItemData<bool> itemData)
        => sceneData.persistentBools.TryGetValue(sceneName, id, out itemData);
}