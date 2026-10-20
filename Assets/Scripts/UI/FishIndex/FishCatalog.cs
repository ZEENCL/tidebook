// FishCatalog.cs
// The list of every fish that appears in the Tidebook, in book order.
// Create one via Assets > Create > Tidebook > Fish Catalog and assign it to TidebookUiController.
// Optional: give an entry its own icon, tint and description text. Without a catalog the controller
// collects the species from every FishSpawner in the scene automatically.

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook.Ui
{
    [System.Serializable]
    public class FishCatalogEntry
    {
        public FishSpecies species;

        [Tooltip("Optional picture. Leave empty to use the generated low-poly fish.")]
        public Sprite icon;

        [Tooltip("Tint of the generated fish (only used when 'Use Tint' is ticked).")]
        public Color tint = Color.white;
        public bool useTint;

        [TextArea(2, 4)]
        [Tooltip("Optional flavor text. Leave empty for an automatic description.")]
        public string blurb;
    }

    [CreateAssetMenu(menuName = "Tidebook/Fish Catalog")]
    public class FishCatalog : ScriptableObject
    {
        public List<FishCatalogEntry> entries = new List<FishCatalogEntry>();
    }

    public static class FishCatalogBuilder
    {
        /// <summary>Uses the catalog's own order (and any custom icons / text).</summary>
        public static List<FishCatalogEntry> FromCatalog(FishCatalog catalog)
        {
            List<FishCatalogEntry> list = new List<FishCatalogEntry>();
            for (int i = 0; i < catalog.entries.Count; i++)
            {
                FishCatalogEntry e = catalog.entries[i];
                if (e != null && e.species != null) list.Add(e);
            }
            return list;
        }

        /// <summary>Builds entries from raw species, ordered by rarity and then name.</summary>
        public static List<FishCatalogEntry> FromSpecies(IEnumerable<FishSpecies> species)
        {
            List<FishCatalogEntry> list = new List<FishCatalogEntry>();
            HashSet<FishSpecies> seen = new HashSet<FishSpecies>();

            foreach (FishSpecies s in species)
            {
                if (s == null || !seen.Add(s)) continue;
                FishCatalogEntry e = new FishCatalogEntry();
                e.species = s;
                list.Add(e);
            }

            list.Sort(delegate(FishCatalogEntry a, FishCatalogEntry b)
            {
                int byRarity = a.species.rarity.CompareTo(b.species.rarity);
                if (byRarity != 0) return byRarity;
                return string.Compare(a.species.displayName, b.species.displayName, System.StringComparison.Ordinal);
            });
            return list;
        }
    }
}
