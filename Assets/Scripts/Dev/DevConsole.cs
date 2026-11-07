// DevConsole.cs
// The INVOKER side of the dev tools. Takes a line of text, looks the command up in the registry, lets its
// parser build a Command, and runs it through a CommandHistory. That is why SetWeather, SetHour, AddCoins,
// SpawnFish and UnlockAllFish can all be undone with "undo" (and redone with "redo").
// Hotkeys and the on-screen console both call Run(), so they behave identically.
// Plain C#: the on-screen part lives in DevConsoleScreen.

namespace Tidebook.Dev
{
    public struct DevResult
    {
        public readonly bool Ok;
        public readonly string Message;

        public DevResult(bool ok, string message)
        {
            Ok = ok;
            Message = message;
        }
    }

    public class DevConsole
    {
        readonly DevContext context;
        readonly DevCommandRegistry registry = new DevCommandRegistry();
        readonly CommandHistory history = new CommandHistory(200);

        public DevConsole(DevContext context)
        {
            this.context = context;
            DevCommandSet.RegisterAll(registry, context);
        }

        public DevContext Context { get { return context; } }
        public DevCommandRegistry Registry { get { return registry; } }
        public CommandHistory History { get { return history; } }

        /// <summary>Runs one line such as "weather rain" or "spawn bay perch 3". Never throws for bad input.</summary>
        public DevResult Run(string line)
        {
            line = line == null ? "" : line.Trim();
            if (line.Length == 0) return new DevResult(true, "");

            string[] parts = line.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
            string name = parts[0].ToLowerInvariant();
            string[] args = new string[parts.Length - 1];
            System.Array.Copy(parts, 1, args, 0, args.Length);

            // Commands about the history itself.
            if (name == "help" || name == "?") return new DevResult(true, HelpText());

            if (name == "undo")
            {
                ICommand undone = history.Undo();
                return undone == null ? new DevResult(false, "Nothing to undo.")
                                      : new DevResult(true, "Undid: " + undone.Description);
            }

            if (name == "redo")
            {
                ICommand redone = history.Redo();
                return redone == null ? new DevResult(false, "Nothing to redo.")
                                      : new DevResult(true, "Redid: " + redone.Description);
            }

            DevCommandInfo info;
            if (!registry.TryFind(name, out info))
                return new DevResult(false, "Unknown command '" + name + "'. Type 'help' to see them all.");

            ICommand command;
            try
            {
                command = info.Parse(args);
            }
            catch (DevCommandException e)
            {
                return new DevResult(false, e.Message);
            }

            history.Execute(command);
            return new DevResult(true, command.Description);
        }

        string HelpText()
        {
            string text = "Commands:";
            for (int i = 0; i < registry.All.Count; i++)
                text += "\n  " + registry.All[i].Usage + "  -  " + registry.All[i].Help;
            text += "\n  undo / redo  -  take back or re-apply the last command";
            text += "\n  clear  -  empty this window";
            return text;
        }
    }
}
