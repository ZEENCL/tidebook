// DevCommandRegistry.cs
// The registry: maps a typed word ("weather", "spawn"...) to a parser that turns the typed arguments
// into a Command object. The console never needs to know about any specific command, so adding a new
// one is a single Register(...) call (see DevCommandSet) and nothing else changes.

using System.Collections.Generic;

namespace Tidebook.Dev
{
    /// <summary>Turns typed arguments into a command, or throws DevCommandException with a message for the player.</summary>
    public delegate ICommand DevCommandParser(string[] args);

    public class DevCommandInfo
    {
        public readonly string Name;
        public readonly string Usage;
        public readonly string Help;
        public readonly DevCommandParser Parse;

        public DevCommandInfo(string name, string usage, string help, DevCommandParser parse)
        {
            Name = name;
            Usage = usage;
            Help = help;
            Parse = parse;
        }
    }

    public class DevCommandRegistry
    {
        readonly Dictionary<string, DevCommandInfo> byName =
            new Dictionary<string, DevCommandInfo>(System.StringComparer.OrdinalIgnoreCase);
        readonly List<DevCommandInfo> all = new List<DevCommandInfo>();

        /// <summary>Every registered command once, in registration order (aliases are not repeated).</summary>
        public IList<DevCommandInfo> All { get { return all; } }

        public void Register(string name, string usage, string help, DevCommandParser parse, params string[] aliases)
        {
            DevCommandInfo info = new DevCommandInfo(name, usage, help, parse);
            all.Add(info);
            byName[name] = info;
            for (int i = 0; i < aliases.Length; i++) byName[aliases[i]] = info;
        }

        public bool TryFind(string name, out DevCommandInfo info)
        {
            return byName.TryGetValue(name, out info);
        }
    }
}
