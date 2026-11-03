using UnityEngine;
// Mild periodic motion of trees/branches, parent-child transform composition for midterm technique ④.
public class Mid_MoonGardenV3WindSway : MonoBehaviour
{
    public float strength=1.1f, speed=.45f, phase;
    Quaternion start;
    void Awake() { start=transform.localRotation; }
    void Update()
    {
        float a=Mathf.Sin(Time.time*speed+phase)*strength;
        float b=Mathf.Cos(Time.time*speed*.73f+phase*.6f)*strength*.49f;
        transform.localRotation=start*Quaternion.Euler(a,0f,b);
    }
}
