// HudStrip.cs
// A small always-on HUD card (bottom-left): the equipped bait, plus the hotkeys for the two menus.
// Matches the GDD's HUD note: "currently equipped bait configuration at a glance".
// It hides itself while a menu is open.

using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public class HudStrip : MonoBehaviour
    {
        EquippedBait equipped;
        GameObject card;
        Image lure;
        Text nameText;
        Text summaryText;

        public static HudStrip Create(RectTransform parent, EquippedBait equipped, string baitKey, string indexKey)
        {
            RectTransform root = UiBuilder.Rect(parent, "HudStrip");
            UiBuilder.Stretch(root);
            HudStrip hud = root.gameObject.AddComponent<HudStrip>();
            hud.Build(root, equipped, baitKey, indexKey);
            return hud;
        }

        void Build(RectTransform root, EquippedBait equippedBait, string baitKey, string indexKey)
        {
            equipped = equippedBait;

            Image bg = UiBuilder.Box(root, "Card", UiPalette.WithAlpha(UiPalette.CoverDark, 0.9f), 26);
            UiBuilder.Fixed(bg.rectTransform, 0f, 0f, 600f, 96f, 36f, 36f);
            UiBuilder.DropShadow(bg, 6f, 0.3f);
            card = bg.gameObject;

            Image iconBg = UiBuilder.Box(bg.transform, "IconBg", UiPalette.Paper, 30);
            UiBuilder.Fixed(iconBg.rectTransform, 0f, 0.5f, 76f, 76f, 14f, 0f);
            lure = UiBuilder.Icon(iconBg.transform, "Lure", null, Color.white);
            UiBuilder.Stretch(lure.rectTransform, 4f, 4f, 4f, 4f);

            Text label = UiBuilder.Label(bg.transform, "Label", "EQUIPPED BAIT", 15, UiPalette.Sun, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiBuilder.TopLeft(label.rectTransform, 104f, 12f, 260f, 20f);

            nameText = UiBuilder.Label(bg.transform, "Name", "", 24, UiPalette.White, TextAnchor.MiddleLeft, FontStyle.Bold, true);
            UiBuilder.TopLeft(nameText.rectTransform, 104f, 32f, 262f, 32f);

            summaryText = UiBuilder.Label(bg.transform, "Summary", "", 18, UiPalette.WaterMid, TextAnchor.MiddleLeft, FontStyle.Normal, true);
            UiBuilder.TopLeft(summaryText.rectTransform, 104f, 64f, 262f, 24f);

            BuildKey(bg.transform, baitKey, "Bait builder", 14f);
            BuildKey(bg.transform, indexKey, "Tidebook", 54f);

            equipped.Selection.Changed += Refresh;
            UiFocus.Changed += OnFocusChanged;
            Refresh();
        }

        static void BuildKey(Transform parent, string key, string description, float y)
        {
            Image cap = UiBuilder.Box(parent, "Key " + key, UiPalette.Paper, 10);
            UiBuilder.TopLeft(cap.rectTransform, 384f, y, 48f, 32f);
            Text keyText = UiBuilder.Label(cap.transform, "Text", key, 18, UiPalette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UiBuilder.Stretch(keyText.rectTransform, 2f, 0f, 2f, 0f);

            Text desc = UiBuilder.Label(parent, "KeyLabel " + key, description, 20, UiPalette.White, TextAnchor.MiddleLeft);
            UiBuilder.TopLeft(desc.rectTransform, 442f, y, 150f, 32f);
        }

        void Refresh()
        {
            lure.sprite = BaitArt.Lure(equipped.Selection.Shape, equipped.Selection.Pattern);
            nameText.text = BaitText.FullName(equipped.Selection);
            summaryText.text = BaitText.Summary(equipped.Selection);
        }

        void OnFocusChanged()
        {
            card.SetActive(!UiFocus.AnyOpen);
        }

        void OnDestroy()
        {
            if (equipped != null) equipped.Selection.Changed -= Refresh;
            UiFocus.Changed -= OnFocusChanged;
        }
    }
}
