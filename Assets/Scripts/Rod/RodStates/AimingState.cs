namespace Tidebook
{
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
}
