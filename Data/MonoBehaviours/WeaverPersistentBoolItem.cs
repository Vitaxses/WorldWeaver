using UnityEngine;
using WorldWeaver.Managers;

namespace WorldWeaver.Data.MonoBehaviours;

public class WeaverPersistentBoolItem : PersistentBoolItem
{
    [SerializeField]
    private string ModId;

    public override void SaveValue(PersistentItemData<bool> newItemData)
    {
        if (Plugin.Instance == null)
            return;

        if (!WeaverDataManager.TryGetWorldWeaverPlugin(ModId, out var plugin))
            return;

        var sceneData = plugin.GetSceneData();  

        if (sceneData == null)
        {
            Plugin.Instance.Logger.LogWarning($"SceneData from {ModId} is null while SaveValue() on PersistentBoolItem");
            return;
        }

        Plugin.Instance.Logger.LogDebug($"Saving PersistentBoolItem: Id={newItemData.ID}, Scene={newItemData.SceneName}, Value={newItemData.Value}, ModId={ModId}");
        sceneData.SetValue(newItemData);
    }

    public override bool TryGetValue(ref PersistentItemData<bool> newItemData)
    {
        if (Plugin.Instance == null)
            return false;

        if (!WeaverDataManager.TryGetWorldWeaverPlugin(ModId, out var plugin))
            return false;

        var sceneData = plugin.GetSceneData();

        if (sceneData == null)
        {
            Plugin.Instance.Logger.LogWarning($"SceneData from {ModId} is null while TryGetValue() on PersistentBoolItem");
            return false;
        }
        
        if (sceneData != null && sceneData.TryGetValue(newItemData.SceneName, newItemData.ID, out PersistentItemData<bool> persistentItemData))
        {
            newItemData.Value = persistentItemData.Value;
            return true;
        }

        return false;
    }

}
