// CatchRewardListener.cs - pays the player when a fish is landed. Knows about the wallet; the rod does not.
namespace Tidebook
{
    public class CatchRewardListener : ICatchListener
    {
        readonly IWallet wallet;

        public CatchRewardListener(IWallet wallet)
        {
            this.wallet = wallet;
        }

        public void OnCatchResolved(CatchReport report)
        {
            if (report.Result != CatchResult.Caught || report.Info == null) return;
            wallet.Add(report.Info.Species.currencyValue);
        }
    }
}
