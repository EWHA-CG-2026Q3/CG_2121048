using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class S12_CustomCubeMesh : MonoBehaviour
{
    void Start()
    {
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0f, 0f, 0f), // 0
            new Vector3(1f, 0f, 0f), // 1
            new Vector3(1f, 1f, 0f), // 2
            new Vector3(0f, 1f, 0f), // 3
            new Vector3(0f, 0f, 1f), // 4
            new Vector3(1f, 0f, 1f), // 5
            new Vector3(1f, 1f, 1f), // 6
            new Vector3(0f, 1f, 1f)  // 7
        };

        int[] triangles = new int[]
        {
            // z = 0
            0, 2, 1,
            0, 3, 2,

            // z = 1
            4, 5, 6,
            4, 6, 7,

            // y = 0
            0, 1, 5,
            0, 5, 4,

            // y = 1
            3, 7, 6,
            3, 6, 2,

            // x = 0
            0, 4, 7,
            0, 7, 3,

            // x = 1
            1, 2, 6,
            1, 6, 5
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;

        GetComponent<MeshRenderer>().sharedMaterial =
            new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }
}