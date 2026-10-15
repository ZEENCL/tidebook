// IFishFactory.cs - Factory abstraction: creates and recycles fish.
using UnityEngine;

namespace Tidebook
{
    public interface IFishFactory
    {
        Fish Create(FishSpecies species, Vector3 position, Bounds wanderArea, float layerY);
        void Release(Fish fish);
    }
}
