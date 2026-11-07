// DevCommandSet.cs
// Registers the dev commands and holds their parsers. A parser only validates what was typed and builds the
// Command object (the CLIENT role); the Command itself does the work when the history runs it.
//
//   weather <clear|rain|storm|fog|next>       -> SetWeatherCommand
//   hour <0-24|day|night|+N|-N> [hold|run]    -> SetHourCommand
//   spawn <fish name> [count]                 -> SpawnFishCommand
//   coins <amount>                            -> AddCoinsCommand
//   unlockall                                 -> UnlockAllFishCommand

using System.Globalization;

namespace Tidebook.Dev
{
    public static class DevCommandSet
    {
        const int MaxSpawn = 20;
        const int MaxCoins = 1000000;

        public static void RegisterAll(DevCommandRegistry registry, DevContext ctx)
        {
            registry.Register("weather", "weather <clear|rain|storm|fog|next>", "change the weather",
                              delegate(string[] a) { return ParseWeather(ctx, a); }, "setweather");

            registry.Register("hour", "hour <0-24|day|night|+N|-N> [hold|run]", "set the time (hold freezes the clock)",
                              delegate(string[] a) { return ParseHour(ctx, a); }, "sethour");

            registry.Register("spawn", "spawn <fish name> [count]", "spawn fish in the nearest water zone",
                              delegate(string[] a) { return ParseSpawn(ctx, a); }, "spawnfish");

            registry.Register("coins", "coins <amount>", "add coins to the wallet",
                              delegate(string[] a) { return ParseCoins(ctx, a); }, "addcoins");

            registry.Register("unlockall", "unlockall", "mark every fish as found in the Tidebook",
                              delegate(string[] a) { return ParseUnlockAll(ctx, a); }, "unlockallfish");
        }

        // ------------------------------------------------------------ parsers

        static ICommand ParseWeather(DevContext ctx, string[] a)
        {
            WorldState world = ctx.RequireWorld();
            if (a.Length != 1) throw new DevCommandException("Usage: weather <clear|rain|storm|fog|next>");

            Weather value;
            if (a[0].ToLowerInvariant() == "next")
            {
                int count = System.Enum.GetValues(typeof(Weather)).Length;
                value = (Weather)(((int)world.weather + 1) % count);
            }
            else if (!TryEnum(a[0], out value))
            {
                throw new DevCommandException("Unknown weather '" + a[0] + "'. Try clear, rain, storm, fog or next.");
            }

            if (value == world.weather) throw new DevCommandException("The weather is already " + value + ".");
            return new SetWeatherCommand(world, value);
        }

        static ICommand ParseHour(DevContext ctx, string[] a)
        {
            WorldState world = ctx.RequireWorld();
            const string usage = "Usage: hour <0-24 | day | night | +N | -N> [hold|run]";
            if (a.Length < 1 || a.Length > 2) throw new DevCommandException(usage);

            TimeFlow flow = TimeFlow.Keep;
            if (a.Length == 2)
            {
                string word = a[1].ToLowerInvariant();
                if (word == "hold") flow = TimeFlow.Hold;
                else if (word == "run") flow = TimeFlow.Run;
                else throw new DevCommandException(usage);
            }

            string t = a[0].ToLowerInvariant();
            float hour;
            if (t == "day")
            {
                hour = 12f;
            }
            else if (t == "night")
            {
                hour = 22f;
            }
            else
            {
                float number;
                if (!float.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                    throw new DevCommandException(usage);

                bool relative = t[0] == '+' || t[0] == '-';
                if (!relative && (number < 0f || number > 24f)) throw new DevCommandException("The hour must be between 0 and 24.");
                hour = relative ? world.hour + number : number;
            }

            return new SetHourCommand(world, hour, flow);
        }

        static ICommand ParseSpawn(DevContext ctx, string[] a)
        {
            if (a.Length == 0)
            {
                string names = "";
                System.Collections.Generic.List<FishSpecies> all = ctx.AllSpecies();
                for (int i = 0; i < all.Count; i++) names += (i > 0 ? ", " : "") + all[i].displayName;
                throw new DevCommandException("Usage: spawn <fish name> [count]. Fish: " + (names.Length > 0 ? names : "(none found)"));
            }

            // "spawn bay perch 3": a trailing whole number is the count, everything before it is the name.
            int count = 1;
            int nameWords = a.Length;
            int parsed;
            if (a.Length > 1 && int.TryParse(a[a.Length - 1], out parsed))
            {
                count = parsed;
                nameWords = a.Length - 1;
            }
            if (count < 1 || count > MaxSpawn) throw new DevCommandException("The count must be between 1 and " + MaxSpawn + ".");

            FishSpecies species = ctx.FindSpecies(string.Join(" ", a, 0, nameWords));
            FishSpawner spawner = ctx.FindSpawnerFor(species);
            return new SpawnFishCommand(spawner, species, count);
        }

        static ICommand ParseCoins(DevContext ctx, string[] a)
        {
            IWallet wallet = ctx.RequireWallet();
            int amount;
            if (a.Length != 1 || !int.TryParse(a[0], out amount) || amount < 1 || amount > MaxCoins)
                throw new DevCommandException("Usage: coins <amount from 1 to " + MaxCoins + ">");
            return new AddCoinsCommand(wallet, amount);
        }

        static ICommand ParseUnlockAll(DevContext ctx, string[] a)
        {
            return new UnlockAllFishCommand(ctx.RequireIndex());
        }

        // ------------------------------------------------------------ helpers

        /// <summary>Case-insensitive enum parse that rejects raw numbers outside the enum.</summary>
        static bool TryEnum<T>(string text, out T value) where T : struct
        {
            return System.Enum.TryParse(text, true, out value) && System.Enum.IsDefined(typeof(T), value);
        }
    }
}
