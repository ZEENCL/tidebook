namespace Tidebook
{
    /// <summary>
    /// The cast is over. Hides the bobber and publishes a CatchReport; listeners decide what that means
    /// (pay the player, release or free the fish, show a message...). Then returns to Aiming.
    /// </summary>
    public class ResolvedState : RodState
    {
        readonly IBobberView bobber;
        readonly ICatchPublisher catches;
        readonly CatchResult result;
        float timer = 1.5f;

        public ResolvedState(RodSession session, StateMachine machine, IRodStateFactory states,
                             IBobberView bobber, ICatchPublisher catches, CatchResult result)
            : base(session, machine, states)
        {
            this.bobber = bobber;
            this.catches = catches;
            this.result = result;
        }

        public override void Enter()
        {
            session.Phase = RodPhase.Resolved;
            bobber.Hide();
            catches.Publish(new CatchReport(result, session.Target, session.Bait, session.Water));
        }

        public override void Tick(float dt)
        {
            timer -= dt;
            if (timer <= 0f) machine.ChangeState(states.Aiming());
        }
    }
}
