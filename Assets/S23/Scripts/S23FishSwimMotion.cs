using UnityEngine;

// Adds a gentle looping swim motion to the existing S23 teal fish.
// Keeps the fish near its original position; does not affect the pavilion or S22 splines.
[DisallowMultipleComponent]
public sealed class S23FishSwimMotion : MonoBehaviour
{
    [Header("Swim near the original position")]
    [Min(0f)] public float leftRightDistance = 0.55f;
    [Min(0f)] public float depthDistance = 0.18f;
    [Min(0f)] public float upDownDistance = 0.09f;
    [Min(0.05f)] public float swimSpeed = 1.15f;
    [Range(0f, 15f)] public float turnAngle = 8f;
    [Range(0f, 10f)] public float rollAngle = 3f;

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;
    private bool initialized;

    private void OnEnable()
    {
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
        initialized = true;
    }

    private void Update()
    {
        if (!Application.isPlaying) return;
        if (!initialized) OnEnable();

        float t = Time.time * swimSpeed;
        float x = Mathf.Sin(t) * leftRightDistance;
        float z = Mathf.Cos(t * 0.55f) * depthDistance - depthDistance;
        float y = Mathf.Sin(t * 1.7f) * upDownDistance;

        transform.localPosition = startLocalPosition + new Vector3(x, y, z);
        transform.localRotation = startLocalRotation * Quaternion.Euler(
            0f, Mathf.Cos(t) * turnAngle, Mathf.Sin(t * 1.3f) * rollAngle);
    }
}
