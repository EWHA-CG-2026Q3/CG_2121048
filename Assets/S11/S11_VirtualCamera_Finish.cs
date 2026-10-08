using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

// 빈 오브젝트(VirtualCamera)에 붙여, 이 오브젝트를 카메라로 삼아 장면을 캔버스에 와이어프레임으로 그림
// 뷰 변환: 카메라를 기준으로 잰 정점 = V × M × 정점, V = R⁻¹ × T⁻¹
// 캔버스에 그리는 규칙: 정투영 (z는 그릴지 말지만 정하고, 위치 계산에는 쓰지 않음)
public class S11_VirtualCamera_Finish : MonoBehaviour
{
    [Header("그릴 오브젝트")]
    public MeshFilter[] targets;

    [Header("캔버스에 담을 범위")]
    public float size = 2f;
    public float near = 0.3f;
    public float far = 20f;

    [Header("캔버스")]
    public RawImage canvasImage;
    public int width = 400;
    public int height = 300;
    public Color backgroundColor = Color.black;
    public Color lineColor = Color.white;

    Texture2D canvas;
    Color[] clearPixels;

    void OnEnable()
    {
        CreateCanvas();
        RenderPipelineManager.beginContextRendering += OnBeginRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginContextRendering -= OnBeginRendering;
    }

    // Animator와 Constraint 등의 움직임이 반영된 뒤 화면을 다시 그림
    void OnBeginRendering(
        ScriptableRenderContext context,
        System.Collections.Generic.List<Camera> cameras)
    {
        if (canvas == null || canvas.width != width || canvas.height != height)
            CreateCanvas();

        DrawSceneOnCanvas();
    }

    // 캔버스 텍스처 생성
    void CreateCanvas()
    {
        canvas = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );

        canvas.filterMode = FilterMode.Point;

        clearPixels = new Color[width * height];

        for (int i = 0; i < clearPixels.Length; i++)
            clearPixels[i] = backgroundColor;

        if (canvasImage != null)
        {
            canvasImage.texture = canvas;

            canvasImage.rectTransform.sizeDelta =
                new Vector2(width, height);
        }
    }

    // --------------------------------------------------
    // 행렬 × 행렬을 "열 단위"로 직접 계산
    //
    // (A × B)의 n번째 열 = A × (B의 n번째 열)
    //
    // 즉,
    // B의 네 열을 하나씩 꺼내서
    // A를 곱한 뒤
    // 결과 행렬의 네 열에 다시 넣음
    // --------------------------------------------------
    Matrix4x4 MultiplyMatrixMatrix(Matrix4x4 A, Matrix4x4 B)
    {
        Matrix4x4 result = Matrix4x4.zero;

        result.SetColumn(0, A * B.GetColumn(0));
        result.SetColumn(1, A * B.GetColumn(1));
        result.SetColumn(2, A * B.GetColumn(2));
        result.SetColumn(3, A * B.GetColumn(3));

        return result;
    }

    // V = R⁻¹ × T⁻¹
    Matrix4x4 BuildViewMatrix(Transform cam)
    {
        // T⁻¹ : 카메라의 이동을 되돌림
        Matrix4x4 Tinv =
            Matrix4x4.Translate(-cam.position);

        // R⁻¹ : 카메라의 회전을 되돌림
        Matrix4x4 Rinv =
            Matrix4x4.Rotate(
                Quaternion.Inverse(cam.rotation)
            );

        // 기존:
        // return Rinv * Tinv;

        // 과제:
        // 행렬 × 행렬을 직접 만든 함수로 계산
        return MultiplyMatrixMatrix(Rinv, Tinv);
    }

    // VirtualCamera가 본 장면을 캔버스에 와이어프레임으로 그림
    void DrawSceneOnCanvas()
    {
        ClearCanvas();

        // 뷰 행렬 V
        Matrix4x4 V = BuildViewMatrix(transform);

        foreach (MeshFilter mf in targets)
        {
            if (mf == null || mf.sharedMesh == null)
                continue;

            // V × M
            Matrix4x4 VM =
                V * mf.transform.localToWorldMatrix;

            Vector3[] verts =
                mf.sharedMesh.vertices;

            int[] tris =
                mf.sharedMesh.triangles;

            for (int i = 0; i < tris.Length; i += 3)
            {
                Vector3 a =
                    VM.MultiplyPoint(
                        verts[tris[i]]
                    );

                Vector3 b =
                    VM.MultiplyPoint(
                        verts[tris[i + 1]]
                    );

                Vector3 c =
                    VM.MultiplyPoint(
                        verts[tris[i + 2]]
                    );

                DrawEdge(a, b);
                DrawEdge(b, c);
                DrawEdge(c, a);
            }
        }

        canvas.Apply();
    }

    // 카메라 기준 좌표를 캔버스 픽셀 좌표로 변환
    bool ToPixel(Vector3 p, out Vector2 pixel)
    {
        pixel = Vector2.zero;

        // near ~ far 밖이면 그리지 않음
        if (p.z < near || p.z > far)
            return false;

        float aspect =
            (float)width / height;

        pixel.x =
            width * 0.5f
            + p.x / (size * aspect)
            * (width * 0.5f);

        pixel.y =
            height * 0.5f
            + p.y / size
            * (height * 0.5f);

        return true;
    }

    // 카메라 기준 두 점 사이에 선을 그림
    void DrawEdge(Vector3 a, Vector3 b)
    {
        if (!ToPixel(a, out Vector2 pa))
            return;

        if (!ToPixel(b, out Vector2 pb))
            return;

        DrawLine(
            Mathf.RoundToInt(pa.x),
            Mathf.RoundToInt(pa.y),
            Mathf.RoundToInt(pb.x),
            Mathf.RoundToInt(pb.y)
        );
    }

    // 브레젠험 선 그리기
    void DrawLine(int x0, int y0, int x1, int y1)
    {
        int dx =
            Mathf.Abs(x1 - x0);

        int sx =
            x0 < x1 ? 1 : -1;

        int dy =
            -Mathf.Abs(y1 - y0);

        int sy =
            y0 < y1 ? 1 : -1;

        int err =
            dx + dy;

        while (true)
        {
            if (
                x0 >= 0 &&
                x0 < width &&
                y0 >= 0 &&
                y0 < height
            )
            {
                canvas.SetPixel(
                    x0,
                    y0,
                    lineColor
                );
            }

            if (x0 == x1 && y0 == y1)
                break;

            int e2 =
                2 * err;

            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    // 캔버스 전체 지우기
    void ClearCanvas()
    {
        canvas.SetPixels(clearPixels);
    }

    // Scene 뷰에 VirtualCamera의 축과 범위 표시
    void OnDrawGizmos()
    {
        Vector3 o =
            transform.position;

        // x축
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            o,
            o + transform.right
        );

        // y축
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            o,
            o + transform.up
        );

        // z축
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(
            o,
            o + transform.forward
        );

        float aspect =
            (float)width / height;

        float hx =
            size * aspect;

        float hy =
            size;

        Vector3 center =
            new Vector3(
                0,
                0,
                (near + far) * 0.5f
            );

        Vector3 extent =
            new Vector3(
                hx * 2,
                hy * 2,
                far - near
            );

        Gizmos.color =
            Color.yellow;

        Gizmos.matrix =
            Matrix4x4.TRS(
                transform.position,
                transform.rotation,
                Vector3.one
            );

        Gizmos.DrawWireCube(
            center,
            extent
        );

        Gizmos.matrix =
            Matrix4x4.identity;
    }
}