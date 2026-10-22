// PixelPainter.cs
// Draws textures from a per-pixel function, with supersampling for smooth edges.
// This is how all temporary art (lures, fish, icons, rounded panels) is made, so the UI needs no image files.

using UnityEngine;

namespace Tidebook.Ui
{
    public static class PixelPainter
    {
        /// <summary>x and y are normalized 0..1 across the texture (y = 0 at the bottom).</summary>
        public delegate Color Shader(float x, float y);

        public static Texture2D Paint(int width, int height, int samples, Shader shader)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            float step = 1f / samples;
            float count = samples * samples;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float r = 0f, g = 0f, b = 0f, a = 0f;
                    for (int sy = 0; sy < samples; sy++)
                    {
                        for (int sx = 0; sx < samples; sx++)
                        {
                            float u = (x + (sx + 0.5f) * step) / width;
                            float v = (y + (sy + 0.5f) * step) / height;
                            Color c = shader(u, v);
                            r += c.r * c.a;
                            g += c.g * c.a;
                            b += c.b * c.a;
                            a += c.a;
                        }
                    }

                    if (a > 0.0001f) pixels[y * width + x] = new Color(r / a, g / a, b / a, a / count);
                    else pixels[y * width + x] = new Color(0f, 0f, 0f, 0f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public static Sprite ToSprite(Texture2D tex, float pixelsPerUnit)
        {
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        public static Sprite ToSliced(Texture2D tex, float border)
        {
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f,
                                 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        // ---------------- geometry helpers (all in the same units as the inputs) ----------------

        /// <summary>Distance from point p to the segment a-b.</summary>
        public static float Segment(float px, float py, float ax, float ay, float bx, float by)
        {
            float abx = bx - ax;
            float aby = by - ay;
            float len2 = abx * abx + aby * aby;
            float t = len2 > 0f ? Mathf.Clamp01(((px - ax) * abx + (py - ay) * aby) / len2) : 0f;
            float dx = px - (ax + abx * t);
            float dy = py - (ay + aby * t);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>Approximate signed distance to an ellipse (negative inside).</summary>
        public static float Ellipse(float px, float py, float cx, float cy, float a, float b)
        {
            float nx = (px - cx) / a;
            float ny = (py - cy) / b;
            return (Mathf.Sqrt(nx * nx + ny * ny) - 1f) * Mathf.Min(a, b);
        }

        /// <summary>Signed distance to a rounded square in 0..1 space (negative inside).</summary>
        public static float RoundedSquare(float x, float y, float radius)
        {
            float qx = Mathf.Abs(x - 0.5f) - 0.5f + radius;
            float qy = Mathf.Abs(y - 0.5f) - 0.5f + radius;
            float ox = Mathf.Max(qx, 0f);
            float oy = Mathf.Max(qy, 0f);
            return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
        }

        public static bool InTriangle(float px, float py, float ax, float ay, float bx, float by, float cx, float cy)
        {
            float d1 = (px - bx) * (ay - by) - (ax - bx) * (py - by);
            float d2 = (px - cx) * (by - cy) - (bx - cx) * (py - cy);
            float d3 = (px - ax) * (cy - ay) - (cx - ax) * (py - ay);
            bool hasNeg = d1 < 0f || d2 < 0f || d3 < 0f;
            bool hasPos = d1 > 0f || d2 > 0f || d3 > 0f;
            return !(hasNeg && hasPos);
        }
    }
}
