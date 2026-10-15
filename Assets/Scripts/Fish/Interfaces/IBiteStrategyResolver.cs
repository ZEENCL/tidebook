// IBiteStrategyResolver.cs - picks the strategy a species should use (keeps the factory closed to change).
namespace Tidebook
{
    public interface IBiteStrategyResolver
    {
        IBiteStrategy Resolve(FishSpecies species);
    }
}
