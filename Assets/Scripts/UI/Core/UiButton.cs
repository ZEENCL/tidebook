// UiButton.cs
// Hover / press / selected feedback with a soft scale animation. Uses unscaled time,
// so it still animates while the game is paused behind a menu.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public class UiButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
                            IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public Graphic background;
        public Color normal = Color.white;
        public Color hover = Color.white;
        public Color pressed = Color.white;
        public Color selectedColor = Color.white;
        public Color disabledColor = Color.gray;

        public Text label;
        public Color labelNormal = Color.black;
        public Color labelSelected = Color.white;

        public float hoverScale = 1.03f;
        public bool selected;
        public bool interactable = true;

        public System.Action Clicked;

        bool hovered;
        bool down;

        void OnEnable()
        {
            hovered = false;
            down = false;
            transform.localScale = Vector3.one;
            ApplyInstant();
        }

        public void ApplyInstant()
        {
            if (background != null) background.color = TargetColor();
            if (label != null) label.color = selected ? labelSelected : labelNormal;
        }

        Color TargetColor()
        {
            if (!interactable) return disabledColor;
            if (selected) return selectedColor;
            if (down) return pressed;
            if (hovered) return hover;
            return normal;
        }

        void Update()
        {
            float k = 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime);

            float s = !interactable ? 1f : down ? 0.97f : hovered ? hoverScale : 1f;
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(s, s, 1f), k);

            if (background != null) background.color = Color.Lerp(background.color, TargetColor(), k);
            if (label != null) label.color = Color.Lerp(label.color, selected ? labelSelected : labelNormal, k);
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (!interactable) return;
            hovered = true;
            UiSounds.Play(UiSound.Hover);
        }

        public void OnPointerExit(PointerEventData e) { hovered = false; down = false; }
        public void OnPointerDown(PointerEventData e) { if (interactable) down = true; }
        public void OnPointerUp(PointerEventData e) { down = false; }

        public void OnPointerClick(PointerEventData e)
        {
            if (!interactable) return;
            UiSounds.Play(UiSound.Click);
            System.Action handler = Clicked;
            if (handler != null) handler();
        }
    }
}
