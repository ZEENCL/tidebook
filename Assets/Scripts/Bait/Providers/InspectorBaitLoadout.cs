// InspectorBaitLoadout.cs
// Placeholder bait source: pick the four bait axes in the Inspector.
// When the bait-builder UI exists, write another IBaitProvider and assign it to RodController.BaitProvider.

using UnityEngine;

namespace Tidebook
{
    public class InspectorBaitLoadout : MonoBehaviour, IBaitProvider
    {
        public Buoyancy buoyancy = Buoyancy.Sinking;
        public BodyShape shape = BodyShape.Slim;
        public ColorPattern colorPattern = ColorPattern.RedWhite;
        public SwimPattern swim = SwimPattern.Steady;

        public Bait BuildBait()
        {
            return new BaitBuilder()
                .WithBuoyancy(buoyancy)
                .WithShape(shape)
                .WithColor(colorPattern)
                .WithSwim(swim)
                .Build();
        }
    }
}
