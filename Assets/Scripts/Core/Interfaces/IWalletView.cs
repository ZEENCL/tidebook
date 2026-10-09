// IWalletView.cs - read-only view of the player's currency (for the HUD).
namespace Tidebook
{
    public interface IWalletView
    {
        int Balance { get; }
    }
}
