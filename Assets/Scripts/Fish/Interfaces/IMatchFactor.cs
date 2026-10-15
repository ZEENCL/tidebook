// IMatchFactor.cs - one scoring factor (buoyancy, color, time...). Add a factor = add one of these.
namespace Tidebook
{
    public interface IMatchFactor
    {
        FactorWeight WeightFor(PreferenceProfile profile);
        bool Matches(PreferenceProfile profile, Bait bait, WaterContext ctx);
    }
}
