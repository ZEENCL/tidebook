// SpriteCache.cs
// A flyweight factory for generated sprites. A sprite that Unity has destroyed (for example after a scene
// reload) counts as gone, so the factory simply paints a fresh one the next time it is asked.

using UnityEngine;

namespace Tidebook.Ui
{
    public class SpriteCache<TKey> : FlyweightFactory<TKey, Sprite>
    {
        public SpriteCache(string name) : base(name, StillExists) { }

        // Unity's overloaded "== null" is what notices a destroyed object, so it must be written on a Sprite.
        static bool StillExists(Sprite sprite)
        {
            return sprite != null;
        }
    }
}
