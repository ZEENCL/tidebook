namespace Tidebook
{
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
