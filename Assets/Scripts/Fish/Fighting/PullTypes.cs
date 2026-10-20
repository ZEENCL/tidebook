// PullTypes.cs
// Small shared types for fish pulls.

namespace Tidebook
{
    /// <summary>A hint for UI / audio / rod shake. Edge-triggered: raised once, not every frame.</summary>
    public enum PullCue { None, Warning, Pull }

    public struct PullResult
    {
        public float impulse;   // tension to add this frame (0 = no pull)
        public PullCue cue;
    }
}
