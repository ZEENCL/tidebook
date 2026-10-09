// IFishInfo.cs - role: read-only fish details for UI and debugging.
namespace Tidebook
{
    public interface IFishInfo
    {
        string DisplayName { get; }
        string StateName { get; }
        float MatchRatio { get; }
        float BiteTimeLeft { get; }
    }
}
