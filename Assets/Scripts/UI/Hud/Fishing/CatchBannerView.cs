// CatchBannerView.cs
// The result card after every cast that involved a fish (it listens to the rod's CatchEvents):
//   Caught        : fish picture, name, rarity, coins earned, "NEW ENTRY" if it is a first
//   Snapped       : "Line snapped" with a tip
//   Escaped       : "Slipped off" with a tip
//   Missed        : "Too slow" with a tip
// Reeling in an empty line shows nothing. The tips teach the mechanic without scolding (low-pressure tone).
//
// MVVM: this is the View of CatchResultViewModel. The wording, the coins and the "new entry" check are
// worked out there; this class only picks colours and the icon, plays the sound and animates the card.

using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public class CatchBannerView : HudView
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
        CatchResultViewModel viewModel;

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

        // ---- MVVM binding ----

        /// <summary>Connects the card to its ViewModel. It shows a card each time the ViewModel produces one.</summary>
        public void Bind(CatchResultViewModel vm)
        {
            Unbind();
            viewModel = vm;
            viewModel.Card.Bind(Show);   // nothing is shown until the first catch (the card starts out null)
        }

        public void Unbind()
        {
            if (viewModel == null) return;
            viewModel.Card.Unbind(Show);
            viewModel = null;
        }

        void Show(CatchCard data)
        {
            if (card == null || data == null) return;

            switch (data.Tone)
            {
                case CardTone.Caught:
                    accent.color = data.Species != null ? UiPalette.ForRarity(data.Species.rarity) : UiPalette.Mint;
                    break;
                case CardTone.Snapped: accent.color = UiPalette.Coral; break;
                case CardTone.Escaped: accent.color = UiPalette.Sky; break;
                default: accent.color = UiPalette.Sun; break;   // Missed
            }

            title.text = data.Title;
            title.color = accent.color;
            nameText.text = data.FishName;
            detail.text = data.Detail;

            FishIndexEntry entry = ctx != null ? ctx.EntryFor(data.Species) : null;
            if (entry != null)
            {
                bool known = entry.Discovered;
                icon.sprite = known ? FishArt.Icon(entry.Source) : FishArt.Silhouette(entry.Source);
            }
            else
            {
                icon.sprite = FishArt.Icon(UiPalette.Coral, 1);
            }

            newBadge.SetActive(data.IsNewEntry);
            shownAt = Time.unscaledTime;
            UiSounds.Play(data.Tone == CardTone.Caught ? UiSound.Catch
                          : data.Tone == CardTone.Snapped ? UiSound.Snap : UiSound.Escape);
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
