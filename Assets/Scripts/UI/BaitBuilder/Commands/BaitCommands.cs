// BaitCommands.cs
// Command pattern for the Bait Builder. One command per chip the player can click.
//   Receiver : BaitSelection (the draft being edited)
//   Invoker  : CommandHistory (owned by BaitBuilderScreen)
//   Client   : BaitBuilderScreen, which creates a command per click and hands it to the history
//
// Each command remembers the value it replaced at Execute() time, so Undo() can put it back.
// (Looking the old value up at Execute rather than in the constructor keeps commands correct
// when they run inside a CompositeCommand or are redone later.)

namespace Tidebook.Ui
{
    public class SetBuoyancyCommand : ICommand
    {
        readonly BaitSelection target;
        readonly Buoyancy newValue;
        Buoyancy oldValue;

        public SetBuoyancyCommand(BaitSelection target, Buoyancy value)
        {
            this.target = target;
            newValue = value;
        }

        public string Description { get { return "Buoyancy: " + BaitText.Name(newValue); } }
        public void Execute() { oldValue = target.Buoyancy; target.SetBuoyancy(newValue); }
        public void Undo() { target.SetBuoyancy(oldValue); }
    }

    public class SetShapeCommand : ICommand
    {
        readonly BaitSelection target;
        readonly BodyShape newValue;
        BodyShape oldValue;

        public SetShapeCommand(BaitSelection target, BodyShape value)
        {
            this.target = target;
            newValue = value;
        }

        public string Description { get { return "Shape: " + BaitText.Name(newValue); } }
        public void Execute() { oldValue = target.Shape; target.SetShape(newValue); }
        public void Undo() { target.SetShape(oldValue); }
    }

    public class SetPatternCommand : ICommand
    {
        readonly BaitSelection target;
        readonly ColorPattern newValue;
        ColorPattern oldValue;

        public SetPatternCommand(BaitSelection target, ColorPattern value)
        {
            this.target = target;
            newValue = value;
        }

        public string Description { get { return "Color: " + BaitText.Name(newValue); } }
        public void Execute() { oldValue = target.Pattern; target.SetPattern(newValue); }
        public void Undo() { target.SetPattern(oldValue); }
    }

    public class SetSwimCommand : ICommand
    {
        readonly BaitSelection target;
        readonly SwimPattern newValue;
        SwimPattern oldValue;

        public SetSwimCommand(BaitSelection target, SwimPattern value)
        {
            this.target = target;
            newValue = value;
        }

        public string Description { get { return "Swim: " + BaitText.Name(newValue); } }
        public void Execute() { oldValue = target.Swim; target.SetSwim(newValue); }
        public void Undo() { target.SetSwim(oldValue); }
    }
}
