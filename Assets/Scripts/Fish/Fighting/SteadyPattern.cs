// SteadyPattern.cs - runtime behaviour for SteadyStyle.
namespace Tidebook
{
    public class SteadyPattern : PullPatternBase
    {
        readonly SteadyStyle style;

        public SteadyPattern(SteadyStyle style, float strength)
            : base(strength, style.warningLead)
        {
            this.style = style;
        }

        protected override float NextInterval(float progress)
        {
            return style.interval;
        }

        protected override float NextImpulse(float progress)
        {
            return style.impulse * (style.strengthBase + strength);
        }
    }
}
