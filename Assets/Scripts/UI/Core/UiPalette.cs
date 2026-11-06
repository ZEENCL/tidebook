// UiPalette.cs
// The Tidebook look: soft ocean teals, warm paper cream, coral and sun-yellow accents.
// Light, playful, low-pressure - matches the GDD's "light and relaxing" tone.
// Change colors here and every screen follows.

using UnityEngine;

namespace Tidebook.Ui
{
    public static class UiPalette
    {
        public static Color Hex(string hex, float alpha = 1f)
        {
            Color c;
            if (!ColorUtility.TryParseHtmlString(hex, out c)) c = Color.magenta;
            c.a = alpha;
            return c;
        }

        // ---- text ----
        public static readonly Color Ink = Hex("#26414F");
        public static readonly Color InkSoft = Hex("#5C7A89");
        public static readonly Color InkFaint = Hex("#9DB0B8");

        // ---- paper and book ----
        public static readonly Color Paper = Hex("#FBF4E4");
        public static readonly Color PaperShade = Hex("#F3E8CF");
        public static readonly Color PaperEdge = Hex("#E2D2AC");
        public static readonly Color Cover = Hex("#2F7384");
        public static readonly Color CoverDark = Hex("#1E4F5E");
        public static readonly Color Spine = Hex("#17414F");
        public static readonly Color Dim = Hex("#0D2733", 0.62f);

        // ---- water ----
        public static readonly Color WaterTop = Hex("#D8F4F5");
        public static readonly Color WaterMid = Hex("#8BD3DE");
        public static readonly Color WaterDeep = Hex("#3A93B0");
        public static readonly Color Sand = Hex("#EAD7A4");

        // ---- accents ----
        public static readonly Color Coral = Hex("#FF8A6B");
        public static readonly Color CoralDark = Hex("#E56B4C");
        public static readonly Color Sun = Hex("#F6C453");
        public static readonly Color Mint = Hex("#7ED6B2");
        public static readonly Color Lavender = Hex("#A9A5F0");
        public static readonly Color Sky = Hex("#5CB6E8");
        public static readonly Color Rose = Hex("#F29AB8");
        public static readonly Color Lime = Hex("#B7DE5E");
        public static readonly Color Teal = Hex("#4FB3BF");
        public static readonly Color White = Hex("#FFFFFF");
        public static readonly Color Locked = Hex("#3B5663");

        public static Color ForRarity(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Uncommon: return Sky;
                case Rarity.Rare: return Lavender;
                case Rarity.Legendary: return Sun;
                default: return Mint;
            }
        }

        public static Color WithAlpha(Color c, float alpha)
        {
            c.a = alpha;
            return c;
        }
    }
}
