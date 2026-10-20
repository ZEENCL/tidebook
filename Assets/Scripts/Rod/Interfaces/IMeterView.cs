// IMeterView.cs - read-only view of one on-screen meter (power, tension, progress, bite window...).
// The HUD just draws whatever meters exist, so a new mechanic can add a meter without touching the HUD.
namespace Tidebook
{
    public interface IMeterView
    {
        string Label { get; }
        float Value { get; }       // 0..1
        bool HasBand { get; }      // a highlighted "safe" range, if the mechanic has one
        float BandMin { get; }
        float BandMax { get; }
    }
}
