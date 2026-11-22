using UnityEngine;

// S22 practice 2: linear interpolation between A and B.
[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class S22_Line_Finish : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public Transform marker;
    [Range(0f, 1f)] public float t = 0.25f;

    protected virtual void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (pointA == null || pointB == null)
            return;

        Vector3 a = pointA.position;
        Vector3 b = pointB.position;
        LineRenderer line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.SetPosition(0, a);
        line.SetPosition(1, b);

        Vector3 p = PointOnSegment(a, b, t);
        if (marker != null)
            marker.position = p;
    }

    // The answer to the S22_Line.cs TODO exercise.
    public static Vector3 PointOnSegment(Vector3 a, Vector3 b, float t)
    {
        return a + t * (b - a);
    }
}
