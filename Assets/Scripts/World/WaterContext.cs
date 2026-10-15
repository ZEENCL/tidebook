namespace Tidebook
{
    public enum DepthLayer { Top, Mid, Bottom }
    public enum TimeOfDay { Day, Night }
    public enum Weather { Clear, Rain, Storm, Fog }

    /// <summary>Snapshot of the conditions at a fishing spot.</summary>
    public struct WaterContext
    {
        public TimeOfDay time;
        public Weather weather;
        public string spotId;
    }
}
