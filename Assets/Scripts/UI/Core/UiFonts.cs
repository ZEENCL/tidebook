// UiFonts.cs
// Resolves the font used by all UI text. Uses Unity's built-in font so nothing needs importing.
// To use your own font: assign TidebookUiController.customFont (or set UiFonts.Override).

using UnityEngine;

namespace Tidebook.Ui
{
    public static class UiFonts
    {
        public static Font Override;
        static Font cached;

        public static Font Default
        {
            get
            {
                if (Override != null) return Override;
                if (cached == null) cached = Load();
                return cached;
            }
        }

        static Font Load()
        {
            Font font = null;
#if UNITY_2022_2_OR_NEWER
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
            if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            return font;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            cached = null;
            Override = null;
        }
    }
}
