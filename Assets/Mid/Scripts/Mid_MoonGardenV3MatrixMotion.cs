using UnityEngine;

// Intermediate project, ④ Transformations: Matrix4x4 composition and parent -> child hierarchy.
public class Mid_MoonGardenV3MatrixMotion : MonoBehaviour
{
    public Vector3 localCenter;
    public float speedDegrees = 22f;
    public float orbitRadius = 0f;
    public float bobAmplitude = .13f;
    public float bobSpeed = 1.25f;
    public float phase = 0f;
    private Quaternion initialRotation;

    void Awake() { initialRotation = transform.localRotation; }
    void Update()
    {
        if (!Application.isPlaying) return;
        float t = Time.time;
        Matrix4x4 translation = Matrix4x4.Translate(localCenter +
            Vector3.up * (Mathf.Sin(t * bobSpeed + phase) * bobAmplitude));
        Matrix4x4 rotation = Matrix4x4.Rotate(Quaternion.Euler(0f, t * speedDegrees + phase * 30f, 0f));
        Matrix4x4 radialOffset = Matrix4x4.Translate(new Vector3(orbitRadius, 0f, 0f));
        Matrix4x4 combined = translation * rotation * radialOffset;
        transform.localPosition = combined.MultiplyPoint3x4(Vector3.zero);
        transform.localRotation = rotation.rotation * initialRotation;
    }
}

// Optional second motion: pulsing fireflies, independent of the crystal carousel.
