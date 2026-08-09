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
    
    public static bool IsCustomTilemapScene()
    {
        bool result = false;
        foreach (var func in TilemapScenePredicates)
        {
            if (func.Invoke(SceneManager.GetActiveScene().name))
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
            .SelectMany(go => go.GetComponentsInChildren<Tilemap>(true)).ToList();

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

            #if DEBUG

            if (!ut.TryGetComponent<TilemapCollider2D>(out var tc2d))
                Plugin.Instance.Logger.LogWarning($"Tilemap: {ut.name} does not have a TilemapCollider2D component, Collision may not work as expected");

            if (ut.gameObject.layer != 8)
                Plugin.Instance.Logger.LogWarning($"Tilemap: {ut.name} is on layer {ut.gameObject.layer}, expected layer 8 (Terrain)");

            #endif
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

    public static Bounds Combine(Bounds a, Bounds b)
    {
        a.Encapsulate(b);
        return a;
    }

    public static void ConvertTk2dTilemaps()
    {
        var scene = SceneManager.GetActiveScene();

        var unityTilemaps = scene.GetRootGameObjects()
            .SelectMany(go => go.GetComponentsInChildren<Tilemap>(true)).ToList();

        var tk2dMaps = scene.GetRootGameObjects()
            .SelectMany(go => go.GetComponentsInChildren<tk2dTileMap>(true))
            .Where(t => t.gameObject.name != "_TileMap (Not used.)").ToList();

        if (unityTilemaps.Count != 1 || tk2dMaps.Count <= 0)
            return;

        Tilemap tilemap = unityTilemaps[0];
        TileBase? unityTile = null;

        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            var tile = tilemap.GetTile(pos);

            if (tile != null)
            {
                unityTile = tile;
                break;
            }
        }

        if (unityTile == null)
        {
            Plugin.Instance.Logger.LogWarning("Could not find any tile in unity tilemap");
            return;
        }

        foreach (var tm in tk2dMaps)
        {
            HandleTk2dTilemap(tilemap, tm, unityTile);

            UnityEngine.Object.Destroy(tm.renderData);
            UnityEngine.Object.Destroy(tm.gameObject);
        }

        tilemap.RefreshAllTiles();
    }

    private static void HandleTk2dTilemap(Tilemap ut, tk2dTileMap tm, TileBase unityTile)
    {
        int partitionSizeX = tm.partitionSizeX;
        int partitionSizeY = tm.partitionSizeY;
        
        for (int i = 0; i < tm.Layers.Length; i++)
        {
            var layer = tm.Layers[i];
            var chunks = layer.spriteChannel.chunks;

            for (int chunk = 0; chunk < chunks.Length; chunk++)
            {
                var spriteChunk = chunks[chunk];

                if (spriteChunk != null && (spriteChunk.gameObject == null || !spriteChunk.gameObject.activeSelf))
                {
                    continue; // Skip disabled or deleted chunks
                }

                int chunkX = chunk % layer.numColumns;
                int chunkY = chunk / layer.numColumns;

                int startX = chunkX * partitionSizeX;
                int startY = chunkY * partitionSizeY;

                int endX = Mathf.Min(startX + partitionSizeX, tm.width);
                int endY = Mathf.Min(startY + partitionSizeY, tm.height);

                for (int x = startX; x < endX; x++)
                {
                    for (int y = startY; y < endY; y++)
                    {
                        int tileId = tm.GetTile(x, y, i);

                        if (tileId < 0)
                            continue;

                        Vector3 worldPosition = tm.GetTilePosition(x, y);
                        Vector3Int cell = ut.WorldToCell(worldPosition);

                        ut.SetTile(cell, unityTile);
                    }
                }
            }   
        }
    }
}
