// MatchFactors.cs
// Each scoring factor is a small object. To add a 7th factor (e.g. bait size):
//   1) add its field + FactorWeight to PreferenceProfile (data)
//   2) add ONE line to MatchFactors.Default below
// The strategies themselves never need to change (Open/Closed).

using System.Collections.Generic;

namespace Tidebook
{
    public sealed class MatchFactor : IMatchFactor
    {
        readonly System.Func<PreferenceProfile, FactorWeight> weight;
        readonly System.Func<PreferenceProfile, Bait, WaterContext, bool> matches;

        public MatchFactor(System.Func<PreferenceProfile, FactorWeight> weight,
                           System.Func<PreferenceProfile, Bait, WaterContext, bool> matches)
        {
            this.weight = weight;
            this.matches = matches;
        }

        public FactorWeight WeightFor(PreferenceProfile profile)
        {
            return weight(profile);
        }

        public bool Matches(PreferenceProfile profile, Bait bait, WaterContext ctx)
        {
            return matches(profile, bait, ctx);
        }
    }

    public static class MatchFactors
    {
        public static readonly IReadOnlyList<IMatchFactor> Default = new IMatchFactor[]
        {
            new MatchFactor(p => p.buoyancyW, (p, bait, ctx) => bait.Buoyancy == p.buoyancy),
            new MatchFactor(p => p.shapeW,    (p, bait, ctx) => bait.Shape == p.shape),
            new MatchFactor(p => p.colorW,    (p, bait, ctx) => bait.Color == p.color),
            new MatchFactor(p => p.swimW,     (p, bait, ctx) => bait.Swim == p.swim),
            new MatchFactor(p => p.timeW,     (p, bait, ctx) => ctx.time == p.time),
            new MatchFactor(p => p.weatherW,  (p, bait, ctx) => ctx.weather == p.weather),
        };
    }
}
