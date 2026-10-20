// RodSession.cs
// Shared data for ONE cast: what the states write and the HUD reads.
// Data only. It implements IRodStatus so observers get a read-only, mechanic-agnostic view.

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook
{
    public class RodSession : IRodStatus
    {
        RodPhase phase;
        readonly RodMeters meters = new RodMeters();

        // ---- exposed to observers (IRodStatus) ----

        /// <summary>Setting a phase also clears the meters, so a meter never outlives the state that published it.</summary>
        public RodPhase Phase
        {
            get { return phase; }
            set
            {
                phase = value;
                meters.Clear();
            }
        }

        public string Message { get; set; }

        public event System.Action<RodCue> Cue;

        IReadOnlyList<IMeterView> IRodStatus.Meters { get { return meters.Items; } }

        // Observers only get the read-only fish info.
        IFishInfo IRodStatus.Target { get { return Target; } }

        // ---- used by the states ----
        public RodMeters Meters { get { return meters; } }
        public Bait Bait { get; set; }
        public Vector3 BaitPosition { get; set; }
        public WaterContext Water { get; set; }
        public IHookableFish Target { get; set; }

        public RodSession()
        {
            Message = "";
        }

        public void RaiseCue(RodCue cue)
        {
            System.Action<RodCue> handler = Cue;
            if (handler != null) handler(cue);
        }
    }
}
