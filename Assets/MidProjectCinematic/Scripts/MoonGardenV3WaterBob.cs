using UnityEngine;
public class MoonGardenV3WaterBob : MonoBehaviour
{
    public float bobHeight=.03f,speed=.65f,phase;
    Vector3 start;
    void Awake(){start=transform.localPosition;}
    void Update(){transform.localPosition=start+Vector3.up*(Mathf.Sin(Time.time*speed+phase)*bobHeight);}
}
