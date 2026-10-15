using System.Collections.Generic;

namespace Tidebook
{
    /// <summary>The static list every Fish adds itself to.</summary>
    public class StaticFishRegistry : IFishRegistry
    {
        public IReadOnlyList<IBaitReactive> ActiveFish { get { return Fish.Active; } }
    }
}
