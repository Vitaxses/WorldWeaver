using UnityEngine;

[ExecuteAlways] // runs in editor
public class WeaverDebugCocoonMarker : MonoBehaviour
{
    public bool show = true;

    public Color color = Color.cyan;
    public float size = 0.5f;

    private void OnDrawGizmos()
    {
        if (!show) return;

        Gizmos.color = color;

        Vector3 pos = transform.position;

        // Draw a small cross (better than a sphere for visibility)
        Gizmos.DrawLine(pos + Vector3.left * size, pos + Vector3.right * size);
        Gizmos.DrawLine(pos + Vector3.up * size, pos + Vector3.down * size);

        // Small center cube
        Gizmos.DrawCube(pos, Vector3.one * size * 0.2f);
    }
}
