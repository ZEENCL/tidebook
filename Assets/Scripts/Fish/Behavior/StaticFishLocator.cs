// StaticFishLocator.cs - default IFishLocator: the static list every Fish adds itself to.
using System.Collections.Generic;

namespace Tidebook
{
    public class StaticFishLocator : IFishLocator
    {
        public IReadOnlyList<IFishCue> ActiveFish { get { return Fish.Active; } }
    }
}
