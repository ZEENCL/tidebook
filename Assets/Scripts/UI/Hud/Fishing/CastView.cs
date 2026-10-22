// CastView.cs
// Everything about the cast (GDD 3.1 and 5.1):
//   Aiming   : a faint marker on the water where a full-power cast would land (so you can see your reach)
//   Charging : a dotted trajectory arc from the rod to the landing point, a landing reticle with the
//              distance in metres, and the cast power meter
// The dots turn coral when the cast would not land on fishing water.

using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public class CastView : HudView
    {
        const int DotCount = 26;

        Image[] dots;
        RectTransform[] dotRects;

        RectTransform reticle;
        Image reticleRing;
        Image reticleDot;
        Text reticleLabel;

        RectTransform meterRoot;
        RectTransform fillRect;
        Image fill;
        Text distanceLabel;

        protected override void Build(RectTransform layer)
        {
            dots = new Image[DotCount];
            dotRects = new RectTransform[DotCount];
            for (int i = 0; i < DotCount; i++)
            {
                Image d = UiBuilder.Icon(layer, "TrajectoryDot", ProceduralSprites.Circle(), Color.white);
                d.preserveAspect = false;
                UiBuilder.Fixed(d.rectTransform, 0.5f, 0.5f, 10f, 10f);
                d.gameObject.SetActive(false);
                dots[i] = d;
                dotRects[i] = d.rectTransform;
            }

            reticle = UiBuilder.Rect(layer, "LandingReticle");
            UiBuilder.Fixed(reticle, 0.5f, 0.5f, 76f, 76f);
            reticleRing = UiBuilder.Icon(reticle, "Ring", HudSprites.Ring(7), Color.white);
            UiBuilder.Stretch(reticleRing.rectTransform);
            reticleDot = UiBuilder.Icon(reticle, "Dot", ProceduralSprites.Circle(), Color.white);
            UiBuilder.Fixed(reticleDot.rectTransform, 0.5f, 0.5f, 10f, 10f);
            reticleLabel = UiBuilder.Label(reticle, "Distance", "", 24, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiBuilder.Fixed(reticleLabel.rectTransform, 0.5f, 0.5f, 140f, 30f, 0f, 58f);
            reticle.gameObject.SetActive(false);

            // power meter under the crosshair
            meterRoot = UiBuilder.Rect(layer, "PowerMeter");
            UiBuilder.Fixed(meterRoot, 0.5f, 0.5f, 300f, 54f, 0f, -104f);

            Text caption = UiBuilder.Label(meterRoot, "Caption", "CAST POWER", 16, UiPalette.WithAlpha(Color.white, 0.9f),
                                           TextAnchor.LowerLeft, FontStyle.Bold);
            UiBuilder.Fixed(caption.rectTransform, 0f, 1f, 160f, 24f, 0f, 0f);

            distanceLabel = UiBuilder.Label(meterRoot, "Distance", "", 18, Color.white, TextAnchor.LowerRight, FontStyle.Bold);
            UiBuilder.Fixed(distanceLabel.rectTransform, 1f, 1f, 120f, 24f, 0f, 0f);

            Image track = UiBuilder.Box(meterRoot, "Track", UiPalette.WithAlpha(UiPalette.CoverDark, 0.82f), 10);
            UiBuilder.Fixed(track.rectTransform, 0.5f, 0f, 300f, 20f, 0f, 0f);
            UiBuilder.DropShadow(track, 4f, 0.25f);

            fill = UiBuilder.Box(track.transform, "Fill", UiPalette.Mint, 8);
            fillRect = fill.rectTransform;
            UiBuilder.Anchor(fillRect, 0f, 0f, 1f, 1f, 3f, 3f, 3f, 3f);

            meterRoot.gameObject.SetActive(false);
        }

        public override void Tick()
        {
            bool charging = ctx.Phase == RodPhase.Charging;
            bool aiming = ctx.Phase == RodPhase.Aiming;
            CastPreview preview = ctx.Status.Preview;

            // ---- power meter ----
            meterRoot.gameObject.SetActive(charging);
            if (charging)
            {
                IMeterView power = ctx.Meter(MeterIds.Power);
                float p = power != null ? power.Value : 0f;
                fill.gameObject.SetActive(p > 0.02f);
                fillRect.anchorMax = new Vector2(Mathf.Clamp01(p), 1f);
                fill.color = p < 0.6f ? Color.Lerp(UiPalette.Mint, UiPalette.Sun, p / 0.6f)
                                      : Color.Lerp(UiPalette.Sun, UiPalette.Coral, (p - 0.6f) / 0.4f);
                distanceLabel.text = preview.Visible ? Mathf.RoundToInt(preview.Distance) + " m" : "";
            }

            bool show = ctx.Cam != null && preview.Visible && (charging || (aiming && preview.InWater));
            if (!show)
            {
                HideTrajectory();
                reticle.gameObject.SetActive(false);
                return;
            }

            Color good = charging ? Color.white : UiPalette.WithAlpha(UiPalette.Mint, 0.6f);
            Color tint = preview.InWater ? good : UiPalette.Coral;

            // ---- landing reticle ----
            Vector2 landing;
            bool landingVisible = ctx.TryProject(preview.Point, out landing);
            reticle.gameObject.SetActive(landingVisible);
            if (landingVisible)
            {
                float pulse = 1f + 0.06f * Mathf.Sin(ctx.Now * 6f);
                reticle.anchoredPosition = landing;
                reticle.localScale = new Vector3(pulse, 0.55f * pulse, 1f);   // flattened so it looks like it lies on the water
                reticleRing.color = tint;
                reticleDot.color = tint;
                reticleLabel.text = charging ? Mathf.RoundToInt(preview.Distance) + " m" : "";
                reticleLabel.color = tint;
                reticleLabel.rectTransform.localScale = new Vector3(1f, 1f / 0.55f, 1f);   // keep the text un-squashed
            }

            // ---- trajectory dots ----
            if (!charging || !ctx.ShowTrajectory)
            {
                HideTrajectory();
                return;
            }

            Vector3 origin = ctx.CastOriginPosition();
            float arcHeight = Mathf.Clamp(preview.Distance * 0.18f, 0.8f, 5f);
            for (int i = 0; i < DotCount; i++)
            {
                float t = (i + 1f) / (DotCount + 1f);
                Vector3 point = Vector3.Lerp(origin, preview.Point, t);
                point.y += arcHeight * 4f * t * (1f - t);

                Vector2 local;
                bool ok = ctx.TryProject(point, out local);
                dots[i].gameObject.SetActive(ok);
                if (!ok) continue;

                float size = Mathf.Lerp(15f, 7f, t) * (1f + 0.25f * Mathf.Sin(ctx.Now * 8f - i * 0.6f));
                dotRects[i].anchoredPosition = local;
                dotRects[i].sizeDelta = new Vector2(size, size);
                dots[i].color = UiPalette.WithAlpha(tint, Mathf.Lerp(0.95f, 0.55f, t));
            }
        }

        void HideTrajectory()
        {
            for (int i = 0; i < DotCount; i++) dots[i].gameObject.SetActive(false);
        }
    }
}
