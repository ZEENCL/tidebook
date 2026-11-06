// BaitText.cs
// Player-facing names and short descriptions for every bait option.

namespace Tidebook.Ui
{
    public static class BaitText
    {
        public static string Name(Buoyancy b)
        {
            switch (b)
            {
                case Buoyancy.Floating: return "Floating";
                case Buoyancy.Suspending: return "Suspending";
                default: return "Sinking";
            }
        }

        public static string Name(BodyShape s)
        {
            switch (s)
            {
                case BodyShape.Round: return "Round";
                case BodyShape.Wide: return "Wide";
                case BodyShape.Curly: return "Curly";
                default: return "Slim";
            }
        }

        public static string Name(ColorPattern c)
        {
            switch (c)
            {
                case ColorPattern.BlueSilver: return "Blue & Silver";
                case ColorPattern.GreenBrown: return "Green & Brown";
                case ColorPattern.Gold: return "Gold";
                case ColorPattern.Glow: return "Glow";
                default: return "Red & White";
            }
        }

        public static string Name(SwimPattern s)
        {
            switch (s)
            {
                case SwimPattern.Darting: return "Darting";
                case SwimPattern.Hopping: return "Hopping";
                case SwimPattern.Drifting: return "Drifting";
                default: return "Steady";
            }
        }

        public static string Describe(Buoyancy b)
        {
            switch (b)
            {
                case Buoyancy.Floating: return "Rides on the surface";
                case Buoyancy.Suspending: return "Hangs in mid-water";
                default: return "Drops to the bottom";
            }
        }

        public static string Describe(SwimPattern s)
        {
            switch (s)
            {
                case SwimPattern.Darting: return "Quick bursts and pauses";
                case SwimPattern.Hopping: return "Little hops up and down";
                case SwimPattern.Drifting: return "A slow, lazy drift";
                default: return "Glides at an even pace";
            }
        }

        /// <summary>"Red & White Slim Lure", "Gold Curly Worm"...</summary>
        public static string FullName(BaitSelection sel)
        {
            string kind = sel.Shape == BodyShape.Curly ? "Worm" : "Lure";
            return Name(sel.Pattern) + " " + Name(sel.Shape) + " " + kind;
        }

        public static string Summary(BaitSelection sel)
        {
            return Name(sel.Buoyancy) + "  /  " + Name(sel.Swim) + " swim";
        }
    }
}
