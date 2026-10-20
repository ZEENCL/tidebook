// UiBuilder.cs
// Small helpers for building uGUI in code (no prefabs needed), so the whole UI imports as plain scripts.
// Layout vocabulary used everywhere:
//   TopLeft / TopRight / BottomLeft / Fixed  -> a fixed-size element pinned to a point
//   Anchor / Stretch                          -> an element that scales with its parent

using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public static class UiBuilder
    {
        public static RectTransform Rect(Transform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        public static void Stretch(RectTransform rt, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        /// <summary>Anchors given as 0..1 fractions of the parent; offsets are pixel insets from those anchors.</summary>
        public static void Anchor(RectTransform rt, float minX, float minY, float maxX, float maxY,
                                  float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
        {
            rt.anchorMin = new Vector2(minX, minY);
            rt.anchorMax = new Vector2(maxX, maxY);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        /// <summary>Fixed-size element whose anchor and pivot are the same point (ax, ay in 0..1).</summary>
        public static void Fixed(RectTransform rt, float ax, float ay, float width, float height, float x = 0f, float y = 0f)
        {
            Vector2 a = new Vector2(ax, ay);
            rt.anchorMin = a;
            rt.anchorMax = a;
            rt.pivot = a;
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(x, y);
        }

        public static void TopLeft(RectTransform rt, float x, float y, float width, float height)
        {
            Fixed(rt, 0f, 1f, width, height, x, -y);
        }

        public static void TopRight(RectTransform rt, float right, float y, float width, float height)
        {
            Fixed(rt, 1f, 1f, width, height, -right, -y);
        }

        public static void BottomLeft(RectTransform rt, float x, float bottom, float width, float height)
        {
            Fixed(rt, 0f, 0f, width, height, x, bottom);
        }

        public static Image Box(Transform parent, string name, Color color, int radius = 0, bool raycast = false)
        {
            RectTransform rt = Rect(parent, name);
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycast;
            if (radius > 0)
            {
                img.sprite = ProceduralSprites.Rounded(radius);
                img.type = Image.Type.Sliced;
            }
            return img;
        }

        public static Image GradientBox(Transform parent, string name, int radius, Color top, Color bottom)
        {
            RectTransform rt = Rect(parent, name);
            Image img = rt.gameObject.AddComponent<Image>();
            img.sprite = ProceduralSprites.RoundedGradient(radius, top, bottom);
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            img.raycastTarget = false;
            return img;
        }

        public static Image Icon(Transform parent, string name, Sprite sprite, Color color)
        {
            RectTransform rt = Rect(parent, name);
            Image img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.preserveAspect = true;
            img.raycastTarget = false;
            return img;
        }

        public static Text Label(Transform parent, string name, string text, int size, Color color,
                                 TextAnchor anchor = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal,
                                 bool bestFit = false)
        {
            RectTransform rt = Rect(parent, name);
            Text t = rt.gameObject.AddComponent<Text>();
            t.font = UiFonts.Default;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = anchor;
            t.fontStyle = style;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.raycastTarget = false;
            t.lineSpacing = 1.05f;
            if (bestFit)
            {
                t.resizeTextForBestFit = true;
                t.resizeTextMinSize = Mathf.Max(10, size / 2);
                t.resizeTextMaxSize = size;
            }
            return t;
        }

        public static void DropShadow(Graphic graphic, float distance = 8f, float alpha = 0.25f)
        {
            Shadow shadow = graphic.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.05f, 0.15f, 0.2f, alpha);
            shadow.effectDistance = new Vector2(0f, -distance);
            shadow.useGraphicAlpha = true;
        }

        /// <summary>Turns a (raycast-enabled) image into a hover/press/select button.</summary>
        public static UiButton Button(Image background, Color normal, Color hover, Color pressed, Color selected)
        {
            background.raycastTarget = true;
            background.color = normal;

            UiButton button = background.gameObject.AddComponent<UiButton>();
            button.background = background;
            button.normal = normal;
            button.hover = hover;
            button.pressed = pressed;
            button.selectedColor = selected;
            return button;
        }
    }
}
