// Credit to Jade for the script!

using UnityEngine;

[ExecuteInEditMode]
public class WeaverDebugCollider : MonoBehaviour
{
    public bool render = true;

    [SerializeField]
    private Color gizmoColor = Color.magenta;

    private void OnDrawGizmos()
    {
        if (!render)
            return;

        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;

        // Box
        var box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Gizmos.DrawWireCube(box.offset, box.size);
        }

        // Composite
        var composite = GetComponent<CompositeCollider2D>();
        if (composite != null)
        {
            DrawComposite(composite);
        }

        // Polygon
        var poly = GetComponent<PolygonCollider2D>();
        if (poly != null)
        {
            DrawPolygon(poly);
        }

        // Edge
        var edge = GetComponent<EdgeCollider2D>();
        if (edge != null)
        {
            DrawEdge(edge);
        }
    }

    static void DrawComposite(CompositeCollider2D col)
    {
        for (int i = 0; i < col.pathCount; i++)
        {
            int count = col.GetPathPointCount(i);
            Vector2[] points = new Vector2[count];
            col.GetPath(i, points);

            for (int j = 0; j < count; j++)
            {
                Vector2 a = points[j];
                Vector2 b = points[(j + 1) % count];
                Gizmos.DrawLine(a, b);
            }
        }
    }

    static void DrawPolygon(PolygonCollider2D col)
    {
        for (int i = 0; i < col.pathCount; i++)
        {
            var points = col.GetPath(i);

            for (int j = 0; j < points.Length; j++)
            {
                Vector2 a = points[j];
                Vector2 b = points[(j + 1) % points.Length];
                Gizmos.DrawLine(a, b);
            }
        }
    }

    static void DrawEdge(EdgeCollider2D col)
    {
        var points = col.points;

        for (int i = 0; i < points.Length - 1; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
    }
}
