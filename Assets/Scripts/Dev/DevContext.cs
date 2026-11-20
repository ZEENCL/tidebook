// DevContext.cs
// The RECEIVERS the dev commands act on, looked up lazily so the console works no matter which
// object finished its setup first (the rod creates its wallet in Start, WorldState sets Instance in Awake...).
// Each Require... method throws a DevCommandException with a readable message when the thing is missing,
// which the console prints instead of failing silently.

using System.Collections.Generic;
using Tidebook.Ui;
using UnityEngine;

namespace Tidebook.Dev
{
    /// <summary>A command could not be built from what was typed. The message is shown to the player.</summary>
    public class DevCommandException : System.Exception
    {
        public DevCommandException(string message) : base(message) { }
    }

    public class DevContext
    {
        readonly RodController rod;
        readonly FishIndexLog log;

        public DevContext(RodController rod, FishIndexLog log)
        {
            this.rod = rod;
            this.log = log;
        }

        public WorldState World { get { return WorldState.Instance; } }

        public WorldState RequireWorld()
        {
            WorldState world = WorldState.Instance;
            if (world == null) throw new DevCommandException("There is no WorldState in the scene.");
            return world;
        }

        public IWallet RequireWallet()
        {
            IWallet wallet = rod != null ? rod.Wallet : null;
            if (wallet == null) throw new DevCommandException("The wallet is not ready (no RodController, or it has not started yet).");
            return wallet;
        }

        public FishIndexLog RequireIndex()
        {
            if (log == null) throw new DevCommandException("There is no Tidebook progress log.");
            return log;
        }

        // ------------------------------------------------------------ fish lookup

        /// <summary>Every species the game knows about: the Tidebook catalog plus whatever the spawners use.</summary>
        public List<FishSpecies> AllSpecies()
        {
            List<FishSpecies> all = new List<FishSpecies>();

            if (log != null)
            {
                for (int i = 0; i < log.Entries.Count; i++) AddUnique(all, log.Entries[i].Species);
            }

            FishSpawner[] spawners = UiFind.All<FishSpawner>();
            for (int i = 0; i < spawners.Length; i++)
            {
                for (int k = 0; k < spawners[i].species.Count; k++) AddUnique(all, spawners[i].species[k]);
            }
            return all;
        }

        static void AddUnique(List<FishSpecies> list, FishSpecies species)
        {
            if (species != null && !list.Contains(species)) list.Add(species);
        }

        /// <summary>"perch", "bay perch" or "BayPerch" all find "Bay Perch". Must match exactly one species.</summary>
        public FishSpecies FindSpecies(string query)
        {
            string wanted = Normalize(query);
            List<FishSpecies> all = AllSpecies();
            if (all.Count == 0) throw new DevCommandException("No fish species found (no spawners or Tidebook catalog).");

            List<FishSpecies> matches = new List<FishSpecies>();
            for (int i = 0; i < all.Count; i++)
            {
                if (Normalize(all[i].displayName) == wanted || Normalize(all[i].name) == wanted) return all[i];
                if (Normalize(all[i].displayName).Contains(wanted) || Normalize(all[i].name).Contains(wanted)) matches.Add(all[i]);
            }

            if (matches.Count == 1) return matches[0];
            if (matches.Count == 0) throw new DevCommandException("No fish called '" + query + "'. Species: " + Names(all));
            throw new DevCommandException("'" + query + "' matches several fish: " + Names(matches));
        }

        /// <summary>The spawner that can host this species right now (preferring ones that list it, then the closest to the camera).</summary>
        public FishSpawner FindSpawnerFor(FishSpecies species)
        {
            FishSpawner[] spawners = UiFind.All<FishSpawner>();
            if (spawners.Length == 0) throw new DevCommandException("There is no FishSpawner in the scene.");

            Camera cam = Camera.main;
            FishSpawner best = null;
            bool bestListsIt = false;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < spawners.Length; i++)
            {
                if (!spawners[i].CanSpawn(species)) continue;

                bool listsIt = spawners[i].species.Contains(species);
                float distance = cam != null ? (cam.transform.position - spawners[i].transform.position).sqrMagnitude : 0f;

                bool better = best == null
                              || (listsIt && !bestListsIt)
                              || (listsIt == bestListsIt && distance < bestDistance);
                if (better)
                {
                    best = spawners[i];
                    bestListsIt = listsIt;
                    bestDistance = distance;
                }
            }

            if (best == null)
                throw new DevCommandException(species.displayName + " cannot live in any water zone right now (check the time, weather and spot).");
            return best;
        }

        static string Normalize(string s)
        {
            return s == null ? "" : s.Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        }

        static string Names(List<FishSpecies> list)
        {
            string text = "";
            for (int i = 0; i < list.Count; i++) text += (i > 0 ? ", " : "") + list[i].displayName;
            return text;
        }
    }
}
