// Bait.cs
// Bait (immutable product) + BaitBuilder (Builder pattern).

namespace Tidebook
{
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
