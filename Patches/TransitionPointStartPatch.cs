using WorldWeaver.Managers;

namespace WorldWeaver.Patches;

[HarmonyPatch(typeof(TransitionPoint), nameof(TransitionPoint.Start))]
internal static class TransitionPointStartPatch
{
    [HarmonyPostfix]
    static void Postfix_Start(TransitionPoint __instance)
    {
        var transitionData = WeaverTransitionManager.GetCurrentGroup()?.GetTransitionData(__instance.name);
        if (transitionData == null)
            return;

        if (!string.IsNullOrEmpty(transitionData.destinationGate))
            __instance.SetTargetDoor(transitionData.destinationGate);

        if (!string.IsNullOrEmpty(transitionData.destinationScene))
            __instance.SetTargetScene(transitionData.destinationScene);
    }
}
