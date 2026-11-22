using UnityEngine;

// S22 practice 3: a Bezier curve evaluated using De Casteljau interpolation.
[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class S22_Curve_Finish : MonoBehaviour
{
    public Transform[] points = new Transform[4];
    public Transform marker;
    [Range(0f, 1f)] public float t = 0.25f;
    [Range(2, 128)] public int segments = 32;
    public bool showConstructionGizmos = true;

    private void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (!HasValidPoints())
            return;

        Vector3[] ctrl = ControlPositions();
        int steps = Mathf.Max(2, segments);
        LineRenderer line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = steps + 1;
        for (int i = 0; i <= steps; i++)
        {
            float ti = (float)i / steps;
            line.SetPosition(i, DeCasteljau(ctrl, ti));
        }
        if (marker != null)
            marker.position = DeCasteljau(ctrl, t);
    }

    public static Vector3 DeCasteljau(Vector3[] control, float t)
    {
        if (control == null || control.Length == 0)
            return Vector3.zero;

        Vector3[] q = (Vector3[])control.Clone();
        for (int count = q.Length; count > 1; count--)
            for (int i = 0; i < count - 1; i++)
                q[i] = S22_Line_Finish.PointOnSegment(q[i], q[i + 1], t);
        return q[0];
    }

    private bool HasValidPoints()
    {
        if (points == null || points.Length < 2) return false;
        foreach (Transform p in points)
            if (p == null) return false;
        return true;
    }

    private Vector3[] ControlPositions()
    {
        Vector3[] result = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++) result[i] = points[i].position;
        return result;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showConstructionGizmos || !HasValidPoints()) return;
        Vector3[] q = ControlPositions();
        for (int count = q.Length, stage = 0; count > 1; count--, stage++)
        {
            Gizmos.color = stage == 0 ? Color.gray :
                           stage == 1 ? Color.green :
                           stage == 2 ? new Color(0.8f, 0.4f, 1f) : Color.yellow;
            for (int i = 0; i < count - 1; i++)
                Gizmos.DrawLine(q[i], q[i + 1]);
            for (int i = 0; i < count - 1; i++)
                q[i] = Vector3.Lerp(q[i], q[i + 1], t);
            for (int i = 0; i < count - 1; i++)
                Gizmos.DrawSphere(q[i], 0.09f);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(q[0], 0.15f);
    }
}
