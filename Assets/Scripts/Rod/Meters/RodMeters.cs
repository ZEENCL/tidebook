// RodMeters.cs
// The set of meters currently visible. States call Set(...) each frame; RodSession clears the set
// whenever the rod enters a new phase, so a meter never outlives the state that published it.
using System.Collections.Generic;
using UnityEngine;

namespace Tidebook
{
    public class RodMeters
    {
        readonly List<Meter> items = new List<Meter>();

        public IReadOnlyList<IMeterView> Items { get { return items; } }

        public void Clear()
        {
            items.Clear();
        }

        /// <summary>Show/update a plain 0..1 meter.</summary>
        public void Set(string label, float value)
        {
            Meter m = Find(label);
            m.Value = Mathf.Clamp01(value);
            m.HasBand = false;
        }

        /// <summary>Show/update a meter with a highlighted safe band.</summary>
        public void Set(string label, float value, float bandMin, float bandMax)
        {
            Meter m = Find(label);
            m.Value = Mathf.Clamp01(value);
            m.HasBand = true;
            m.BandMin = bandMin;
            m.BandMax = bandMax;
        }

        Meter Find(string label)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Label == label) return items[i];
            }

            Meter created = new Meter(label);
            items.Add(created);
            return created;
        }
    }
}
