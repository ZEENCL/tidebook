// IState.cs - one state in a StateMachine (rod states and fish states).
namespace Tidebook
{
    public interface IState
    {
        void Enter();
        void Tick(float dt);
        void Exit();
    }
}
