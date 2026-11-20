// ProceduralSprites.cs
// Cached generic shapes used everywhere: 9-sliced rounded rectangles, circles, soft glows, gradients.
// Rounded sprites use 100 pixels-per-unit (matching the canvas), so a radius of 24 means a 24px corner on screen.

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook.Ui
{
    public static class ProceduralSprites
    {
        // Flyweight: one shared sprite per radius / gradient / shape. Panels and buttons only differ in
        // their extrinsic state (size, tint), which each Image keeps for itself.
        static readonly SpriteCache<int> rounded = new SpriteCache<int>("Rounded sprites");
        static readonly SpriteCache<string> gradients = new SpriteCache<string>("Gradient sprites");
        static readonly SpriteCache<string> shapes = new SpriteCache<string>("Shape sprites");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            rounded.Clear();
            gradients.Clear();
            shapes.Clear();
        }

        /// <summary>A white rounded rectangle, 9-sliced so it can be any size. Tint it with Image.color.</summary>
        public static Sprite Rounded(int radius)
        {
            Sprite sprite;
            if (rounded.TryGet(radius, out sprite)) return sprite;

            int size = radius * 2 + 4;
            Texture2D tex = PixelPainter.Paint(size, size, 1, delegate(float x, float y)
            {
                float d = RoundedDistance(x * size, y * size, size, size, radius);
                return new Color(1f, 1f, 1f, Mathf.Clamp01(0.5f - d));
            });

            sprite = PixelPainter.ToSliced(tex, radius + 1);
            rounded.Add(radius, sprite);
            return sprite;
        }

        /// <summary>A rounded rectangle with a vertical color gradient (9-sliced; the gradient stretches).</summary>
        public static Sprite RoundedGradient(int radius, Color top, Color bottom)
        {
            string key = radius + "|" + ColorUtility.ToHtmlStringRGBA(top) + "|" + ColorUtility.ToHtmlStringRGBA(bottom);
            Sprite sprite;
            if (gradients.TryGet(key, out sprite)) return sprite;

            int width = radius * 2 + 4;
            int height = radius * 2 + 64;
            Texture2D tex = PixelPainter.Paint(width, height, 1, delegate(float x, float y)
            {
                float d = RoundedDistance(x * width, y * height, width, height, radius);
                Color c = Color.Lerp(bottom, top, y);
                c.a *= Mathf.Clamp01(0.5f - d);
                return c;
            });

            sprite = PixelPainter.ToSliced(tex, radius + 1);
            gradients.Add(key, sprite);
            return sprite;
        }

        static float RoundedDistance(float px, float py, float width, float height, float radius)
        {
            float cx = px - width * 0.5f;
            float cy = py - height * 0.5f;
            float qx = Mathf.Abs(cx) - (width * 0.5f - radius);
            float qy = Mathf.Abs(cy) - (height * 0.5f - radius);
            float ox = Mathf.Max(qx, 0f);
            float oy = Mathf.Max(qy, 0f);
            return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
        }

        public static Sprite Circle()
        {
            Sprite sprite;
            if (shapes.TryGet("circle", out sprite)) return sprite;

            const int size = 128;
            Texture2D tex = PixelPainter.Paint(size, size, 1, delegate(float x, float y)
            {
                float dx = x * size - size * 0.5f;
                float dy = y * size - size * 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                return new Color(1f, 1f, 1f, Mathf.Clamp01(size * 0.5f - 0.5f - dist + 0.5f));
            });

            sprite = PixelPainter.ToSprite(tex, 100f);
            shapes.Add("circle", sprite);
            return sprite;
        }

        /// <summary>A white radial falloff, handy for glows and soft ground shadows.</summary>
        public static Sprite SoftCircle()
        {
            Sprite sprite;
            if (shapes.TryGet("soft", out sprite)) return sprite;

            const int size = 128;
            Texture2D tex = PixelPainter.Paint(size, size, 1, delegate(float x, float y)
            {
                float dx = (x - 0.5f) * 2f;
                float dy = (y - 0.5f) * 2f;
                float t = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                return new Color(1f, 1f, 1f, t * t);
            });

            sprite = PixelPainter.ToSprite(tex, 100f);
            shapes.Add("soft", sprite);
            return sprite;
        }
    }
}
