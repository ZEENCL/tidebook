// BurstAndRestPattern.cs - runtime behaviour for BurstAndRestStyle.
// Sequence: rest -> pull -> gap -> pull -> gap -> pull -> rest ...
// Only the first pull of a burst is telegraphed (a warning before every quick pull would be noise).
using UnityEngine;

namespace Tidebook
{
    public class BurstAndRestPattern : PullPatternBase
    {
        readonly BurstAndRestStyle style;
        int remainingInBurst;
        bool nextIsBurstStart;

        public BurstAndRestPattern(BurstAndRestStyle style, float strength)
            : base(strength, style.warningLead)
        {
            this.style = style;
        }

        protected override void OnBegin()
        {
            remainingInBurst = 0;
        }

        protected override float NextInterval(float progress)
        {
            if (remainingInBurst <= 0)
            {
                remainingInBurst = Mathf.Max(1, style.pullsPerBurst);
                nextIsBurstStart = true;
                return style.restSeconds;
            }

            nextIsBurstStart = false;
            return style.pullGap;
        }

        protected override float NextImpulse(float progress)
        {
            remainingInBurst--;
            return style.impulse * (style.strengthBase + strength);
        }

        protected override bool ShouldWarnBeforeNext()
        {
            return nextIsBurstStart;
        }
    }
}
