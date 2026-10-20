// RandomBurstsPattern.cs - runtime behaviour for RandomBurstsStyle.
using UnityEngine;

namespace Tidebook
{
    public class RandomBurstsPattern : PullPatternBase
    {
        readonly RandomBurstsStyle style;

        public RandomBurstsPattern(RandomBurstsStyle style, float strength)
            : base(strength, style.warningLead)
        {
            this.style = style;
        }

        protected override float NextInterval(float progress)
        {
            return Random.Range(style.minInterval, Mathf.Lerp(style.maxIntervalCalm, style.maxIntervalFierce, strength));
        }

        protected override float NextImpulse(float progress)
        {
            return Random.Range(style.minImpulse, style.maxImpulse) * (style.strengthBase + strength);
        }
    }
}
