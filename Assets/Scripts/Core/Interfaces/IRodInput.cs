namespace Tidebook
{
    public interface IRodInput
    {
        bool ActionPressed { get; }    // cast start / set hook
        bool ActionReleased { get; }   // cast release
        bool ReelHeld { get; }         // reel while held
        bool CancelPressed { get; }    // cancel charge / reel the bait back in
    }
}
