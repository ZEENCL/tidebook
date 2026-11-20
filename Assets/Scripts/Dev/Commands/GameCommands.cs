// GameCommands.cs
// Dev commands that touch fish, coins and Tidebook progress. Each one is a Command with a receiver:
//   SpawnFishCommand      -> FishSpawner      (Undo removes the fish it made)
//   AddCoinsCommand       -> IWallet          (Undo takes the same amount back)
//   UnlockAllFishCommand  -> FishIndexLog     (Undo restores the progress it had before)

using System.Collections.Generic;
using Tidebook.Ui;

namespace Tidebook.Dev
{
    public class SpawnFishCommand : ICommand
    {
        readonly FishSpawner spawner;
        readonly FishSpecies species;
        readonly int count;
        readonly List<Fish> made = new List<Fish>();

        public SpawnFishCommand(FishSpawner spawner, FishSpecies species, int count)
        {
            this.spawner = spawner;
            this.species = species;
            this.count = count;
        }

        public string Description { get { return "Spawn " + count + " x " + species.displayName; } }

        public void Execute()
        {
            made.Clear();
            for (int i = 0; i < count; i++)
            {
                Fish fish = spawner.SpawnSpecies(species);
                if (fish != null) made.Add(fish);
            }
        }

        public void Undo()
        {
            // Fish that were already caught, pooled or are mid-fight are left alone (Despawn refuses them).
            for (int i = 0; i < made.Count; i++) spawner.Despawn(made[i]);
            made.Clear();
        }
    }

    public class AddCoinsCommand : ICommand
    {
        readonly IWallet wallet;
        readonly int amount;

        public AddCoinsCommand(IWallet wallet, int amount)
        {
            this.wallet = wallet;
            this.amount = amount;
        }

        public string Description { get { return "Add " + amount + " coins"; } }
        public void Execute() { wallet.Add(amount); }
        public void Undo() { wallet.Add(-amount); }
    }

    public class UnlockAllFishCommand : ICommand
    {
        readonly FishIndexLog log;
        FishIndexLog.EntryProgress[] before;

        public UnlockAllFishCommand(FishIndexLog log)
        {
            this.log = log;
        }

        public string Description { get { return "Unlock all fish"; } }

        public void Execute()
        {
            before = log.CaptureProgress();
            log.UnlockAll();
        }

        public void Undo()
        {
            if (before != null) log.RestoreProgress(before);
        }
    }
}
