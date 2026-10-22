// IRodStatus.cs - read-only snapshot of the rod for the HUD (or audio, or any other observer).
// Generic on purpose: no mechanic-specific fields. Mechanics publish meters and cues instead.
using System.Collections.Generic;
using UnityEngine;

namespace Tidebook
{
    public interface IRodStatus
    {
        RodPhase Phase { get; }
        string Message { get; }
        IReadOnlyList<IMeterView> Meters { get; }
        IFishInfo Target { get; }

        /// <summary>Where the bait is in the world (valid while Settling, Waiting, BiteWindow and Reeling).</summary>
        Vector3 BaitPosition { get; }

        /// <summary>The cast the player would make right now (published while Aiming and Charging).</summary>
        CastPreview Preview { get; }

        /// <summary>Fish that have noticed the bait during this cast (for shadows, bubbles and sounds).</summary>
        IReadOnlyList<IFishCue> Watchers { get; }

        /// <summary>Raised at key moments (bite started, pull warning, pull).</summary>
        event System.Action<RodCue> Cue;
    }
}
