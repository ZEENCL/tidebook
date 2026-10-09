// FishFactory.cs
// Factory pattern: turns a FishSpecies (data) into a ready-to-use Fish, including the strategy
// chosen by the injected resolver. Also pools fish so schools can spawn/despawn cheaply.

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook
{
    public class FishFactory : IFishFactory
    {
        readonly Transform poolRoot;
        readonly IBiteStrategyResolver strategies;
        readonly Dictionary<FishSpecies, Stack<Fish>> pool = new Dictionary<FishSpecies, Stack<Fish>>();

        public FishFactory(Transform poolRoot, IBiteStrategyResolver strategies)
        {
            this.poolRoot = poolRoot;
            this.strategies = strategies;
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
            fish.Init(species, strategies.Resolve(species), wanderArea, layerY, Release);
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

        static Fish Build(FishSpecies species)
        {
            GameObject go;
            if (species.prefab != null)
            {
                go = Object.Instantiate(species.prefab);
            }
            else
            {
                // No model yet: a plain capsule is enough for the prototype.
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
