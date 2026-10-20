// ICatchEvents.cs - what other systems need to listen for outcomes (Fish Index, achievements, audio...).
namespace Tidebook
{
    public interface ICatchEvents
    {
        void Subscribe(ICatchListener listener);
        void Unsubscribe(ICatchListener listener);
    }
}
