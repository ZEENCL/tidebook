// IFightTarget.cs - role: a hooked fish being reeled in (used while Reeling).
namespace Tidebook
{
    public interface IFightTarget
    {
        float FightStrength { get; }
        int Reward { get; }
        void Escape();
        void Catch();
    }
}
