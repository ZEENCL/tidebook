// BobberView.cs
// The waiting-and-biting feedback from the GDD (3.3 and 3.4), anchored to the bobber:
//   Settling   : "Sinking..."
//   Waiting    : nothing until a fish reacts, then a bubble above the bobber:
//                  "?"   a fish is curious
//                  "..." a fish is interested and nibbling
//   BiteWindow : a big "!" bubble, a CLICK! prompt and a ring that counts down the bite window
//   Reeling    : just the bobber marker
// The alert stays on screen even if the bobber is off to the side (it pins to the screen edge).

using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public class BobberView : HudView
    {
        RectTransform root;
        Image marker;
        Image markerDot;
        Image biteRing;
        RectTransform bubble;
        Image bubbleImage;
        Text glyph;
        Text label;

        FishCueState lastState = FishCueState.None;
        float popAt;

        protected override void Build(RectTransform layer)
        {
            root = UiBuilder.Rect(layer, "Bobber");
            UiBuilder.Fixed(root, 0.5f, 0.5f, 10f, 10f);

            biteRing = UiBuilder.Icon(root, "BiteRing", HudSprites.Ring(10), UiPalette.Coral);
            UiBuilder.Fixed(biteRing.rectTransform, 0.5f, 0.5f, 150f, 150f);
            biteRing.type = Image.Type.Filled;
            biteRing.fillMethod = Image.FillMethod.Radial360;
            biteRing.fillOrigin = (int)Image.Origin360.Top;
            biteRing.fillClockwise = false;

            marker = UiBuilder.Icon(root, "Marker", HudSprites.Ring(8), Color.white);
            UiBuilder.Fixed(marker.rectTransform, 0.5f, 0.5f, 38f, 38f);
            markerDot = UiBuilder.Icon(root, "Dot", ProceduralSprites.Circle(), Color.white);
            UiBuilder.Fixed(markerDot.rectTransform, 0.5f, 0.5f, 9f, 9f);

            bubble = UiBuilder.Rect(root, "Bubble");
            UiBuilder.Fixed(bubble, 0.5f, 0.5f, 92f, 104f, 0f, 84f);
            bubbleImage = bubble.gameObject.AddComponent<Image>();
            bubbleImage.sprite = HudSprites.Bubble();
            bubbleImage.raycastTarget = false;
            UiBuilder.DropShadow(bubbleImage, 4f, 0.25f);

            glyph = UiBuilder.Label(bubble, "Glyph", "", 52, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiBuilder.Stretch(glyph.rectTransform, 0f, 24f, 0f, 0f);

            label = UiBuilder.Label(root, "Label", "", 26, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiBuilder.Fixed(label.rectTransform, 0.5f, 0.5f, 320f, 40f, 0f, -52f);

            root.gameObject.SetActive(false);
        }

        public override void Tick()
        {
            RodPhase phase = ctx.Phase;
            bool active = phase == RodPhase.Settling || phase == RodPhase.Waiting
                          || phase == RodPhase.BiteWindow || phase == RodPhase.Reeling;
            root.gameObject.SetActive(active);
            if (!active) return;

            bool offscreen;
            root.anchoredPosition = ctx.ProjectClamped(ctx.Status.BaitPosition + Vector3.up * 0.15f, 90f, out offscreen);

            FishCueState top = TopWatcherState();
            if (phase == RodPhase.BiteWindow) top = FishCueState.Biting;
            if (phase == RodPhase.Reeling) top = FishCueState.Hooked;

            if (top != lastState)
            {
                lastState = top;
                popAt = ctx.Now;
            }

            // defaults
            bool showBubble = false;
            bool showRing = false;
            string labelText = "";
            Color accent = Color.white;
            string glyphText = "";

            switch (phase)
            {
                case RodPhase.Settling:
                    labelText = "Sinking" + Dots();
                    break;

                case RodPhase.Waiting:
                    if (top == FishCueState.Curious)
                    {
                        showBubble = true; accent = UiPalette.Sky; glyphText = "?";
                        labelText = "Something's curious";
                    }
                    else if (top == FishCueState.Interested)
                    {
                        showBubble = true; accent = UiPalette.Sun; glyphText = Dots();
                        labelText = "Nibbling";
                    }
                    break;

                case RodPhase.BiteWindow:
                {
                    showBubble = true; showRing = true;
                    accent = UiPalette.Coral; glyphText = "!";
                    labelText = "CLICK!";
                    IMeterView window = ctx.Meter(MeterIds.BiteWindow);
                    biteRing.fillAmount = window != null ? window.Value : 1f;
                    break;
                }
            }

            // ---- apply ----
            biteRing.gameObject.SetActive(showRing);
            bubble.gameObject.SetActive(showBubble);
            if (showBubble)
            {
                float age = Mathf.Clamp01((ctx.Now - popAt) / 0.28f);
                float overshoot = 1f + 0.35f * Mathf.Sin(age * Mathf.PI) * (1f - age);   // little pop when the state changes
                float pulse = phase == RodPhase.BiteWindow ? 1f + 0.1f * Mathf.Sin(ctx.Now * 22f) : 1f;
                float shake = phase == RodPhase.BiteWindow ? Mathf.Sin(ctx.Now * 45f) * 5f : 0f;

                bubble.localScale = Vector3.one * (Mathf.Lerp(0.6f, 1f, age) * overshoot * pulse);
                bubble.anchoredPosition = new Vector2(shake, 84f);
                bubbleImage.color = accent;
                glyph.text = glyphText;
            }

            label.text = labelText;
            label.color = phase == RodPhase.BiteWindow ? UiPalette.Coral : UiPalette.WithAlpha(Color.white, 0.9f);
            label.fontSize = phase == RodPhase.BiteWindow ? 34 : 24;

            Color markerColor = phase == RodPhase.BiteWindow ? UiPalette.Coral : Color.white;
            marker.color = markerColor;
            markerDot.color = markerColor;
        }

        FishCueState TopWatcherState()
        {
            FishCueState top = FishCueState.None;
            System.Collections.Generic.IReadOnlyList<IFishCue> watchers = ctx.Status.Watchers;
            for (int i = 0; i < watchers.Count; i++)
            {
                if (watchers[i].CueState > top) top = watchers[i].CueState;
            }
            return top;
        }

        string Dots()
        {
            int n = 1 + (int)(ctx.Now * 3f) % 3;
            return n == 1 ? "." : n == 2 ? ".." : "...";
        }
    }
}
