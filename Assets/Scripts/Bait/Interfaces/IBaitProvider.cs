// IBaitProvider.cs - where the rod gets the bait to cast (inspector now, bait-builder UI later).
namespace Tidebook
{
    public interface IBaitProvider
    {
        Bait BuildBait();
    }
}
