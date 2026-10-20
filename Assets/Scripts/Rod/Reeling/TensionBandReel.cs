// TensionBandReel.cs
// The player-side reel mechanic, with NO knowledge of fish, wallets or UI:
//   - holding the reel raises tension, letting go lowers it
//   - a fish pull adds an impulse on top (supplied by an IPullPattern, not decided here)
//   - while tension stays inside the safe band, progress climbs; outside it, progress slips
//   - tension >= 1 snaps the line, progress <= 0 lets the fish slip off, progress >= 1 lands it
// Tuning numbers come from ReelSettings; the few fixed constants below are named for clarity.
using UnityEngine;

namespace Tidebook
{
    public enum ReelStatus { InProgress, Landed, LineSnapped, SlippedOff }

    public class TensionBandReel
    {
        const float BandCenter = 0.5f;
        const float StartTension = 0.5f;
        const float StartProgress = 0.25f;
        const float WidestBand = 0.25f;          // safe band half-width for a pushover fish
        const float NarrowestBand = 0.12f;       // ... and for the strongest fish
        const float StrengthSlowdown = 0.5f;     // a max-strength fish gains progress at half speed

        readonly ReelSettings settings;
        float strength;

        public float Tension { get; private set; }
        public float Progress { get; private set; }
        public float BandHalfWidth { get; private set; }
        public float BandMin { get { return BandCenter - BandHalfWidth; } }
        public float BandMax { get { return BandCenter + BandHalfWidth; } }

        public TensionBandReel(ReelSettings settings)
        {
            this.settings = settings;
        }

        public void Begin(float fightStrength)
        {
            strength = fightStrength;
            Tension = StartTension;
            Progress = StartProgress;
            BandHalfWidth = Mathf.Lerp(WidestBand, NarrowestBand, strength);   // stronger fish = narrower safe band
        }

        public ReelStatus Tick(float dt, bool reelHeld, float pullImpulse)
        {
            if (reelHeld) Tension += settings.tensionRate * dt;
            else Tension -= settings.relaxRate * dt;

            Tension += pullImpulse;

            if (Tension >= 1f) return ReelStatus.LineSnapped;
            Tension = Mathf.Max(0f, Tension);

            bool inBand = Mathf.Abs(Tension - BandCenter) <= BandHalfWidth;
            if (inBand) Progress += settings.progressRate * (1f - StrengthSlowdown * strength) * dt;
            else Progress -= settings.slipRate * dt;

            if (Progress >= 1f) return ReelStatus.Landed;
            if (Progress <= 0f) return ReelStatus.SlippedOff;
            return ReelStatus.InProgress;
        }
    }
}
