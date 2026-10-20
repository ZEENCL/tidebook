// ICatchListener.cs - reacts when a cast is resolved (caught, snapped, escaped, missed, retrieved).
// Examples: pay the wallet, release the fish, show a message, later: update the Fish Index.
namespace Tidebook
{
    public interface ICatchListener
    {
        void OnCatchResolved(CatchReport report);
    }
}
