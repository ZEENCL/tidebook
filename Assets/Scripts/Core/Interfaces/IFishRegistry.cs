// IFishRegistry.cs - where the rod looks for fish that could notice its bait.
using System.Collections.Generic;

namespace Tidebook
{
    public interface IFishRegistry
    {
        IReadOnlyList<IBaitReactive> ActiveFish { get; }
    }
}
