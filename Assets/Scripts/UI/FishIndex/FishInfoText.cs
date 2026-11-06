// FishInfoText.cs
// Turns fish data into short player-facing text for the Tidebook and the bait notes.

using UnityEngine;

namespace Tidebook.Ui
{
    public static class FishInfoText
    {
        public static readonly KnownFactors[] AllFactors =
        {
            KnownFactors.Buoyancy, KnownFactors.Shape, KnownFactors.Pattern,
            KnownFactors.Swim, KnownFactors.Time, KnownFactors.Weather
        };

        // ---------------------------------------------------------------- general info

        public static string RarityName(Rarity rarity)
        {
            return rarity.ToString();
        }

        public static string Depth(DepthLayer layer)
        {
            switch (layer)
            {
                case DepthLayer.Top: return "Top water";
                case DepthLayer.Bottom: return "Near the bottom";
                default: return "Mid water";
            }
        }

        public static string Habitat(FishSpecies species)
        {
            if (species.allowedSpotIds == null || species.allowedSpotIds.Length == 0) return "Anywhere";
            return string.Join(", ", species.allowedSpotIds);
        }

        public static string ActiveTime(FishSpecies species)
        {
            PreferenceProfile p = species.profile;
            string time = p.time == TimeOfDay.Night ? "night" : "daytime";
            if (p.timeW.required) return char.ToUpper(time[0]) + time.Substring(1) + " only";
            if (p.timeW.weight > 0f) return "Prefers " + time;
            return "Day or night";
        }

        public static string WeatherText(FishSpecies species)
        {
            PreferenceProfile p = species.profile;
            if (p.weatherW.required) return Capitalize(WeatherNoun(p.weather)) + " only";
            if (p.weatherW.weight > 0f) return "Prefers " + WeatherNoun(p.weather);
            return "Any weather";
        }

        public static string WeatherNoun(Weather weather)
        {
            switch (weather)
            {
                case Weather.Rain: return "rain";
                case Weather.Storm: return "storms";
                case Weather.Fog: return "fog";
                default: return "clear skies";
            }
        }

        static string Capitalize(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return char.ToUpper(text[0]) + text.Substring(1);
        }

        public static string Blurb(FishCatalogEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.blurb)) return entry.blurb;

            FishSpecies s = entry.species;
            string rarity = s.rarity.ToString().ToLower();
            string article = (rarity[0] == 'u' || rarity[0] == 'a' || rarity[0] == 'e' || rarity[0] == 'i' || rarity[0] == 'o') ? "An " : "A ";

            string where;
            switch (s.layer)
            {
                case DepthLayer.Top: where = "near the surface"; break;
                case DepthLayer.Bottom: where = "close to the bottom"; break;
                default: where = "in open water"; break;
            }

            string look = (s.allowedSpotIds == null || s.allowedSpotIds.Length == 0)
                ? "You can find it in most waters."
                : "Look for it around " + Habitat(s) + ".";

            return article + rarity + " fish that swims " + where + ". " + look;
        }

        // ---------------------------------------------------------------- bait notes

        public static bool HasWeight(KnownFactors factor, PreferenceProfile p)
        {
            switch (factor)
            {
                case KnownFactors.Buoyancy: return p.buoyancyW.weight > 0f;
                case KnownFactors.Shape: return p.shapeW.weight > 0f;
                case KnownFactors.Pattern: return p.colorW.weight > 0f;
                case KnownFactors.Swim: return p.swimW.weight > 0f;
                case KnownFactors.Time: return p.timeW.weight > 0f;
                case KnownFactors.Weather: return p.weatherW.weight > 0f;
            }
            return false;
        }

        /// <summary>Short chip text, e.g. "Sinking", "Red & White", "Night".</summary>
        public static string ShortValue(KnownFactors factor, PreferenceProfile p)
        {
            switch (factor)
            {
                case KnownFactors.Buoyancy: return BaitText.Name(p.buoyancy);
                case KnownFactors.Shape: return BaitText.Name(p.shape);
                case KnownFactors.Pattern: return BaitText.Name(p.color);
                case KnownFactors.Swim: return BaitText.Name(p.swim);
                case KnownFactors.Time: return p.time == TimeOfDay.Night ? "Night" : "Day";
                case KnownFactors.Weather: return Capitalize(WeatherNoun(p.weather));
            }
            return "?";
        }

        /// <summary>A full sentence, e.g. "Likes sinking bait".</summary>
        public static string Sentence(KnownFactors factor, PreferenceProfile p)
        {
            switch (factor)
            {
                case KnownFactors.Buoyancy: return "Likes " + BaitText.Name(p.buoyancy).ToLower() + " bait";
                case KnownFactors.Shape: return "Likes " + BaitText.Name(p.shape).ToLower() + " shapes";
                case KnownFactors.Pattern: return "Drawn to " + BaitText.Name(p.color).ToLower();
                case KnownFactors.Swim: return "Likes a " + BaitText.Name(p.swim).ToLower() + " swim";
                case KnownFactors.Time: return "Bites at " + (p.time == TimeOfDay.Night ? "night" : "day");
                case KnownFactors.Weather: return "Bites in " + WeatherNoun(p.weather);
            }
            return "";
        }

        public static Color FactorColor(KnownFactors factor)
        {
            switch (factor)
            {
                case KnownFactors.Buoyancy: return UiPalette.Sky;
                case KnownFactors.Shape: return UiPalette.Mint;
                case KnownFactors.Pattern: return UiPalette.Sun;
                case KnownFactors.Swim: return UiPalette.Lavender;
                case KnownFactors.Time: return UiPalette.Rose;
                default: return UiPalette.Teal;
            }
        }

        /// <summary>How many of this fish's weighted preferences the player has not discovered yet.</summary>
        public static int UnknownCount(FishIndexEntry entry)
        {
            int count = 0;
            for (int i = 0; i < AllFactors.Length; i++)
            {
                if (HasWeight(AllFactors[i], entry.Species.profile) && (entry.Known & AllFactors[i]) == 0) count++;
            }
            return count;
        }
    }
}
