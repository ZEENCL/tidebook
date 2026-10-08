using System.Collections.Generic;
using UnityEngine;

namespace Tidebook
{
    /// <summary>Legacy UnityEngine.Input (mouse). Replace for the new Input System.</summary>
    public class LegacyRodInput : IRodInput
    {
        public bool ActionPressed { get { return Input.GetMouseButtonDown(0); } }
        public bool ActionReleased { get { return Input.GetMouseButtonUp(0); } }
        public bool ReelHeld { get { return Input.GetMouseButton(0); } }
        public bool CancelPressed { get { return Input.GetMouseButtonDown(1); } }
    }

    /// <summary>The static list every Fish adds itself to.</summary>
    public class StaticFishRegistry : IFishRegistry
    {
        public IReadOnlyList<Fish> ActiveFish { get { return Fish.Active; } }
    }

    /// <summary>The FishSpawner zones in the scene.</summary>
    public class SpawnerWaterZones : IWaterZones
    {
        public IWaterZone FindAt(Vector3 worldPos)
        {
            return FishSpawner.FindAt(worldPos);
        }
    }
}
