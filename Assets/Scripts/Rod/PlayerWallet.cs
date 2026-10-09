// PlayerWallet.cs
// The player's currency. Kept separate so shops / saving can build on it later.

namespace Tidebook
{
    public class PlayerWallet : IWallet
    {
        public int Balance { get; private set; }

        public void Add(int amount)
        {
            Balance += amount;
        }
    }
}
