using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(GradeMarker))]
public class WeaverDebugGradeMarker : MonoBehaviour
{
    public bool show = true;

    public Color innerColor = Color.cyan;
    public Color outerColor = Color.blue;

    private GradeMarker? marker;

    private void OnDrawGizmos()
    {
        if (!show) return;

        if (marker == null)
            marker = GetComponent<GradeMarker>();

        if (marker == null) return;

        Vector3 pos = transform.position;

        float inner = marker.maxIntensityRadius;
        float outer = marker.cutoffRadius;

        // Outer cutoff ring
        Gizmos.color = outerColor;
        Gizmos.DrawWireSphere(pos, outer);

        // Inner full-intensity ring
        Gizmos.color = innerColor;
        Gizmos.DrawWireSphere(pos, inner);
    }
}
