// RodTypes.cs
// Small shared types for the rod module.

namespace Tidebook
{
    /// <summary>What the rod is doing right now (set by each state; read by the HUD).</summary>
    public enum RodPhase { Aiming, Charging, Settling, Waiting, BiteWindow, Reeling, Resolved }

    public enum CatchResult { Caught, Snapped, Escaped, Missed, Retrieved }
}
