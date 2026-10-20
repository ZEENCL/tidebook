namespace Tidebook
{
    /// <summary>
    /// Runs the reeling phase and nothing else. The reel math lives in TensionBandReel, the fish's pull rhythm in
    /// an IPullPattern, and the consequences (wallet, fish release, messages) in catch listeners.
    /// This state only wires them together each frame and decides which state comes next.
    /// </summary>
    public class ReelingState : RodState
    {
        readonly IReelInput input;
        readonly TensionBandReel reel;

        IPullPattern pull;

        public ReelingState(RodSession session, StateMachine machine, IRodStateFactory states,
                            IReelInput input, ReelSettings settings)
            : base(session, machine, states)
        {
            this.input = input;
            reel = new TensionBandReel(settings);
        }

        public override void Enter()
        {
            session.Phase = RodPhase.Reeling;
            session.Message = "Hooked! Hold LMB to reel, release to ease off.";

            IFightTarget fight = session.Target;
            pull = fight.CreatePullPattern();
            pull.Begin();
            reel.Begin(fight.FightStrength);
            PublishMeters();
        }

        public override void Tick(float dt)
        {
            PullResult pulled = pull.Tick(dt, reel.Progress);
            if (pulled.cue == PullCue.Warning) session.RaiseCue(RodCue.PullWarning);
            else if (pulled.cue == PullCue.Pull) session.RaiseCue(RodCue.Pulled);

            ReelStatus status = reel.Tick(dt, input.ReelHeld, pulled.impulse);
            PublishMeters();

            switch (status)
            {
                case ReelStatus.Landed:
                    machine.ChangeState(states.Resolved(CatchResult.Caught));
                    break;
                case ReelStatus.LineSnapped:
                    machine.ChangeState(states.Resolved(CatchResult.Snapped));
                    break;
                case ReelStatus.SlippedOff:
                    machine.ChangeState(states.Resolved(CatchResult.Escaped));
                    break;
            }
        }

        void PublishMeters()
        {
            session.Meters.Set("Tension", reel.Tension, reel.BandMin, reel.BandMax);
            session.Meters.Set("Progress", reel.Progress);
        }
    }
}
