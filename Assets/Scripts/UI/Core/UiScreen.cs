// UiScreen.cs
// Base class for a full-screen menu: dimmed backdrop + a centered window that fades and eases in.
// Subclasses only describe the window size and build their contents in BuildWindow().

using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public abstract class UiScreen : MonoBehaviour
    {
        const float FadeSeconds = 0.16f;

        CanvasGroup group;
        RectTransform window;
        float amount;
        bool open;

        public bool IsOpen { get { return open; } }

        protected abstract Vector2 WindowSize { get; }
        protected abstract void BuildWindow(RectTransform window);

        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnTick() { }

        /// <summary>Called by UiRoot right after the component is added.</summary>
        public void Init()
        {
            group = gameObject.GetComponent<CanvasGroup>();
            if (group == null) group = gameObject.AddComponent<CanvasGroup>();

            RectTransform self = (RectTransform)transform;
            UiBuilder.Stretch(self);

            Image dim = UiBuilder.Box(self, "Dim", UiPalette.Dim, 0, true);   // also blocks clicks to the game
            UiBuilder.Stretch(dim.rectTransform);

            window = UiBuilder.Rect(self, "Window");
            UiBuilder.Fixed(window, 0.5f, 0.5f, WindowSize.x, WindowSize.y);
            BuildWindow(window);

            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
            gameObject.SetActive(false);
        }

        public void Open()
        {
            if (open) return;
            open = true;
            gameObject.SetActive(true);
            group.blocksRaycasts = true;
            group.interactable = true;
            UiFocus.Push(this);
            UiSounds.Play(UiSound.Open);
            OnOpen();
        }

        public void Close()
        {
            if (!open) return;
            open = false;
            group.blocksRaycasts = false;
            group.interactable = false;
            UiFocus.Pop(this);
            UiSounds.Play(UiSound.Close);
            OnClose();
        }

        void Update()
        {
            amount = Mathf.MoveTowards(amount, open ? 1f : 0f, Time.unscaledDeltaTime / FadeSeconds);
            float e = amount * amount * (3f - 2f * amount);

            group.alpha = e;
            window.localScale = Vector3.one * Mathf.Lerp(0.95f, 1f, e);
            window.anchoredPosition = new Vector2(0f, Mathf.Lerp(-18f, 0f, e));

            if (open) OnTick();
            if (!open && amount <= 0f) gameObject.SetActive(false);
        }

        void OnDisable()
        {
            // If the screen is destroyed or deactivated while open, don't leave the game stuck "in a menu".
            if (open)
            {
                open = false;
                UiFocus.Pop(this);
            }
        }
    }
}
