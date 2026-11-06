// BaitPreviewTank.cs
// A small animated "water tank" that shows what the current bait does:
//   - the lure picture shows its shape and color
//   - the depth it hangs at shows its buoyancy (floating / suspending / sinking)
//   - the way it moves shows its swim pattern (steady / darting / hopping / drifting)
// The GDD says the player never sees the bait underwater in the game, so this preview is the
// builder's way of making the four invisible stats readable.

using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public class BaitPreviewTank : MonoBehaviour
    {
        const int BubbleCount = 7;

        RectTransform tank;
        RectTransform baitRect;
        RectTransform lineRect;
        Image baitImage;
        readonly RectTransform[] bubbles = new RectTransform[BubbleCount];
        readonly float[] bubbleSeed = new float[BubbleCount];

        float width;
        float height;
        float surfaceY;      // y (from the bottom of the tank) of the water surface
        float floorY;        // y of the top of the sand

        Buoyancy buoyancy = Buoyancy.Sinking;
        SwimPattern swim = SwimPattern.Steady;

        public void Build(RectTransform root, float tankWidth, float tankHeight)
        {
            tank = root;
            width = tankWidth;
            height = tankHeight;
            surfaceY = height - 56f;
            floorY = 44f;

            Image water = UiBuilder.GradientBox(root, "Water", 30, UiPalette.WaterTop, UiPalette.WaterDeep);
            UiBuilder.Stretch(water.rectTransform);

            // surface line
            Image surface = UiBuilder.Box(root, "Surface", UiPalette.WithAlpha(UiPalette.White, 0.85f), 3);
            UiBuilder.Fixed(surface.rectTransform, 0.5f, 0f, width - 40f, 5f, 0f, surfaceY);

            // sand
            Image sand = UiBuilder.Box(root, "Sand", UiPalette.Sand, 20);
            UiBuilder.Fixed(sand.rectTransform, 0.5f, 0f, width - 28f, 56f, 0f, 14f);

            // bubbles
            for (int i = 0; i < BubbleCount; i++)
            {
                float size = 12f + (i % 3) * 7f;
                Image b = UiBuilder.Icon(root, "Bubble", ProceduralSprites.Circle(), UiPalette.WithAlpha(UiPalette.White, 0.35f));
                b.preserveAspect = false;
                bubbles[i] = b.rectTransform;
                SetBottomLeft(bubbles[i], size, size);
                bubbleSeed[i] = (i * 0.173f + 0.11f) % 1f;
            }

            // fishing line
            Image line = UiBuilder.Box(root, "Line", UiPalette.WithAlpha(UiPalette.White, 0.75f));
            lineRect = line.rectTransform;
            lineRect.anchorMin = new Vector2(0f, 1f);
            lineRect.anchorMax = new Vector2(0f, 1f);
            lineRect.pivot = new Vector2(0.5f, 1f);

            // the lure
            Image bait = UiBuilder.Icon(root, "Lure", BaitArt.Lure(BodyShape.Slim, ColorPattern.RedWhite), Color.white);
            baitImage = bait;
            baitRect = bait.rectTransform;
            SetBottomLeft(baitRect, 140f, 140f);

            Text caption = UiBuilder.Label(root, "Caption", "LIVE PREVIEW", 16, UiPalette.WithAlpha(UiPalette.Ink, 0.55f),
                                           TextAnchor.UpperLeft, FontStyle.Bold);
            UiBuilder.TopLeft(caption.rectTransform, 26f, 18f, 300f, 24f);
        }

        static void SetBottomLeft(RectTransform rt, float w, float h)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
        }

        public void SetBait(BaitSelection selection)
        {
            buoyancy = selection.Buoyancy;
            swim = selection.Swim;
            baitImage.sprite = BaitArt.Lure(selection.Shape, selection.Pattern);
        }

        void Update()
        {
            if (baitRect == null) return;
            float t = Time.unscaledTime;

            float depth = buoyancy == Buoyancy.Floating ? 0.97f : buoyancy == Buoyancy.Suspending ? 0.55f : 0.16f;
            float dir;
            float yOffset;
            float x = Motion(swim, t, out dir, out yOffset);

            float minX = 90f;
            float maxX = width - 90f;
            float minY = floorY + 56f;
            float maxY = surfaceY - 6f;

            float px = Mathf.Lerp(minX, maxX, x * 0.5f + 0.5f);
            float py = Mathf.Lerp(minY, maxY, Mathf.Clamp01(depth + yOffset));
            if (buoyancy == Buoyancy.Floating) py += Mathf.Sin(t * 2.4f) * 3f;   // bobbing on the surface

            baitRect.anchoredPosition = new Vector2(px, py);
            baitRect.localScale = new Vector3(dir >= 0f ? 1f : -1f, 1f, 1f);
            baitRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 2.5f) * 3f);

            // fishing line from the top of the tank down to the lure
            lineRect.anchoredPosition = new Vector2(px, 0f);
            lineRect.sizeDelta = new Vector2(3f, Mathf.Max(0f, height - py));

            for (int i = 0; i < BubbleCount; i++)
            {
                float travel = Mathf.Repeat(bubbleSeed[i] + t * (0.05f + (i % 3) * 0.02f), 1f);
                float bx = 40f + ((i * 97f) % (width - 80f));
                bubbles[i].anchoredPosition = new Vector2(bx + Mathf.Sin(t + i) * 6f, Mathf.Lerp(floorY, surfaceY - 10f, travel));
            }
        }

        /// <summary>Returns horizontal position in -1..1; outputs facing direction and a vertical offset (in depth units).</summary>
        static float Motion(SwimPattern pattern, float t, out float dir, out float yOffset)
        {
            float x;
            yOffset = 0f;

            switch (pattern)
            {
                case SwimPattern.Darting:
                {
                    float phase = t * 0.75f;
                    float i = Mathf.Floor(phase);
                    float f = phase - i;
                    float from = Waypoint(i);
                    float to = Waypoint(i + 1f);
                    float k = f < 0.3f ? 1f - Mathf.Pow(1f - f / 0.3f, 3f) : 1f;   // fast burst, then a pause
                    x = Mathf.Lerp(from, to, k);
                    dir = to >= from ? 1f : -1f;
                    if (f < 0.3f) yOffset = Mathf.Sin(f / 0.3f * Mathf.PI) * 0.03f;
                    return x;
                }
                case SwimPattern.Hopping:
                    x = Mathf.Sin(t * 0.35f) * 0.8f;
                    dir = Mathf.Cos(t * 0.35f) >= 0f ? 1f : -1f;
                    yOffset = Mathf.Abs(Mathf.Sin(t * 3f)) * 0.16f;
                    return x;
                case SwimPattern.Drifting:
                    x = Mathf.Sin(t * 0.22f) * 0.9f;
                    dir = Mathf.Cos(t * 0.22f) >= 0f ? 1f : -1f;
                    yOffset = Mathf.Sin(t * 0.55f) * 0.05f;
                    return x;
                default:   // Steady
                    x = Mathf.Sin(t * 0.7f) * 0.85f;
                    dir = Mathf.Cos(t * 0.7f) >= 0f ? 1f : -1f;
                    yOffset = Mathf.Sin(t * 2.2f) * 0.015f;
                    return x;
            }
        }

        static float Waypoint(float index)
        {
            return Mathf.Sin(index * 12.9898f + 3.1f) * 0.85f;   // deterministic pseudo-random spots
        }
    }
}
