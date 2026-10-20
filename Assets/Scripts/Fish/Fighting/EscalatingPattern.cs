// EscalatingPattern.cs - runtime behaviour for EscalatingStyle.
using UnityEngine;

namespace Tidebook
{
    public class EscalatingPattern : PullPatternBase
    {
        readonly EscalatingStyle style;

        public EscalatingPattern(EscalatingStyle style, float strength)
            : base(strength, style.warningLead)
        {
            this.style = style;
        }

        protected override float NextInterval(float progress)
        {
            return Mathf.Lerp(style.startInterval, style.endInterval, progress);
        }

        protected override float NextImpulse(float progress)
        {
            float escalation = Mathf.Lerp(1f, style.endImpulseMultiplier, progress);
            return style.impulse * (style.strengthBase + strength) * escalation;
        }
    }
}
