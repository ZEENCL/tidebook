// CatchRewards.cs
// The reward RULE, in one place (MVVM: this belongs to the Model, not to any screen).
// The wallet payout, the on-screen message and the catch card all ask here instead of each
// reading species.currencyValue themselves, so changing how rewards work means editing one method.

namespace Tidebook
{
    public static class CatchRewards
    {
        /// <summary>Coins a resolved cast pays out. Only a landed fish pays; everything else is 0.</summary>
        public static int CoinsFor(CatchReport report)
        {
            if (report.Result != CatchResult.Caught || report.Info == null || report.Info.Species == null) return 0;
            return report.Info.Species.currencyValue;
        }
    }
}
