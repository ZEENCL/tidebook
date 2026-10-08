namespace Tidebook
{
    public interface IState
    {
        void Enter();
        void Tick(float dt);
        void Exit();
    }
}
