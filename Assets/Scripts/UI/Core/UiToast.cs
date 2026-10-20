// UiToast.cs
// A small banner that slides in from the top ("New entry! Bay Perch added to your Tidebook").
// Queues messages so quick successive catches don't overlap.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public class UiToast : MonoBehaviour
    {
        const float SlideSeconds = 0.35f;
        const float HoldSeconds = 3.2f;

        struct Entry
        {
            public string title;
            public string body;
            public Sprite icon;
        }

        readonly Queue<Entry> queue = new Queue<Entry>();

        CanvasGroup group;
        RectTransform card;
        Text titleText;
        Text bodyText;
        Image iconImage;

        bool showing;
        float timer;

        public static UiToast Create(RectTransform parent)
        {
            RectTransform rt = UiBuilder.Rect(parent, "Toast");
            UiBuilder.Stretch(rt);
            UiToast toast = rt.gameObject.AddComponent<UiToast>();
            toast.Build(rt);
            return toast;
        }

        void Build(RectTransform root)
        {
            group = gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            Image bg = UiBuilder.Box(root, "Card", UiPalette.WithAlpha(UiPalette.CoverDark, 0.96f), 28);
            card = bg.rectTransform;
            UiBuilder.Fixed(card, 0.5f, 1f, 640f, 120f, 0f, -36f);
            UiBuilder.DropShadow(bg, 8f, 0.3f);

            Image iconBg = UiBuilder.Box(card, "IconBg", UiPalette.Paper, 40);
            UiBuilder.Fixed(iconBg.rectTransform, 0f, 0.5f, 92f, 92f, 14f, 0f);

            iconImage = UiBuilder.Icon(iconBg.transform, "Icon", null, Color.white);
            UiBuilder.Stretch(iconImage.rectTransform, 8f, 8f, 8f, 8f);

            titleText = UiBuilder.Label(card, "Title", "", 20, UiPalette.Sun, TextAnchor.LowerLeft, FontStyle.Bold);
            UiBuilder.Anchor(titleText.rectTransform, 0f, 0.5f, 1f, 1f, 122f, 0f, 20f, 14f);

            bodyText = UiBuilder.Label(card, "Body", "", 28, UiPalette.White, TextAnchor.UpperLeft, FontStyle.Bold, true);
            UiBuilder.Anchor(bodyText.rectTransform, 0f, 0f, 1f, 0.5f, 122f, 14f, 20f, 0f);
        }

        public void Show(string title, string body, Sprite icon)
        {
            Entry e = new Entry();
            e.title = title;
            e.body = body;
            e.icon = icon;
            queue.Enqueue(e);
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;

            if (!showing)
            {
                if (queue.Count == 0) return;
                Entry e = queue.Dequeue();
                titleText.text = e.title.ToUpper();
                bodyText.text = e.body;
                iconImage.sprite = e.icon;
                iconImage.enabled = e.icon != null;
                showing = true;
                timer = 0f;
                UiSounds.Play(UiSound.Discover);
            }

            timer += dt;
            float total = SlideSeconds * 2f + HoldSeconds;

            float k;   // 0 hidden .. 1 fully shown
            if (timer < SlideSeconds) k = timer / SlideSeconds;
            else if (timer > total - SlideSeconds) k = (total - timer) / SlideSeconds;
            else k = 1f;
            k = Mathf.Clamp01(k);
            float eased = k * k * (3f - 2f * k);

            group.alpha = eased;
            card.anchoredPosition = new Vector2(0f, -36f + (1f - eased) * 60f);

            if (timer >= total)
            {
                showing = false;
                group.alpha = 0f;
            }
        }
    }
}
