// WeightedSpeciesSelector.cs
// Default way a spawner picks the next species: random, weighted by FishSpecies.spawnWeight.
// Swap in another ISpeciesSelector (e.g. "favor species the player hasn't caught yet") without touching the spawner.

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook
{
    public class WeightedSpeciesSelector : ISpeciesSelector
    {
        public FishSpecies Pick(IReadOnlyList<FishSpecies> candidates, WaterContext ctx)
        {
            float total = 0f;
            for (int i = 0; i < candidates.Count; i++) total += candidates[i].spawnWeight;
            if (total <= 0f) return null;

            float roll = Random.value * total;
            for (int i = 0; i < candidates.Count; i++)
            {
                roll -= candidates[i].spawnWeight;
                if (roll <= 0f) return candidates[i];
            }
            return candidates[candidates.Count - 1];
        }
    }
}
