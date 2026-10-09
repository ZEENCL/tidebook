// DefaultServices.cs
// Default implementations of the rod's interfaces. RodController uses these unless you inject your own.

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook
{
    /// <summary>Legacy UnityEngine.Input (mouse). Replace for the new Input System.</summary>
    public class LegacyRodInput : IRodInput
    {
        public bool ChargePressed { get { return Input.GetMouseButtonDown(0); } }
        public bool ChargeReleased { get { return Input.GetMouseButtonUp(0); } }
        public bool HookPressed { get { return Input.GetMouseButtonDown(0); } }
        public bool ReelHeld { get { return Input.GetMouseButton(0); } }
        public bool CancelPressed { get { return Input.GetMouseButtonDown(1); } }
    }

    /// <summary>The static list every Fish adds itself to.</summary>
    public class StaticFishRegistry : IFishRegistry
    {
        public IReadOnlyList<IBaitReactive> ActiveFish { get { return Fish.Active; } }
    }

    /// <summary>The WaterZone components in the scene.</summary>
    public class WaterZoneLookup : IWaterZones
    {
        public IWaterZone FindAt(Vector3 worldPos)
        {
            return WaterZone.FindAt(worldPos);
        }
    }
}
