// IWaterZone.cs - one fishable area: surface height + current conditions.
namespace Tidebook
{
    public interface IWaterZone
    {
        float SurfaceY { get; }
        WaterContext Context { get; }
    }
}
