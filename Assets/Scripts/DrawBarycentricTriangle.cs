using UnityEngine;

public class DrawBarycentricTriangle : MonoBehaviour
{
    Texture2D tex;

    void Start()
    {
        GenerateTexture();
    }

    void GenerateTexture()
    {
        int n = 512;
        Color[] pixels = new Color[n * n];
        
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i >= j)
                {
                    Barycentric b = new Barycentric(new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(i * 1f / n, j * 1f / n));
                    var c = b.Interpolate(Color.red, Color.green, Color.blue);
                    int k = i * n + j;
                    pixels[k] = c;
                }
            }
        }

        tex = new Texture2D(n, n);
        tex.SetPixels(pixels);
        tex.Apply();
    }

    void OnGUI()
    {
        if (tex != null)
        {
            GUI.DrawTexture(new Rect(0, 0, 512, 512), tex);
        }
    }
}
