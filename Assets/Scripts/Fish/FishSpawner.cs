// FishSpawner.cs
// Population only: keeps a number of fish alive inside this object's WaterZone.
// The zone's geometry lives in WaterZone, species choice in ISpeciesSelector,
// fish creation in IFishFactory. Assign Factory / Selector before Start() to swap them.

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook
{
    [RequireComponent(typeof(WaterZone))]
    public class FishSpawner : MonoBehaviour
    {
        [Header("Population")]
        public List<FishSpecies> species = new List<FishSpecies>();
        public int targetPopulation = 12;
        public int maxSpawnPerTick = 3;
        public float tickInterval = 1f;

        [Header("Streaming")]
        public Transform player;               // defaults to the main camera
        public float activeRadius = 80f;       // fish are despawned when the player is farther than this

        public IFishFactory Factory { get; set; }
        public ISpeciesSelector Selector { get; set; }

        WaterZone zone;
        readonly List<Fish> spawned = new List<Fish>();
        readonly List<FishSpecies> candidates = new List<FishSpecies>();
        float timer;

        void Start()
        {
            zone = GetComponent<WaterZone>();
            if (zone == null) zone = gameObject.AddComponent<WaterZone>();

            if (Factory == null)
            {
                GameObject poolRoot = new GameObject("FishPool_" + zone.spotId);
                Factory = new FishFactory(poolRoot.transform, new BiteStrategyRegistry());
            }
            if (Selector == null) Selector = new WeightedSpeciesSelector();
        }

        void Update()
        {
            if (zone == null) return;
            if (player == null && Camera.main != null) player = Camera.main.transform;

            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = tickInterval;

            // Forget fish that were caught / pooled.
            spawned.RemoveAll(f => f == null || !f.gameObject.activeSelf);

            // Player far away: free everything, spawn again when they come back.
            if (player != null && (player.position - transform.position).sqrMagnitude > activeRadius * activeRadius)
            {
                DespawnAll();
                return;
            }

            WaterContext ctx = zone.Context;

            // Time/weather changed: remove species that can't exist now (never yank a fish that is mid-bite).
            for (int i = spawned.Count - 1; i >= 0; i--)
            {
                Fish f = spawned[i];
                if (!f.IsBusy && !f.Species.CanExistIn(ctx))
                {
                    Factory.Release(f);
                    spawned.RemoveAt(i);
                }
            }

            // Top the population back up a few fish at a time.
            int spawnedThisTick = 0;
            while (spawned.Count < targetPopulation && spawnedThisTick < maxSpawnPerTick)
            {
                if (!SpawnOne(ctx)) break;
                spawnedThisTick++;
            }
        }

        bool SpawnOne(WaterContext ctx)
        {
            candidates.Clear();
            for (int i = 0; i < species.Count; i++)
            {
                if (species[i] != null && species[i].CanExistIn(ctx)) candidates.Add(species[i]);
            }
            if (candidates.Count == 0) return false;

            FishSpecies pick = Selector.Pick(candidates, ctx);
            if (pick == null) return false;

            Bounds area = zone.Area;
            float y = zone.LayerY(pick.layer);
            Vector3 pos = new Vector3(
                Random.Range(area.min.x, area.max.x),
                y,
                Random.Range(area.min.z, area.max.z));

            spawned.Add(Factory.Create(pick, pos, area, y));
            return true;
        }

        void DespawnAll()
        {
            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] != null && spawned[i].gameObject.activeSelf) Factory.Release(spawned[i]);
            }
            spawned.Clear();
        }
    }
}
