// EquippedBait.cs
// The bait the rod will cast. Implements IBaitProvider, so assigning it to RodController.BaitProvider
// is all that is needed to connect the builder UI to the game (TidebookUiController does this for you).

namespace Tidebook.Ui
{
    public class EquippedBait : IBaitProvider
    {
        public BaitSelection Selection { get; private set; }

        public EquippedBait()
        {
            Selection = new BaitSelection();
        }

        public Bait BuildBait()
        {
            return Selection.Build();
        }
    }
}
