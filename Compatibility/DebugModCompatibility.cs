using System.Collections;
using System.Reflection;
using UnityEngine.Tilemaps;
using WorldWeaver.Managers;

namespace WorldWeaver.Compatibility;

public static class DebugModCompatibility
{
    static Type? hitboxRenderType;
    static Type? bindableFunctionsType;
    static Type? hitboxTypeType;

    static PropertyInfo? hitboxRenderInstanceProperty;
    
    static FieldInfo? terrainTypeField;
    static FieldInfo? collidersField;
    static FieldInfo? saveLevelStateActionField;

    static MethodInfo? shouldCullColliderMethod;
    static MethodInfo? drawPointSequenceMethod;
    static MethodInfo? updateHitboxMethod;

    public static void Patch(Assembly assembly, Harmony harmony)
    {
        hitboxRenderType = assembly.GetType("DebugMod.Hitbox.HitboxRender");
        bindableFunctionsType = assembly.GetType("DebugMod.BindableFunctions");
        hitboxTypeType = hitboxRenderType.GetNestedType("HitboxType", BindingFlags.NonPublic);
        
        hitboxRenderInstanceProperty = hitboxRenderType.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        terrainTypeField = hitboxTypeType.GetField("Terrain", BindingFlags.Public | BindingFlags.Static);
        collidersField = hitboxRenderType.GetField("colliders", BindingFlags.NonPublic | BindingFlags.Instance);
        saveLevelStateActionField = bindableFunctionsType.GetField("saveLevelStateAction", BindingFlags.Static | BindingFlags.NonPublic);

        shouldCullColliderMethod = AccessTools.Method(hitboxRenderType, "ShouldCullCollider", [typeof(Camera), typeof(Collider2D)]);
        drawPointSequenceMethod = AccessTools.Method(hitboxRenderType, "DrawPointSequence", [typeof(List<Vector2>), typeof(Camera), typeof(Collider2D), hitboxTypeType, typeof(float)]);
        updateHitboxMethod = AccessTools.Method(hitboxRenderType, "UpdateHitbox", [typeof(GameObject)]);

        harmony.Patch(original: AccessTools.Method(hitboxRenderType, "TryAddHitboxes"), postfix: new HarmonyMethod(typeof(DebugModCompatibility), nameof(TryAddHitboxes_Postfix)));
        harmony.Patch(original: AccessTools.Method(hitboxRenderType, "DrawHitbox"), postfix: new HarmonyMethod(typeof(DebugModCompatibility), nameof(DrawHitbox_Postfix)));
        
        harmony.Patch(original: AccessTools.Method(bindableFunctionsType, "GameManager_SaveLevelState_Postfix"), prefix: new HarmonyMethod(typeof(DebugModCompatibility), nameof(SaveLevelStatePatch_Prefix)));
    }

    static void SaveLevelStatePatch_Prefix()
    {
        string saveLevelStateAction = (string)saveLevelStateActionField!.GetValue(null);
        if (saveLevelStateAction != null && saveLevelStateAction != "block")
        {
            WeaverDataManager.ResetScenePersistentItems(saveLevelStateAction);
        }
    }

    static void DrawHitbox_Postfix(object __instance, Camera camera, Collider2D collider2D, object hitboxType, float lineWidth)
    {
        if (collider2D == null || !collider2D.isActiveAndEnabled)
            return;

        if (collider2D is TilemapCollider2D tmc2d)
        {   
            var composite = tmc2d.composite;

            if ((bool)shouldCullColliderMethod!.Invoke(__instance, [camera, composite]))
                return;

            for (int i = 0; i < composite.pathCount; i++)
            {
                Vector2[] path = new Vector2[composite.GetPathPointCount(i)];
                composite.GetPath(i, path);
                List<Vector2> points = new(path);

                if (points.Count > 0)
                    points.Add(points[0]);

                drawPointSequenceMethod?.Invoke(__instance, [points, camera, collider2D, terrainTypeField!.GetValue(null), lineWidth]);
            }
        }
    }

    static void TryAddHitboxes_Postfix(object __instance, Collider2D collider2D)
    {
        if (collider2D == null || collider2D is not TilemapCollider2D)
            return;

        var dict = collidersField?.GetValue(__instance);

        if (dict is not IDictionary colliders)
            return;

        var terrainType = terrainTypeField!.GetValue(null);
        if (!colliders.Contains(collider2D))
            colliders.Add(collider2D, terrainType);
    }

    public static void UpdateHitbox(GameObject go)
    {
        if (hitboxRenderInstanceProperty?.GetValue(null) != null)
            updateHitboxMethod?.Invoke(hitboxRenderInstanceProperty.GetValue(null), [go]);
    }
}