using System.Collections.Generic;
using UnityEngine;

namespace Tidebook
{
    public class FishSpawner : MonoBehaviour, IWaterZone
    {
        /// <summary>All zones in the scene. The rod uses this to find which zone a cast landed in.</summary>
        public static readonly List<FishSpawner> All = new List<FishSpawner>();

        [Header("Zone")]
        public string spotId = "Bay";
        public Vector3 areaSize = new Vector3(30f, 0f, 30f);   // only x and z are used
        public float depth = 6f;

        [Header("Population")]
        public List<FishSpecies> species = new List<FishSpecies>();
        public int targetPopulation = 12;
        public int maxSpawnPerTick = 3;
        public float tickInterval = 1f;

        [Header("Streaming")]
        public Transform player;               // defaults to the main camera
        public float activeRadius = 80f;       // fish are despawned when the player is farther than this

        public float SurfaceY { get { return transform.position.y; } }
        public WaterContext Context { get { return WorldState.ContextFor(spotId); } }

        public Bounds Area
        {
            get { return new Bounds(transform.position + Vector3.down * (depth * 0.5f), new Vector3(areaSize.x, depth, areaSize.z)); }
        }

        FishFactory factory;
        readonly List<Fish> spawned = new List<Fish>();
        float timer;

        void Awake()
        {
            GameObject poolRoot = new GameObject("FishPool_" + spotId);
            factory = new FishFactory(poolRoot.transform);
        }

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        void Update()
        {
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

            WaterContext ctx = Context;

            // Time/weather changed: remove species that can't exist now (never yank a fish that is mid-bite).
            for (int i = spawned.Count - 1; i >= 0; i--)
            {
                Fish f = spawned[i];
                if (!f.IsBusy && !f.Species.CanExistIn(ctx))
                {
                    factory.Release(f);
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
            FishSpecies pick = PickSpecies(ctx);
            if (pick == null) return false;

            Bounds area = Area;
            float y = LayerY(pick.layer);
            Vector3 pos = new Vector3(
                Random.Range(area.min.x, area.max.x),
                y,
                Random.Range(area.min.z, area.max.z));

            Fish fish = factory.Create(pick, pos, area, y);
            spawned.Add(fish);
            return true;
        }

        FishSpecies PickSpecies(WaterContext ctx)
        {
            float total = 0f;
            for (int i = 0; i < species.Count; i++)
            {
                if (species[i] != null && species[i].CanExistIn(ctx)) total += species[i].spawnWeight;
            }
            if (total <= 0f) return null;

            float roll = Random.value * total;
            for (int i = 0; i < species.Count; i++)
            {
                FishSpecies s = species[i];
                if (s == null || !s.CanExistIn(ctx)) continue;
                roll -= s.spawnWeight;
                if (roll <= 0f) return s;
            }
            return null;
        }

        public float LayerY(DepthLayer layer)
        {
            switch (layer)
            {
                case DepthLayer.Top: return SurfaceY - 0.5f;
                case DepthLayer.Bottom: return SurfaceY - depth + 0.5f;
                default: return SurfaceY - depth * 0.5f;
            }
        }

        void DespawnAll()
        {
            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] != null && spawned[i].gameObject.activeSelf) factory.Release(spawned[i]);
            }
            spawned.Clear();
        }

        /// <summary>Is this world position inside the zone's horizontal footprint?</summary>
        public bool Contains(Vector3 worldPos)
        {
            Vector3 d = worldPos - transform.position;
            return Mathf.Abs(d.x) <= areaSize.x * 0.5f && Mathf.Abs(d.z) <= areaSize.z * 0.5f;
        }

        public static FishSpawner FindAt(Vector3 worldPos)
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
