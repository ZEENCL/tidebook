// BaitFlyweights.cs
// Flyweight factory for Bait. A Bait is immutable and there are only a few hundred possible combinations
// (3 buoyancies x 4 shapes x 5 patterns x 4 swims), so identical baits do not need separate objects.
//   intrinsic (shared, stored in the Bait) : buoyancy, shape, colour pattern, swim pattern
//   extrinsic (NOT stored in the Bait)     : where it landed, which fish is looking at it. Callers pass those in,
//                                            for example fish.TryNotice(bait, position, ctx).
// BaitBuilder collects the player's choices (Builder pattern) and then asks this factory for the shared instance.

namespace Tidebook
{
    public static class BaitFlyweights
    {
        static readonly FlyweightFactory<int, Bait> cache = new FlyweightFactory<int, Bait>("Baits");

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            cache.Clear();
        }

        /// <summary>The one shared Bait for this combination. Asking twice returns the very same object.</summary>
        public static Bait Get(Buoyancy buoyancy, BodyShape shape, ColorPattern color, SwimPattern swim)
        {
            // 5 bits per choice, so each enum can grow to 32 values before keys could collide.
            int key = (int)buoyancy | ((int)shape << 5) | ((int)color << 10) | ((int)swim << 15);

            Bait bait;
            if (cache.TryGet(key, out bait)) return bait;

            bait = new Bait(buoyancy, shape, color, swim);
            cache.Add(key, bait);
            return bait;
        }
    }
}
