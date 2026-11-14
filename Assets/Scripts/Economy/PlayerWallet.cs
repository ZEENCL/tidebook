// PlayerWallet.cs
// The player's currency. Kept separate so shops / saving can build on it later.
// This is the MODEL for coins: it holds the balance and announces changes (BalanceChanged).

namespace Tidebook
{
    public class PlayerWallet : IWallet
    {
        public int Balance { get; private set; }

        public event System.Action<int> BalanceChanged;

        public void Add(int amount)
        {
            if (amount == 0) return;

            Balance += amount;

            System.Action<int> handler = BalanceChanged;
            if (handler != null) handler(Balance);
        }
    }
}
