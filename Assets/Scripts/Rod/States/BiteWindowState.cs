using UnityEngine;

namespace Tidebook
{
    /// <summary>A fish is biting. Click before its window closes to set the hook.</summary>
    public class BiteWindowState : RodState
    {
        const string MeterLabel = "Bite window";

        readonly IHookInput input;
        IBiteTarget target;
        IFishInfo info;
        float window;

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
            info = session.Target;
            window = Mathf.Max(0.01f, info.BiteTimeLeft);   // the fish sets its full window before announcing the bite

            session.Meters.Set(MeterLabel, 1f);
            session.RaiseCue(RodCue.BiteStarted);
        }

        public override void Tick(float dt)
        {
            // The fish's window ran out (it left the Biting state on its own).
            if (!target.IsBiting)
            {
                machine.ChangeState(states.Resolved(CatchResult.Missed));
                return;
            }

            session.Meters.Set(MeterLabel, info.BiteTimeLeft / window);

            if (input.HookPressed && target.Hook()) machine.ChangeState(states.Reeling());
        }
    }
}
