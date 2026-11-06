// BaitArt.cs
// Temporary art for the bait system, all generated in code (the GDD says the player never sees the
// bait underwater, so this only appears in the builder and HUD):
//   Lure(shape, color)       - the little lure picture, flat-shaded and friendly
//   Swatch(color)            - round color/pattern chip
//   BuoyancyIcon(buoyancy)   - mini water column showing where the bait sits
//   SwimIcon(swim)           - path glyph showing how the bait moves

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook.Ui
{
    public static class BaitArt
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        // a wavy "worm" centre line, computed once
        static readonly float[] WormX = new float[30];
        static readonly float[] WormY = new float[30];

        static readonly Color Rim = new Color(0.10f, 0.20f, 0.26f, 1f);
        static readonly Color Clear = new Color(0f, 0f, 0f, 0f);

        static BaitArt()
        {
            for (int i = 0; i < WormX.Length; i++)
            {
                float t = i / (float)(WormX.Length - 1);
                WormX[i] = 0.12f + 0.76f * t;
                WormY[i] = 0.5f + 0.2f * Mathf.Sin(t * Mathf.PI * 2.5f);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            cache.Clear();
        }

        static Sprite Cached(string key, int size, int samples, PixelPainter.Shader shader)
        {
            Sprite sprite;
            if (cache.TryGetValue(key, out sprite) && sprite != null) return sprite;
            sprite = PixelPainter.ToSprite(PixelPainter.Paint(size, size, samples, shader), 100f);
            cache[key] = sprite;
            return sprite;
        }

        // =====================================================================
        // Lure
        // =====================================================================
        public static Sprite Lure(BodyShape shape, ColorPattern pattern)
        {
            return Cached("lure|" + shape + "|" + pattern, 128, 3, delegate(float x, float y)
            {
                return LurePixel(shape, pattern, x, y);
            });
        }

        static Color LurePixel(BodyShape shape, ColorPattern pattern, float x, float y)
        {
            float d = BodyDistance(shape, x, y);
            bool tail = InTail(shape, x, y);

            if (d > 0f && !tail)
            {
                if (pattern == ColorPattern.Glow)
                {
                    float g = Mathf.Clamp01(1f - d / 0.14f);
                    return new Color(0.78f, 1f, 0.55f, g * g * 0.55f);   // soft halo around glowing lures
                }
                return Clear;
            }

            Color c = Pattern(pattern, Mathf.Clamp01((x - 0.1f) / 0.8f), y);
            if (d > 0f) c = Color.Lerp(c, Color.black, 0.18f);       // tail fin a touch darker
            else if (d > -0.02f) c = Color.Lerp(c, Rim, 0.6f);        // outline

            if (shape != BodyShape.Curly)
            {
                float ex = x - EyeX(shape);
                float ey = y - 0.55f;
                float eye = Mathf.Sqrt(ex * ex + ey * ey);
                if (eye < 0.022f) c = Rim;
                else if (eye < 0.05f) c = Color.white;
            }
            return c;
        }

        static float EyeX(BodyShape shape)
        {
            switch (shape)
            {
                case BodyShape.Round: return 0.66f;
                case BodyShape.Wide: return 0.72f;
                default: return 0.80f;
            }
        }

        static float BodyDistance(BodyShape shape, float x, float y)
        {
            switch (shape)
            {
                case BodyShape.Round:
                    return PixelPainter.Ellipse(x, y, 0.53f, 0.50f, 0.26f, 0.25f);
                case BodyShape.Wide:
                    return PixelPainter.Ellipse(x, y, 0.52f, 0.50f, 0.34f, 0.19f);
                case BodyShape.Curly:
                {
                    float best = 10f;
                    for (int i = 0; i < WormX.Length - 1; i++)
                    {
                        float seg = PixelPainter.Segment(x, y, WormX[i], WormY[i], WormX[i + 1], WormY[i + 1]);
                        if (seg < best) best = seg;
                    }
                    return best - 0.07f;
                }
                default:
                    return PixelPainter.Ellipse(x, y, 0.54f, 0.50f, 0.38f, 0.12f);
            }
        }

        static bool InTail(BodyShape shape, float x, float y)
        {
            switch (shape)
            {
                case BodyShape.Round: return PixelPainter.InTriangle(x, y, 0.30f, 0.50f, 0.12f, 0.70f, 0.12f, 0.30f);
                case BodyShape.Wide: return PixelPainter.InTriangle(x, y, 0.20f, 0.50f, 0.04f, 0.72f, 0.04f, 0.28f);
                case BodyShape.Curly: return false;
                default: return PixelPainter.InTriangle(x, y, 0.18f, 0.50f, 0.02f, 0.68f, 0.02f, 0.32f);
            }
        }

        /// <summary>u runs along the body (0..1), v runs bottom..top (0..1).</summary>
        static Color Pattern(ColorPattern pattern, float u, float v)
        {
            switch (pattern)
            {
                case ColorPattern.BlueSilver:
                {
                    Color top = UiPalette.Hex("#3B7DDD");
                    Color belly = UiPalette.Hex("#D8E1EC");
                    return Color.Lerp(belly, top, Mathf.SmoothStep(0.30f, 0.70f, v));
                }
                case ColorPattern.GreenBrown:
                {
                    Color green = UiPalette.Hex("#5FA24A");
                    Color brown = UiPalette.Hex("#7B5B3A");
                    bool spot = Mathf.Sin(u * 17f) * Mathf.Sin(v * 13f + u * 5f) > 0.55f;
                    return spot ? brown : green;
                }
                case ColorPattern.Gold:
                {
                    Color gold = UiPalette.Hex("#F0B429");
                    Color light = UiPalette.Hex("#FFE59A");
                    return Color.Lerp(gold, light, Mathf.SmoothStep(0.45f, 1f, v));
                }
                case ColorPattern.Glow:
                {
                    Color rim = UiPalette.Hex("#B8FF7A");
                    Color core = UiPalette.Hex("#F4FFD0");
                    float centre = 1f - Mathf.Abs(v - 0.5f) * 2f;
                    return Color.Lerp(rim, core, Mathf.Clamp01(centre));
                }
                default:   // RedWhite
                {
                    Color red = UiPalette.Hex("#E5484D");
                    Color white = UiPalette.Hex("#FFF6EE");
                    float s = Mathf.Repeat(u * 5f, 1f);
                    return (s > 0.55f && s < 0.80f) ? white : red;
                }
            }
        }

        // =====================================================================
        // Swatch (round color chip)
        // =====================================================================
        public static Sprite Swatch(ColorPattern pattern)
        {
            return Cached("swatch|" + pattern, 96, 3, delegate(float x, float y)
            {
                float dx = x - 0.5f;
                float dy = y - 0.5f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                if (r > 0.46f) return Clear;

                Color c = Pattern(pattern, x, y);
                if (r > 0.42f) c = Color.Lerp(c, Rim, 0.55f);
                return c;
            });
        }

        // =====================================================================
        // Buoyancy icon: where the bait sits in the water column
        // =====================================================================
        public static Sprite BuoyancyIcon(Buoyancy buoyancy)
        {
            return Cached("buoy|" + buoyancy, 96, 3, delegate(float x, float y)
            {
                if (PixelPainter.RoundedSquare(x, y, 0.18f) > 0f) return Clear;

                float surface = 0.78f + Mathf.Sin(x * Mathf.PI * 4f) * 0.025f;
                Color c = y < surface
                    ? Color.Lerp(UiPalette.WaterDeep, UiPalette.WaterMid, Mathf.Clamp01(y / surface + 0.25f))
                    : UiPalette.WaterTop;

                if (Mathf.Abs(y - surface) < 0.018f) c = Color.white;

                float baitY = buoyancy == Buoyancy.Floating ? 0.80f : buoyancy == Buoyancy.Suspending ? 0.50f : 0.20f;
                if (Mathf.Abs(x - 0.5f) < 0.012f && y > baitY && y < 1f) c = UiPalette.InkFaint;   // fishing line

                float dx = x - 0.5f;
                float dy = y - baitY;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist < 0.115f) c = Rim;
                if (dist < 0.095f) c = UiPalette.Coral;
                return c;
            });
        }

        // =====================================================================
        // Swim icon: the path the bait takes
        // =====================================================================
        public static Sprite SwimIcon(SwimPattern swim)
        {
            float[] segs = SwimSegments(swim);
            return Cached("swim|" + swim, 96, 3, delegate(float x, float y)
            {
                float best = 10f;
                for (int i = 0; i + 3 < segs.Length; i += 4)
                {
                    float d = PixelPainter.Segment(x, y, segs[i], segs[i + 1], segs[i + 2], segs[i + 3]);
                    if (d < best) best = d;
                }
                return best < 0.045f ? UiPalette.Ink : Clear;
            });
        }

        // flat list of line segments: ax, ay, bx, by, ax, ay, bx, by ...
        static float[] SwimSegments(SwimPattern swim)
        {
            switch (swim)
            {
                case SwimPattern.Darting:
                    return new float[]
                    {
                        0.10f, 0.50f, 0.30f, 0.50f,
                        0.30f, 0.50f, 0.44f, 0.80f,
                        0.44f, 0.80f, 0.58f, 0.20f,
                        0.58f, 0.20f, 0.70f, 0.50f,
                        0.70f, 0.50f, 0.90f, 0.50f
                    };
                case SwimPattern.Hopping:
                    return Curve(delegate(float t) { return 0.30f + 0.42f * Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2f)); });
                case SwimPattern.Drifting:
                    return Curve(delegate(float t) { return 0.50f + 0.20f * Mathf.Sin(t * Mathf.PI * 2.2f); });
                default:   // Steady: a straight arrow
                    return new float[]
                    {
                        0.10f, 0.50f, 0.88f, 0.50f,
                        0.88f, 0.50f, 0.72f, 0.64f,
                        0.88f, 0.50f, 0.72f, 0.36f
                    };
            }
        }

        delegate float CurveFn(float t);

        static float[] Curve(CurveFn fn)
        {
            const int points = 28;
            float[] segs = new float[(points - 1) * 4];
            for (int i = 0; i < points - 1; i++)
            {
                float t0 = i / (float)(points - 1);
                float t1 = (i + 1) / (float)(points - 1);
                segs[i * 4] = 0.10f + 0.80f * t0;
                segs[i * 4 + 1] = fn(t0);
                segs[i * 4 + 2] = 0.10f + 0.80f * t1;
                segs[i * 4 + 3] = fn(t1);
            }
            return segs;
        }
    }
}
