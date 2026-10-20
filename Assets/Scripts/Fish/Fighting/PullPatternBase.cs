// PullPatternBase.cs
// Template Method: handles the "countdown to the next pull + telegraph warning" plumbing,
// so each rhythm only says (1) how long until the next pull and (2) how hard it is.
//
// Telegraphing: shortly before a pull the pattern emits a one-time Warning cue
// (the lead time comes from the FightStyle asset; 0 disables warnings).

using UnityEngine;

namespace Tidebook
{
    public abstract class PullPatternBase : IPullPattern
    {
        protected readonly float strength;      // species fightStrength, 0..1
        readonly float warningLead;

        float timeToNext;
        bool warned;
        bool warnBeforeNext;

        protected PullPatternBase(float strength, float warningLead)
        {
            this.strength = strength;
            this.warningLead = warningLead;
        }

        public void Begin()
        {
            OnBegin();
            Schedule(0f);
        }

        public PullResult Tick(float dt, float progress)
        {
            PullResult result = new PullResult();
            timeToNext -= dt;

            if (timeToNext <= 0f)
            {
                result.impulse = NextImpulse(progress);
                result.cue = PullCue.Pull;
                Schedule(progress);
                return result;
            }

            if (warnBeforeNext && !warned && warningLead > 0f && timeToNext <= warningLead)
            {
                warned = true;
                result.cue = PullCue.Warning;
            }
            return result;
        }

        void Schedule(float progress)
        {
            timeToNext = Mathf.Max(0.05f, NextInterval(progress));
            warned = false;
            warnBeforeNext = ShouldWarnBeforeNext();   // evaluated after NextInterval so subclasses can use their counters
        }

        /// <summary>Reset any extra counters a pattern keeps.</summary>
        protected virtual void OnBegin() { }

        /// <summary>Seconds until the next pull.</summary>
        protected abstract float NextInterval(float progress);

        /// <summary>How much tension the pull that is firing NOW adds.</summary>
        protected abstract float NextImpulse(float progress);

        /// <summary>Should a Warning cue precede the pull that was just scheduled?</summary>
        protected virtual bool ShouldWarnBeforeNext() { return true; }
    }
}
