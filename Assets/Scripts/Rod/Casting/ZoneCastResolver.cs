// ZoneCastResolver.cs
// Cast math only: camera direction + power -> a landing point on the water zone's surface.
// Crosshair gives the direction, power gives the distance (min..max range).
// A cast only counts if it lands inside a water zone (that is our "water" for now).

using UnityEngine;

namespace Tidebook
{
    public class ZoneCastResolver : ICastLandingResolver
    {
        readonly Transform aim;
        readonly IWaterZones zones;
        readonly CastSettings cast;

        public ZoneCastResolver(Transform aim, IWaterZones zones, CastSettings cast)
        {
            this.aim = aim;
            this.zones = zones;
            this.cast = cast;
        }

        public bool TryResolve(float power, out Vector3 point, out IWaterZone zone)
        {
            point = Vector3.zero;
            zone = null;

            Vector3 dir = aim.forward;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return false;   // looking straight up/down
            dir.Normalize();

            float distance = Mathf.Lerp(cast.minRange, cast.maxRange, power);
            Vector3 flat = aim.position + dir * distance;

            zone = zones.FindAt(flat);
            if (zone == null) return false;

            point = new Vector3(flat.x, zone.SurfaceY, flat.z);
            return true;
        }

        public CastPreview Preview(float power)
        {
            CastPreview preview = new CastPreview();

            Vector3 dir = aim.forward;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return preview;   // looking straight up/down: nothing to show
            dir.Normalize();

            float distance = Mathf.Lerp(cast.minRange, cast.maxRange, power);
            Vector3 flat = aim.position + dir * distance;
            IWaterZone zone = zones.FindAt(flat);

            preview.Visible = true;
            preview.Power = power;
            preview.Distance = distance;
            preview.InWater = zone != null;
            preview.Point = zone != null
                ? new Vector3(flat.x, zone.SurfaceY, flat.z)
                : new Vector3(flat.x, aim.position.y - 1.7f, flat.z);   // over land: roughly ground level
            return preview;
        }
    }
}
