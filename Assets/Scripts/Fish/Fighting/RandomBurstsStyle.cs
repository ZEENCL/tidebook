// RandomBurstsStyle.cs - irregular yanks at random intervals. The defaults reproduce the original prototype.
using UnityEngine;

namespace Tidebook
{
    [CreateAssetMenu(menuName = "Tidebook/Fight Styles/Random Bursts")]
    public class RandomBurstsStyle : FightStyle
    {
        [Header("Timing")]
        public float minInterval = 0.6f;
        public float maxIntervalCalm = 1.6f;      // longest gap for a weak fish (strength 0)
        public float maxIntervalFierce = 0.9f;    // longest gap for a strong fish (strength 1)

        [Header("Force")]
        public float minImpulse = 0.10f;
        public float maxImpulse = 0.20f;
        public float strengthBase = 0.6f;         // impulse is multiplied by (strengthBase + fightStrength)

        public override IPullPattern Create(float strength)
        {
            return new RandomBurstsPattern(this, strength);
        }
    }
}
