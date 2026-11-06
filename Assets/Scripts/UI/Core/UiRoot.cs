// UiRoot.cs
// Creates the Canvas (1920x1080 reference, scales with the screen) and the layers screens live on.
// Also makes sure an EventSystem exists so clicks work.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public class UiRoot : MonoBehaviour
    {
        public RectTransform HudLayer { get; private set; }
        public RectTransform ScreenLayer { get; private set; }
        public RectTransform ToastLayer { get; private set; }

        public static UiRoot Create(int sortingOrder)
        {
            GameObject go = new GameObject("Tidebook UI", typeof(RectTransform));

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();

            UiRoot root = go.AddComponent<UiRoot>();
            root.HudLayer = Layer(go.transform, "Hud");
            root.ScreenLayer = Layer(go.transform, "Screens");
            root.ToastLayer = Layer(go.transform, "Toasts");

            EnsureEventSystem();
            return root;
        }

        static RectTransform Layer(Transform parent, string name)
        {
            RectTransform rt = UiBuilder.Rect(parent, name);
            UiBuilder.Stretch(rt);
            return rt;
        }

        static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            if (UiFind.First<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        public T AddScreen<T>(string name) where T : UiScreen
        {
            RectTransform rt = UiBuilder.Rect(ScreenLayer, name);
            T screen = rt.gameObject.AddComponent<T>();
            screen.Init();
            return screen;
        }

        public UiToast CreateToast()
        {
            return UiToast.Create(ToastLayer);
        }

        public HudStrip CreateHud(EquippedBait equipped, string baitKeyLabel, string indexKeyLabel)
        {
            return HudStrip.Create(HudLayer, equipped, baitKeyLabel, indexKeyLabel);
        }
    }
}
