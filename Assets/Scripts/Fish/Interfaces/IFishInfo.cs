// IFishInfo.cs - role: read-only fish details for UI, debugging and catch reports.
namespace Tidebook
{
    public interface IFishInfo
    {
        FishSpecies Species { get; }
        string DisplayName { get; }
        string StateName { get; }
        float MatchRatio { get; }
        float BiteTimeLeft { get; }
    }
}
