using UnityEngine;

// A close-up Game camera follows the ACTUAL fish along the S22 spline.
// It does not create extra objects or affect the pavilion geometry.
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class S23FishFollowCamera : MonoBehaviour
{
    public Transform fishRoot;
    public Vector3 cameraOffset = new Vector3(3.7f, 2.6f, -7.5f);
    public Vector3 lookAtOffset = new Vector3(0f, 1.3f, 0f);
    [Range(0.05f, 1f)] public float cameraSmoothing = 0.20f;

    private void LateUpdate()
    {
        if (!Application.isPlaying || fishRoot == null) return;
        Vector3 desired = fishRoot.position + cameraOffset;
        // A tracking shot showing the actual moving fish clearly.
        float t = 1f - Mathf.Pow(1f - cameraSmoothing, Time.deltaTime * 60f);
        transform.position = Vector3.Lerp(transform.position, desired, t);
        transform.LookAt(fishRoot.position + lookAtOffset);
    }
}
