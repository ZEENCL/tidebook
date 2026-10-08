namespace Tidebook
{
    public enum Buoyancy { Floating, Suspending, Sinking }
    public enum BodyShape { Slim, Round, Wide, Curly }                       // placeholder values
    public enum ColorPattern { RedWhite, BlueSilver, GreenBrown, Gold, Glow } // placeholder values
    public enum SwimPattern { Steady, Darting, Hopping, Drifting }           // placeholder values
    public enum DepthLayer { Top, Mid, Bottom }
    public enum TimeOfDay { Day, Night }
    public enum Weather { Clear, Rain, Storm, Fog }
    public enum Rarity { Common, Uncommon, Rare, Legendary }

    /// <summary>Snapshot of the conditions at a fishing spot. Fed into bite strategies.</summary>
    public struct WaterContext
    {
        public TimeOfDay time;
        public Weather weather;
        public string spotId;
    }

    // ------------------------------------------------------------------
    // State pattern plumbing (shared by RodController and Fish)
    // ------------------------------------------------------------------
    public class StateMachine
    {
        public IState Current { get; private set; }

        public string CurrentName
        {
            get { return Current == null ? "-" : Current.GetType().Name; }
        }

        /// <summary>Exit the old state, then enter the new one. Safe to call from inside a state's Tick
        /// (just 'return' right after calling it).</summary>
        public void ChangeState(IState next)
        {
            if (Current != null) Current.Exit();
            Current = next;
            if (Current != null) Current.Enter();
        }

        public void Tick(float dt)
        {
            if (Current != null) Current.Tick(dt);
        }
    }

    // ------------------------------------------------------------------
    // Bait (immutable) + BaitBuilder (Builder pattern)
    // ------------------------------------------------------------------
    public sealed class Bait
    {
        public readonly Buoyancy Buoyancy;
        public readonly BodyShape Shape;
        public readonly ColorPattern Color;
        public readonly SwimPattern Swim;

        public Bait(Buoyancy buoyancy, BodyShape shape, ColorPattern color, SwimPattern swim)
        {
            Buoyancy = buoyancy;
            Shape = shape;
            Color = color;
            Swim = swim;
        }

        public override string ToString()
        {
            return Buoyancy + " / " + Shape + " / " + Color + " / " + Swim;
        }
    }

    public class BaitBuilder
    {
        Buoyancy buoyancy = Buoyancy.Sinking;
        BodyShape shape = BodyShape.Slim;
        ColorPattern color = ColorPattern.RedWhite;
        SwimPattern swim = SwimPattern.Steady;

        public BaitBuilder WithBuoyancy(Buoyancy v) { buoyancy = v; return this; }
        public BaitBuilder WithShape(BodyShape v) { shape = v; return this; }
        public BaitBuilder WithColor(ColorPattern v) { color = v; return this; }
        public BaitBuilder WithSwim(SwimPattern v) { swim = v; return this; }

        public Bait Build()
        {
            return new Bait(buoyancy, shape, color, swim);
        }
    }
}
