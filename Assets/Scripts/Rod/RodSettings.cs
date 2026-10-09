// RodSettings.cs
// Tuning values, grouped by concern. Plain serializable classes so they show up as foldouts
// in the Inspector and can be edited live while playing.

namespace Tidebook
{
    [System.Serializable]
    public class CastSettings
    {
        public float minRange = 4f;
        public float maxRange = 25f;
        public float chargeSeconds = 1.2f;    // time to reach full power
    }

    [System.Serializable]
    public class ReelSettings
    {
        public float tensionRate = 0.7f;      // tension gained per second while holding reel
        public float relaxRate = 0.6f;        // tension lost per second while not holding
        public float progressRate = 0.18f;    // reel progress per second while tension is in the safe band
        public float slipRate = 0.12f;        // reel progress lost per second outside the band
    }
}
