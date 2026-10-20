// CatchReport.cs
// Everything listeners may want to know about one resolved cast. Built by ResolvedState.
// Info / Outcome are null when no fish was involved (e.g. the player reeled in an empty line).
namespace Tidebook
{
    public struct CatchReport
    {
        public readonly CatchResult Result;
        public readonly IFishInfo Info;
        public readonly IFishOutcome Outcome;
        public readonly Bait Bait;
        public readonly WaterContext Water;

        public CatchReport(CatchResult result, IHookableFish fish, Bait bait, WaterContext water)
        {
            Result = result;
            Info = fish;
            Outcome = fish;
            Bait = bait;
            Water = water;
        }
    }
}
