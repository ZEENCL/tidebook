// IFishOutcome.cs - role: what happens to the fish once the fight is decided.
// Only the outcome listener uses this; the reeling state never touches it.
namespace Tidebook
{
    public interface IFishOutcome
    {
        void Escape();   // fish got away (line snapped / slipped off)
        void Catch();    // fish was landed (returned to the pool)
    }
}
