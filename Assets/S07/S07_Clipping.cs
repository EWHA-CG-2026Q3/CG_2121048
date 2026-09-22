using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class S07_Clipping : MonoBehaviour
{
    [SerializeField] private int canvasWidth = 256;
    [SerializeField] private int canvasHeight = 256;
    [SerializeField] private int clipMargin = 40;

    // 왼쪽(x < 40) + 위쪽(y > 216)을 동시에 넘어가도록 설계
    [SerializeField]
    private List<Vector2> polygon = new List<Vector2> {
        new Vector2(10, 100),
        new Vector2(120, 250),
        new Vector2(200, 120)
    };

    [SerializeField] private Color fillColor = new Color(1f, 0.6f, 0.2f, 1f);
    [SerializeField] private Color marginOutlineColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    private Texture2D canvasTexture;
    private RawImage targetImage;

    void OnEnable() { RedrawAll(); }
    void OnValidate() { RedrawAll(); }

    private void RedrawAll()
    {
        targetImage = GetComponent<RawImage>();
        if (targetImage == null) return;

        if (canvasTexture == null ||
            canvasTexture.width != canvasWidth ||
            canvasTexture.height != canvasHeight)
        {
            canvasTexture = new Texture2D(canvasWidth, canvasHeight);
            canvasTexture.filterMode = FilterMode.Point;
        }

        for (int x = 0; x < canvasWidth; x++)
            for (int y = 0; y < canvasHeight; y++)
                canvasTexture.SetPixel(x, y, Color.black);

        DrawMarginOutline();

        List<Vector2> clipped = polygon;
        clipped = ClipLeft(clipped, clipMargin);
        clipped = ClipRight(clipped, canvasWidth - clipMargin);
        clipped = ClipBottom(clipped, clipMargin);
        clipped = ClipTop(clipped, canvasHeight - clipMargin);

        FillPolygon(clipped, fillColor);

        canvasTexture.Apply();
        targetImage.texture = canvasTexture;
    }

    private void DrawMarginOutline()
    {
        for (int x = clipMargin; x < canvasWidth - clipMargin; x++)
        {
            SetPixelSafe(x, clipMargin, marginOutlineColor);
            SetPixelSafe(x, canvasHeight - clipMargin - 1, marginOutlineColor);
        }

        for (int y = clipMargin; y < canvasHeight - clipMargin; y++)
        {
            SetPixelSafe(clipMargin, y, marginOutlineColor);
            SetPixelSafe(canvasWidth - clipMargin - 1, y, marginOutlineColor);
        }
    }

    private void SetPixelSafe(int x, int y, Color color)
    {
        if (x >= 0 && x < canvasWidth && y >= 0 && y < canvasHeight)
            canvasTexture.SetPixel(x, y, color);
    }

    // 왼쪽 경계: x >= boundary
    private List<Vector2> ClipLeft(List<Vector2> input, float boundary)
    {
        List<Vector2> output = new List<Vector2>();

        for (int i = 0; i < input.Count; i++)
        {
            Vector2 current = input[i];
            Vector2 previous = input[(i - 1 + input.Count) % input.Count];

            bool currentInside = current.x >= boundary;
            bool previousInside = previous.x >= boundary;

            if (currentInside)
            {
                if (!previousInside)
                    output.Add(GetIntersectionX(previous, current, boundary));

                output.Add(current);
            }
            else if (previousInside)
            {
                output.Add(GetIntersectionX(previous, current, boundary));
            }
        }

        return output;
    }

    // TODO 완성: 오른쪽 경계 x <= boundary
    private List<Vector2> ClipRight(List<Vector2> input, float boundary)
    {
        List<Vector2> output = new List<Vector2>();

        for (int i = 0; i < input.Count; i++)
        {
            Vector2 current = input[i];
            Vector2 previous = input[(i - 1 + input.Count) % input.Count];

            bool currentInside = current.x <= boundary;
            bool previousInside = previous.x <= boundary;

            if (currentInside)
            {
                if (!previousInside)
                    output.Add(GetIntersectionX(previous, current, boundary));

                output.Add(current);
            }
            else if (previousInside)
            {
                output.Add(GetIntersectionX(previous, current, boundary));
            }
        }

        return output;
    }

    // TODO 완성: 아래쪽 경계 y >= boundary
    private List<Vector2> ClipBottom(List<Vector2> input, float boundary)
    {
        List<Vector2> output = new List<Vector2>();

        for (int i = 0; i < input.Count; i++)
        {
            Vector2 current = input[i];
            Vector2 previous = input[(i - 1 + input.Count) % input.Count];

            bool currentInside = current.y >= boundary;
            bool previousInside = previous.y >= boundary;

            if (currentInside)
            {
                if (!previousInside)
                    output.Add(GetIntersectionY(previous, current, boundary));

                output.Add(current);
            }
            else if (previousInside)
            {
                output.Add(GetIntersectionY(previous, current, boundary));
            }
        }

        return output;
    }

    // TODO 완성: 위쪽 경계 y <= boundary
    private List<Vector2> ClipTop(List<Vector2> input, float boundary)
    {
        List<Vector2> output = new List<Vector2>();

        for (int i = 0; i < input.Count; i++)
        {
            Vector2 current = input[i];
            Vector2 previous = input[(i - 1 + input.Count) % input.Count];

            bool currentInside = current.y <= boundary;
            bool previousInside = previous.y <= boundary;

            if (currentInside)
            {
                if (!previousInside)
                    output.Add(GetIntersectionY(previous, current, boundary));

                output.Add(current);
            }
            else if (previousInside)
            {
                output.Add(GetIntersectionY(previous, current, boundary));
            }
        }

        return output;
    }

    private Vector2 GetIntersectionX(Vector2 p1, Vector2 p2, float boundaryX)
    {
        float t = (boundaryX - p1.x) / (p2.x - p1.x);

        return new Vector2(
            boundaryX,
            p1.y + t * (p2.y - p1.y)
        );
    }

    // TODO 완성: y 경계와의 교점
    private Vector2 GetIntersectionY(Vector2 p1, Vector2 p2, float boundaryY)
    {
        float t = (boundaryY - p1.y) / (p2.y - p1.y);

        return new Vector2(
            p1.x + t * (p2.x - p1.x),
            boundaryY
        );
    }

    private void FillPolygon(List<Vector2> poly, Color color)
    {
        if (poly.Count < 3) return;

        for (int i = 1; i < poly.Count - 1; i++)
            DrawTriangle(poly[0], poly[i], poly[i + 1], color);
    }

    private void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, Color color)
    {
        for (int x = 0; x < canvasWidth; x++)
        {
            for (int y = 0; y < canvasHeight; y++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);

                if (IsInsideTriangle(p, a, b, c))
                    canvasTexture.SetPixel(x, y, color);
            }
        }
    }

    private bool IsInsideTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float denom =
            a.x * (b.y - c.y) +
            b.x * (c.y - a.y) +
            c.x * (a.y - b.y);

        float w1 =
            (p.x * (b.y - c.y) +
            b.x * (c.y - p.y) +
            c.x * (p.y - b.y)) / denom;

        float w2 =
            (a.x * (p.y - c.y) +
            p.x * (c.y - a.y) +
            c.x * (a.y - p.y)) / denom;

        float w3 = 1f - w1 - w2;

        return w1 >= 0f && w2 >= 0f && w3 >= 0f;
    }
}