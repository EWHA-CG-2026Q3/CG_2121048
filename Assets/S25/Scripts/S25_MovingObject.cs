using UnityEngine;

// Moving, non-static object: Baked Reflection Probe cannot capture its current position.
public class S25_MovingObject : MonoBehaviour
{
    public float amplitude = 1.1f;
    public float speed = 0.7f;
    private Vector3 origin;
    void Start() { origin = transform.position; }
    void Update()
    {
        transform.position = origin + new Vector3(Mathf.Sin(Time.time * speed) * amplitude,
            Mathf.Sin(Time.time * speed * 1.4f) * 0.25f, 0f);
        transform.Rotate(0f, 18f * Time.deltaTime, 0f, Space.World);
    }
}
