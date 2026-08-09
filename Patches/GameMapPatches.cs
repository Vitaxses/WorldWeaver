using System.Reflection;
using System.Reflection.Emit;

namespace WorldWeaver.Patches;

[HarmonyPatch(typeof(GameMap), nameof(GameMap.LevelReady))]
public static class GameMapPatches
{
    // This should stop the tilemap not found error spam
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var targetField = AccessTools.Field(typeof(GameMap), nameof(GameMap.corpseSceneMapZone));

        foreach (var instruction in instructions)
        {
            yield return instruction; // Do original stuff

            if (instruction.opcode == OpCodes.Stfld && instruction.operand is FieldInfo field && field == targetField)
            {
                yield return new CodeInstruction(OpCodes.Ret); // Return
                yield break;
            }
        }
    }
    
    [HarmonyPostfix]
    static void Postfix(GameMap __instance)
    {
        __instance.currentSceneSize = new(__instance.gm.sceneWidth, __instance.gm.sceneHeight);
    }
}