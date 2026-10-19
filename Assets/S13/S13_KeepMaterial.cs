using UnityEngine;

[DefaultExecutionOrder(1000)]
public class S13_KeepMaterial : MonoBehaviour
{
    public Material materialToKeep;

    void Start()
    {
        ApplyMaterial();
    }

    void LateUpdate()
    {
        ApplyMaterial();
    }

    void ApplyMaterial()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();

        if (mr != null && materialToKeep != null)
            mr.sharedMaterial = materialToKeep;
    }
}