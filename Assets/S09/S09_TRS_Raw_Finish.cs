using UnityEngine;

// 동차좌표와 4×4 행렬을 이용해 shear 변환을 적용
// Unity의 Matrix4x4 타입 없이 float[4,4] 배열만으로 계산함
[ExecuteAlways]
[RequireComponent(typeof(DiamondMesh))]
public class S09_TRS_Raw_Finish : MonoBehaviour
{
    // 학번 끝자리 8 → (8 + 1) / 5 = 1.8
    [SerializeField] float k = 1.8f;

    DiamondMesh diamondMesh;

    void OnEnable()
    {
        diamondMesh = GetComponent<DiamondMesh>();
    }

    void Update()
    {
        if (diamondMesh == null || diamondMesh.BaseVertices == null)
            return;

        Vector3[] verts =
            ApplyShearRaw(diamondMesh.BaseVertices, k);

        diamondMesh.SetVertices(verts);
    }

    // Inspector에서 k값이 바뀔 때
    // 꼭대기 정점 (0.5, 1, 0.5)의 변환 결과 출력
    void OnValidate()
    {
        Vector3 topVertex = new Vector3(0.5f, 1f, 0.5f);

        float[,] H = ShearMatrixRaw(k);

        Vector4 h = ToHomogeneous(topVertex);
        h = MultiplyMatrixVectorRaw(H, h);

        Vector3 result = FromHomogeneous(h);

        Debug.Log(
            $"k = {k}, 꼭대기 정점 (0.5, 1, 0.5) → {result}"
        );
    }

    // ---------- 행렬 빌더 ----------

    // shear:
    // (x, y, z) → (x + k*y, y, z)
    //
    // e1 → (1, 0, 0)
    // e2 → (k, 1, 0)
    // e3 → (0, 0, 1)
    // 원점 → (0, 0, 0)
    float[,] ShearMatrixRaw(float k)
    {
        return new float[,]
        {
            { 1f,  k,  0f, 0f },
            { 0f, 1f,  0f, 0f },
            { 0f, 0f,  1f, 0f },
            { 0f, 0f,  0f, 1f }
        };
    }

    // ---------- 동차좌표 ----------

    Vector4 ToHomogeneous(Vector3 v)
    {
        return new Vector4(v.x, v.y, v.z, 1f);
    }

    Vector3 FromHomogeneous(Vector4 h)
    {
        return new Vector3(h.x, h.y, h.z);
    }

    // 교수님 원본의 4×4 행렬 × 벡터 코드 그대로
    Vector4 MultiplyMatrixVectorRaw(float[,] M, Vector4 v)
    {
        float[] input = { v.x, v.y, v.z, v.w };
        float[] result = new float[4];

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                result[row] += M[row, col] * input[col];
            }
        }

        return new Vector4(
            result[0],
            result[1],
            result[2],
            result[3]
        );
    }

    // ---------- 적용 ----------

    Vector3[] ApplyShearRaw(
        Vector3[] baseVertices,
        float k)
    {
        float[,] H = ShearMatrixRaw(k);

        Vector3[] verts =
            new Vector3[baseVertices.Length];

        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector4 h =
                ToHomogeneous(baseVertices[i]);

            h =
                MultiplyMatrixVectorRaw(H, h);

            verts[i] =
                FromHomogeneous(h);
        }

        return verts;
    }
}