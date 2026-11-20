// WalletViewModel.cs
// MVVM for the coin counter.
//   Model      : PlayerWallet (the balance; raises BalanceChanged)
//   ViewModel  : this class. Exposes what the screen needs and nothing about how it is drawn:
//                  Coins        the balance to show (Observable, so the View is told when it changes)
//                  CoinsEarned  raised with the amount whenever coins are gained (drives the "+12" pop)
//   View       : StatusView, which binds to both and draws them
// The View no longer polls the wallet every frame or works out "did the balance go up?" itself.

namespace Tidebook.Ui
{
    public class WalletViewModel : System.IDisposable
    {
        readonly IWalletView wallet;

        public readonly Observable<int> Coins;

        /// <summary>Raised with the amount gained. Not raised when coins are spent or taken away.</summary>
        public event System.Action<int> CoinsEarned;

        public WalletViewModel(IWalletView wallet)
        {
            this.wallet = wallet;
            Coins = new Observable<int>(wallet.Balance);
            wallet.BalanceChanged += OnBalanceChanged;
        }

        void OnBalanceChanged(int balance)
        {
            int gained = balance - Coins.Value;
            Coins.Value = balance;

            if (gained > 0 && CoinsEarned != null) CoinsEarned(gained);
        }

        public void Dispose()
        {
            wallet.BalanceChanged -= OnBalanceChanged;
        }
    }
}
