// ICatchPublisher.cs - what the rod needs to announce an outcome.
namespace Tidebook
{
    public interface ICatchPublisher
    {
        void Publish(CatchReport report);
    }
}
