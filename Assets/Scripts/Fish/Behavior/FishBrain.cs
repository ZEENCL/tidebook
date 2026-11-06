// FishBrain.cs
// Behaviour only: owns the fish's StateMachine and the data the states share.
// No MonoBehaviour, no pooling, no UI - so it can be reasoned about (and tested) on its own.

using UnityEngine;

namespace Tidebook
{
    public class FishBrain
    {
        public readonly StateMachine Machine = new StateMachine();
        public readonly FishMotor Motor;
        public readonly FishSpecies Species;

        public float MatchRatio { get; private set; }
        public Vector3 BaitPosition { get; private set; }
        public float BiteTimeLeft { get; set; }

        /// <summary>Raised when the fish enters the Biting state.</summary>
        public event System.Action Bitten;

        readonly IBiteStrategy strategy;
        readonly float detectionRadius;
        float ignoreUntil;

        public FishBrain(FishSpecies species, IBiteStrategy strategy, FishMotor motor, float detectionRadius)
        {
            Species = species;
            this.strategy = strategy;
            Motor = motor;
            this.detectionRadius = detectionRadius;
            Machine.ChangeState(new WanderState(this));
        }

        public string StateName { get { return Machine.CurrentName; } }
        public bool IsBiting { get { return Machine.Current is BitingState; } }

        public FishCueState CueState
        {
            get
            {
                IState s = Machine.Current;
                if (s is CuriousState) return FishCueState.Curious;
                if (s is InterestedState) return FishCueState.Interested;
                if (s is BitingState) return FishCueState.Biting;
                if (s is HookedState) return FishCueState.Hooked;
                return FishCueState.None;
            }
        }
        public bool IsBusy { get { return Machine.Current is BitingState || Machine.Current is HookedState; } }

        /// <summary>Bait landed at 'position'. Returns true if this fish noticed it and reacts.</summary>
        public bool TryNotice(Bait bait, Vector3 position, WaterContext ctx)
        {
            if (!(Machine.Current is WanderState)) return false;   // already busy
            if (Time.time < ignoreUntil) return false;              // recently spooked

            Vector3 flat = position - Motor.Position;
            flat.y = 0f;
            if (flat.magnitude > detectionRadius) return false;

            BiteEvaluation eval = strategy.Evaluate(Species.profile, bait, ctx);
            if (!eval.eligible) return false;                       // gate failed / nothing matched

            MatchRatio = eval.matchRatio;
            BaitPosition = position;
            Machine.ChangeState(new CuriousState(this));
            return true;
        }

        public bool Hook()
        {
            if (!IsBiting) return false;
            Machine.ChangeState(new HookedState(this));
            return true;
        }

        public void LoseInterest()
        {
            if (Machine.Current is HookedState) return;
            GoWander(3f);
        }

        public void Escape()
        {
            GoWander(8f);
        }

        public void GoWander(float cooldownSeconds)
        {
            ignoreUntil = Time.time + cooldownSeconds;
            MatchRatio = 0f;
            BiteTimeLeft = 0f;
            Machine.ChangeState(new WanderState(this));
        }

        public void RaiseBitten()
        {
            System.Action handler = Bitten;
            if (handler != null) handler();
        }
    }
}
