namespace Tidebook
{
    /// <summary>On the line. The rod's ReelingState runs the fight; the fish just holds here.</summary>
    public class HookedState : FishState
    {
        public HookedState(FishBrain brain) : base(brain) { }
    }
}
