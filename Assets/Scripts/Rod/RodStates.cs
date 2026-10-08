using System.Collections.Generic;
using UnityEngine;

namespace Tidebook
{
    public abstract class RodState : IState
    {
        protected readonly RodController rod;
        protected RodState(RodController controller) { rod = controller; }
        public virtual void Enter() { }
        public virtual void Tick(float dt) { }
        public virtual void Exit() { }
    }

    public enum CatchResult { Caught, Snapped, Escaped, Missed, Retrieved }

    // Ready to cast: crosshair is live, nothing in the water.
    public class AimingState : RodState
    {
        public AimingState(RodController c) : base(c) { }

        public override void Enter()
        {
            rod.Power = 0f;
            if (string.IsNullOrEmpty(rod.Message)) rod.Message = "Hold LMB to charge a cast.";
        }

        public override void Tick(float dt)
        {
            if (rod.ActionPressed) rod.Machine.ChangeState(new ChargingState(rod));
        }
    }

    // Holding the button: power meter fills. Release to cast.
    public class ChargingState : RodState
    {
        public ChargingState(RodController c) : base(c) { }

        public override void Enter()
        {
            rod.Power = 0f;
            rod.Target = null;
            rod.Message = "Charging... release to cast (RMB cancels).";
        }

        public override void Tick(float dt)
        {
            rod.Power = Mathf.Clamp01(rod.Power + dt / rod.chargeSeconds);

            if (rod.CancelPressed)
            {
                rod.Message = "Cast cancelled.";
                rod.Machine.ChangeState(new AimingState(rod));
                return;
            }

            if (rod.ActionReleased)
            {
                Vector3 landing;
                IWaterZone zone;
                if (rod.TryResolveLanding(rod.Power, out landing, out zone))
                {
                    rod.BaitPosition = landing;
                    rod.Context = zone.Context;
                    rod.CurrentBait = rod.BuildBait();
                    rod.SpawnBobber(landing);
                    rod.Machine.ChangeState(new SettlingState(rod));
                }
                else
                {
                    rod.Message = "That cast didn't land in any fishing zone.";
                    rod.Machine.ChangeState(new AimingState(rod));
                }
            }
        }
    }

    // Bait is sinking to its depth. Heavier bait takes longer to settle.
    public class SettlingState : RodState
    {
        float timer;

        public SettlingState(RodController c) : base(c) { }

        public override void Enter()
        {
            rod.Message = "Bait is settling...";
            switch (rod.CurrentBait.Buoyancy)
            {
                case Buoyancy.Floating: timer = 0.2f; break;
                case Buoyancy.Suspending: timer = 0.6f; break;
                default: timer = 1.2f; break;
            }
        }

        public override void Tick(float dt)
        {
            if (rod.CancelPressed)
            {
                rod.Machine.ChangeState(new ResolvedState(rod, CatchResult.Retrieved));
                return;
            }

            timer -= dt;
            if (timer <= 0f) rod.Machine.ChangeState(new WaitingState(rod));
        }
    }

    // Bait is in place. Every fish nearby gets a chance to notice it; we wait for the first bite.
    public class WaitingState : RodState
    {
        readonly List<Fish> noticed = new List<Fish>();

        public WaitingState(RodController c) : base(c) { }

        public override void Enter()
        {
            rod.Message = "Waiting for a bite... (RMB reels in)";

            foreach (Fish fish in rod.Registry.ActiveFish)
            {
                if (fish.TryNotice(rod.CurrentBait, rod.BaitPosition, rod.Context))
                {
                    fish.Bit += OnFishBit;
                    noticed.Add(fish);
                }
            }
        }

        public override void Tick(float dt)
        {
            if (rod.CancelPressed)
                rod.Machine.ChangeState(new ResolvedState(rod, CatchResult.Retrieved));
        }

        void OnFishBit(Fish fish)
        {
            // First fish to bite wins.
            rod.Target = fish;
            rod.Machine.ChangeState(new BiteWindowState(rod));
        }

        public override void Exit()
        {
            for (int i = 0; i < noticed.Count; i++)
            {
                noticed[i].Bit -= OnFishBit;
                if (noticed[i] != rod.Target) noticed[i].LoseInterest();   // everyone else wanders off
            }
            noticed.Clear();
        }
    }

    // A fish is biting. Click before its window closes to set the hook.
    public class BiteWindowState : RodState
    {
        public BiteWindowState(RodController c) : base(c) { }

        public override void Enter()
        {
            rod.Message = "BITE! Click now!";
        }

        public override void Tick(float dt)
        {
            // Fish window ran out (it left the Biting state on its own).
            if (!rod.Target.IsBiting)
            {
                rod.Message = "Too slow, it got away.";
                rod.Machine.ChangeState(new ResolvedState(rod, CatchResult.Missed));
                return;
            }

            if (rod.ActionPressed && rod.Target.Hook())
                rod.Machine.ChangeState(new ReelingState(rod));
        }
    }

    // Tension mini-game. Hold to reel (tension rises), release to relax (tension falls).
    // The fish yanks now and then, spiking tension. Stay near the middle band to gain progress.
    // Tension >= 1 snaps the line. Progress <= 0 lets the fish escape. Progress >= 1 lands it.
    public class ReelingState : RodState
    {
        float yankTimer;

        public ReelingState(RodController c) : base(c) { }

        public override void Enter()
        {
            float fight = rod.Target.Species.fightStrength;
            rod.Tension = 0.5f;
            rod.ReelProgress = 0.25f;
            rod.BandHalfWidth = Mathf.Lerp(0.25f, 0.12f, fight);   // stronger fish = narrower safe band
            yankTimer = Random.Range(0.6f, 1.6f);
            rod.Message = "Hooked! Hold LMB to reel, release to ease off.";
        }

        public override void Tick(float dt)
        {
            float fight = rod.Target.Species.fightStrength;

            // Player input.
            if (rod.ReelHeld) rod.Tension += rod.reelTensionRate * dt;
            else rod.Tension -= rod.relaxTensionRate * dt;

            // Fish yanks.
            yankTimer -= dt;
            if (yankTimer <= 0f)
            {
                rod.Tension += Random.Range(0.10f, 0.20f) * (0.6f + fight);
                yankTimer = Random.Range(0.6f, Mathf.Lerp(1.6f, 0.9f, fight));
            }

            if (rod.Tension >= 1f)
            {
                rod.Target.Escape();
                rod.Message = "The line snapped!";
                rod.Machine.ChangeState(new ResolvedState(rod, CatchResult.Snapped));
                return;
            }
            rod.Tension = Mathf.Max(0f, rod.Tension);

            // Progress only moves forward while tension stays in the safe band.
            bool inBand = Mathf.Abs(rod.Tension - 0.5f) <= rod.BandHalfWidth;
            if (inBand) rod.ReelProgress += rod.progressRate * (1f - 0.5f * fight) * dt;
            else rod.ReelProgress -= rod.slipRate * dt;

            if (rod.ReelProgress >= 1f)
            {
                rod.Currency += rod.Target.Species.currencyValue;
                rod.Message = "Caught a " + rod.Target.Species.displayName + "! +" + rod.Target.Species.currencyValue;
                rod.Target.Catch();
                rod.Machine.ChangeState(new ResolvedState(rod, CatchResult.Caught));
                return;
            }

            if (rod.ReelProgress <= 0f)
            {
                rod.Target.Escape();
                rod.Message = "It slipped off the hook...";
                rod.Machine.ChangeState(new ResolvedState(rod, CatchResult.Escaped));
            }
        }
    }

    // Short cooldown that cleans up the bobber, then returns to Aiming.
    public class ResolvedState : RodState
    {
        readonly CatchResult result;
        float timer = 1.5f;

        public ResolvedState(RodController c, CatchResult r) : base(c) { result = r; }

        public override void Enter()
        {
            rod.ClearBobber();
            if (result == CatchResult.Retrieved) rod.Message = "Reeled in the line.";
        }

        public override void Tick(float dt)
        {
            timer -= dt;
            if (timer <= 0f) rod.Machine.ChangeState(new AimingState(rod));
        }
    }
}
