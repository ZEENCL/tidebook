// HudSprites.cs
// Temporary art for the gameplay HUD, all generated in code: rings, a speech bubble, a coin,
// sun / moon and weather icons.

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook.Ui
{
    public static class HudSprites
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        static readonly Color Clear = new Color(0f, 0f, 0f, 0f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            cache.Clear();
        }

        static Sprite Make(string key, int width, int height, int samples, PixelPainter.Shader shader)
        {
            Sprite sprite;
            if (cache.TryGetValue(key, out sprite) && sprite != null) return sprite;
            sprite = PixelPainter.ToSprite(PixelPainter.Paint(width, height, samples, shader), 100f);
            cache[key] = sprite;
            return sprite;
        }

        /// <summary>A white ring. Thickness is in pixels of a 128px texture.</summary>
        public static Sprite Ring(int thickness)
        {
            return Make("ring|" + thickness, 128, 128, 2, delegate(float x, float y)
            {
                float dx = x - 0.5f;
                float dy = y - 0.5f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float outer = 0.48f;
                float inner = outer - thickness / 128f;
                return (r <= outer && r >= inner) ? Color.white : Clear;
            });
        }

        /// <summary>A white speech bubble with a tail at the bottom.</summary>
        public static Sprite Bubble()
        {
            return Make("bubble", 128, 144, 3, delegate(float x, float y)
            {
                float px = x * 128f;
                float py = y * 144f;
                float dx = px - 64f;
                float dy = py - 84f;
                bool body = dx * dx + dy * dy <= 56f * 56f;
                bool tail = PixelPainter.InTriangle(px, py, 44f, 46f, 84f, 46f, 64f, 6f);
                return (body || tail) ? Color.white : Clear;
            });
        }

        public static Sprite Coin()
        {
            return Make("coin", 64, 64, 3, delegate(float x, float y)
            {
                float dx = x - 0.5f;
                float dy = y - 0.5f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                if (r > 0.46f) return Clear;

                Color c = UiPalette.Sun;
                if (r > 0.40f) c = UiPalette.Hex("#D9A02B");
                else if (r > 0.27f && r < 0.31f) c = UiPalette.Hex("#FFE59A");

                float hx = x - 0.38f;
                float hy = y - 0.64f;
                if (hx * hx + hy * hy < 0.0045f) c = Color.Lerp(c, Color.white, 0.8f);
                return c;
            });
        }

        public static Sprite Sun()
        {
            return Make("sun", 96, 96, 3, delegate(float x, float y)
            {
                float dx = x - 0.5f;
                float dy = y - 0.5f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                if (r < 0.2f) return UiPalette.Sun;

                for (int k = 0; k < 8; k++)
                {
                    float a = k * Mathf.PI / 4f;
                    float ca = Mathf.Cos(a);
                    float sa = Mathf.Sin(a);
                    float d = PixelPainter.Segment(dx, dy, ca * 0.29f, sa * 0.29f, ca * 0.43f, sa * 0.43f);
                    if (d < 0.038f) return UiPalette.Sun;
                }
                return Clear;
            });
        }

        public static Sprite Moon()
        {
            return Make("moon", 96, 96, 3, delegate(float x, float y)
            {
                float ax = x - 0.46f;
                float ay = y - 0.5f;
                float bx = x - 0.60f;
                float by = y - 0.58f;
                bool inMoon = ax * ax + ay * ay < 0.32f * 0.32f;
                bool cut = bx * bx + by * by < 0.27f * 0.27f;
                return (inMoon && !cut) ? UiPalette.Hex("#DDE6FF") : Clear;
            });
        }

        public static Sprite WeatherIcon(Weather weather)
        {
            return Make("weather|" + weather, 96, 96, 3, delegate(float x, float y)
            {
                return WeatherPixel(weather, x, y);
            });
        }

        static bool InCloud(float x, float y)
        {
            return Circle(x, y, 0.34f, 0.56f, 0.16f) || Circle(x, y, 0.52f, 0.64f, 0.21f)
                   || Circle(x, y, 0.68f, 0.55f, 0.15f)
                   || (x > 0.30f && x < 0.78f && y > 0.40f && y < 0.54f);
        }

        static bool Circle(float x, float y, float cx, float cy, float r)
        {
            float dx = x - cx;
            float dy = y - cy;
            return dx * dx + dy * dy < r * r;
        }

        static Color WeatherPixel(Weather weather, float x, float y)
        {
            switch (weather)
            {
                case Weather.Rain:
                case Weather.Storm:
                {
                    if (InCloud(x, y)) return y < 0.46f ? UiPalette.Hex("#C9DCE3") : UiPalette.Hex("#EAF4F7");

                    if (weather == Weather.Rain)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            float sx = 0.38f + i * 0.14f;
                            if (PixelPainter.Segment(x, y, sx, 0.34f, sx - 0.04f, 0.18f) < 0.03f) return UiPalette.Sky;
                        }
                    }
                    else
                    {
                        if (PixelPainter.Segment(x, y, 0.56f, 0.40f, 0.46f, 0.25f) < 0.035f) return UiPalette.Sun;
                        if (PixelPainter.Segment(x, y, 0.46f, 0.25f, 0.55f, 0.25f) < 0.035f) return UiPalette.Sun;
                        if (PixelPainter.Segment(x, y, 0.55f, 0.25f, 0.44f, 0.08f) < 0.035f) return UiPalette.Sun;
                    }
                    return Clear;
                }
                case Weather.Fog:
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float yy = 0.66f - i * 0.17f;
                        float x0 = 0.18f + (i % 2) * 0.08f;
                        if (PixelPainter.Segment(x, y, x0, yy, x0 + 0.62f, yy) < 0.04f) return UiPalette.Hex("#D2E3EA");
                    }
                    return Clear;
                }
                default:   // clear: a little sparkle
                {
                    float dx = Mathf.Abs(x - 0.5f);
                    float dy = Mathf.Abs(y - 0.5f);
                    bool vertical = dx / 0.10f + dy / 0.36f < 1f;
                    bool horizontal = dx / 0.36f + dy / 0.10f < 1f;
                    return (vertical || horizontal) ? UiPalette.Hex("#FFE59A") : Clear;
                }
            }
        }
    }
}
