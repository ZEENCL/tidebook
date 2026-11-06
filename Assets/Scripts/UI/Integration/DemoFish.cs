// DemoFish.cs
// A dozen sample species so the Tidebook can be shown (and screenshotted) in an empty scene.
// Only used when no FishCatalog is assigned and no FishSpawner in the scene has any species.
// These are runtime-only objects; they are never saved as assets.

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook.Ui
{
    public static class DemoFish
    {
        public static List<FishSpecies> Create()
        {
            List<FishSpecies> list = new List<FishSpecies>();
            FishSpecies s;

            s = Make("Bay Perch", Rarity.Common, DepthLayer.Mid, 12, 0.2f, "Bay");
            s.profile.buoyancy = Buoyancy.Sinking; s.profile.buoyancyW.weight = 2f;
            s.profile.color = ColorPattern.RedWhite; s.profile.colorW.weight = 1f;
            list.Add(s);

            s = Make("Dock Minnow", Rarity.Common, DepthLayer.Top, 10, 0.1f, "Bay");
            s.profile.buoyancy = Buoyancy.Floating; s.profile.buoyancyW.weight = 2f;
            s.profile.shape = BodyShape.Round; s.profile.shapeW.weight = 1f;
            s.profile.swim = SwimPattern.Hopping; s.profile.swimW.weight = 1f;
            list.Add(s);

            s = Make("Reed Darter", Rarity.Common, DepthLayer.Mid, 14, 0.3f, "River");
            s.profile.swim = SwimPattern.Darting; s.profile.swimW.weight = 2f;
            s.profile.color = ColorPattern.GreenBrown; s.profile.colorW.weight = 1f;
            list.Add(s);

            s = Make("Silver Tench", Rarity.Common, DepthLayer.Bottom, 15, 0.25f, "Bay", "River");
            s.profile.buoyancy = Buoyancy.Sinking; s.profile.buoyancyW.weight = 1f;
            s.profile.color = ColorPattern.BlueSilver; s.profile.colorW.weight = 2f;
            list.Add(s);

            s = Make("Mud Whisker", Rarity.Uncommon, DepthLayer.Bottom, 28, 0.4f, "River");
            s.profile.buoyancy = Buoyancy.Sinking; s.profile.buoyancyW.weight = 2f;
            s.profile.shape = BodyShape.Curly; s.profile.shapeW.weight = 2f;
            s.profile.swim = SwimPattern.Drifting; s.profile.swimW.weight = 1f;
            list.Add(s);

            s = Make("Lantern Carp", Rarity.Uncommon, DepthLayer.Mid, 32, 0.35f, "Lake");
            s.profile.color = ColorPattern.Glow; s.profile.colorW.weight = 2f;
            s.profile.buoyancy = Buoyancy.Suspending; s.profile.buoyancyW.weight = 1f;
            s.profile.time = TimeOfDay.Night; s.profile.timeW.weight = 2f;
            list.Add(s);

            s = Make("Rain Trout", Rarity.Uncommon, DepthLayer.Top, 30, 0.45f, "River");
            s.profile.color = ColorPattern.Gold; s.profile.colorW.weight = 1f;
            s.profile.swim = SwimPattern.Hopping; s.profile.swimW.weight = 2f;
            s.profile.weather = Weather.Rain; s.profile.weatherW.weight = 2f;
            list.Add(s);

            s = Make("Glass Eel", Rarity.Uncommon, DepthLayer.Bottom, 34, 0.4f, "Lake");
            s.profile.shape = BodyShape.Curly; s.profile.shapeW.weight = 2f;
            s.profile.swim = SwimPattern.Darting; s.profile.swimW.weight = 1f;
            list.Add(s);

            s = Make("Moonfin", Rarity.Rare, DepthLayer.Bottom, 60, 0.7f, "Deep Cove");
            s.biteStrategy = BiteStrategyType.Gated;
            s.profile.time = TimeOfDay.Night; s.profile.timeW.weight = 2f; s.profile.timeW.required = true;
            s.profile.buoyancy = Buoyancy.Sinking; s.profile.buoyancyW.weight = 2f;
            s.profile.swim = SwimPattern.Drifting; s.profile.swimW.weight = 1f;
            s.profile.color = ColorPattern.Glow; s.profile.colorW.weight = 1f;
            list.Add(s);

            s = Make("Storm Pike", Rarity.Rare, DepthLayer.Mid, 70, 0.75f, "Lake");
            s.biteStrategy = BiteStrategyType.Gated;
            s.profile.weather = Weather.Storm; s.profile.weatherW.weight = 2f; s.profile.weatherW.required = true;
            s.profile.shape = BodyShape.Wide; s.profile.shapeW.weight = 2f;
            s.profile.swim = SwimPattern.Darting; s.profile.swimW.weight = 2f;
            s.profile.color = ColorPattern.RedWhite; s.profile.colorW.weight = 1f;
            list.Add(s);

            s = Make("Gilded Koi", Rarity.Legendary, DepthLayer.Top, 150, 0.85f, "Bay");
            s.biteStrategy = BiteStrategyType.Gated;
            s.profile.time = TimeOfDay.Day; s.profile.timeW.weight = 1f; s.profile.timeW.required = true;
            s.profile.color = ColorPattern.Gold; s.profile.colorW.weight = 3f;
            s.profile.buoyancy = Buoyancy.Floating; s.profile.buoyancyW.weight = 2f;
            s.profile.swim = SwimPattern.Steady; s.profile.swimW.weight = 1f;
            list.Add(s);

            s = Make("Aurora Ray", Rarity.Legendary, DepthLayer.Mid, 200, 0.9f, "Deep Cove");
            s.biteStrategy = BiteStrategyType.Gated;
            s.profile.time = TimeOfDay.Night; s.profile.timeW.weight = 1f; s.profile.timeW.required = true;
            s.profile.weather = Weather.Fog; s.profile.weatherW.weight = 1f; s.profile.weatherW.required = true;
            s.profile.buoyancy = Buoyancy.Suspending; s.profile.buoyancyW.weight = 2f;
            s.profile.color = ColorPattern.Glow; s.profile.colorW.weight = 2f;
            s.profile.swim = SwimPattern.Drifting; s.profile.swimW.weight = 2f;
            s.profile.shape = BodyShape.Round; s.profile.shapeW.weight = 1f;
            list.Add(s);

            return list;
        }

        static FishSpecies Make(string name, Rarity rarity, DepthLayer layer, int value, float fight, params string[] spots)
        {
            FishSpecies s = ScriptableObject.CreateInstance<FishSpecies>();
            s.name = name;
            s.displayName = name;
            s.rarity = rarity;
            s.layer = layer;
            s.currencyValue = value;
            s.fightStrength = fight;
            s.allowedSpotIds = spots;
            return s;
        }
    }
}
