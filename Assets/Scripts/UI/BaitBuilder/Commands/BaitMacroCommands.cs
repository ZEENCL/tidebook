// BaitMacroCommands.cs
// Commands that change several bait axes at once. They are CompositeCommands built from the single-axis
// commands, so one Undo / Redo takes back or re-applies the whole group as a single step.
// Axes that would not change are left out, so a macro never records "nothing happened" steps.
// Check IsEmpty before handing one to the history.

using UnityEngine;

namespace Tidebook.Ui
{
    public abstract class BaitMacroCommand : CompositeCommand
    {
        protected BaitMacroCommand(string description) : base(description) { }

        protected void AddChanges(BaitSelection target, Buoyancy buoyancy, BodyShape shape,
                                  ColorPattern pattern, SwimPattern swim)
        {
            if (target.Buoyancy != buoyancy) Add(new SetBuoyancyCommand(target, buoyancy));
            if (target.Shape != shape) Add(new SetShapeCommand(target, shape));
            if (target.Pattern != pattern) Add(new SetPatternCommand(target, pattern));
            if (target.Swim != swim) Add(new SetSwimCommand(target, swim));
        }
    }

    /// <summary>Rolls a random bait. The dice are rolled once, in the constructor, so Redo gives the same bait.</summary>
    public class RandomizeBaitCommand : BaitMacroCommand
    {
        public RandomizeBaitCommand(BaitSelection target) : base("Randomize bait")
        {
            Buoyancy buoyancy = RandomValue<Buoyancy>();
            BodyShape shape = RandomValue<BodyShape>();
            ColorPattern pattern = RandomValue<ColorPattern>();
            SwimPattern swim = RandomValue<SwimPattern>();

            // Never roll the bait the player already has: force one random axis to something else.
            bool unchanged = buoyancy == target.Buoyancy && shape == target.Shape
                             && pattern == target.Pattern && swim == target.Swim;
            if (unchanged)
            {
                switch (Random.Range(0, 4))
                {
                    case 0: buoyancy = Different(buoyancy); break;
                    case 1: shape = Different(shape); break;
                    case 2: pattern = Different(pattern); break;
                    default: swim = Different(swim); break;
                }
            }

            AddChanges(target, buoyancy, shape, pattern, swim);
        }

        static T RandomValue<T>()
        {
            System.Array values = System.Enum.GetValues(typeof(T));
            return (T)values.GetValue(Random.Range(0, values.Length));
        }

        static T Different<T>(T current)
        {
            System.Array values = System.Enum.GetValues(typeof(T));
            int index = System.Array.IndexOf(values, current);
            int next = (index + 1 + Random.Range(0, values.Length - 1)) % values.Length;
            return (T)values.GetValue(next);
        }
    }

    /// <summary>Puts the draft back to a given bait (the equipped one). This is what the Revert button does.</summary>
    public class ResetBaitCommand : BaitMacroCommand
    {
        public ResetBaitCommand(BaitSelection target, BaitSelection source) : base("Revert to equipped bait")
        {
            AddChanges(target, source.Buoyancy, source.Shape, source.Pattern, source.Swim);
        }
    }
}
