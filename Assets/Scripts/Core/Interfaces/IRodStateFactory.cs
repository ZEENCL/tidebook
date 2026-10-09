// IRodStateFactory.cs - lets states ask for the next state without knowing how it is built.
namespace Tidebook
{
    public interface IRodStateFactory
    {
        IState Aiming();
        IState Charging();
        IState Settling();
        IState Waiting();
        IState BiteWindow();
        IState Reeling();
        IState Resolved(CatchResult result);
    }
}
