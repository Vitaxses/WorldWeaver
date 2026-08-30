using WorldWeaver.Managers;

namespace WorldWeaver.Patches;

[HarmonyPatch("Awake")]
public static class MasterListsPatch
{
    [HarmonyPatch(typeof(QuestManager))]
    [HarmonyPostfix]
    static void Awake(QuestManager __instance) => WeaverMasterLists.QuestList.AddToMasterList();
    
    [HarmonyPatch(typeof(MateriumItemManager), nameof(MateriumItemManager.Start))]
    [HarmonyPostfix]
    static void Start() => WeaverMasterLists.MateriumList.AddToMasterList();
    
    
    [HarmonyPatch(typeof(CollectableRelicManager), MethodType.Constructor)]
    static class RelicPatch
    {
        [HarmonyPostfix]
        static void RelicCtor() => WeaverMasterLists.CollectableRelicList.AddToMasterList();
    }

    [HarmonyPatch(typeof(EnemyJournalManager))]
    [HarmonyPostfix]
    static void Awake(EnemyJournalManager __instance) => WeaverMasterLists.EnemyJournalList.AddToMasterList();

    [HarmonyPatch(typeof(CollectableItemManager))]
    [HarmonyPostfix]
    static void Awake(CollectableItemManager __instance) => WeaverMasterLists.CollectableItemList.AddToMasterList();
    
    [HarmonyPatch(typeof(ToolItemManager))]
    [HarmonyPostfix]
    static void Awake(ToolItemManager __instance) => WeaverMasterLists.ToolItemList.AddToMasterList();
}
