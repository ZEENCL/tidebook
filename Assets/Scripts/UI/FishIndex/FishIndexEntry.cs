// FishIndexEntry.cs
// What the player has learned about one species, plus the read-only view the UI uses.

using System.Collections.Generic;

namespace Tidebook.Ui
{
    /// <summary>Which bait / world preferences of a fish the player has worked out so far.</summary>
    [System.Flags]
    public enum KnownFactors
    {
        None = 0,
        Buoyancy = 1,
        Shape = 2,
        Pattern = 4,
        Swim = 8,
        Time = 16,
        Weather = 32
    }

    public class FishIndexEntry
    {
        public int Number;                       // "No. 07" in the book
        public FishCatalogEntry Source;          // species + optional art / text
        public bool Discovered;
        public int TimesCaught;
        public KnownFactors Known;

        public string FirstSpot;
        public TimeOfDay FirstTime;
        public Weather FirstWeather;

        public FishSpecies Species { get { return Source.species; } }
    }

    public interface IFishIndexView
    {
        IReadOnlyList<FishIndexEntry> Entries { get; }
        int DiscoveredCount { get; }

        /// <summary>Raised whenever progress changes.</summary>
        event System.Action Changed;

        /// <summary>Raised the first time a species is caught.</summary>
        event System.Action<FishIndexEntry> Discovered;
    }
}
