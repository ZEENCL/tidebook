// CastPreview.cs
// What a cast would do if the player released right now. Published by the rod while aiming and charging
// so the HUD can draw the trajectory line without knowing any cast math.
using UnityEngine;

namespace Tidebook
{
    public struct CastPreview
    {
        public bool Visible;     // false when there is nothing to show (e.g. looking straight up)
        public bool InWater;     // true if the landing point is inside a fishing zone
        public Vector3 Point;    // where the bait would land (ground level when over land)
        public float Power;      // 0..1
        public float Distance;   // metres from the player
    }
}
