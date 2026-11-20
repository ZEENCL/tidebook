// CatchRewardListener.cs - pays the player when a fish is landed. Knows about the wallet; the rod does not.
// How much a catch pays is decided by CatchRewards; this class only applies it to the wallet (the Model).
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
            int coins = CatchRewards.CoinsFor(report);
            if (coins > 0) wallet.Add(coins);
        }
    }
}
