// HintView.cs
// Plain-language guidance, in the GDD's low-pressure spirit:
//   - a prompt pill that says what the controls do right now
//   - a message line for things the player should know ("no water in reach", "quiet here...")

using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public class HintView : HudView
    {
        const float QuietSeconds = 22f;

        GameObject pill;
        Text pillText;
        Text messageText;

        string flashText = "";
        float flashUntil;

        protected override void Build(RectTransform layer)
        {
            Image bg = UiBuilder.Box(layer, "PromptPill", UiPalette.WithAlpha(UiPalette.CoverDark, 0.82f), 24);
            UiBuilder.Fixed(bg.rectTransform, 0.5f, 0f, 700f, 54f, 0f, 150f);
            pill = bg.gameObject;

            pillText = UiBuilder.Label(bg.transform, "Text", "", 24, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UiBuilder.Stretch(pillText.rectTransform, 16f, 0f, 16f, 0f);

            messageText = UiBuilder.Label(layer, "Message", "", 28, UiPalette.Coral, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UiBuilder.Fixed(messageText.rectTransform, 0.5f, 0.5f, 1000f, 44f, 0f, -170f);
        }

        public override void OnCue(RodCue cue)
        {
            if (cue == RodCue.CastRejected)
            {
                flashText = "That cast needs to land on fishing water.";
                flashUntil = ctx.Now + 2.4f;
            }
        }

        public override void Tick()
        {
            // ---- prompt pill ----
            string prompt = null;
            switch (ctx.Phase)
            {
                case RodPhase.Aiming: prompt = "Hold LMB  to charge a cast"; break;
                case RodPhase.Charging: prompt = "Release to cast     |     RMB  cancel"; break;
                case RodPhase.Settling: prompt = "Your bait is sinking..."; break;
                case RodPhase.Waiting: prompt = "Waiting for a bite...     |     RMB  reel in"; break;
                case RodPhase.BiteWindow: prompt = "Click now to hook it!"; break;
            }
            pill.SetActive(prompt != null);
            if (prompt != null) pillText.text = prompt;

            // ---- message line (first match wins) ----
            string message = "";
            Color color = UiPalette.Coral;
            float alpha = 1f;

            if (ctx.Now < flashUntil)
            {
                message = flashText;
                alpha = Mathf.Clamp01((flashUntil - ctx.Now) / 0.5f);
            }
            else if (ctx.Phase == RodPhase.Aiming && ctx.Status.Preview.Visible && !ctx.Status.Preview.InWater)
            {
                message = "No fishing water in reach. Walk closer to the shore.";
                color = UiPalette.WithAlpha(Color.white, 0.9f);
            }
            else if (ctx.Phase == RodPhase.Waiting && ctx.PhaseTime > QuietSeconds && ctx.Status.Watchers.Count == 0)
            {
                message = "Quiet here... try a different bait (B) or another spot.";
                color = UiPalette.WithAlpha(Color.white, 0.9f);
            }

            messageText.text = message;
            messageText.color = UiPalette.WithAlpha(color, alpha * color.a);
        }
    }
}
