// IWalletView.cs - read-only view of the player's currency (for the HUD).
namespace Tidebook
{
    public interface IWalletView
    {
        int Balance { get; }

        /// <summary>Raised after every change, with the new balance. ViewModels listen to this instead of polling Balance.</summary>
        event System.Action<int> BalanceChanged;
    }
}
