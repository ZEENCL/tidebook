// IPullPattern.cs
// Strategy: the RHYTHM of a hooked fish's pulls (when it spikes the line tension, and how hard).
// Each fight gets its own fresh instance, so it can keep timers and counters freely.
namespace Tidebook
{
    public interface IPullPattern
    {
        /// <summary>Reset timers/counters. Called once when the fight starts.</summary>
        void Begin();

        /// <summary>
        /// Advance by dt. 'progress' is how far along the reel is (0..1), so a pattern can escalate.
        /// Returns what happened this frame (an impulse to add to tension, and/or a cue for UI/audio).
        /// </summary>
        PullResult Tick(float dt, float progress);
    }
}
