using UnityEngine;

namespace Tidebook
{
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
}
