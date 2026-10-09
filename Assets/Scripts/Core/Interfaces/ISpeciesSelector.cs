// ISpeciesSelector.cs - how a spawner chooses which species to spawn next.
using System.Collections.Generic;

namespace Tidebook
{
    public interface ISpeciesSelector
    {
        FishSpecies Pick(IReadOnlyList<FishSpecies> candidates, WaterContext ctx);
    }
}
