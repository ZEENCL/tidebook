// BaitSelection.cs
// The four bait axes the player is choosing, plus a Changed event so the UI can react.
// EquippedBait (below) is the one the rod actually casts; the builder edits a separate draft copy.

using System.Collections.Generic;

namespace Tidebook.Ui
{
    public class BaitSelection
    {
        public Buoyancy Buoyancy { get; private set; }
        public BodyShape Shape { get; private set; }
        public ColorPattern Pattern { get; private set; }
        public SwimPattern Swim { get; private set; }

        public event System.Action Changed;

        public BaitSelection()
        {
            Buoyancy = Buoyancy.Sinking;
            Shape = BodyShape.Slim;
            Pattern = ColorPattern.RedWhite;
            Swim = SwimPattern.Steady;
        }

        public void SetBuoyancy(Buoyancy value) { if (Buoyancy != value) { Buoyancy = value; Raise(); } }
        public void SetShape(BodyShape value) { if (Shape != value) { Shape = value; Raise(); } }
        public void SetPattern(ColorPattern value) { if (Pattern != value) { Pattern = value; Raise(); } }
        public void SetSwim(SwimPattern value) { if (Swim != value) { Swim = value; Raise(); } }

        public void Set(Buoyancy buoyancy, BodyShape shape, ColorPattern pattern, SwimPattern swim)
        {
            Buoyancy = buoyancy;
            Shape = shape;
            Pattern = pattern;
            Swim = swim;
            Raise();
        }

        public void CopyFrom(BaitSelection other)
        {
            Set(other.Buoyancy, other.Shape, other.Pattern, other.Swim);
        }

        public bool SameAs(BaitSelection other)
        {
            return other != null && Buoyancy == other.Buoyancy && Shape == other.Shape
                   && Pattern == other.Pattern && Swim == other.Swim;
        }

        public Bait Build()
        {
            return new BaitBuilder()
                .WithBuoyancy(Buoyancy)
                .WithShape(Shape)
                .WithColor(Pattern)
                .WithSwim(Swim)
                .Build();
        }

        void Raise()
        {
            System.Action handler = Changed;
            if (handler != null) handler();
        }
    }
}
