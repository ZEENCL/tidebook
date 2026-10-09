// RodStates.cs
// State pattern for rod usage. Each state gets ONLY what it needs (narrow input slices,
// specific services) plus the session data, the state machine, and a factory for the next state.
//
//   Aiming --(press)--> Charging --(release)--> Settling --(timer)--> Waiting
//   Waiting --(a fish bites)--> BiteWindow --(click in time)--> Reeling
//   BiteWindow --(too slow)--> Resolved(Missed)
//   Reeling --> Resolved(Caught | Snapped | Escaped)
//   Settling / Waiting --(cancel)--> Resolved(Retrieved)
//   Resolved --(short delay)--> Aiming

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook
{
    public abstract class RodState : IState
    {
        protected readonly RodSession session;
        protected readonly StateMachine machine;
        protected readonly IRodStateFactory states;

        protected RodState(RodSession session, StateMachine machine, IRodStateFactory states)
        {
            this.session = session;
            this.machine = machine;
            this.states = states;
        }

        public virtual void Enter() { }
        public virtual void Tick(float dt) { }
        public virtual void Exit() { }
    }

    /// <summary>Ready to cast: crosshair is live, nothing in the water.</summary>
    public class AimingState : RodState
    {
        readonly ICastInput input;

        public AimingState(RodSession session, StateMachine machine, IRodStateFactory states, ICastInput input)
            : base(session, machine, states)
        {
            this.input = input;
        }

        public override void Enter()
        {
            session.Phase = RodPhase.Aiming;
            session.Power = 0f;
            if (string.IsNullOrEmpty(session.Message)) session.Message = "Hold LMB to charge a cast.";
        }

        public override void Tick(float dt)
        {
            if (input.ChargePressed) machine.ChangeState(states.Charging());
        }
    }

    /// <summary>Holding the button: power meter fills. Release to cast.</summary>
    public class ChargingState : RodState
    {
        readonly ICastInput input;
        readonly CastSettings cast;
        readonly ICastLandingResolver resolver;
        readonly IBaitProvider baits;
        readonly IBobberView bobber;

        public ChargingState(RodSession session, StateMachine machine, IRodStateFactory states,
                             ICastInput input, CastSettings cast, ICastLandingResolver resolver,
                             IBaitProvider baits, IBobberView bobber)
            : base(session, machine, states)
        {
            this.input = input;
            this.cast = cast;
            this.resolver = resolver;
            this.baits = baits;
            this.bobber = bobber;
        }

        public override void Enter()
        {
            session.Phase = RodPhase.Charging;
            session.Power = 0f;
            session.Target = null;
            session.Message = "Charging... release to cast (RMB cancels).";
        }

        public override void Tick(float dt)
        {
            session.Power = Mathf.Clamp01(session.Power + dt / cast.chargeSeconds);

            if (input.CancelPressed)
            {
                session.Message = "Cast cancelled.";
                machine.ChangeState(states.Aiming());
                return;
            }

            if (!input.ChargeReleased) return;

            Vector3 landing;
            IWaterZone zone;
            if (resolver.TryResolve(session.Power, out landing, out zone))
            {
                session.BaitPosition = landing;
                session.Water = zone.Context;
                session.Bait = baits.BuildBait();
                bobber.Show(landing);
                machine.ChangeState(states.Settling());
            }
            else
            {
                session.Message = "That cast didn't land in any fishing zone.";
                machine.ChangeState(states.Aiming());
            }
        }
    }

    /// <summary>Bait is sinking to its depth. Heavier bait takes longer to settle.</summary>
    public class SettlingState : RodState
    {
        readonly ICastInput input;
        float timer;

        public SettlingState(RodSession session, StateMachine machine, IRodStateFactory states, ICastInput input)
            : base(session, machine, states)
        {
            this.input = input;
        }

        public override void Enter()
        {
            session.Phase = RodPhase.Settling;
            session.Message = "Bait is settling...";
            switch (session.Bait.Buoyancy)
            {
                case Buoyancy.Floating: timer = 0.2f; break;
                case Buoyancy.Suspending: timer = 0.6f; break;
                default: timer = 1.2f; break;
            }
        }

        public override void Tick(float dt)
        {
            if (input.CancelPressed)
            {
                machine.ChangeState(states.Resolved(CatchResult.Retrieved));
                return;
            }

            timer -= dt;
            if (timer <= 0f) machine.ChangeState(states.Waiting());
        }
    }

    /// <summary>Bait is in place. Every fish nearby gets a chance to notice it; we wait for the first bite.</summary>
    public class WaitingState : RodState
    {
        readonly ICastInput input;
        readonly IFishRegistry registry;
        readonly List<IBaitReactive> noticed = new List<IBaitReactive>();

        public WaitingState(RodSession session, StateMachine machine, IRodStateFactory states,
                            ICastInput input, IFishRegistry registry)
            : base(session, machine, states)
        {
            this.input = input;
            this.registry = registry;
        }

        public override void Enter()
        {
            session.Phase = RodPhase.Waiting;
            session.Message = "Waiting for a bite... (RMB reels in)";

            IReadOnlyList<IBaitReactive> fish = registry.ActiveFish;
            for (int i = 0; i < fish.Count; i++)
            {
                if (fish[i].TryNotice(session.Bait, session.BaitPosition, session.Water))
                {
                    fish[i].Bit += OnFishBit;
                    noticed.Add(fish[i]);
                }
            }
        }

        public override void Tick(float dt)
        {
            if (input.CancelPressed) machine.ChangeState(states.Resolved(CatchResult.Retrieved));
        }

        void OnFishBit(IHookableFish fish)
        {
            // First fish to bite wins.
            session.Target = fish;
            machine.ChangeState(states.BiteWindow());
        }

        public override void Exit()
        {
            for (int i = 0; i < noticed.Count; i++)
            {
                noticed[i].Bit -= OnFishBit;
                if (!object.ReferenceEquals(noticed[i], session.Target)) noticed[i].LoseInterest();   // everyone else wanders off
            }
            noticed.Clear();
        }
    }

    /// <summary>A fish is biting. Click before its window closes to set the hook.</summary>
    public class BiteWindowState : RodState
    {
        readonly IHookInput input;
        IBiteTarget target;

        public BiteWindowState(RodSession session, StateMachine machine, IRodStateFactory states, IHookInput input)
            : base(session, machine, states)
        {
            this.input = input;
        }

        public override void Enter()
        {
            session.Phase = RodPhase.BiteWindow;
            session.Message = "BITE! Click now!";
            target = session.Target;
        }

        public override void Tick(float dt)
        {
            // The fish's window ran out (it left the Biting state on its own).
            if (!target.IsBiting)
            {
                session.Message = "Too slow, it got away.";
                machine.ChangeState(states.Resolved(CatchResult.Missed));
                return;
            }

            if (input.HookPressed && target.Hook()) machine.ChangeState(states.Reeling());
        }
    }

    /// <summary>
    /// Tension mini-game. Hold to reel (tension rises), release to relax (tension falls).
    /// The fish yanks now and then, spiking tension. Stay near the middle band to gain progress.
    /// Tension >= 1 snaps the line. Progress <= 0 lets the fish escape. Progress >= 1 lands it.
    /// </summary>
    public class ReelingState : RodState
    {
        readonly IReelInput input;
        readonly ReelSettings reel;
        readonly IWallet wallet;

        IFightTarget fight;
        IFishInfo info;
        float yankTimer;

        public ReelingState(RodSession session, StateMachine machine, IRodStateFactory states,
                            IReelInput input, ReelSettings reel, IWallet wallet)
            : base(session, machine, states)
        {
            this.input = input;
            this.reel = reel;
            this.wallet = wallet;
        }

        public override void Enter()
        {
            session.Phase = RodPhase.Reeling;
            fight = session.Target;
            info = session.Target;

            session.Tension = 0.5f;
            session.ReelProgress = 0.25f;
            session.BandHalfWidth = Mathf.Lerp(0.25f, 0.12f, fight.FightStrength);   // stronger fish = narrower safe band
            yankTimer = Random.Range(0.6f, 1.6f);
            session.Message = "Hooked! Hold LMB to reel, release to ease off.";
        }

        public override void Tick(float dt)
        {
            float strength = fight.FightStrength;

            // Player input.
            if (input.ReelHeld) session.Tension += reel.tensionRate * dt;
            else session.Tension -= reel.relaxRate * dt;

            // Fish yanks.
            yankTimer -= dt;
            if (yankTimer <= 0f)
            {
                session.Tension += Random.Range(0.10f, 0.20f) * (0.6f + strength);
                yankTimer = Random.Range(0.6f, Mathf.Lerp(1.6f, 0.9f, strength));
            }

            if (session.Tension >= 1f)
            {
                fight.Escape();
                session.Message = "The line snapped!";
                machine.ChangeState(states.Resolved(CatchResult.Snapped));
                return;
            }
            session.Tension = Mathf.Max(0f, session.Tension);

            // Progress only moves forward while tension stays in the safe band.
            bool inBand = Mathf.Abs(session.Tension - 0.5f) <= session.BandHalfWidth;
            if (inBand) session.ReelProgress += reel.progressRate * (1f - 0.5f * strength) * dt;
            else session.ReelProgress -= reel.slipRate * dt;

            if (session.ReelProgress >= 1f)
            {
                wallet.Add(fight.Reward);
                session.Message = "Caught a " + info.DisplayName + "! +" + fight.Reward;
                fight.Catch();
                machine.ChangeState(states.Resolved(CatchResult.Caught));
                return;
            }

            if (session.ReelProgress <= 0f)
            {
                fight.Escape();
                session.Message = "It slipped off the hook...";
                machine.ChangeState(states.Resolved(CatchResult.Escaped));
            }
        }
    }

    /// <summary>Short cooldown that removes the bobber, then returns to Aiming.</summary>
    public class ResolvedState : RodState
    {
        readonly IBobberView bobber;
        readonly CatchResult result;
        float timer = 1.5f;

        public ResolvedState(RodSession session, StateMachine machine, IRodStateFactory states,
                             IBobberView bobber, CatchResult result)
            : base(session, machine, states)
        {
            this.bobber = bobber;
            this.result = result;
        }

        public override void Enter()
        {
            session.Phase = RodPhase.Resolved;
            bobber.Hide();
            if (result == CatchResult.Retrieved) session.Message = "Reeled in the line.";
        }

        public override void Tick(float dt)
        {
            timer -= dt;
            if (timer <= 0f) machine.ChangeState(states.Aiming());
        }
    }
}
