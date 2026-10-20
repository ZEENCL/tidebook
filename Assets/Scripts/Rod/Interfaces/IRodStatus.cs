// IRodStatus.cs - read-only snapshot of the rod for the HUD (or audio, or any other observer).
// Generic on purpose: no mechanic-specific fields. Mechanics publish meters and cues instead.
using System.Collections.Generic;

namespace Tidebook
{
    public interface IRodStatus
    {
        RodPhase Phase { get; }
        string Message { get; }
        IReadOnlyList<IMeterView> Meters { get; }
        IFishInfo Target { get; }

        /// <summary>Raised at key moments (bite started, pull warning, pull).</summary>
        event System.Action<RodCue> Cue;
    }
}
