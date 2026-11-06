// RodTypes.cs
// Small shared types for the rod module.

namespace Tidebook
{
    /// <summary>What the rod is doing right now (set by each state; read by the HUD).</summary>
    public enum RodPhase { Aiming, Charging, Settling, Waiting, BiteWindow, Reeling, Resolved }

    public enum CatchResult { Caught, Snapped, Escaped, Missed, Retrieved }

    /// <summary>Moments the HUD, audio or rod animation may want to react to.</summary>
    public enum RodCue
    {
        BiteStarted, PullWarning, Pulled,
        CastLaunched,   // the bait left the rod and landed in a fishing zone
        CastRejected,   // the player released a cast that would not land on fishing water
        HookSet         // the player hooked the fish; the fight begins
    }
}
