// IWaterZones.cs - finds which water zone (if any) covers a world position.
using UnityEngine;

namespace Tidebook
{
    public interface IWaterZones
    {
        IWaterZone FindAt(Vector3 worldPos);
    }
}
