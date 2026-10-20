// EscalatingStyle.cs - starts relaxed and pulls faster (and harder) as you get closer to landing it.
using UnityEngine;

namespace Tidebook
{
    [CreateAssetMenu(menuName = "Tidebook/Fight Styles/Escalating")]
    public class EscalatingStyle : FightStyle
    {
        [Min(0.1f)] public float startInterval = 1.6f;      // gap when the reel progress is 0
        [Min(0.1f)] public float endInterval = 0.6f;        // gap when the reel progress is 1
        public float impulse = 0.12f;
        public float endImpulseMultiplier = 1.5f;           // impulse is scaled up to this as progress rises
        public float strengthBase = 0.6f;

        public override IPullPattern Create(float strength)
        {
            return new EscalatingPattern(this, strength);
        }
    }
}
