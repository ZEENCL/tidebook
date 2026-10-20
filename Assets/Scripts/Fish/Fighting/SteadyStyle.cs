// SteadyStyle.cs - a metronome: the same pull at the same interval. Easy to learn.
using UnityEngine;

namespace Tidebook
{
    [CreateAssetMenu(menuName = "Tidebook/Fight Styles/Steady")]
    public class SteadyStyle : FightStyle
    {
        [Min(0.1f)] public float interval = 1.0f;
        public float impulse = 0.15f;
        public float strengthBase = 0.6f;

        public override IPullPattern Create(float strength)
        {
            return new SteadyPattern(this, strength);
        }
    }
}
