using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace WorldWeaver.Managers;

public static class WeaverTilemapManager 
{
    private static readonly List<Func<string, bool>> TilemapScenePredicates = [];

    public static bool IsCustomTilemapScene(string sceneName)
    {
        bool result = false;
        foreach (var func in TilemapScenePredicates)
        {
            if (func.Invoke(sceneName))
                result = true;
        }

        return result;
    }

    public static void AddCustomTilemapScenePredicate(Func<string, bool> predicate)
    {
        TilemapScenePredicates.Add(predicate);
    }
    
    public static void UpdateSceneDimensions()
    {
        var scene = SceneManager.GetActiveScene();

        Bounds? combinedBounds = null;

        var unityTilemaps = scene.GetRootGameObjects()
            .SelectMany(go => go.GetComponentsInChildren<Tilemap>(true));

        var tk2dMaps = scene.GetRootGameObjects()
            .SelectMany(go => go.GetComponentsInChildren<tk2dTileMap>(true))
            .Where(t => t.gameObject.name != "_TileMap (Not used.)");

        foreach (var ut in unityTilemaps)
        {
            ut.CompressBounds();
            var b = ut.cellBounds;

            var bounds = new Bounds();
            bounds.SetMinMax(ut.CellToWorld(b.min), ut.CellToWorld(b.max));

            combinedBounds = combinedBounds == null ? bounds : Combine(combinedBounds.Value, bounds);
        }

        foreach (var tm in tk2dMaps)
        {
            var bounds = new Bounds();
            bounds.SetMinMax(tm.transform.position, tm.transform.position + new Vector3(tm.width, tm.height, 0f));

            combinedBounds = combinedBounds == null ? bounds : Combine(combinedBounds.Value, bounds);
        }

        if (combinedBounds == null)
            return;

        var min = combinedBounds.Value.min;
        var max = combinedBounds.Value.max;

        GameManager.instance.sceneWidth = Mathf.RoundToInt(max.x - min.x);
        GameManager.instance.sceneHeight = Mathf.RoundToInt(max.y - min.y);
    }

    private static Bounds Combine(Bounds a, Bounds b)
    {
        a.Encapsulate(b);
        return a;
    }
}
