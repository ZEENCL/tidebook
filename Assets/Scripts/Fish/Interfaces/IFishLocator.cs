// IFishLocator.cs - where visual feedback (ripples, shadows) finds the fish currently in the world.
using System.Collections.Generic;

namespace Tidebook
{
    public interface IFishLocator
    {
        IReadOnlyList<IFishCue> ActiveFish { get; }
    }
}
