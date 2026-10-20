// FishSpecies.cs
// Fish are DATA: one ScriptableObject asset per species.
// Create via: Assets > Create > Tidebook > Fish Species
//
// Soft-preference model:
//   - each factor has a preferred value + a weight (weight 0 = fish doesn't care)
//   - 'required' turns a factor into a hard gate (enforced by GatedBiteStrategy)

using UnityEngine;

namespace Tidebook
{
    [System.Serializable]
    public struct FactorWeight
    {
        [Min(0f)] public float weight;   // 0 = ignored
        public bool required;            // hard gate (only enforced for Gated species)
    }

    [System.Serializable]
    public class PreferenceProfile
    {
        public Buoyancy buoyancy;
        public FactorWeight buoyancyW;

        public BodyShape shape;
        public FactorWeight shapeW;

        public ColorPattern color;
        public FactorWeight colorW;

        public SwimPattern swim;
        public FactorWeight swimW;

        public TimeOfDay time;
        public FactorWeight timeW;

        public Weather weather;
        public FactorWeight weatherW;
    }

    [CreateAssetMenu(menuName = "Tidebook/Fish Species")]
    public class FishSpecies : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Fish";
        public Rarity rarity = Rarity.Common;
        public int currencyValue = 10;

        [Header("World placement")]
        public DepthLayer layer = DepthLayer.Mid;          // decides spawn height
        public string[] allowedSpotIds;                    // empty = can live anywhere
        [Min(0f)] public float spawnWeight = 1f;           // relative spawn frequency

        [Header("Behaviour")]
        public BiteStrategyType biteStrategy = BiteStrategyType.Weighted;   // set Gated for rare fish with required factors
        public float swimSpeed = 1.5f;
        [Range(0f, 1f)] public float fightStrength = 0.3f; // 0 = pushover, 1 = brutal

        [Tooltip("Pull rhythm while this fish is hooked. Leave empty for the default (random bursts).")]
        public FightStyle fightStyle;

        [Header("Optional visuals (prototype falls back to a capsule)")]
        public GameObject prefab;

        [Header("Bait preferences")]
        public PreferenceProfile profile = new PreferenceProfile();

        /// <summary>Creates a fresh pull pattern for one fight (fight style asset + this species' strength).</summary>
        public IPullPattern CreatePullPattern()
        {
            FightStyle style = fightStyle != null ? fightStyle : FightStyle.Default;
            return style.Create(fightStrength);
        }

        /// <summary>Can this species currently exist at this spot? (spot list + required time/weather)</summary>
        public bool CanExistIn(WaterContext ctx)
        {
            if (allowedSpotIds != null && allowedSpotIds.Length > 0 &&
                System.Array.IndexOf(allowedSpotIds, ctx.spotId) < 0)
                return false;

            if (profile.timeW.required && ctx.time != profile.time) return false;
            if (profile.weatherW.required && ctx.weather != profile.weather) return false;
            return true;
        }
    }
}
