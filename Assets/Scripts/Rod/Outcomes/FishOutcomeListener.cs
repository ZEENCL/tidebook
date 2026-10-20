// FishOutcomeListener.cs
// Tells the fish what happened to it: landed fish go back to the pool, lost fish swim off.
// (Missed / Retrieved need nothing: the fish already handled those on its own.)
namespace Tidebook
{
    public class FishOutcomeListener : ICatchListener
    {
        public void OnCatchResolved(CatchReport report)
        {
            if (report.Outcome == null) return;

            switch (report.Result)
            {
                case CatchResult.Caught:
                    report.Outcome.Catch();
                    break;
                case CatchResult.Snapped:
                case CatchResult.Escaped:
                    report.Outcome.Escape();
                    break;
            }
        }
    }
}
