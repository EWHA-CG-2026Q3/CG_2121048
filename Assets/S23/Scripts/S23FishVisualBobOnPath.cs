using UnityEngine;

// The fish's SplineAnimate moves the ROOT; this component only adds a soft swim
// to its child visuals, so the two components never fight over a Transform.
[DisallowMultipleComponent]
public sealed class S23FishVisualBobOnPath : MonoBehaviour
{
    [Range(0f, 0.4f)] public float bobHeight = 0.13f;
    [Range(0f, 15f)] public float yawAngle = 8f;
    [Range(0f, 10f)] public float rollAngle = 4f;
    [Min(0.1f)] public float swimSpeed = 1.7f;

    private Vector3 restPosition;
    private Quaternion restRotation;

    private void OnEnable()
    {
        restPosition = transform.localPosition;
        restRotation = transform.localRotation;
    }

    private void Update()
    {
        if (!Application.isPlaying) return;
        float t = Time.time * swimSpeed;
        transform.localPosition = restPosition + Vector3.up * (Mathf.Sin(t) * bobHeight);
        transform.localRotation = restRotation * Quaternion.Euler(0f,
            Mathf.Sin(t * 0.75f) * yawAngle,
            Mathf.Cos(t * 0.92f) * rollAngle);
    }
}
