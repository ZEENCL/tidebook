// BiteStrategyRegistry.cs
// Maps a species' BiteStrategyType to a strategy instance. Register() lets you plug in
// a new or replacement strategy without touching FishFactory.

using System.Collections.Generic;

namespace Tidebook
{
    public class BiteStrategyRegistry : IBiteStrategyResolver
    {
        readonly Dictionary<BiteStrategyType, IBiteStrategy> map = new Dictionary<BiteStrategyType, IBiteStrategy>();

        public BiteStrategyRegistry()
        {
            IBiteStrategy weighted = new WeightedBiteStrategy();
            map[BiteStrategyType.Weighted] = weighted;
            map[BiteStrategyType.Gated] = new GatedBiteStrategy(weighted);   // composition
        }

        public void Register(BiteStrategyType type, IBiteStrategy strategy)
        {
            map[type] = strategy;
        }

        public IBiteStrategy Resolve(FishSpecies species)
        {
            IBiteStrategy strategy;
            if (map.TryGetValue(species.biteStrategy, out strategy)) return strategy;
            return map[BiteStrategyType.Weighted];
        }
    }
}
