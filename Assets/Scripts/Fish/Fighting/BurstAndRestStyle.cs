// BurstAndRestStyle.cs - a quick flurry of pulls, then a calm spell. Rewards patience and timing.
using UnityEngine;

namespace Tidebook
{
    [CreateAssetMenu(menuName = "Tidebook/Fight Styles/Burst And Rest")]
    public class BurstAndRestStyle : FightStyle
    {
        [Min(1)] public int pullsPerBurst = 3;
        [Min(0.1f)] public float pullGap = 0.4f;        // seconds between pulls inside a burst
        [Min(0.1f)] public float restSeconds = 2.0f;    // calm time before each burst
        public float impulse = 0.12f;
        public float strengthBase = 0.6f;

        public override IPullPattern Create(float strength)
        {
            return new BurstAndRestPattern(this, strength);
        }
    }
}
