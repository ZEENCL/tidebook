// RodHud.cs
// Debug HUD only. Reads a read-only status, a read-only wallet view, and the bait provider.
// Replace with your real UI later; nothing else depends on this class.

using System.Text;
using UnityEngine;

namespace Tidebook
{
    public class RodHud : MonoBehaviour
    {
        IRodStatus status;
        IWalletView wallet;
        IBaitProvider baits;

        public void Bind(IRodStatus status, IWalletView wallet, IBaitProvider baits)
        {
            this.status = status;
            this.wallet = wallet;
            this.baits = baits;
        }

        void OnGUI()
        {
            if (status == null) return;

            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;
            GUI.Label(new Rect(cx - 6f, cy - 12f, 30f, 24f), "+");   // crosshair

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Rod state: " + status.Phase);
            sb.AppendLine(status.Message);
            if (baits != null) sb.AppendLine("Bait: " + baits.BuildBait());
            if (wallet != null) sb.AppendLine("Currency: " + wallet.Balance);

            if (status.Phase == RodPhase.Charging)
                sb.AppendLine("Power   " + Bar(status.Power));

            if (status.Phase == RodPhase.Reeling)
            {
                sb.AppendLine("Tension " + Bar(status.Tension) + "  (keep near the middle)");
                sb.AppendLine("Progress " + Bar(status.ReelProgress));
            }

            IFishInfo target = status.Target;
            if (status.Phase == RodPhase.BiteWindow && target != null)
                sb.AppendLine("!!! BITE !!!  time left: " + target.BiteTimeLeft.ToString("0.0"));

            if (target != null)
                sb.AppendLine("[debug] target: " + target.DisplayName + " | " + target.StateName +
                              " | match " + target.MatchRatio.ToString("0.00"));

            GUI.Label(new Rect(10f, 10f, 520f, 240f), sb.ToString());
        }

        static string Bar(float value)
        {
            int filled = Mathf.RoundToInt(Mathf.Clamp01(value) * 20f);
            return "[" + new string('#', filled) + new string('-', 20 - filled) + "]";
        }
    }
}
