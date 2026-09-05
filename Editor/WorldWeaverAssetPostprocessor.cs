using UnityEditor;
using WorldWeaver.Editor.Windows;

public class WorldWeaverAssetPostprocessor : AssetPostprocessor
{
    private static bool isProcessing;

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (isProcessing || !SceneTeleportMapWindow.AutoGenerateTPMWithImport)
            return;

        try
        {
            isProcessing = true;
            SceneTeleportMapWindow.Generate();
        }
        finally
        {
            isProcessing = false;
        }
    }
}