// IHookableFish.cs - the fish as seen once it bites: just the small role interfaces combined.
// Consumers should still hold the narrowest role they need (IBiteTarget, IFightTarget, IFishInfo).
namespace Tidebook
{
    public interface IHookableFish : IBiteTarget, IFightTarget, IFishInfo
    {
    }
}
