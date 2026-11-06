// TidebookScreen.cs
// The Tidebook (Fish Index) from the GDD (section 5.2): an open book of every fish in the game.
//   Left page : title, discovery progress, rarity filters and a scrollable grid of fish cards.
//               Undiscovered fish appear as locked silhouettes to encourage exploration.
//   Right page: the selected fish - picture, rarity, description, habitat, depth, active time, weather,
//               the bait notes the player has worked out, and catch stats.
// Controls: mouse (click cards, wheel to scroll), arrow keys to browse, Esc to close.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public class TidebookScreen : UiScreen
    {
        const float WindowW = 1640f;
        const float WindowH = 940f;

        const int Columns = 3;
        const float CellW = 223f;
        const float CellH = 200f;
        const float Gap = 14f;
        const float PadTop = 8f;
        const float PadBottom = 8f;
        const int NoteSlots = 6;

        protected override Vector2 WindowSize { get { return new Vector2(WindowW, WindowH); } }

        class CardView
        {
            public FishIndexEntry entry;
            public RectTransform root;
            public UiButton button;
            public Image ring;
            public Image fish;
            public Image rarityDot;
            public Text number;
            public Text name;
        }

        IFishIndexView index;
        readonly List<CardView> cards = new List<CardView>();
        readonly List<CardView> visible = new List<CardView>();
        CardView selected;
        int filterIndex;

        // left page
        Text progressText;
        RectTransform fillRt;
        UiButton[] filterButtons;
        ScrollRect scroll;
        RectTransform viewportRt;
        RectTransform content;

        // right page
        Text numberText;
        Image badgeBg;
        Text badgeText;
        Image fishImage;
        Text nameText;
        Text blurbText;
        readonly Text[] infoValues = new Text[4];
        Text notesHeader;
        readonly Image[] noteBgs = new Image[NoteSlots];
        readonly Text[] noteTexts = new Text[NoteSlots];
        Text statsText;

        // =====================================================================
        // Public API
        // =====================================================================

        public void Bind(IFishIndexView view)
        {
            index = view;
            index.Changed += OnIndexChanged;

            IReadOnlyList<FishIndexEntry> all = index.Entries;
            for (int i = 0; i < all.Count; i++) cards.Add(MakeCard(all[i]));
            // pictures are generated the first time the book is opened (see OnOpen)
        }

        void OnIndexChanged()
        {
            if (IsOpen) Refresh();
        }

        // =====================================================================
        // Layout
        // =====================================================================

        protected override void BuildWindow(RectTransform w)
        {
            // the book cover
            Image cover = UiBuilder.Box(w, "Cover", UiPalette.CoverDark, 40);
            UiBuilder.Stretch(cover.rectTransform);
            UiBuilder.DropShadow(cover, 16f, 0.4f);

            Image coverInner = UiBuilder.Box(w, "CoverInner", UiPalette.Cover, 34);
            UiBuilder.Stretch(coverInner.rectTransform, 8f, 8f, 8f, 8f);

            RectTransform left = MakePage(w, "LeftPage", true);
            RectTransform right = MakePage(w, "RightPage", false);

            Image spine = UiBuilder.Box(w, "Spine", UiPalette.Spine, 6);
            UiBuilder.Fixed(spine.rectTransform, 0.5f, 0.5f, 30f, WindowH - 52f);

            BuildLeftPage(left);
            BuildRightPage(right);
        }

        RectTransform MakePage(RectTransform w, string name, bool leftSide)
        {
            Image page = UiBuilder.Box(w, name, UiPalette.Paper, 24);
            if (leftSide) UiBuilder.Anchor(page.rectTransform, 0f, 0f, 0.5f, 1f, 26f, 26f, 15f, 26f);
            else UiBuilder.Anchor(page.rectTransform, 0.5f, 0f, 1f, 1f, 15f, 26f, 26f, 26f);
            return page.rectTransform;
        }

        // ---------------------------------------------------------------- left page

        void BuildLeftPage(RectTransform page)
        {
            Image logo = UiBuilder.Icon(page, "Logo", FishArt.Icon(UiPalette.Coral, 1), Color.white);
            UiBuilder.TopLeft(logo.rectTransform, 36f, 30f, 82f, 52f);

            Text title = UiBuilder.Label(page, "Title", "Tidebook", 54, UiPalette.Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            UiBuilder.TopLeft(title.rectTransform, 128f, 20f, 420f, 70f);

            Text tagline = UiBuilder.Label(page, "Tagline", "Field notes on every fish you find", 22,
                                           UiPalette.InkSoft, TextAnchor.UpperLeft);
            UiBuilder.TopLeft(tagline.rectTransform, 38f, 94f, 520f, 30f);

            progressText = UiBuilder.Label(page, "Progress", "0 / 0", 44, UiPalette.Coral, TextAnchor.UpperRight, FontStyle.Bold);
            UiBuilder.TopRight(progressText.rectTransform, 36f, 22f, 220f, 56f);

            Text discovered = UiBuilder.Label(page, "DiscoveredLabel", "DISCOVERED", 16, UiPalette.InkFaint,
                                              TextAnchor.UpperRight, FontStyle.Bold);
            UiBuilder.TopRight(discovered.rectTransform, 36f, 78f, 220f, 22f);

            // progress bar
            Image barBg = UiBuilder.Box(page, "BarBg", UiPalette.PaperEdge, 8);
            UiBuilder.TopLeft(barBg.rectTransform, 36f, 140f, 707f, 16f);
            Image fill = UiBuilder.Box(barBg.transform, "Fill", UiPalette.Coral, 8);
            fillRt = fill.rectTransform;
            UiBuilder.Anchor(fillRt, 0f, 0f, 0f, 1f);

            BuildFilters(page);
            BuildGrid(page);

            Text hint = UiBuilder.Label(page, "Hint", "Arrow keys  browse        Esc  close", 18, UiPalette.InkFaint,
                                        TextAnchor.MiddleLeft);
            UiBuilder.TopLeft(hint.rectTransform, 38f, 852f, 700f, 26f);
        }

        void BuildFilters(RectTransform page)
        {
            RectTransform row = UiBuilder.Rect(page, "Filters");
            UiBuilder.TopLeft(row, 36f, 172f, 707f, 44f);

            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            string[] names = { "All", "Common", "Uncommon", "Rare", "Legendary" };
            float[] widths = { 84f, 128f, 148f, 100f, 150f };
            filterButtons = new UiButton[names.Length];

            for (int i = 0; i < names.Length; i++)
            {
                Image bg = UiBuilder.Box(row, "Filter " + names[i], UiPalette.PaperShade, 20, true);
                UiButton button = UiBuilder.Button(bg, UiPalette.PaperShade, UiPalette.Hex("#F8EFD9"),
                                                   UiPalette.PaperEdge, UiPalette.Cover);
                LayoutElement size = bg.gameObject.AddComponent<LayoutElement>();
                size.preferredWidth = widths[i];
                size.preferredHeight = 44f;

                Text text = UiBuilder.Label(bg.transform, "Text", names[i], 20, UiPalette.InkSoft,
                                            TextAnchor.MiddleCenter, FontStyle.Bold);
                UiBuilder.Stretch(text.rectTransform);
                button.label = text;
                button.labelNormal = UiPalette.InkSoft;
                button.labelSelected = UiPalette.White;
                button.selected = i == 0;

                int chosen = i;
                button.Clicked = delegate { SetFilter(chosen); };
                filterButtons[i] = button;
            }
        }

        void BuildGrid(RectTransform page)
        {
            RectTransform scrollRt = UiBuilder.Rect(page, "Scroll");
            UiBuilder.TopLeft(scrollRt, 36f, 232f, 707f, 608f);
            scroll = scrollRt.gameObject.AddComponent<ScrollRect>();

            viewportRt = UiBuilder.Rect(scrollRt, "Viewport");
            UiBuilder.Stretch(viewportRt);
            Image catcher = viewportRt.gameObject.AddComponent<Image>();   // lets empty space receive the mouse wheel
            catcher.color = new Color(0f, 0f, 0f, 0f);
            catcher.raycastTarget = true;
            viewportRt.gameObject.AddComponent<RectMask2D>();

            content = UiBuilder.Rect(viewportRt, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;

            GridLayoutGroup grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(CellW, CellH);
            grid.spacing = new Vector2(Gap, Gap);
            grid.padding = new RectOffset(4, 4, (int)PadTop, (int)PadBottom);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Columns;
            grid.childAlignment = TextAnchor.UpperLeft;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRt;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;
            scroll.inertia = true;
            scroll.decelerationRate = 0.12f;
        }

        CardView MakeCard(FishIndexEntry entry)
        {
            CardView c = new CardView();
            c.entry = entry;

            c.root = UiBuilder.Rect(content, "Card " + entry.Number);

            c.ring = UiBuilder.Box(c.root, "Ring", UiPalette.Coral, 26);
            UiBuilder.Stretch(c.ring.rectTransform, -6f, -6f, -6f, -6f);
            c.ring.gameObject.SetActive(false);

            Image bg = UiBuilder.Box(c.root, "Bg", UiPalette.PaperShade, 22, true);
            UiBuilder.Stretch(bg.rectTransform);
            c.button = UiBuilder.Button(bg, UiPalette.PaperShade, UiPalette.Hex("#F8EFD9"),
                                        UiPalette.PaperEdge, UiPalette.PaperShade);

            c.number = UiBuilder.Label(bg.transform, "Number", "", 18, UiPalette.InkFaint, TextAnchor.UpperLeft, FontStyle.Bold);
            UiBuilder.TopLeft(c.number.rectTransform, 14f, 10f, 120f, 22f);

            c.rarityDot = UiBuilder.Icon(bg.transform, "Dot", ProceduralSprites.Circle(), UiPalette.Mint);
            UiBuilder.TopRight(c.rarityDot.rectTransform, 14f, 14f, 14f, 14f);

            c.fish = UiBuilder.Icon(bg.transform, "Fish", null, Color.white);
            UiBuilder.Anchor(c.fish.rectTransform, 0.08f, 0.28f, 0.92f, 0.84f);

            c.name = UiBuilder.Label(bg.transform, "Name", "", 22, UiPalette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UiBuilder.Anchor(c.name.rectTransform, 0f, 0f, 1f, 0.26f, 8f, 2f, 8f, 0f);

            CardView local = c;
            c.button.Clicked = delegate { Select(local); };
            return c;
        }

        // ---------------------------------------------------------------- right page

        void BuildRightPage(RectTransform page)
        {
            const float x = 36f;
            const float width = 707f;

            numberText = UiBuilder.Label(page, "Number", "No. 01", 26, UiPalette.InkSoft, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiBuilder.TopLeft(numberText.rectTransform, x, 28f, 220f, 40f);

            // close button
            Image closeBg = UiBuilder.Box(page, "Close", UiPalette.PaperShade, 26, true);
            UiButton close = UiBuilder.Button(closeBg, UiPalette.PaperShade, UiPalette.Hex("#F8EFD9"),
                                              UiPalette.PaperEdge, UiPalette.PaperEdge);
            UiBuilder.TopRight(closeBg.rectTransform, 36f, 22f, 52f, 52f);
            Text closeText = UiBuilder.Label(closeBg.transform, "Text", "X", 24, UiPalette.InkSoft, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiBuilder.Stretch(closeText.rectTransform);
            close.label = closeText;
            close.labelNormal = UiPalette.InkSoft;
            close.labelSelected = UiPalette.InkSoft;
            close.Clicked = Close;

            // rarity badge
            badgeBg = UiBuilder.Box(page, "Badge", UiPalette.Mint, 20);
            UiBuilder.TopRight(badgeBg.rectTransform, 100f, 29f, 150f, 40f);
            badgeText = UiBuilder.Label(badgeBg.transform, "Text", "Common", 20, UiPalette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiBuilder.Stretch(badgeText.rectTransform);

            // picture frame
            Image frame = UiBuilder.GradientBox(page, "Frame", 26, UiPalette.WaterTop, UiPalette.WaterMid);
            UiBuilder.TopLeft(frame.rectTransform, x, 82f, width, 260f);

            float[] bx = { 70f, 130f, 590f, 640f };
            float[] by = { 60f, 170f, 90f, 190f };
            float[] bs = { 22f, 14f, 18f, 26f };
            for (int i = 0; i < bx.Length; i++)
            {
                Image bubble = UiBuilder.Icon(frame.transform, "Bubble", ProceduralSprites.Circle(),
                                              UiPalette.WithAlpha(UiPalette.White, 0.4f));
                bubble.preserveAspect = false;
                UiBuilder.TopLeft(bubble.rectTransform, bx[i], by[i], bs[i], bs[i]);
            }

            Image groundShadow = UiBuilder.Icon(frame.transform, "Shadow", ProceduralSprites.SoftCircle(),
                                                UiPalette.WithAlpha(UiPalette.Ink, 0.22f));
            groundShadow.preserveAspect = false;
            UiBuilder.Fixed(groundShadow.rectTransform, 0.5f, 0f, 300f, 40f, 0f, 16f);

            fishImage = UiBuilder.Icon(frame.transform, "Fish", null, Color.white);
            UiBuilder.Fixed(fishImage.rectTransform, 0.5f, 0.5f, 352f, 220f, 0f, 8f);

            // name + description
            nameText = UiBuilder.Label(page, "Name", "???", 50, UiPalette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold, true);
            UiBuilder.TopLeft(nameText.rectTransform, x, 352f, width, 62f);

            blurbText = UiBuilder.Label(page, "Blurb", "", 22, UiPalette.InkSoft, TextAnchor.UpperLeft);
            UiBuilder.TopLeft(blurbText.rectTransform, x, 418f, width, 64f);

            // info cards (2 x 2)
            RectTransform info = UiBuilder.Rect(page, "Info");
            UiBuilder.TopLeft(info, x, 492f, width, 192f);
            GridLayoutGroup infoGrid = info.gameObject.AddComponent<GridLayoutGroup>();
            infoGrid.cellSize = new Vector2(347f, 90f);
            infoGrid.spacing = new Vector2(13f, 12f);
            infoGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            infoGrid.constraintCount = 2;

            string[] titles = { "HABITAT", "DEPTH", "ACTIVE", "WEATHER" };
            for (int i = 0; i < titles.Length; i++)
            {
                Image card = UiBuilder.Box(info, titles[i], UiPalette.PaperShade, 18);

                Text label = UiBuilder.Label(card.transform, "Label", titles[i], 17, UiPalette.InkFaint, TextAnchor.UpperLeft, FontStyle.Bold);
                UiBuilder.TopLeft(label.rectTransform, 18f, 12f, 300f, 22f);

                infoValues[i] = UiBuilder.Label(card.transform, "Value", "???", 26, UiPalette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold, true);
                UiBuilder.TopLeft(infoValues[i].rectTransform, 18f, 38f, 311f, 44f);
            }

            // bait notes
            notesHeader = UiBuilder.Label(page, "NotesHeader",
                                          "<b>BAIT NOTES</b>   <color=#9DB0B8>what you've worked out so far</color>",
                                          20, UiPalette.Ink, TextAnchor.MiddleLeft);
            UiBuilder.TopLeft(notesHeader.rectTransform, x + 4f, 698f, width, 28f);

            RectTransform notes = UiBuilder.Rect(page, "Notes");
            UiBuilder.TopLeft(notes, x, 734f, width, 92f);
            GridLayoutGroup noteGrid = notes.gameObject.AddComponent<GridLayoutGroup>();
            noteGrid.cellSize = new Vector2(168f, 42f);
            noteGrid.spacing = new Vector2(11f, 8f);
            noteGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            noteGrid.constraintCount = 4;

            for (int i = 0; i < NoteSlots; i++)
            {
                Image chip = UiBuilder.Box(notes, "Note", UiPalette.PaperShade, 18);
                noteBgs[i] = chip;
                noteTexts[i] = UiBuilder.Label(chip.transform, "Text", "", 19, UiPalette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold, true);
                UiBuilder.Stretch(noteTexts[i].rectTransform, 8f, 0f, 8f, 0f);
            }

            statsText = UiBuilder.Label(page, "Stats", "", 21, UiPalette.InkSoft, TextAnchor.MiddleLeft, FontStyle.Normal, true);
            UiBuilder.TopLeft(statsText.rectTransform, x + 4f, 836f, width, 30f);
        }

        // =====================================================================
        // Behaviour
        // =====================================================================

        protected override void OnOpen()
        {
            Refresh();
            if (selected != null) ScrollIntoView(selected);
        }

        protected override void OnTick()
        {
            int delta = 0;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) delta = -1;
            else if (Input.GetKeyDown(KeyCode.RightArrow)) delta = 1;
            else if (Input.GetKeyDown(KeyCode.UpArrow)) delta = -Columns;
            else if (Input.GetKeyDown(KeyCode.DownArrow)) delta = Columns;

            if (delta != 0) MoveSelection(delta);
        }

        void MoveSelection(int delta)
        {
            if (visible.Count == 0) return;
            int current = selected != null ? visible.IndexOf(selected) : 0;
            if (current < 0) current = 0;
            int next = Mathf.Clamp(current + delta, 0, visible.Count - 1);
            if (next != current) Select(visible[next]);
        }

        void SetFilter(int filter)
        {
            filterIndex = filter;
            for (int i = 0; i < filterButtons.Length; i++) filterButtons[i].selected = i == filter;
            Refresh();
            scroll.normalizedPosition = new Vector2(0f, 1f);
        }

        void Select(CardView card)
        {
            selected = card;
            UpdateSelectionVisuals();
            ShowDetail(card);
            ScrollIntoView(card);
        }

        void Refresh()
        {
            if (index == null) return;

            int total = index.Entries.Count;
            int found = index.DiscoveredCount;
            progressText.text = found + " / " + total;
            float fraction = total > 0 ? (float)found / total : 0f;
            fillRt.gameObject.SetActive(found > 0);
            fillRt.anchorMax = new Vector2(Mathf.Clamp01(fraction), 1f);

            visible.Clear();
            for (int i = 0; i < cards.Count; i++)
            {
                CardView c = cards[i];
                UpdateCard(c);

                bool show = filterIndex == 0 || (int)c.entry.Species.rarity == filterIndex - 1;
                c.root.gameObject.SetActive(show);
                if (show) visible.Add(c);
            }

            if (selected == null || !visible.Contains(selected)) selected = visible.Count > 0 ? visible[0] : null;
            UpdateSelectionVisuals();
            ShowDetail(selected);
        }

        void UpdateCard(CardView c)
        {
            FishIndexEntry e = c.entry;
            bool known = e.Discovered;

            c.number.text = "No. " + e.Number.ToString("00");
            c.fish.sprite = known ? FishArt.Icon(e.Source) : FishArt.Silhouette(e.Source);
            c.name.text = known ? e.Species.displayName : "???";
            c.name.color = known ? UiPalette.Ink : UiPalette.InkFaint;
            c.rarityDot.color = known ? UiPalette.ForRarity(e.Species.rarity) : UiPalette.WithAlpha(UiPalette.InkFaint, 0.5f);
        }

        void UpdateSelectionVisuals()
        {
            for (int i = 0; i < cards.Count; i++) cards[i].ring.gameObject.SetActive(cards[i] == selected);
        }

        void ScrollIntoView(CardView card)
        {
            int idx = visible.IndexOf(card);
            if (idx < 0) return;

            int row = idx / Columns;
            float top = PadTop + row * (CellH + Gap);
            float bottom = top + CellH;
            int rows = Mathf.CeilToInt(visible.Count / (float)Columns);
            float contentHeight = PadTop + PadBottom + rows * (CellH + Gap) - Gap;
            float viewHeight = viewportRt.rect.height;
            float maxScroll = Mathf.Max(0f, contentHeight - viewHeight);
            if (maxScroll <= 0f) return;

            float current = (1f - scroll.verticalNormalizedPosition) * maxScroll;
            float target = current;
            if (top - 8f < current) target = top - 8f;
            else if (bottom + 8f > current + viewHeight) target = bottom + 8f - viewHeight;

            scroll.verticalNormalizedPosition = 1f - Mathf.Clamp(target, 0f, maxScroll) / maxScroll;
        }

        // ---------------------------------------------------------------- detail page

        void ShowDetail(CardView card)
        {
            if (card == null)
            {
                numberText.text = "";
                badgeBg.color = UiPalette.PaperShade;
                badgeText.text = "";
                fishImage.enabled = false;
                nameText.text = "Nothing here yet";
                nameText.color = UiPalette.InkFaint;
                blurbText.text = "Add fish to a Fish Catalog (or a Fish Spawner) and they will show up in your Tidebook.";
                for (int i = 0; i < infoValues.Length; i++) { infoValues[i].text = "-"; infoValues[i].color = UiPalette.InkFaint; }
                SetNotesVisible(false);
                statsText.text = "";
                return;
            }

            FishIndexEntry e = card.entry;
            FishSpecies species = e.Species;
            PreferenceProfile p = species.profile;
            bool known = e.Discovered;

            numberText.text = "No. " + e.Number.ToString("00");

            badgeBg.color = known ? UiPalette.ForRarity(species.rarity) : UiPalette.PaperShade;
            badgeText.text = known ? FishInfoText.RarityName(species.rarity) : "Unknown";
            badgeText.color = known ? UiPalette.Ink : UiPalette.InkFaint;

            fishImage.enabled = true;
            fishImage.sprite = known ? FishArt.Icon(e.Source) : FishArt.Silhouette(e.Source);

            nameText.text = known ? species.displayName : "???";
            nameText.color = known ? UiPalette.Ink : UiPalette.InkFaint;
            blurbText.text = known ? FishInfoText.Blurb(e.Source) : "Catch this fish to record it in your Tidebook.";

            infoValues[0].text = known ? FishInfoText.Habitat(species) : "???";
            infoValues[1].text = known ? FishInfoText.Depth(species.layer) : "???";
            infoValues[2].text = known ? FishInfoText.ActiveTime(species) : "???";
            infoValues[3].text = known ? FishInfoText.WeatherText(species) : "???";
            for (int i = 0; i < infoValues.Length; i++) infoValues[i].color = known ? UiPalette.Ink : UiPalette.InkFaint;

            if (!known)
            {
                SetNotesVisible(false);
                statsText.text = "Not discovered yet";
                return;
            }

            // bait notes: what the player has worked out, then "?" for what is still hidden
            SetNotesVisible(true);
            int slot = 0;
            for (int i = 0; i < FishInfoText.AllFactors.Length && slot < NoteSlots; i++)
            {
                KnownFactors f = FishInfoText.AllFactors[i];
                if (FishInfoText.HasWeight(f, p) && (e.Known & f) != 0)
                    SetNote(slot++, FishInfoText.ShortValue(f, p), UiPalette.WithAlpha(FishInfoText.FactorColor(f), 0.6f), UiPalette.Ink);
            }
            int unknown = FishInfoText.UnknownCount(e);
            for (int i = 0; i < unknown && slot < NoteSlots; i++)
                SetNote(slot++, "?", UiPalette.PaperShade, UiPalette.InkFaint);
            for (int i = slot; i < NoteSlots; i++) noteBgs[i].gameObject.SetActive(false);

            // stats
            string caught = e.TimesCaught == 1 ? "Caught once" : "Caught " + e.TimesCaught + " times";
            string first = string.IsNullOrEmpty(e.FirstSpot)
                ? ""
                : "First found at " + e.FirstSpot + " (" + (e.FirstTime == TimeOfDay.Night ? "night" : "day") + ", "
                  + FishInfoText.WeatherNoun(e.FirstWeather) + ")";
            string worth = "Worth " + species.currencyValue;
            statsText.text = first.Length > 0 ? caught + "   |   " + first + "   |   " + worth : caught + "   |   " + worth;
        }

        void SetNotesVisible(bool visibleNow)
        {
            notesHeader.gameObject.SetActive(visibleNow);
            for (int i = 0; i < NoteSlots; i++) noteBgs[i].gameObject.SetActive(visibleNow);
        }

        void SetNote(int slot, string text, Color background, Color textColor)
        {
            noteBgs[slot].gameObject.SetActive(true);
            noteBgs[slot].color = background;
            noteTexts[slot].text = text;
            noteTexts[slot].color = textColor;
        }
    }
}
