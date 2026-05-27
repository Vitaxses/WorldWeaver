using UnityEngine;
using UnityEditor;

public enum BakeMode
{
    All,

    Left,
    Right,
    Up,
    Down,

    Horizontal,
    Vertical
}

[CustomEditor(typeof(CameraLockArea))]
[CanEditMultipleObjects]
public class CameraLockAreaEditor : Editor
{
    private BakeMode bakeMode = BakeMode.All;
    private float margin = 0f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        bakeMode = (BakeMode)EditorGUILayout.EnumPopup("Bake Mode", bakeMode);
        margin = EditorGUILayout.FloatField("Margin", margin);

        if (GUILayout.Button("Bake Bounds From BoxCollider2D"))
        {
            foreach (var obj in targets)
            {
                Bake((CameraLockArea)obj);
            }

            Debug.Log($"Baked {targets.Length} CameraLockArea(s) with mode: {bakeMode}");
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Reset All Bounds (-1)"))
        {
            foreach (var obj in targets)
            {
                ResetBounds((CameraLockArea)obj);
            }
        }
    }

    private void Bake(CameraLockArea cla)
    {
        BoxCollider2D box = cla.GetComponent<BoxCollider2D>();

        if (box == null)
        {
            Debug.LogWarning($"No BoxCollider2D found on {cla.gameObject.name}");
            return;
        }

        Undo.RecordObject(cla, "Bake CameraLockArea Bounds");

        Bounds b = box.bounds;

        float left = b.min.x - margin;
        float right = b.max.x + margin;
        float top = b.max.y + margin;
        float bottom = b.min.y - margin;

        if (bakeMode == BakeMode.All)
        {
            ResetCameraBoundsOnly(cla);
        }

        switch (bakeMode)
        {
            case BakeMode.All:
                cla.cameraXMin = left;
                cla.cameraXMax = right;
                cla.cameraYMin = bottom;
                cla.cameraYMax = top;
                break;

            case BakeMode.Left:
                cla.cameraXMin = left;
                break;

            case BakeMode.Right:
                cla.cameraXMax = right;
                break;

            case BakeMode.Up:
                cla.cameraYMax = top;
                break;

            case BakeMode.Down:
                cla.cameraYMin = bottom;
                break;

            case BakeMode.Horizontal:
                cla.cameraXMin = left;
                cla.cameraXMax = right;
                break;

            case BakeMode.Vertical:
                cla.cameraYMin = bottom;
                cla.cameraYMax = top;
                break;
        }

        bool affectsVertical =
            bakeMode == BakeMode.All ||
            bakeMode == BakeMode.Vertical ||
            bakeMode == BakeMode.Up ||
            bakeMode == BakeMode.Down;

        if (affectsVertical)
        {
            if (cla.preventLookDown)
            {
                cla.lookYMin = b.min.y;
            }

            if (cla.preventLookUp)
            {
                cla.lookYMax = b.max.y;
            }
        }

        cla.positionSpace = Space.World;

        EditorUtility.SetDirty(cla);
    }

    private void ResetBounds(CameraLockArea cla)
    {
        Undo.RecordObject(cla, "Reset CameraLockArea Bounds");

        cla.cameraXMin = -1f;
        cla.cameraXMax = -1f;
        cla.cameraYMin = -1f;
        cla.cameraYMax = -1f;

        cla.lookYMin = -1f;
        cla.lookYMax = -1f;

        EditorUtility.SetDirty(cla);
    }

    private void ResetCameraBoundsOnly(CameraLockArea cla)
    {
        cla.cameraXMin = -1f;
        cla.cameraXMax = -1f;
        cla.cameraYMin = -1f;
        cla.cameraYMax = -1f;
    }
}
