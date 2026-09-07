using System.Reflection;
using System.Reflection.Emit;

namespace WorldWeaver.Patches;

#pragma warning disable HARMONIZE001 // Ambiguous target found for patch

// Fix null error on death
[HarmonyPatch(typeof(HeroController), nameof(HeroController.Die), MethodType.Enumerator)]
static class HeroControllerPatch
{
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Die(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();

        var gmField = AccessTools.Field(typeof(HeroController), "gm");
        var widthField = AccessTools.Field(typeof(tk2dTileMap), "width");
        var heightField = AccessTools.Field(typeof(tk2dTileMap), "height");

        for (int i = 1; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand is FieldInfo field && (field == heightField || field == widthField))
            {
                var loadGm = new CodeInstruction(OpCodes.Ldloc_1);
                loadGm.labels.AddRange(codes[i - 1].labels);

                codes[i - 1] = loadGm;
                codes[i] = new CodeInstruction(OpCodes.Ldfld, gmField);

                codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldfld, field == heightField ? AccessTools.Field(typeof(GameManager), "sceneHeight") : AccessTools.Field(typeof(GameManager), "sceneWidth")));
            }
        }

        return codes;
    }
}


#pragma warning restore HARMONIZE001 // Ambiguous target found for patch