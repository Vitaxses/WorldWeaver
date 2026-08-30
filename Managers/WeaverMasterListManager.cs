using System.Collections;

namespace WorldWeaver.Managers;

public static class WeaverMasterLists
{
    public static CollectableItemListManager CollectableItemList { get; } = new();
    public static QuestListManager QuestList { get; } = new();
    public static ToolItemListManager ToolItemList { get; } = new();
    public static MateriumManager MateriumList { get; } = new();
    public static RelicManager CollectableRelicList { get; } = new();
    public static JournalManager EnemyJournalList { get; } = new();
}

public class WeaverItem
{
    public string? AddressablesKey { get; }
    public SavedItem? Item { get; }
    public bool IsList { get; }
    
    public bool IsAddressable => !string.IsNullOrEmpty(AddressablesKey);

    public WeaverItem(string addressablesKey, bool isList)
    {
        AddressablesKey = addressablesKey;
        IsList = isList;
    }
    
    public WeaverItem(SavedItem item)
    {
        Item = item;
    }
}

public abstract class ListManager<MList, Item> where MList : NamedScriptableObjectList<Item> where Item : SavedItem
{
    public abstract MList? MasterList { get; }
    public bool AddingItemsToMasterList { get; private set; }

    protected List<WeaverItem>? Items { get; set; } = new();

    public virtual void AddItemToMasterList(WeaverItem item)
    {
        if (Items != null && !Items.Contains(item))
            Items.Add(item);
    }

    public virtual void AddItemToMasterList(Item item) => AddItemToMasterList(new WeaverItem(item));

    public virtual void AddItems(MList items)
    {
        foreach (var item in items)
            AddItemToMasterList(item);
    }

    public virtual void AddItems(string addressablesKey, bool isList) => Items?.Add(new WeaverItem(addressablesKey, isList)); 

    public void AddToMasterList()
    {
        if (AddingItemsToMasterList || MasterList == null || Items == null)
            return;

        Plugin.Instance.StartCoroutine(CoAddToMasterList());
    }
    
    protected virtual IEnumerator CoAddToMasterList()
    {
        AddingItemsToMasterList = true;

        List<Item> itemList = new();
        List<WeaverItem> itemsUsingAddressables = new();
        List<string> itemLists = new();

        foreach (var item in Items!)
        {
            if (item.IsAddressable)
            {
                if (item.IsList)
                    itemLists.Add(item.AddressablesKey!);
                else
                    itemsUsingAddressables.Add(item);
            }
            
            else 
            
            if (item.Item is Item savedItem)
            {
                itemList.Add(savedItem);
            }
        }

        // Load all collectable items using addressables
        yield return Addressables.LoadAssetsAsync<Item>(itemsUsingAddressables.Select(i => i.AddressablesKey), itemList.Add, Addressables.MergeMode.Union);
        
        // Load all collectable item lists using addressables
        yield return Addressables.LoadAssetsAsync<MList>
        (
            keys: itemLists, 
            callback: (list) => {
                foreach (var item in list)
                    itemList.Add(item);
            }, 
            mode: Addressables.MergeMode.Union
        );

        foreach (var item in itemList)
        {
            if (MasterList!.Contains(item))
                continue;

            MasterList.Add(item);
            Plugin.Instance.Logger.LogDebug($"Added {item.GetType().Name}: {item.name} to masterlist");
        }

        Items = null;
        AddingItemsToMasterList = false;
    }
}

public class CollectableItemListManager : ListManager<CollectableItemList, CollectableItem>
{
    public override CollectableItemList? MasterList => CollectableItemManager.Instance.masterList;
}

public class QuestListManager : ListManager<QuestList, BasicQuestBase>
{
    public override QuestList? MasterList => QuestManager.Instance.masterList;
}

public class ToolItemListManager : ListManager<ToolItemList, ToolItem>
{
    public override ToolItemList? MasterList => ToolItemManager.Instance.toolItems;
}

public class MateriumManager : ListManager<MateriumItemList, MateriumItem>
{
    public override MateriumItemList? MasterList => MateriumItemManager.Instance.masterList;
}

public class RelicManager : ListManager<CollectableRelicList, CollectableRelic>
{
    public override CollectableRelicList? MasterList => CollectableRelicManager.Instance.masterList;
}

public class JournalManager : ListManager<EnemyJournalRecordList, EnemyJournalRecord>
{
    public override EnemyJournalRecordList? MasterList => EnemyJournalManager.Instance.recordList;
}