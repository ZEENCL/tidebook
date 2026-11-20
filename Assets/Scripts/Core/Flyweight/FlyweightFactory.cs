// FlyweightFactory.cs
// Flyweight pattern. Some objects are needed again and again with the SAME contents: a rounded-corner sprite
// of radius 24, the lure picture for "Slim + Gold", a Bait with the same four choices. Instead of making a new
// copy every time, a flyweight factory keeps ONE shared object per key and hands that same object back.
//
//   intrinsic state  = what is the same for everyone using the object (the pixels of the sprite, the bait's four
//                      choices). It lives INSIDE the shared object and must never change after it is made.
//   extrinsic state  = what differs per use (where the bait landed, which Image shows the sprite). The caller
//                      keeps it and passes it in; it is never stored in the shared object.
//
// This one class replaces the "dictionary + TryGetValue + store" code that was copy-pasted into FishArt, BaitArt,
// HudSprites and ProceduralSprites. It also counts how often it created versus reused an object, which the dev
// console shows with the "flyweights" command.

using System.Collections.Generic;

namespace Tidebook
{
    /// <summary>Read-only numbers about one factory (so they can be listed together).</summary>
    public interface IFlyweightStats
    {
        string Name { get; }
        int Count { get; }    // shared objects alive right now
        int Hits { get; }     // times an existing object was reused
        int Misses { get; }   // times a new object had to be made
    }

    /// <summary>Every FlyweightFactory registers itself here, so tools can list them all.</summary>
    public static class FlyweightRegistry
    {
        static readonly List<IFlyweightStats> all = new List<IFlyweightStats>();

        public static IList<IFlyweightStats> All { get { return all; } }

        public static void Add(IFlyweightStats factory)
        {
            all.Add(factory);
        }

        /// <summary>One line per factory, e.g. "Bait: 3 shared, 41 reused, 3 created".</summary>
        public static string Report()
        {
            if (all.Count == 0) return "No flyweight factories yet.";

            string text = "";
            for (int i = 0; i < all.Count; i++)
            {
                IFlyweightStats f = all[i];
                text += (i > 0 ? "\n" : "") + f.Name + ": " + f.Count + " shared, " + f.Hits + " reused, " + f.Misses + " created";
            }
            return text;
        }
    }

    public class FlyweightFactory<TKey, TValue> : IFlyweightStats where TValue : class
    {
        readonly Dictionary<TKey, TValue> items = new Dictionary<TKey, TValue>();
        readonly System.Func<TValue, bool> isAlive;
        readonly string name;

        public int Hits { get; private set; }
        public int Misses { get; private set; }

        /// <param name="name">Shown in the flyweights report.</param>
        /// <param name="isAlive">Optional. Return false for a stored object that is no longer usable (for example a
        /// destroyed Unity object); the factory then makes a fresh one. Null means "any stored object is fine".</param>
        public FlyweightFactory(string name, System.Func<TValue, bool> isAlive = null)
        {
            this.name = name;
            this.isAlive = isAlive;
            FlyweightRegistry.Add(this);
        }

        public string Name { get { return name; } }
        public int Count { get { return items.Count; } }

        /// <summary>The shared object for this key if there is a usable one (counts a reuse), otherwise false (counts a miss).</summary>
        public bool TryGet(TKey key, out TValue value)
        {
            if (items.TryGetValue(key, out value) && value != null && (isAlive == null || isAlive(value)))
            {
                Hits++;
                return true;
            }

            value = null;
            Misses++;
            return false;
        }

        /// <summary>Stores the shared object for this key (replacing a dead one). Use after TryGet missed.</summary>
        public void Add(TKey key, TValue value)
        {
            items[key] = value;
        }

        /// <summary>Shortcut: the shared object for this key, making it with 'create' the first time.</summary>
        public TValue Get(TKey key, System.Func<TValue> create)
        {
            TValue value;
            if (TryGet(key, out value)) return value;

            value = create();
            Add(key, value);
            return value;
        }

        /// <summary>Forgets everything and resets the counters (used when the game restarts without reloading code).</summary>
        public void Clear()
        {
            items.Clear();
            Hits = 0;
            Misses = 0;
        }
    }
}
