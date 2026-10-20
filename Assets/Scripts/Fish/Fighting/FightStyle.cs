// FightStyle.cs
// A fight style is a ScriptableObject ASSET that holds the tuning for one pull rhythm and creates
// the runtime pattern. Config lives in the asset; timers and counters live in the per-fight pattern,
// so fish never share (or change) state through the asset.
//
// Assign a style to a species via FishSpecies.fightStyle. Empty = DefaultStyle (random bursts).

using UnityEngine;

namespace Tidebook
{
    public abstract class FightStyle : ScriptableObject
    {
        [Tooltip("Seconds of warning before a pull (0 = no telegraph).")]
        [Min(0f)] public float warningLead = 0.35f;

        /// <summary>Create a fresh runtime pattern for one fight.</summary>
        public abstract IPullPattern Create(float strength);

        static FightStyle fallback;

        /// <summary>Used when a species has no style assigned. Behaves like the original prototype.</summary>
        public static FightStyle Default
        {
            get
            {
                if (fallback == null)
                {
                    fallback = CreateInstance<RandomBurstsStyle>();
                    fallback.name = "Default (Random Bursts)";
                    fallback.hideFlags = HideFlags.HideAndDontSave;
                }
                return fallback;
            }
        }
    }
}
