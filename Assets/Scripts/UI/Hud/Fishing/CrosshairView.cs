// CrosshairView.cs
// The crosshair (GDD 5.1). It changes with the rod: steady while aiming, grows and warms up while
// charging, dims while you wait, pulses on a bite, and shakes when a cast is rejected.

using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public class CrosshairView : HudView
    {
        RectTransform root;
        Image ring;
        Image dot;
        float shakeUntil;

        protected override void Build(RectTransform layer)
        {
            root = UiBuilder.Rect(layer, "Crosshair");
            UiBuilder.Fixed(root, 0.5f, 0.5f, 40f, 40f);

            ring = UiBuilder.Icon(root, "Ring", HudSprites.Ring(5), Color.white);
            UiBuilder.Stretch(ring.rectTransform);

            dot = UiBuilder.Icon(root, "Dot", ProceduralSprites.Circle(), Color.white);
            UiBuilder.Fixed(dot.rectTransform, 0.5f, 0.5f, 7f, 7f);
        }

        public override void OnCue(RodCue cue)
        {
            if (cue == RodCue.CastRejected) shakeUntil = ctx.Now + 0.35f;
        }

        public override void Tick()
        {
            float alpha = 0.85f;
            float scale = 1f;
            Color color = Color.white;

            switch (ctx.Phase)
            {
                case RodPhase.Charging:
                {
                    IMeterView power = ctx.Meter(MeterIds.Power);
                    scale = 1f + 0.5f * (power != null ? power.Value : 0f);
                    color = UiPalette.Sun;
                    alpha = 1f;
                    break;
                }
                case RodPhase.Settling:
                case RodPhase.Waiting:
                    alpha = 0.35f;
                    break;
                case RodPhase.BiteWindow:
                    color = UiPalette.Coral;
                    scale = 1f + 0.15f * Mathf.Sin(ctx.Now * 18f);
                    alpha = 1f;
                    break;
                case RodPhase.Reeling:
                    alpha = 0.2f;
                    break;
                case RodPhase.Resolved:
                    alpha = 0.5f;
                    break;
            }

            Vector2 shake = Vector2.zero;
            if (ctx.Now < shakeUntil) shake = new Vector2(Mathf.Sin(ctx.Now * 60f) * 9f * ((shakeUntil - ctx.Now) / 0.35f), 0f);

            root.anchoredPosition = shake;
            root.localScale = Vector3.one * scale;

            Color tinted = new Color(color.r, color.g, color.b, alpha);
            ring.color = tinted;
            dot.color = tinted;
        }
    }
}
