namespace Tidebook
{
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
}
