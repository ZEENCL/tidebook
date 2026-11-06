namespace Tidebook
{
    /// <summary>Ready to cast: crosshair is live, nothing in the water.</summary>
    public class AimingState : RodState
    {
        readonly ICastInput input;
        readonly ICastLandingResolver resolver;

        public AimingState(RodSession session, StateMachine machine, IRodStateFactory states,
                           ICastInput input, ICastLandingResolver resolver)
            : base(session, machine, states)
        {
            this.input = input;
            this.resolver = resolver;
        }

        public override void Enter()
        {
            session.Phase = RodPhase.Aiming;
            if (string.IsNullOrEmpty(session.Message)) session.Message = "Hold LMB to charge a cast.";
        }

        public override void Tick(float dt)
        {
            session.SetPreview(resolver.Preview(1f));   // a full-power cast shows how far the rod can reach

            if (input.ChargePressed) machine.ChangeState(states.Charging());
        }
    }
}
