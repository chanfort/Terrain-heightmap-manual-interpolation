using UnityEngine;

// Source http://wiki.unity3d.com/index.php?title=Barycentric&oldid=19264

public struct Barycentric
{
    public float u;
    public float v;
    public float w;

    public Barycentric(Vector2 aV1, Vector2 aV2, Vector2 aV3, Vector2 aP)
    {
        Vector2 a = aV2 - aV3;
        Vector2 b = aV1 - aV3;
        Vector2 c = aP - aV3;

        float aLen = a.x * a.x + a.y * a.y;
        float bLen = b.x * b.x + b.y * b.y;

        float ab = a.x * b.x + a.y * b.y;
        float ac = a.x * c.x + a.y * c.y;
        float bc = b.x * c.x + b.y * c.y;

        float d = aLen * bLen - ab * ab;

        u = (aLen * bc - ab * ac) / d;
        v = (bLen * ac - ab * bc) / d;
        w = 1.0f - u - v;
    }

    public Color Interpolate(Color v1, Color v2, Color v3)
    {
        return v1 * u + v2 * v + v3 * w;
    }
}
