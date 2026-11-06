// FishIndexLog.cs
// The player's Fish Index progress. It listens to the rod's catch events (ICatchListener), so a fish is
// "logged to the Fish Index" exactly as the GDD describes, without the rod knowing the index exists.
//
// What gets recorded per species: discovered?, times caught, the first catch (where / when / weather),
// and which bait preferences were revealed. A preference is revealed when a catch is made with a bait that
// matched it (the match is what made the fish bite, so the player has "worked it out").
// Progress is saved to PlayerPrefs as JSON.

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook.Ui
{
    public class FishIndexLog : ICatchListener, IFishIndexView
    {
        const string SaveKey = "tidebook.fishindex.v1";

        [System.Serializable]
        public class RecordSave
        {
            public string id;
            public int caught;
            public int known;
            public string spot;
            public int time;
            public int weather;
        }

        [System.Serializable]
        public class IndexSave
        {
            public List<RecordSave> records = new List<RecordSave>();
        }

        readonly List<FishIndexEntry> entries = new List<FishIndexEntry>();
        readonly bool persist;

        public IReadOnlyList<FishIndexEntry> Entries { get { return entries; } }

        public int DiscoveredCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < entries.Count; i++) if (entries[i].Discovered) count++;
                return count;
            }
        }

        public event System.Action Changed;
        public event System.Action<FishIndexEntry> Discovered;

        public FishIndexLog(IList<FishCatalogEntry> catalog, bool persistProgress)
        {
            persist = persistProgress;

            for (int i = 0; i < catalog.Count; i++)
            {
                FishIndexEntry entry = new FishIndexEntry();
                entry.Number = i + 1;
                entry.Source = catalog[i];
                entries.Add(entry);
            }

            if (persist) Load();
        }

        // ---------------------------------------------------------------- ICatchListener

        public void OnCatchResolved(CatchReport report)
        {
            if (report.Result != CatchResult.Caught || report.Info == null || report.Info.Species == null) return;

            FishIndexEntry entry = Find(report.Info.Species);
            if (entry == null) return;

            bool firstTime = !entry.Discovered;
            entry.Discovered = true;
            entry.TimesCaught++;

            if (firstTime)
            {
                entry.FirstSpot = report.Water.spotId;
                entry.FirstTime = report.Water.time;
                entry.FirstWeather = report.Water.weather;
            }

            RevealPreferences(entry, report.Bait, report.Water);

            if (persist) Save();
            Raise();

            if (firstTime)
            {
                System.Action<FishIndexEntry> handler = Discovered;
                if (handler != null) handler(entry);
            }
        }

        FishIndexEntry Find(FishSpecies species)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Species == species) return entries[i];
            }
            return null;
        }

        /// <summary>A preference the fish cares about AND the bait matched is now known to the player.</summary>
        static void RevealPreferences(FishIndexEntry entry, Bait bait, WaterContext water)
        {
            PreferenceProfile p = entry.Species.profile;
            if (bait != null)
            {
                if (p.buoyancyW.weight > 0f && bait.Buoyancy == p.buoyancy) entry.Known |= KnownFactors.Buoyancy;
                if (p.shapeW.weight > 0f && bait.Shape == p.shape) entry.Known |= KnownFactors.Shape;
                if (p.colorW.weight > 0f && bait.Color == p.color) entry.Known |= KnownFactors.Pattern;
                if (p.swimW.weight > 0f && bait.Swim == p.swim) entry.Known |= KnownFactors.Swim;
            }
            if (p.timeW.weight > 0f && water.time == p.time) entry.Known |= KnownFactors.Time;
            if (p.weatherW.weight > 0f && water.weather == p.weather) entry.Known |= KnownFactors.Weather;
        }

        // ---------------------------------------------------------------- preview / reset

        /// <summary>For screenshots and demos: marks the first few fish as found (not saved).</summary>
        public void PreviewUnlock(int count)
        {
            for (int i = 0; i < entries.Count && i < count; i++)
            {
                FishIndexEntry e = entries[i];
                e.Discovered = true;
                e.TimesCaught = 1 + (i * 3) % 7;
                e.FirstSpot = (e.Species.allowedSpotIds != null && e.Species.allowedSpotIds.Length > 0)
                    ? e.Species.allowedSpotIds[0] : "Bay";
                e.FirstTime = e.Species.profile.time;
                e.FirstWeather = e.Species.profile.weather;

                for (int k = 0; k < FishInfoText.AllFactors.Length; k++)
                {
                    KnownFactors f = FishInfoText.AllFactors[k];
                    if (FishInfoText.HasWeight(f, e.Species.profile) && (i + k) % 2 == 0) e.Known |= f;
                }
            }
            Raise();
        }

        /// <summary>Forgets all progress (and the saved file).</summary>
        public void ClearProgress()
        {
            for (int i = 0; i < entries.Count; i++)
            {
                FishIndexEntry e = entries[i];
                e.Discovered = false;
                e.TimesCaught = 0;
                e.Known = KnownFactors.None;
                e.FirstSpot = null;
            }
            PlayerPrefs.DeleteKey(SaveKey);
            Raise();
        }

        // ---------------------------------------------------------------- saving

        void Save()
        {
            IndexSave data = new IndexSave();
            for (int i = 0; i < entries.Count; i++)
            {
                FishIndexEntry e = entries[i];
                if (!e.Discovered) continue;

                RecordSave r = new RecordSave();
                r.id = e.Species.name;
                r.caught = e.TimesCaught;
                r.known = (int)e.Known;
                r.spot = e.FirstSpot;
                r.time = (int)e.FirstTime;
                r.weather = (int)e.FirstWeather;
                data.records.Add(r);
            }

            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        void Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return;

            IndexSave data = JsonUtility.FromJson<IndexSave>(PlayerPrefs.GetString(SaveKey));
            if (data == null || data.records == null) return;

            for (int i = 0; i < data.records.Count; i++)
            {
                RecordSave r = data.records[i];
                for (int k = 0; k < entries.Count; k++)
                {
                    if (entries[k].Species.name != r.id) continue;

                    entries[k].Discovered = true;
                    entries[k].TimesCaught = r.caught;
                    entries[k].Known = (KnownFactors)r.known;
                    entries[k].FirstSpot = r.spot;
                    entries[k].FirstTime = (TimeOfDay)r.time;
                    entries[k].FirstWeather = (Weather)r.weather;
                    break;
                }
            }
        }

        void Raise()
        {
            System.Action handler = Changed;
            if (handler != null) handler();
        }
    }
}
