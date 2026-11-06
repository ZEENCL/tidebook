// FishArt.cs
// Temporary fish art: a flat-shaded "low-poly" fish drawn from triangles (matching the GDD's low-poly,
// playful style). Undiscovered fish use the same shape as a solid silhouette.
// A FishCatalogEntry.icon (if you assign one) always wins over this generated art.

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook.Ui
{
    public static class FishArt
    {
        // each row: ax, ay, bx, by, cx, cy, shade
        static readonly float[][] Triangles = new float[][]
        {
            // body (fan around the centre)
            new float[] { 0.56f, 0.50f, 0.93f, 0.50f, 0.66f, 0.74f, 1.06f },
            new float[] { 0.56f, 0.50f, 0.66f, 0.74f, 0.38f, 0.72f, 1.12f },
            new float[] { 0.56f, 0.50f, 0.38f, 0.72f, 0.20f, 0.58f, 1.00f },
            new float[] { 0.56f, 0.50f, 0.20f, 0.58f, 0.20f, 0.42f, 0.92f },
            new float[] { 0.56f, 0.50f, 0.20f, 0.42f, 0.38f, 0.28f, 0.82f },
            new float[] { 0.56f, 0.50f, 0.38f, 0.28f, 0.66f, 0.26f, 0.74f },
            new float[] { 0.56f, 0.50f, 0.66f, 0.26f, 0.93f, 0.50f, 0.86f },
            // tail
            new float[] { 0.20f, 0.58f, 0.03f, 0.82f, 0.12f, 0.50f, 0.96f },
            new float[] { 0.20f, 0.42f, 0.12f, 0.50f, 0.03f, 0.18f, 0.80f },
            new float[] { 0.20f, 0.58f, 0.12f, 0.50f, 0.20f, 0.42f, 0.88f },
            // fins
            new float[] { 0.38f, 0.72f, 0.50f, 0.93f, 0.66f, 0.74f, 1.20f },
            new float[] { 0.38f, 0.28f, 0.50f, 0.07f, 0.66f, 0.26f, 0.70f }
        };

        static readonly float[] HeightScale = { 0.85f, 1.00f, 1.12f };

        static readonly Color[] Tints =
        {
            UiPalette.Coral, UiPalette.Sun, UiPalette.Mint, UiPalette.Sky,
            UiPalette.Lavender, UiPalette.Rose, UiPalette.Lime, UiPalette.Teal
        };

        // bounding box of each triangle (minX, maxX, minY, maxY) for quick rejection while painting
        static readonly float[][] TriBounds = new float[Triangles.Length][];

        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        static FishArt()
        {
            for (int i = 0; i < Triangles.Length; i++)
            {
                float[] t = Triangles[i];
                TriBounds[i] = new float[]
                {
                    Mathf.Min(t[0], Mathf.Min(t[2], t[4])), Mathf.Max(t[0], Mathf.Max(t[2], t[4])),
                    Mathf.Min(t[1], Mathf.Min(t[3], t[5])), Mathf.Max(t[1], Mathf.Max(t[3], t[5]))
                };
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            cache.Clear();
        }

        // ---------------------------------------------------------------- public API

        /// <summary>The colourful picture of a fish (use for discovered fish).</summary>
        public static Sprite Icon(FishCatalogEntry entry)
        {
            if (entry.icon != null) return entry.icon;
            return Generate(TintFor(entry), VariantFor(entry), false);
        }

        /// <summary>The dark silhouette (use for undiscovered fish).</summary>
        public static Sprite Silhouette(FishCatalogEntry entry)
        {
            return Generate(Color.white, VariantFor(entry), true);
        }

        /// <summary>A generic fish picture in one color (used for the Tidebook title).</summary>
        public static Sprite Icon(Color tint, int variant)
        {
            return Generate(tint, variant, false);
        }

        public static Color TintFor(FishCatalogEntry entry)
        {
            if (entry.useTint) return entry.tint;
            if (entry.species == null) return UiPalette.Sky;
            return Tints[Hash(entry.species.name) % Tints.Length];
        }

        public static int VariantFor(FishCatalogEntry entry)
        {
            if (entry.species == null) return 1;
            return (Hash(entry.species.name) / 7) % HeightScale.Length;
        }

        // ---------------------------------------------------------------- generation

        static int Hash(string text)
        {
            // simple stable hash (string.GetHashCode is not guaranteed stable across runs/platforms)
            unchecked
            {
                int h = 5381;
                if (text != null)
                {
                    for (int i = 0; i < text.Length; i++) h = h * 33 + text[i];
                }
                return h & 0x7fffffff;
            }
        }

        static Sprite Generate(Color tint, int variant, bool silhouette)
        {
            variant = Mathf.Clamp(variant, 0, HeightScale.Length - 1);
            string key = ColorUtility.ToHtmlStringRGB(tint) + "|" + variant + "|" + (silhouette ? "s" : "c");

            Sprite sprite;
            if (cache.TryGetValue(key, out sprite) && sprite != null) return sprite;

            const int width = 160;
            const int height = 100;
            float hs = HeightScale[variant];

            Texture2D tex = PixelPainter.Paint(width, height, 2, delegate(float x, float y)
            {
                float py = 0.5f + (y - 0.5f) / hs;   // squash/stretch the body vertically per variant

                for (int i = 0; i < Triangles.Length; i++)
                {
                    float[] t = Triangles[i];
                    float[] bb = TriBounds[i];
                    if (x < bb[0] || x > bb[1] || py < bb[2] || py > bb[3]) continue;
                    if (PixelPainter.InTriangle(x, py, t[0], t[1], t[2], t[3], t[4], t[5]))
                    {
                        if (silhouette) return new Color(UiPalette.Locked.r, UiPalette.Locked.g, UiPalette.Locked.b, 0.92f);

                        float shade = t[6];
                        Color c = new Color(
                            Mathf.Clamp01(tint.r * shade),
                            Mathf.Clamp01(tint.g * shade),
                            Mathf.Clamp01(tint.b * shade), 1f);

                        // eye
                        float ex = (x - 0.80f) * 1.6f;
                        float ey = py - 0.57f;
                        float eye = Mathf.Sqrt(ex * ex + ey * ey);
                        if (eye < 0.035f) c = new Color(0.10f, 0.20f, 0.26f, 1f);
                        else if (eye < 0.075f) c = Color.white;
                        return c;
                    }
                }
                return new Color(0f, 0f, 0f, 0f);
            });

            sprite = PixelPainter.ToSprite(tex, 100f);
            cache[key] = sprite;
            return sprite;
        }
    }
}
