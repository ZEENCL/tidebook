// FightView.cs
// The fight panel (GDD 3.4 / 5.1 "reel-in indicator"), shown while a fish is hooked.
//
//   Tension bar : the needle is your line tension; the green band is the safe zone; SLACK on the left,
//                 SNAP on the right. The bar's colour tells you how you are doing:
//                    green  inside the safe band     blue   too slack (reel!)
//                    yellow too tight (ease off)     coral  about to snap
//                 and the fish's PULL RHYTHM is shown on top of that:
//                    amber pulsing glow + "PULL INCOMING"   a pull is about to happen
//                    red flash + shake + "PULL!"            the pull is happening now
//   Progress bar: how close the fish is to being landed.
//   Coaching line: tells the player what to do right now.

using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public class FightView : HudView
    {
        static readonly Color WarnAmber = UiPalette.Hex("#FFB02E");
        static readonly Color PullRed = UiPalette.Hex("#FF4D4D");
        static readonly Color TrackColor = UiPalette.Hex("#10303B");

        const float PullFlashSeconds = 0.45f;
        const float WarnMaxSeconds = 1.6f;

        RectTransform panel;
        Image fishIcon;
        Text nameText;
        Text stateText;
        Image glow;
        RectTransform bandRect;
        RectTransform fillRect;
        Image fill;
        RectTransform needle;
        Image needleImage;
        RectTransform progressFillRect;
        RectTransform progressFish;
        Text coachText;

        float warnUntil;
        float pullUntil;
        float introAt;

        protected override void Build(RectTransform layer)
        {
            panel = UiBuilder.Rect(layer, "FightPanel");
            UiBuilder.Fixed(panel, 0.5f, 0f, 860f, 214f, 0f, 150f);   // sits above the equipped-bait card

            Image bg = UiBuilder.Box(panel, "Card", UiPalette.WithAlpha(UiPalette.CoverDark, 0.9f), 30);
            UiBuilder.Stretch(bg.rectTransform);
            UiBuilder.DropShadow(bg, 8f, 0.35f);

            fishIcon = UiBuilder.Icon(panel, "FishIcon", null, Color.white);
            UiBuilder.TopLeft(fishIcon.rectTransform, 26f, 12f, 84f, 52f);

            nameText = UiBuilder.Label(panel, "Name", "", 28, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold, true);
            UiBuilder.TopLeft(nameText.rectTransform, 122f, 14f, 400f, 48f);

            stateText = UiBuilder.Label(panel, "State", "", 24, Color.white, TextAnchor.MiddleRight, FontStyle.Bold, true);
            UiBuilder.TopRight(stateText.rectTransform, 30f, 14f, 300f, 48f);

            // glow (behind the track) shows the pull telegraph
            glow = UiBuilder.Box(panel, "Glow", UiPalette.WithAlpha(WarnAmber, 0f), 28);
            UiBuilder.TopLeft(glow.rectTransform, 22f, 68f, 816f, 60f);

            Image track = UiBuilder.Box(panel, "Track", TrackColor, 22);
            UiBuilder.TopLeft(track.rectTransform, 30f, 76f, 800f, 44f);

            // danger zones
            Image slack = UiBuilder.Box(track.transform, "SlackZone", UiPalette.WithAlpha(UiPalette.Sky, 0.18f), 10);
            UiBuilder.Anchor(slack.rectTransform, 0f, 0f, 0.10f, 1f, 4f, 4f, 0f, 4f);
            Image snap = UiBuilder.Box(track.transform, "SnapZone", UiPalette.WithAlpha(UiPalette.Coral, 0.30f), 10);
            UiBuilder.Anchor(snap.rectTransform, 0.88f, 0f, 1f, 1f, 0f, 4f, 4f, 4f);

            // safe band
            Image band = UiBuilder.Box(track.transform, "SafeBand", UiPalette.WithAlpha(UiPalette.Mint, 0.38f), 10);
            bandRect = band.rectTransform;
            UiBuilder.Anchor(bandRect, 0.4f, 0f, 0.6f, 1f, 0f, 4f, 0f, 4f);

            // fill
            fill = UiBuilder.Box(track.transform, "Fill", UiPalette.Mint, 18);
            fillRect = fill.rectTransform;
            UiBuilder.Anchor(fillRect, 0f, 0.28f, 0.5f, 0.72f, 6f, 0f, 0f, 0f);

            Text slackLabel = UiBuilder.Label(track.transform, "SlackLabel", "SLACK", 14, UiPalette.WithAlpha(UiPalette.Sky, 0.9f),
                                              TextAnchor.MiddleLeft, FontStyle.Bold);
            UiBuilder.Stretch(slackLabel.rectTransform, 14f, 0f, 0f, 0f);
            Text snapLabel = UiBuilder.Label(track.transform, "SnapLabel", "SNAP", 14, UiPalette.WithAlpha(UiPalette.Coral, 0.95f),
                                             TextAnchor.MiddleRight, FontStyle.Bold);
            UiBuilder.Stretch(snapLabel.rectTransform, 0f, 0f, 14f, 0f);

            // needle
            needleImage = UiBuilder.Box(track.transform, "Needle", Color.white, 4);
            needle = needleImage.rectTransform;
            needle.anchorMin = new Vector2(0.5f, 0.5f);
            needle.anchorMax = new Vector2(0.5f, 0.5f);
            needle.pivot = new Vector2(0.5f, 0.5f);
            needle.sizeDelta = new Vector2(9f, 62f);
            UiBuilder.DropShadow(needleImage, 3f, 0.4f);

            // progress
            Image progressTrack = UiBuilder.Box(panel, "ProgressTrack", TrackColor, 9);
            UiBuilder.TopLeft(progressTrack.rectTransform, 30f, 138f, 800f, 18f);
            Image progressFill = UiBuilder.Box(progressTrack.transform, "Fill", UiPalette.Mint, 7);
            progressFillRect = progressFill.rectTransform;
            UiBuilder.Anchor(progressFillRect, 0f, 0f, 0.25f, 1f, 2f, 2f, 0f, 2f);

            Image marker = UiBuilder.Icon(progressTrack.transform, "FishMarker", FishArt.Icon(UiPalette.Coral, 1), Color.white);
            progressFish = marker.rectTransform;
            progressFish.anchorMin = new Vector2(0.25f, 0.5f);
            progressFish.anchorMax = new Vector2(0.25f, 0.5f);
            progressFish.pivot = new Vector2(0.5f, 0.5f);
            progressFish.sizeDelta = new Vector2(52f, 32f);

            Text hooked = UiBuilder.Label(panel, "HookedLabel", "HOOKED", 14, UiPalette.WithAlpha(Color.white, 0.5f),
                                          TextAnchor.UpperLeft, FontStyle.Bold);
            UiBuilder.TopLeft(hooked.rectTransform, 32f, 158f, 200f, 20f);
            Text landed = UiBuilder.Label(panel, "LandedLabel", "LANDED", 14, UiPalette.WithAlpha(Color.white, 0.5f),
                                          TextAnchor.UpperRight, FontStyle.Bold);
            UiBuilder.TopRight(landed.rectTransform, 30f, 158f, 200f, 20f);

            coachText = UiBuilder.Label(panel, "Coach", "", 24, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UiBuilder.TopLeft(coachText.rectTransform, 30f, 174f, 800f, 34f);

            panel.gameObject.SetActive(false);
        }

        public override void OnCue(RodCue cue)
        {
            switch (cue)
            {
                case RodCue.HookSet:
                    warnUntil = 0f;
                    pullUntil = 0f;
                    introAt = ctx.Now;
                    break;
                case RodCue.PullWarning:
                    warnUntil = ctx.Now + WarnMaxSeconds;
                    break;
                case RodCue.Pulled:
                    warnUntil = 0f;
                    pullUntil = ctx.Now + PullFlashSeconds;
                    break;
            }
        }

        public override void Tick()
        {
            bool active = ctx.Phase == RodPhase.Reeling;
            panel.gameObject.SetActive(active);
            if (!active) return;

            IMeterView tension = ctx.Meter(MeterIds.Tension);
            IMeterView progress = ctx.Meter(MeterIds.Progress);
            if (tension == null || progress == null) return;

            float t = tension.Value;
            float bandMin = tension.BandMin;
            float bandMax = tension.BandMax;
            float now = ctx.Now;

            bool pulling = now < pullUntil;
            bool warning = !pulling && now < warnUntil;

            // ---- bar colour: how the player is doing ----
            Color state = t >= 0.86f ? UiPalette.Coral
                        : t > bandMax ? UiPalette.Sun
                        : t < bandMin ? UiPalette.Sky
                        : UiPalette.Mint;

            // ---- ...then the fish's pull rhythm on top ----
            Color barColor = state;
            float glowAlpha = 0f;
            Color glowColor = WarnAmber;
            if (warning)
            {
                float wave = 0.5f + 0.5f * Mathf.Sin(now * 14f);
                barColor = Color.Lerp(state, WarnAmber, 0.5f + 0.35f * wave);
                glowColor = WarnAmber;
                glowAlpha = 0.25f + 0.35f * wave;
            }
            else if (pulling)
            {
                float k = Mathf.Clamp01(1f - (pullUntil - now) / PullFlashSeconds);   // 0 at the pull, 1 when it fades
                barColor = Color.Lerp(PullRed, state, k * 0.6f);
                glowColor = PullRed;
                glowAlpha = 0.65f * (1f - k);
            }

            fill.color = barColor;
            glow.color = UiPalette.WithAlpha(glowColor, glowAlpha);
            needleImage.color = pulling ? PullRed : Color.white;

            // ---- geometry ----
            bandRect.anchorMin = new Vector2(bandMin, 0f);
            bandRect.anchorMax = new Vector2(bandMax, 1f);
            fillRect.anchorMax = new Vector2(Mathf.Max(0.01f, t), 0.72f);
            needle.anchorMin = new Vector2(t, 0.5f);
            needle.anchorMax = new Vector2(t, 0.5f);
            needle.anchoredPosition = Vector2.zero;

            progressFillRect.anchorMax = new Vector2(Mathf.Clamp01(progress.Value), 1f);
            progressFish.anchorMin = new Vector2(Mathf.Clamp01(progress.Value), 0.5f);
            progressFish.anchorMax = new Vector2(Mathf.Clamp01(progress.Value), 0.5f);
            progressFish.anchoredPosition = Vector2.zero;

            // ---- shake on a pull, pop-in when the fight begins ----
            Vector2 offset = new Vector2(0f, 150f);
            if (pulling)
            {
                float falloff = (pullUntil - now) / PullFlashSeconds;
                offset += new Vector2(Mathf.Sin(now * 70f) * 8f, Mathf.Cos(now * 55f) * 4f) * falloff;
            }
            panel.anchoredPosition = offset;

            float intro = Mathf.Clamp01((now - introAt) / 0.25f);
            panel.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, intro);

            // ---- who is on the line ----
            IFishInfo target = ctx.Status.Target;
            if (target != null) ShowFish(target);

            // ---- state text + coaching ----
            string coach;
            Color coachColor;
            string stateLabel;
            Color stateColor;
            if (pulling)
            {
                stateLabel = "PULL!"; stateColor = PullRed;
                coach = "Hold on!"; coachColor = PullRed;
            }
            else if (warning)
            {
                stateLabel = "PULL INCOMING"; stateColor = WarnAmber;
                coach = "Pull incoming - ease off!"; coachColor = WarnAmber;
            }
            else if (t > bandMax)
            {
                stateLabel = t >= 0.86f ? "ABOUT TO SNAP" : "TOO TIGHT";
                stateColor = state;
                coach = "Too tight - let go of the reel!"; coachColor = state;
            }
            else if (t < bandMin)
            {
                stateLabel = "SLACK"; stateColor = state;
                coach = "Slack line - reel it in!"; coachColor = state;
            }
            else
            {
                stateLabel = "STEADY"; stateColor = UiPalette.Mint;
                coach = "Nice and steady - keep it in the green"; coachColor = UiPalette.Mint;
            }

            stateText.text = stateLabel;
            stateText.color = stateColor;
            coachText.text = coach;
            coachText.color = coachColor;
        }

        void ShowFish(IFishInfo target)
        {
            FishIndexEntry entry = ctx.EntryFor(target.Species);
            bool known = entry != null && entry.Discovered;

            nameText.text = known ? target.DisplayName : "Unknown fish";
            if (entry != null)
            {
                fishIcon.sprite = known ? FishArt.Icon(entry.Source) : FishArt.Silhouette(entry.Source);
                fishIcon.enabled = true;
            }
            else
            {
                fishIcon.enabled = false;
            }
        }
    }
}
