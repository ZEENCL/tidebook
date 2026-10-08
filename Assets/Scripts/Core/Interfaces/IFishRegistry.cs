using System.Collections.Generic;

namespace Tidebook
{
    public interface IFishRegistry
    {
        IReadOnlyList<Fish> ActiveFish { get; }
    }
}
