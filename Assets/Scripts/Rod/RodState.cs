// Shared base class for the rod state pattern.

namespace Tidebook
{
    public abstract class RodState : IState
    {
        protected readonly RodSession session;
        protected readonly StateMachine machine;
        protected readonly IRodStateFactory states;

        protected RodState(RodSession session, StateMachine machine, IRodStateFactory states)
        {
            this.session = session;
            this.machine = machine;
            this.states = states;
        }

        public virtual void Enter() { }
        public virtual void Tick(float dt) { }
        public virtual void Exit() { }
    }
}
