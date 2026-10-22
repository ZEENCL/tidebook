// IFishCue.cs
// Role: what the player can SEE of a fish that has noticed their bait.
// Used for the approaching shadow, the bubbles above the bobber and the water ripples.
using UnityEngine;

namespace Tidebook
{
    public enum FishCueState { None, Curious, Interested, Biting, Hooked }

    public interface IFishCue
    {
        Vector3 Position { get; }
        Vector3 Forward { get; }
        FishCueState CueState { get; }

        /// <summary>Roughly how big the fish looks, in metres (bigger shadow = rarer fish).</summary>
        float SizeHint { get; }
    }
}
