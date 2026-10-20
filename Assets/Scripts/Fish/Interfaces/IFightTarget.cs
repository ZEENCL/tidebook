// IFightTarget.cs - role: a hooked fish being reeled in (used while Reeling).
// It only describes HOW the fish fights. Rewards and what happens afterwards live elsewhere.
namespace Tidebook
{
    public interface IFightTarget
    {
        float FightStrength { get; }

        /// <summary>A fresh pull pattern for this fish's fight (from its species' fight style).</summary>
        IPullPattern CreatePullPattern();
    }
}
