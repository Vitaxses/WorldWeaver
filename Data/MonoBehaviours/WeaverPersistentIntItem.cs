using UnityEngine;
using WorldWeaver.Managers;

namespace WorldWeaver.Data.MonoBehaviours;

public class WeaverPersistentIntItem : PersistentIntItem
{
    [SerializeField]
    private string ModId;

    public override void SaveValue(PersistentItemData<int> newItemData)
    {
        if (Plugin.Instance == null)
            return;

        if (!WeaverDataManager.TryGetWorldWeaverPlugin(ModId, out var plugin))
            return;

        var sceneData = plugin.GetSceneData();  

        if (sceneData == null)
        {
            Plugin.Instance.Logger.LogWarning($"SceneData from {ModId} is null while SaveValue() on PersistentIntItem");
            return;
        }

        sceneData.SetValue(newItemData);
    }

    public override bool TryGetValue(ref PersistentItemData<int> newItemData)
    {
        if (Plugin.Instance == null)
            return false;

        if (!WeaverDataManager.TryGetWorldWeaverPlugin(ModId, out var plugin))
            return false;

        var sceneData = plugin.GetSceneData();

        if (sceneData == null)
        {
            Plugin.Instance.Logger.LogWarning($"SceneData from {ModId} is null while TryGetValue() on PersistentIntItem");
            return false;
        }
        
        if (sceneData != null && sceneData.TryGetValue(newItemData.SceneName, newItemData.ID, out PersistentItemData<int> persistentItemData))
        {
            newItemData.Value = persistentItemData.Value;
            return true;
        }

        return false;
    }

}
