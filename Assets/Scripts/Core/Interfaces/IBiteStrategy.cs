namespace Tidebook
{
    public interface IBiteStrategy
    {
        BiteEvaluation Evaluate(PreferenceProfile p, Bait bait, WaterContext ctx);
    }
}
