// CatchResultViewModel.cs
// MVVM for the catch result card.
//   Model      : the rod's CatchReport (what happened) and CatchRewards (what it pays)
//   ViewModel  : this class. Listens to the rod's catch events and turns each one into a CatchCard that is
//                ready to show: the words, the coins, whether it is a new Tidebook entry.
//   View       : CatchBannerView, which binds to Card and only decides colours, icon and animation
// The wording and reward rules used to live inside the View. Now the View has no gameplay logic in it.

namespace Tidebook.Ui
{
    /// <summary>Which kind of result the card shows. The View maps this to a colour.</summary>
    public enum CardTone { Caught, Snapped, Escaped, Missed }

    /// <summary>Everything the card needs. A new CatchCard is made for every catch.</summary>
    public class CatchCard
    {
        public readonly CardTone Tone;
        public readonly string Title;
        public readonly string FishName;
        public readonly string Detail;
        public readonly FishSpecies Species;   // for the icon and rarity colour; null when unknown
        public readonly int Coins;
        public readonly bool IsNewEntry;

        public CatchCard(CardTone tone, string title, string fishName, string detail,
                         FishSpecies species, int coins, bool isNewEntry)
        {
            Tone = tone;
            Title = title;
            FishName = fishName;
            Detail = detail;
            Species = species;
            Coins = coins;
            IsNewEntry = isNewEntry;
        }
    }

    public class CatchResultViewModel : ICatchListener
    {
        readonly System.Func<FishSpecies, FishIndexEntry> entryFor;

        /// <summary>The newest card. Null until the first catch. Each catch assigns a new object, so Changed fires every time.</summary>
        public readonly Observable<CatchCard> Card = new Observable<CatchCard>();

        /// <param name="entryFor">Looks a species up in the Tidebook (used to tell whether this catch is a first).</param>
        public CatchResultViewModel(System.Func<FishSpecies, FishIndexEntry> entryFor)
        {
            this.entryFor = entryFor;
        }

        public void OnCatchResolved(CatchReport report)
        {
            if (report.Result == CatchResult.Retrieved) return;   // reeling in an empty line shows nothing

            FishSpecies species = report.Info != null ? report.Info.Species : null;

            switch (report.Result)
            {
                case CatchResult.Caught:
                {
                    int coins = CatchRewards.CoinsFor(report);
                    FishIndexEntry entry = (entryFor != null && species != null) ? entryFor(species) : null;
                    bool isNew = entry != null && entry.TimesCaught == 1;   // the Tidebook has already counted this catch

                    Card.Value = new CatchCard(
                        CardTone.Caught, "CAUGHT!",
                        species != null ? species.displayName : "A fish",
                        species != null ? species.rarity + "     +" + coins + " coins" : "",
                        species, coins, isNew);
                    break;
                }
                case CatchResult.Snapped:
                    Card.Value = new CatchCard(CardTone.Snapped, "LINE SNAPPED", "It got away!",
                        "Tip: let go of the reel before the bar reaches SNAP.", species, 0, false);
                    break;
                case CatchResult.Escaped:
                    Card.Value = new CatchCard(CardTone.Escaped, "SLIPPED OFF", "It got away!",
                        "Tip: keep the needle inside the green band to make progress.", species, 0, false);
                    break;
                default:   // Missed
                    Card.Value = new CatchCard(CardTone.Missed, "TOO SLOW", "It spat the bait out.",
                        "Tip: click as soon as you see the ! above your bobber.", species, 0, false);
                    break;
            }
        }
    }
}
