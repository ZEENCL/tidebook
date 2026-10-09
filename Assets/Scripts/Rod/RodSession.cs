// RodSession.cs
// Shared data for ONE cast: what the states write and the HUD reads.
// Data only. It implements IRodStatus so observers get a read-only view.

using UnityEngine;

namespace Tidebook
{
    public class RodSession : IRodStatus
    {
        // ---- exposed to observers (IRodStatus) ----
        public RodPhase Phase { get; set; }
        public string Message { get; set; }
        public float Power { get; set; }           // 0..1 while charging
        public float Tension { get; set; }         // 0..1 while reeling (snaps at 1)
        public float ReelProgress { get; set; }    // 0..1 while reeling (caught at 1, escapes at 0)
        public float BandHalfWidth { get; set; }   // safe band is 0.5 +/- this

        // ---- used by the states ----
        public Bait Bait { get; set; }
        public Vector3 BaitPosition { get; set; }
        public WaterContext Water { get; set; }
        public IHookableFish Target { get; set; }

        // Observers only get the read-only fish info.
        IFishInfo IRodStatus.Target { get { return Target; } }

        public RodSession()
        {
            Message = "";
        }
    }
}
