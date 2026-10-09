// BiteStrategies.cs
// Strategy pattern: "how does this fish score a bait?" is swappable.
//   WeightedBiteStrategy -> sums weights of matched factors (no hard gates)
//   GatedBiteStrategy    -> enforces 'required' factors, then DELEGATES scoring to another strategy
//
// Both iterate over IMatchFactor objects, so new factors never require editing them.
// matchRatio = sum(weight of matched factors) / sum(weight of all weighted factors)

using System.Collections.Generic;

namespace Tidebook
{
    /// <summary>Which strategy a species uses. Set per species asset; BiteStrategyRegistry maps it to an instance.</summary>
    public enum BiteStrategyType { Weighted, Gated }

    public struct BiteEvaluation
    {
        public bool eligible;       // false = this fish ignores the bait entirely
        public float matchRatio;    // 0..1, drives how fast/committed the fish is

        public static readonly BiteEvaluation None = new BiteEvaluation();
    }

    public class WeightedBiteStrategy : IBiteStrategy
    {
        readonly IReadOnlyList<IMatchFactor> factors;

        public WeightedBiteStrategy() : this(MatchFactors.Default) { }

        public WeightedBiteStrategy(IReadOnlyList<IMatchFactor> factors)
        {
            this.factors = factors;
        }

        public BiteEvaluation Evaluate(PreferenceProfile p, Bait bait, WaterContext ctx)
        {
            float score = 0f;
            float total = 0f;

            for (int i = 0; i < factors.Count; i++)
            {
                float weight = factors[i].WeightFor(p).weight;
                if (weight <= 0f) continue;                       // fish doesn't care about this factor

                total += weight;
                if (factors[i].Matches(p, bait, ctx)) score += weight;
            }

            // Nothing weighted, or zero factors matched -> fish ignores this bait.
            if (total <= 0f || score <= 0f) return BiteEvaluation.None;

            BiteEvaluation result = new BiteEvaluation();
            result.eligible = true;
            result.matchRatio = score / total;
            return result;
        }
    }

    public class GatedBiteStrategy : IBiteStrategy
    {
        readonly IBiteStrategy inner;
        readonly IReadOnlyList<IMatchFactor> factors;

        public GatedBiteStrategy(IBiteStrategy inner) : this(inner, MatchFactors.Default) { }

        public GatedBiteStrategy(IBiteStrategy inner, IReadOnlyList<IMatchFactor> factors)
        {
            this.inner = inner;
            this.factors = factors;
        }

        public BiteEvaluation Evaluate(PreferenceProfile p, Bait bait, WaterContext ctx)
        {
            // Any required factor that doesn't match = the fish is not in the pool.
            for (int i = 0; i < factors.Count; i++)
            {
                if (factors[i].WeightFor(p).required && !factors[i].Matches(p, bait, ctx))
                    return BiteEvaluation.None;
            }

            return inner.Evaluate(p, bait, ctx);
        }
    }
}
