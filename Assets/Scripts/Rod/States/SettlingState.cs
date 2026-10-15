namespace Tidebook
{
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
}
