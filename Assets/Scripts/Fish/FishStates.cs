// FishStates.cs
// State pattern for a single fish:
//   Wander -> Curious -> Interested -> Biting -> Hooked
// The injected strategy only produces MatchRatio (0..1); these states turn it into timing:
// good match = quick, committed, forgiving bite window; poor match = slow, flaky, tight window.

using UnityEngine;

namespace Tidebook
{
    public abstract class FishState : IState
    {
        protected readonly FishBrain brain;
        protected FishState(FishBrain brain) { this.brain = brain; }
        public virtual void Enter() { }
        public virtual void Tick(float dt) { }
        public virtual void Exit() { }
    }

    /// <summary>Idle swimming around the zone, unaware of any bait.</summary>
    public class WanderState : FishState
    {
        Vector3 target;
        float pause;

        public WanderState(FishBrain brain) : base(brain) { }

        public override void Enter()
        {
            target = brain.Motor.RandomPointInArea();
            pause = 0f;
        }

        public override void Tick(float dt)
        {
            if (pause > 0f)
            {
                pause -= dt;
                return;
            }

            if (brain.Motor.MoveToward(target, brain.Species.swimSpeed, dt))
            {
                pause = Random.Range(0.5f, 2.5f);
                target = brain.Motor.RandomPointInArea();
            }
        }
    }

    /// <summary>Noticed the bait. Better match = shorter hesitation.</summary>
    public class CuriousState : FishState
    {
        float timer;

        public CuriousState(FishBrain brain) : base(brain) { }

        public override void Enter()
        {
            timer = Mathf.Lerp(4f, 0.5f, brain.MatchRatio);
        }

        public override void Tick(float dt)
        {
            timer -= dt;
            if (timer <= 0f) brain.Machine.ChangeState(new InterestedState(brain));
        }
    }

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

    /// <summary>The bite window. Better match = longer (more forgiving) window.</summary>
    public class BitingState : FishState
    {
        float timeLeft;

        public BitingState(FishBrain brain) : base(brain) { }

        public override void Enter()
        {
            timeLeft = Mathf.Lerp(0.5f, 2f, brain.MatchRatio);
            brain.BiteTimeLeft = timeLeft;
            brain.RaiseBitten();   // tells whoever is listening (the rod) "I'm biting!"
        }

        public override void Tick(float dt)
        {
            timeLeft -= dt;
            brain.BiteTimeLeft = Mathf.Max(0f, timeLeft);
            if (timeLeft <= 0f) brain.GoWander(8f);   // player missed it
        }
    }

    /// <summary>On the line. The rod's ReelingState runs the fight; the fish just holds here.</summary>
    public class HookedState : FishState
    {
        public HookedState(FishBrain brain) : base(brain) { }
    }
}
