// IRodStatus.cs - read-only snapshot of the rod for the HUD (or any other observer).
namespace Tidebook
{
    public interface IRodStatus
    {
        RodPhase Phase { get; }
        string Message { get; }
        float Power { get; }
        float Tension { get; }
        float ReelProgress { get; }
        float BandHalfWidth { get; }
        IFishInfo Target { get; }
    }
}
