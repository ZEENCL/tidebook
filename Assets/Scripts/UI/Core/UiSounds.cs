// UiSounds.cs
// A tiny seam for UI audio (the GDD asks for satisfying feedback sounds).
// No audio ships yet: implement IUiSound, then set UiSounds.Provider = new MyUiSounds().

namespace Tidebook.Ui
{
    public enum UiSound { Hover, Click, Open, Close, Confirm, Discover }

    public interface IUiSound
    {
        void Play(UiSound sound);
    }

    public static class UiSounds
    {
        public static IUiSound Provider;

        public static void Play(UiSound sound)
        {
            if (Provider != null) Provider.Play(sound);
        }
    }
}
