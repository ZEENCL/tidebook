// FishingTypes.cs
// Shared vocabulary: enums + the WaterContext snapshot. Data only, no behaviour.
//
// NOTE: no "using System;" anywhere in this project (System.Random clashes with UnityEngine.Random).

namespace Tidebook
{
    public enum Buoyancy { Floating, Suspending, Sinking }
    public enum BodyShape { Slim, Round, Wide, Curly }                       // placeholder values
    public enum ColorPattern { RedWhite, BlueSilver, GreenBrown, Gold, Glow } // placeholder values
    public enum SwimPattern { Steady, Darting, Hopping, Drifting }           // placeholder values
    public enum DepthLayer { Top, Mid, Bottom }
    public enum TimeOfDay { Day, Night }
    public enum Weather { Clear, Rain, Storm, Fog }
    public enum Rarity { Common, Uncommon, Rare, Legendary }

    /// <summary>Snapshot of the conditions at a fishing spot.</summary>
    public struct WaterContext
    {
        public TimeOfDay time;
        public Weather weather;
        public string spotId;
    }
}
