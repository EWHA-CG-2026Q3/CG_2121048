using UnityEngine;

// S22: Spline Animate moves the camera; this script turns it toward the Mover.
public class S22_LookAtTarget : MonoBehaviour
{
    public Transform target;

    private void LateUpdate()
    {
        if (target != null)
            transform.LookAt(target, Vector3.up);
    }
}
