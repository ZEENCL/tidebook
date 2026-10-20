using UnityEngine;

namespace Tidebook
{
    /// <summary>The WaterZone components in the scene.</summary>
    public class WaterZoneLookup : IWaterZones
    {
        public IWaterZone FindAt(Vector3 worldPos)
        {
            return WaterZone.FindAt(worldPos);
        }
    }
}
