// ICommand.cs
// Command pattern: one user action packaged as an object that knows how to do itself AND how to take
// itself back. Commands hold a reference to the RECEIVER (the object that really changes, e.g. BaitSelection,
// WorldState) plus whatever they need to restore the previous state. The INVOKER (CommandHistory) just
// runs them and remembers them; it never needs to know what they do.

namespace Tidebook
{
    public interface ICommand
    {
        /// <summary>Short human-readable label, e.g. "Swim: Darting". Shown in the UI / console.</summary>
        string Description { get; }

        /// <summary>Applies the change. Must be safe to call again after Undo() (that is how Redo works).</summary>
        void Execute();

        /// <summary>Restores exactly what Execute() changed.</summary>
        void Undo();
    }
}
