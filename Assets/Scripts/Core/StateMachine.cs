// StateMachine.cs
// Tiny generic state machine shared by the rod and every fish.

namespace Tidebook
{
    public class StateMachine
    {
        public IState Current { get; private set; }

        public string CurrentName
        {
            get { return Current == null ? "-" : Current.GetType().Name; }
        }

        /// <summary>Exit the old state, enter the new one. Safe to call inside a state's Tick (then 'return').</summary>
        public void ChangeState(IState next)
        {
            if (Current != null) Current.Exit();
            Current = next;
            if (Current != null) Current.Enter();
        }

        public void Tick(float dt)
        {
            if (Current != null) Current.Tick(dt);
        }
    }
}
