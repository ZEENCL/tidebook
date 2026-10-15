using UnityEngine;

namespace Tidebook
{
    /// <summary>Swims to the bait and nibbles. Bite chance and patience scale with MatchRatio.</summary>
    public class InterestedState : FishState
    {
        float dwell;
        float giveUpAfter;
        float biteRatePerSecond;

        public InterestedState(FishBrain brain) : base(brain) { }

        public override void Enter()
        {
            dwell = 0f;
            giveUpAfter = Mathf.Lerp(2.5f, 12f, brain.MatchRatio);      // poor match: loses interest sooner
            biteRatePerSecond = Mathf.Lerp(0.05f, 1.5f, brain.MatchRatio);
        }

        public override void Tick(float dt)
        {
            bool arrived = brain.Motor.MoveToward(brain.BaitPosition, brain.Species.swimSpeed, dt);
            if (!arrived) return;

            dwell += dt;

            if (Random.value < biteRatePerSecond * dt)
            {
                brain.Machine.ChangeState(new BitingState(brain));
                return;
            }

            if (dwell >= giveUpAfter) brain.GoWander(5f);
        }
    }
}
