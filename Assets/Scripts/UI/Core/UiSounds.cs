// UiSounds.cs
// A tiny seam for game and UI audio (the GDD asks for satisfying feedback sounds: ripples, bite
// indicators, hook-set and successful catches). No audio ships yet: implement IUiSound, then set
// UiSounds.Provider = new MyUiSounds() and play whatever you like for each moment.

namespace Tidebook.Ui
{
    public enum UiSound
    {
        // menus
        Hover, Click, Open, Close, Confirm, Discover,
        // fishing
        Cast,          // line leaves the rod
        Splash,        // bait hits the water
        Reject,        // cast would not land on water
        Nibble,        // a fish is nibbling the bait
        Bite,          // the bite alert
        HookSet,       // the fish is hooked
        PullWarning,   // a pull is about to happen
        Pull,          // the fish pulls
        Catch,         // fish landed
        Snap,          // line snapped
        Escape         // fish got away
    }

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
