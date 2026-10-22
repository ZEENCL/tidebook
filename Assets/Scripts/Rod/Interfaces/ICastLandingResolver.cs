// ICastLandingResolver.cs - turns cast power + aim into a landing point inside a water zone.
using UnityEngine;

namespace Tidebook
{
    public interface ICastLandingResolver
    {
        bool TryResolve(float power, out Vector3 point, out IWaterZone zone);

        /// <summary>Where a cast of this power would land right now (for the trajectory line). Works even over dry land.</summary>
        CastPreview Preview(float power);
    }
}
