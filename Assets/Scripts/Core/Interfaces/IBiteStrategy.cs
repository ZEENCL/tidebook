// IBiteStrategy.cs - Strategy pattern: scores how well a bait matches a fish.
namespace Tidebook
{
    public interface IBiteStrategy
    {
        BiteEvaluation Evaluate(PreferenceProfile p, Bait bait, WaterContext ctx);
    }
}
