using UnityEngine;

namespace Tidebook
{
    public interface IWaterZones
    {
        /// <summary>The zone covering this position, or null if it isn't fishable water.</summary>
        IWaterZone FindAt(Vector3 worldPos);
    }
}
