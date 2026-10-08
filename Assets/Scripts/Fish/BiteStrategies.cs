namespace Tidebook
{
    /// <summary>Which strategy a species uses. Set per species asset; FishFactory maps it to an instance.</summary>
    public enum BiteStrategyType { Weighted, Gated }

    public struct BiteEvaluation
    {
        public bool eligible;       // false = this fish ignores the bait entirely
        public float matchRatio;    // 0..1, drives how fast/committed the fish is

        public static readonly BiteEvaluation None = new BiteEvaluation();
    }

    public class WeightedBiteStrategy : IBiteStrategy
    {
        public BiteEvaluation Evaluate(PreferenceProfile p, Bait bait, WaterContext ctx)
        {
            float score = 0f;
            float total = 0f;

            Tally(ref score, ref total, p.buoyancyW.weight, bait.Buoyancy == p.buoyancy);
            Tally(ref score, ref total, p.shapeW.weight, bait.Shape == p.shape);
            Tally(ref score, ref total, p.colorW.weight, bait.Color == p.color);
            Tally(ref score, ref total, p.swimW.weight, bait.Swim == p.swim);
            Tally(ref score, ref total, p.timeW.weight, ctx.time == p.time);
            Tally(ref score, ref total, p.weatherW.weight, ctx.weather == p.weather);

            // Nothing weighted, or zero factors matched -> fish doesn't care about this bait at all.
            if (total <= 0f || score <= 0f) return BiteEvaluation.None;

            BiteEvaluation result = new BiteEvaluation();
            result.eligible = true;
            result.matchRatio = score / total;
            return result;
        }

        static void Tally(ref float score, ref float total, float weight, bool match)
        {
            if (weight <= 0f) return;   // fish doesn't care about this factor
            total += weight;
            if (match) score += weight;
        }
    }

    /// <summary>Decorator-style strategy: enforces hard gates, then asks the wrapped strategy for the score.</summary>
    public class GatedBiteStrategy : IBiteStrategy
    {
        readonly IBiteStrategy inner;

        public GatedBiteStrategy(IBiteStrategy inner)
        {
            this.inner = inner;
        }

        public BiteEvaluation Evaluate(PreferenceProfile p, Bait bait, WaterContext ctx)
        {
            // Any required factor that doesn't match = fish is not in the pool.
            if (p.buoyancyW.required && bait.Buoyancy != p.buoyancy) return BiteEvaluation.None;
            if (p.shapeW.required && bait.Shape != p.shape) return BiteEvaluation.None;
            if (p.colorW.required && bait.Color != p.color) return BiteEvaluation.None;
            if (p.swimW.required && bait.Swim != p.swim) return BiteEvaluation.None;
            if (p.timeW.required && ctx.time != p.time) return BiteEvaluation.None;
            if (p.weatherW.required && ctx.weather != p.weather) return BiteEvaluation.None;

            return inner.Evaluate(p, bait, ctx);
        }
    }
}
