// WaterZone.cs
// Geometry and conditions of one fishing spot. Place this object AT THE WATER SURFACE height.
// The cast resolver uses it to decide where a cast can land; FishSpawner uses it to place fish.

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook
{
    public class WaterZone : MonoBehaviour, IWaterZone
    {
        /// <summary>All zones in the scene.</summary>
        public static readonly List<WaterZone> All = new List<WaterZone>();

        public string spotId = "Bay";
        public Vector3 areaSize = new Vector3(30f, 0f, 30f);   // only x and z are used
        public float depth = 6f;

        public float SurfaceY { get { return transform.position.y; } }
        public WaterContext Context { get { return WorldState.ContextFor(spotId); } }

        /// <summary>Box fish swim inside: from the surface down to 'depth'.</summary>
        public Bounds Area
        {
            get { return new Bounds(transform.position + Vector3.down * (depth * 0.5f), new Vector3(areaSize.x, depth, areaSize.z)); }
        }

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public float LayerY(DepthLayer layer)
        {
            switch (layer)
            {
                case DepthLayer.Top: return SurfaceY - 0.5f;
                case DepthLayer.Bottom: return SurfaceY - depth + 0.5f;
                default: return SurfaceY - depth * 0.5f;
            }
        }

        /// <summary>Is this world position inside the zone's horizontal footprint?</summary>
        public bool Contains(Vector3 worldPos)
        {
            Vector3 d = worldPos - transform.position;
            return Mathf.Abs(d.x) <= areaSize.x * 0.5f && Mathf.Abs(d.z) <= areaSize.z * 0.5f;
        }

        public static WaterZone FindAt(Vector3 worldPos)
        {
            for (int i = 0; i < All.Count; i++)
            {
                if (All[i].Contains(worldPos)) return All[i];
            }
            return null;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.6f);
            Gizmos.DrawWireCube(Area.center, Area.size);
        }
    }
}
