// IBaitReactive.cs - role: a fish that can notice bait and announce a bite (used while Waiting).
namespace Tidebook
{
    public interface IBaitReactive
    {
        event System.Action<IHookableFish> Bit;
        bool TryNotice(Bait bait, UnityEngine.Vector3 position, WaterContext ctx);
        void LoseInterest();

        /// <summary>Position / mood of this fish for visual feedback (shadows, bubbles).</summary>
        IFishCue Cue { get; }
    }
}
