// RodStateFactory.cs
// Builds each rod state with exactly the services it needs. This is the one place that knows
// which state depends on what. To add a state: write the class, add one method here and to IRodStateFactory.

namespace Tidebook
{
    public class RodStateFactory : IRodStateFactory
    {
        readonly StateMachine machine;
        readonly RodSession session;
        readonly IRodInput input;
        readonly CastSettings cast;
        readonly ReelSettings reel;
        readonly ICastLandingResolver resolver;
        readonly IBaitProvider baits;
        readonly IBobberView bobber;
        readonly IFishRegistry registry;
        readonly ICatchPublisher catches;

        public RodStateFactory(StateMachine machine, RodSession session, IRodInput input,
                               CastSettings cast, ReelSettings reel, ICastLandingResolver resolver,
                               IBaitProvider baits, IBobberView bobber, IFishRegistry registry,
                               ICatchPublisher catches)
        {
            this.machine = machine;
            this.session = session;
            this.input = input;
            this.cast = cast;
            this.reel = reel;
            this.resolver = resolver;
            this.baits = baits;
            this.bobber = bobber;
            this.registry = registry;
            this.catches = catches;
        }

        public IState Aiming() { return new AimingState(session, machine, this, input); }

        public IState Charging() { return new ChargingState(session, machine, this, input, cast, resolver, baits, bobber); }

        public IState Settling() { return new SettlingState(session, machine, this, input); }

        public IState Waiting() { return new WaitingState(session, machine, this, input, registry); }

        public IState BiteWindow() { return new BiteWindowState(session, machine, this, input); }

        public IState Reeling() { return new ReelingState(session, machine, this, input, reel); }

        public IState Resolved(CatchResult result) { return new ResolvedState(session, machine, this, bobber, catches, result); }
    }
}
