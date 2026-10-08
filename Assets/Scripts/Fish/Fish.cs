using System.Collections.Generic;
using UnityEngine;

namespace Tidebook
{
    public class Fish : MonoBehaviour
    {
        /// <summary>All currently active (spawned, not pooled) fish. The rod scans this when bait lands.</summary>
        public static readonly List<Fish> Active = new List<Fish>();

        [Header("Tuning (prototype)")]
        public float detectionRadius = 10f;   // horizontal distance at which a fish can notice bait
        public float reachDistance = 0.6f;    // how close counts as "at the bait"

        public FishSpecies Species { get; private set; }
        public float MatchRatio { get; private set; }
        public float BiteTimeLeft { get; private set; }

        public string StateName { get { return sm == null ? "-" : sm.CurrentName; } }
        public bool IsBiting { get { return sm != null && sm.Current is BitingState; } }
        public bool IsBusy { get { return sm != null && (sm.Current is BitingState || sm.Current is HookedState); } }

        /// <summary>Raised the moment this fish enters the Biting state.</summary>
        public event System.Action<Fish> Bit;

        StateMachine sm;
        IBiteStrategy strategy;
        Bounds area;
        float layerY;
        System.Action<Fish> release;

        Vector3 baitPos;
        float ignoreUntil;

        // ------------------------------------------------------------------
        // Lifecycle (called by FishFactory)
        // ------------------------------------------------------------------
        void OnEnable() { Active.Add(this); }
        void OnDisable() { Active.Remove(this); }

        public void Init(FishSpecies species, IBiteStrategy biteStrategy, Bounds wanderArea,
                         float swimLayerY, System.Action<Fish> releaseCallback)
        {
            Species = species;
            strategy = biteStrategy;
            area = wanderArea;
            layerY = swimLayerY;
            release = releaseCallback;

            MatchRatio = 0f;
            BiteTimeLeft = 0f;
            ignoreUntil = 0f;

            sm = new StateMachine();
            sm.ChangeState(new WanderState(this));
        }

        void Update()
        {
            if (sm != null) sm.Tick(Time.deltaTime);
        }

        // ------------------------------------------------------------------
        // Public API used by the rod
        // ------------------------------------------------------------------

        /// <summary>Bait landed at 'position'. Returns true if this fish noticed it and reacts.</summary>
        public bool TryNotice(Bait bait, Vector3 position, WaterContext ctx)
        {
            if (sm == null || !(sm.Current is WanderState)) return false;   // already busy
            if (Time.time < ignoreUntil) return false;                       // recently spooked

            Vector3 flat = position - transform.position;
            flat.y = 0f;
            if (flat.magnitude > detectionRadius) return false;

            BiteEvaluation eval = strategy.Evaluate(Species.profile, bait, ctx);
            if (!eval.eligible) return false;                                // gate failed / no match at all

            MatchRatio = eval.matchRatio;
            baitPos = position;
            sm.ChangeState(new CuriousState(this));
            return true;
        }

        /// <summary>Player reacted inside the bite window. Returns true if the fish is now hooked.</summary>
        public bool Hook()
        {
            if (!IsBiting) return false;
            sm.ChangeState(new HookedState(this));
            return true;
        }

        /// <summary>Bait was removed / another fish took it. Hooked fish are unaffected.</summary>
        public void LoseInterest()
        {
            if (sm == null || sm.Current is HookedState) return;
            ToWander(3f);
        }

        /// <summary>Fish got away (line snapped / escaped while reeling).</summary>
        public void Escape()
        {
            ToWander(8f);
        }

        /// <summary>Fish was caught: send it back to the pool (spawner will refill the population).</summary>
        public void Catch()
        {
            if (release != null) release(this);
            else Destroy(gameObject);
        }

        // ------------------------------------------------------------------
        // Internals
        // ------------------------------------------------------------------
        void ToWander(float cooldownSeconds)
        {
            ignoreUntil = Time.time + cooldownSeconds;
            MatchRatio = 0f;
            BiteTimeLeft = 0f;
            sm.ChangeState(new WanderState(this));
        }

        /// <summary>Moves horizontally toward target at this fish's swim layer. Returns true when arrived.</summary>
        bool MoveToward(Vector3 target, float speed, float dt)
        {
            target.y = layerY;
            Vector3 toTarget = target - transform.position;
            if (toTarget.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(toTarget);

            transform.position = Vector3.MoveTowards(transform.position, target, speed * dt);

            Vector3 remaining = target - transform.position;
            remaining.y = 0f;
            return remaining.magnitude <= reachDistance;
        }

        void RaiseBit()
        {
            System.Action<Fish> handler = Bit;
            if (handler != null) handler(this);
        }

        Vector3 RandomPointInArea()
        {
            return new Vector3(
                Random.Range(area.min.x, area.max.x),
                layerY,
                Random.Range(area.min.z, area.max.z));
        }

        // ------------------------------------------------------------------
        // States (nested so they can use the fish's private helpers)
        // ------------------------------------------------------------------
        abstract class FishState : IState
        {
            protected readonly Fish f;
            protected FishState(Fish fish) { f = fish; }
            public virtual void Enter() { }
            public virtual void Tick(float dt) { }
            public virtual void Exit() { }
        }

        // Idle swimming around the zone, unaware of any bait.
        class WanderState : FishState
        {
            Vector3 target;
            float pause;

            public WanderState(Fish fish) : base(fish) { }

            public override void Enter()
            {
                target = f.RandomPointInArea();
                pause = 0f;
            }

            public override void Tick(float dt)
            {
                if (pause > 0f)
                {
                    pause -= dt;
                    return;
                }

                if (f.MoveToward(target, f.Species.swimSpeed, dt))
                {
                    pause = Random.Range(0.5f, 2.5f);
                    target = f.RandomPointInArea();
                }
            }
        }

        // Noticed the bait. Better match = shorter hesitation.
        class CuriousState : FishState
        {
            float timer;

            public CuriousState(Fish fish) : base(fish) { }

            public override void Enter()
            {
                timer = Mathf.Lerp(4f, 0.5f, f.MatchRatio);
            }

            public override void Tick(float dt)
            {
                timer -= dt;
                if (timer <= 0f) f.sm.ChangeState(new InterestedState(f));
            }
        }

        // Swims to the bait and nibbles around it. Bite chance and patience scale with MatchRatio.
        class InterestedState : FishState
        {
            float dwell;
            float giveUpAfter;
            float biteRatePerSecond;

            public InterestedState(Fish fish) : base(fish) { }

            public override void Enter()
            {
                dwell = 0f;
                giveUpAfter = Mathf.Lerp(2.5f, 12f, f.MatchRatio);      // poor match: loses interest sooner
                biteRatePerSecond = Mathf.Lerp(0.05f, 1.5f, f.MatchRatio);
            }

            public override void Tick(float dt)
            {
                bool arrived = f.MoveToward(f.baitPos, f.Species.swimSpeed, dt);
                if (!arrived) return;

                dwell += dt;

                if (Random.value < biteRatePerSecond * dt)
                {
                    f.sm.ChangeState(new BitingState(f));
                    return;
                }

                if (dwell >= giveUpAfter) f.ToWander(5f);
            }
        }

        // The bite window. Better match = longer (more forgiving) window.
        class BitingState : FishState
        {
            float timeLeft;

            public BitingState(Fish fish) : base(fish) { }

            public override void Enter()
            {
                timeLeft = Mathf.Lerp(0.5f, 2f, f.MatchRatio);
                f.BiteTimeLeft = timeLeft;

                f.RaiseBit();   // tells the rod "I'm biting!"
            }

            public override void Tick(float dt)
            {
                timeLeft -= dt;
                f.BiteTimeLeft = Mathf.Max(0f, timeLeft);
                if (timeLeft <= 0f) f.ToWander(8f);   // player missed it
            }
        }

        // On the line. The rod's ReelingState runs the fight; the fish just holds position here.
        class HookedState : FishState
        {
            public HookedState(Fish fish) : base(fish) { }
        }
    }
}
