// CompositeCommand.cs
// A "macro" command: several commands that count as ONE step in the history, so a single Undo
// takes back the whole group. Undo runs the children in reverse order (last done, first undone).

using System.Collections.Generic;

namespace Tidebook
{
    public class CompositeCommand : ICommand
    {
        readonly List<ICommand> children = new List<ICommand>();
        readonly string description;

        public CompositeCommand(string description)
        {
            this.description = description;
        }

        public string Description { get { return description; } }
        public int Count { get { return children.Count; } }
        public bool IsEmpty { get { return children.Count == 0; } }

        public CompositeCommand Add(ICommand child)
        {
            if (child != null) children.Add(child);
            return this;
        }

        public void Execute()
        {
            for (int i = 0; i < children.Count; i++) children[i].Execute();
        }

        public void Undo()
        {
            for (int i = children.Count - 1; i >= 0; i--) children[i].Undo();
        }
    }
}
