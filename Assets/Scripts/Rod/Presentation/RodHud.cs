// RodHud.cs
// Debug HUD only. Draws whatever meters the rod publishes (no mechanic-specific code), shows the
// latest cue, and reads a read-only wallet view and the bait provider.
// Replace with your real UI later; nothing else depends on this class.

using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Tidebook
{
    public class RodHud : MonoBehaviour
    {
        const float CueSeconds = 0.7f;

        IRodStatus status;
        IWalletView wallet;
        IBaitProvider baits;

        string cueText = "";
        float cueUntil;

        public void Bind(IRodStatus status, IWalletView wallet, IBaitProvider baits)
        {
            if (this.status != null) this.status.Cue -= OnCue;

            this.status = status;
            this.wallet = wallet;
            this.baits = baits;

            if (this.status != null) this.status.Cue += OnCue;
        }

        void OnDestroy()
        {
            if (status != null) status.Cue -= OnCue;
        }

        void OnCue(RodCue cue)
        {
            switch (cue)
            {
                case RodCue.BiteStarted: cueText = "!!! BITE !!!"; break;
                case RodCue.PullWarning: cueText = "!! pull incoming !!"; break;
                case RodCue.Pulled: cueText = ">>> PULL <<<"; break;
            }
            cueUntil = Time.time + CueSeconds;
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

            IReadOnlyList<IMeterView> meters = status.Meters;
            for (int i = 0; i < meters.Count; i++)
                sb.AppendLine(meters[i].Label.PadRight(12) + Bar(meters[i]));

            if (Time.time < cueUntil) sb.AppendLine(cueText);

            IFishInfo target = status.Target;
            if (target != null)
                sb.AppendLine("[debug] target: " + target.DisplayName + " | " + target.StateName +
                              " | match " + target.MatchRatio.ToString("0.00"));

            GUI.Label(new Rect(10f, 10f, 520f, 260f), sb.ToString());
        }

        /// <summary>20-cell bar. '#' = filled. If the meter has a safe band, its edges are drawn as [ and ].</summary>
        static string Bar(IMeterView meter)
        {
            const int cells = 20;
            char[] bar = new char[cells];
            int filled = Mathf.RoundToInt(Mathf.Clamp01(meter.Value) * cells);
            for (int i = 0; i < cells; i++) bar[i] = i < filled ? '#' : '-';

            if (meter.HasBand)
            {
                int lo = Mathf.Clamp(Mathf.RoundToInt(meter.BandMin * cells), 0, cells - 1);
                int hi = Mathf.Clamp(Mathf.RoundToInt(meter.BandMax * cells) - 1, lo, cells - 1);
                bar[lo] = '[';
                bar[hi] = ']';
            }
            return "|" + new string(bar) + "|";
        }
    }
}
