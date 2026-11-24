using UnityEngine;

// Wiggles only the existing tail child; eyes, foot and body stay attached to the fish root.
[DisallowMultipleComponent]
public sealed class S23FishTailWagMotion : MonoBehaviour
{
    [Range(0f, 35f)] public float wagAngle = 16f;
    [Min(0.1f)] public float wagSpeed = 5.2f;

    private Quaternion restingLocalRotation;

    private void OnEnable()
    {
        restingLocalRotation = transform.localRotation;
    }

    private void Update()
    {
        if (!Application.isPlaying) return;
        float yaw = Mathf.Sin(Time.time * wagSpeed) * wagAngle;
        transform.localRotation = restingLocalRotation * Quaternion.Euler(0f, yaw, 0f);
    }
}
