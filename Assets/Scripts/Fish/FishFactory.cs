using System.Collections.Generic;
using UnityEngine;

namespace Tidebook
{
    public class FishFactory
    {
        // Strategies are stateless, so one shared instance per type is enough.
        static readonly Dictionary<BiteStrategyType, IBiteStrategy> Strategies = CreateDefaultStrategies();

        static Dictionary<BiteStrategyType, IBiteStrategy> CreateDefaultStrategies()
        {
            IBiteStrategy weighted = new WeightedBiteStrategy();
            Dictionary<BiteStrategyType, IBiteStrategy> map = new Dictionary<BiteStrategyType, IBiteStrategy>();
            map[BiteStrategyType.Weighted] = weighted;
            map[BiteStrategyType.Gated] = new GatedBiteStrategy(weighted);   // composition
            return map;
        }

        /// <summary>Plug in or replace a strategy without touching the factory's logic.</summary>
        public static void RegisterStrategy(BiteStrategyType type, IBiteStrategy strategy)
        {
            Strategies[type] = strategy;
        }

        readonly Transform poolRoot;
        readonly Dictionary<FishSpecies, Stack<Fish>> pool = new Dictionary<FishSpecies, Stack<Fish>>();

        public FishFactory(Transform poolRoot)
        {
            this.poolRoot = poolRoot;
        }

        public Fish Create(FishSpecies species, Vector3 position, Bounds wanderArea, float layerY)
        {
            Fish fish = null;

            Stack<Fish> stack;
            if (pool.TryGetValue(species, out stack) && stack.Count > 0)
                fish = stack.Pop();
            else
                fish = Build(species);

            fish.transform.SetParent(null);
            fish.transform.position = position;
            fish.gameObject.SetActive(true);
            fish.Init(species, ResolveStrategy(species), wanderArea, layerY, Release);
            return fish;
        }

        /// <summary>Return a fish to the pool (deactivates it).</summary>
        public void Release(Fish fish)
        {
            fish.gameObject.SetActive(false);
            fish.transform.SetParent(poolRoot);

            Stack<Fish> stack;
            if (!pool.TryGetValue(fish.Species, out stack))
            {
                stack = new Stack<Fish>();
                pool[fish.Species] = stack;
            }
            stack.Push(fish);
        }

        static IBiteStrategy ResolveStrategy(FishSpecies species)
        {
            IBiteStrategy strategy;
            if (Strategies.TryGetValue(species.biteStrategy, out strategy)) return strategy;
            return Strategies[BiteStrategyType.Weighted];
        }

        static Fish Build(FishSpecies species)
        {
            GameObject go;
            if (species.prefab != null)
            {
                go = Object.Instantiate(species.prefab);
            }
            else
            {
                // No model yet: a plain capsule on its side is enough for the prototype.
                go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                go.transform.localScale = new Vector3(0.3f, 0.3f, 0.5f);
                Collider col = go.GetComponent<Collider>();
                if (col != null) Object.Destroy(col);
            }

            go.name = species.displayName;

            Fish fish = go.GetComponent<Fish>();
            if (fish == null) fish = go.AddComponent<Fish>();
            return fish;
        }
    }
}
