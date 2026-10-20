// Meter.cs - one meter's current values. Written by states, read through IMeterView.
namespace Tidebook
{
    public class Meter : IMeterView
    {
        public string Label { get; private set; }
        public float Value { get; set; }
        public bool HasBand { get; set; }
        public float BandMin { get; set; }
        public float BandMax { get; set; }

        public Meter(string label)
        {
            Label = label;
        }
    }
}
