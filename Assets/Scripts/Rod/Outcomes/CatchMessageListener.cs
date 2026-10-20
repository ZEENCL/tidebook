// CatchMessageListener.cs - turns an outcome into the on-screen message.
namespace Tidebook
{
    public class CatchMessageListener : ICatchListener
    {
        readonly RodSession session;

        public CatchMessageListener(RodSession session)
        {
            this.session = session;
        }

        public void OnCatchResolved(CatchReport report)
        {
            switch (report.Result)
            {
                case CatchResult.Caught:
                    string name = report.Info != null ? report.Info.DisplayName : "fish";
                    int reward = report.Info != null ? report.Info.Species.currencyValue : 0;
                    session.Message = "Caught a " + name + "! +" + reward;
                    break;
                case CatchResult.Snapped:
                    session.Message = "The line snapped!";
                    break;
                case CatchResult.Escaped:
                    session.Message = "It slipped off the hook...";
                    break;
                case CatchResult.Missed:
                    session.Message = "Too slow, it got away.";
                    break;
                case CatchResult.Retrieved:
                    session.Message = "Reeled in the line.";
                    break;
            }
        }
    }
}
