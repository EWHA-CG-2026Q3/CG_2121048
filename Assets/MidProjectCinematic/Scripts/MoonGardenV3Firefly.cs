using UnityEngine;


public class MoonGardenV3Firefly : MonoBehaviour
{
    public float phase;
    public float pulseSpeed = 1.7f;
    public float bobHeight = .1f;
    private Vector3 startPosition;
    private Vector3 startScale;
    void Awake() { startPosition = transform.localPosition; startScale = transform.localScale; }
    void Update()
    {
        if (!Application.isPlaying) return;
        float pulse = .65f + .35f * Mathf.Sin(Time.time * pulseSpeed + phase);
        transform.localScale = startScale * pulse;
        transform.localPosition = startPosition + Vector3.up *
            (Mathf.Sin(Time.time * .7f + phase) * bobHeight);
    }
}
