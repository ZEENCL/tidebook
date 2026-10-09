// IWallet.cs - currency the rod can add to when a fish is landed.
namespace Tidebook
{
    public interface IWallet : IWalletView
    {
        void Add(int amount);
    }
}
