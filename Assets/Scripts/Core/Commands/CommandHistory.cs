// CommandHistory.cs
// The INVOKER of the Command pattern. Runs commands and keeps an undo stack and a redo stack.
//   Execute(cmd) -> runs it, pushes it on the undo stack, and clears redo (a new action ends the "future").
//   Undo()       -> pops the newest command, calls Undo() on it, moves it to the redo stack.
//   Redo()       -> pops the newest undone command, calls Execute() again, moves it back.
// Plain C# (no Unity types), so it can be reused for any feature that needs undo.

using System.Collections.Generic;

namespace Tidebook
{
    public class CommandHistory
    {
        readonly List<ICommand> undoStack = new List<ICommand>();   // newest at the end
        readonly List<ICommand> redoStack = new List<ICommand>();
        readonly int capacity;

        /// <summary>Raised after anything changes (execute, undo, redo, clear) so buttons can refresh.</summary>
        public event System.Action Changed;

        /// <param name="capacity">Oldest commands are forgotten beyond this many. 0 = unlimited.</param>
        public CommandHistory(int capacity = 100)
        {
            this.capacity = capacity;
        }

        public bool CanUndo { get { return undoStack.Count > 0; } }
        public bool CanRedo { get { return redoStack.Count > 0; } }
        public int UndoCount { get { return undoStack.Count; } }
        public int RedoCount { get { return redoStack.Count; } }

        /// <summary>Label of the command Undo() would take back, or null.</summary>
        public string UndoLabel { get { return CanUndo ? undoStack[undoStack.Count - 1].Description : null; } }

        /// <summary>Label of the command Redo() would re-apply, or null.</summary>
        public string RedoLabel { get { return CanRedo ? redoStack[redoStack.Count - 1].Description : null; } }

        /// <summary>Runs the command and records it so it can be undone.</summary>
        public void Execute(ICommand command)
        {
            if (command == null) return;

            command.Execute();
            undoStack.Add(command);
            redoStack.Clear();

            if (capacity > 0 && undoStack.Count > capacity) undoStack.RemoveAt(0);
            Raise();
        }

        /// <summary>Takes back the newest command. Returns it (or null when there was nothing to undo).</summary>
        public ICommand Undo()
        {
            if (!CanUndo) return null;

            ICommand command = undoStack[undoStack.Count - 1];
            undoStack.RemoveAt(undoStack.Count - 1);
            command.Undo();
            redoStack.Add(command);
            Raise();
            return command;
        }

        /// <summary>Re-applies the newest undone command. Returns it (or null when there was nothing to redo).</summary>
        public ICommand Redo()
        {
            if (!CanRedo) return null;

            ICommand command = redoStack[redoStack.Count - 1];
            redoStack.RemoveAt(redoStack.Count - 1);
            command.Execute();
            undoStack.Add(command);
            Raise();
            return command;
        }

        /// <summary>Forgets everything (use when the thing being edited is replaced wholesale).</summary>
        public void Clear()
        {
            if (undoStack.Count == 0 && redoStack.Count == 0) return;
            undoStack.Clear();
            redoStack.Clear();
            Raise();
        }

        void Raise()
        {
            System.Action handler = Changed;
            if (handler != null) handler();
        }
    }
}
