// CatchBannerView.cs
// The result card after every cast that involved a fish (it listens to the rod's CatchEvents):
//   Caught        : fish picture, name, rarity, coins earned, "NEW ENTRY" if it is a first
//   Snapped       : "Line snapped" with a tip
//   Escaped       : "Slipped off" with a tip
//   Missed        : "Too slow" with a tip
// Reeling in an empty line shows nothing. The tips teach the mechanic without scolding (low-pressure tone).

using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public class CatchBannerView : HudView, ICatchListener
    {
        const float Duration = 3.6f;

        CanvasGroup group;
        RectTransform card;
        Image accent;
        Image icon;
        Text title;
        Text nameText;
        Text detail;
        GameObject newBadge;

        float shownAt = -100f;

        protected override void Build(RectTransform layer)
        {
            RectTransform root = UiBuilder.Rect(layer, "CatchBanner");
            UiBuilder.Stretch(root);
            group = root.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            Image bg = UiBuilder.Box(root, "Card", UiPalette.WithAlpha(UiPalette.CoverDark, 0.95f), 28);
            card = bg.rectTransform;
            UiBuilder.Fixed(card, 0.5f, 1f, 680f, 132f, 0f, -190f);
            UiBuilder.DropShadow(bg, 8f, 0.35f);

            accent = UiBuilder.Box(card, "Accent", UiPalette.Mint, 6);
            UiBuilder.Fixed(accent.rectTransform, 0f, 0.5f, 12f, 96f, 14f, 0f);

            Image iconBg = UiBuilder.Box(card, "IconBg", UiPalette.Paper, 40);
            UiBuilder.Fixed(iconBg.rectTransform, 0f, 0.5f, 98f, 98f, 40f, 0f);
            icon = UiBuilder.Icon(iconBg.transform, "Icon", null, Color.white);
            UiBuilder.Stretch(icon.rectTransform, 8f, 8f, 8f, 8f);

            title = UiBuilder.Label(card, "Title", "", 20, UiPalette.Mint, TextAnchor.LowerLeft, FontStyle.Bold);
            UiBuilder.Anchor(title.rectTransform, 0f, 0.62f, 1f, 1f, 160f, 0f, 20f, 12f);

            nameText = UiBuilder.Label(card, "Name", "", 36, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold, true);
            UiBuilder.Anchor(nameText.rectTransform, 0f, 0.32f, 1f, 0.66f, 160f, 0f, 20f, 0f);

            detail = UiBuilder.Label(card, "Detail", "", 20, UiPalette.WaterMid, TextAnchor.UpperLeft, FontStyle.Normal, true);
            UiBuilder.Anchor(detail.rectTransform, 0f, 0f, 1f, 0.34f, 160f, 14f, 20f, 0f);

            Image badge = UiBuilder.Box(card, "NewBadge", UiPalette.Sun, 16);
            UiBuilder.TopRight(badge.rectTransform, 20f, 16f, 132f, 32f);
            Text badgeText = UiBuilder.Label(badge.transform, "Text", "NEW ENTRY", 16, UiPalette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiBuilder.Stretch(badgeText.rectTransform);
            newBadge = badge.gameObject;
        }

        public void OnCatchResolved(CatchReport report)
        {
            if (card == null || report.Result == CatchResult.Retrieved) return;

            FishSpecies species = report.Info != null ? report.Info.Species : null;
            FishIndexEntry entry = ctx != null ? ctx.EntryFor(species) : null;

            bool isNew = false;
            switch (report.Result)
            {
                case CatchResult.Caught:
                {
                    accent.color = species != null ? UiPalette.ForRarity(species.rarity) : UiPalette.Mint;
                    title.text = "CAUGHT!";
                    title.color = accent.color;
                    nameText.text = species != null ? species.displayName : "A fish";
                    detail.text = species != null
                        ? species.rarity + "     +" + species.currencyValue + " coins"
                        : "";
                    isNew = entry != null && entry.TimesCaught == 1;
                    break;
                }
                case CatchResult.Snapped:
                    accent.color = UiPalette.Coral;
                    title.text = "LINE SNAPPED";
                    title.color = UiPalette.Coral;
                    nameText.text = "It got away!";
                    detail.text = "Tip: let go of the reel before the bar reaches SNAP.";
                    break;
                case CatchResult.Escaped:
                    accent.color = UiPalette.Sky;
                    title.text = "SLIPPED OFF";
                    title.color = UiPalette.Sky;
                    nameText.text = "It got away!";
                    detail.text = "Tip: keep the needle inside the green band to make progress.";
                    break;
                default:   // Missed
                    accent.color = UiPalette.Sun;
                    title.text = "TOO SLOW";
                    title.color = UiPalette.Sun;
                    nameText.text = "It spat the bait out.";
                    detail.text = "Tip: click as soon as you see the ! above your bobber.";
                    break;
            }

            if (entry != null)
            {
                bool known = entry.Discovered;
                icon.sprite = known ? FishArt.Icon(entry.Source) : FishArt.Silhouette(entry.Source);
            }
            else
            {
                icon.sprite = FishArt.Icon(UiPalette.Coral, 1);
            }

            newBadge.SetActive(isNew);
            shownAt = Time.unscaledTime;
            UiSounds.Play(report.Result == CatchResult.Caught ? UiSound.Catch
                          : report.Result == CatchResult.Snapped ? UiSound.Snap : UiSound.Escape);
        }

        public override void Tick()
        {
            float age = Time.unscaledTime - shownAt;
            if (age < 0f || age > Duration)
            {
                group.alpha = 0f;
                return;
            }

            float inK = Mathf.Clamp01(age / 0.3f);
            float outK = Mathf.Clamp01((Duration - age) / 0.4f);
            float k = Mathf.Min(inK, outK);
            float eased = k * k * (3f - 2f * k);

            group.alpha = eased;
            card.anchoredPosition = new Vector2(0f, -190f + (1f - inK) * 40f);
        }
    }
}
