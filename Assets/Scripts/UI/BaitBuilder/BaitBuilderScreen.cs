// BaitBuilderScreen.cs
// The Bait Builder menu from the GDD (section 7.2): four independent axes, freely combinable from the start.
//   Left  : choose Buoyancy, Body Shape, Color / Pattern and Swim Pattern
//   Right : a live animated preview, the bait's name and tags, your Tidebook notes about a target fish,
//           and Equip / Revert buttons.
// The player edits a DRAFT; "Equip Bait" copies it to the EquippedBait the rod casts with.
//
// The notes only show what the player has already discovered about a fish (see FishIndexLog),
// so the game keeps its "figure out what they like" loop instead of exposing the hidden numbers.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public class BaitBuilderScreen : UiScreen
    {
        const float WindowW = 1560f;
        const float WindowH = 920f;
        const int MaxTargets = 8;
        const int NoteSlots = 6;

        protected override Vector2 WindowSize { get { return new Vector2(WindowW, WindowH); } }

        EquippedBait equipped;
        IFishIndexView index;
        readonly BaitSelection draft = new BaitSelection();

        Buoyancy[] buoyancyValues;
        BodyShape[] shapeValues;
        ColorPattern[] patternValues;
        SwimPattern[] swimValues;

        UiButton[] buoyancyButtons;
        UiButton[] shapeButtons;
        UiButton[] patternButtons;
        UiButton[] swimButtons;
        Image[] shapeIcons;

        BaitPreviewTank tank;
        Text nameText;
        readonly Text[] tagTexts = new Text[4];
        UiButton equipButton;
        Text equipLabel;

        readonly UiButton[] targetButtons = new UiButton[MaxTargets];
        readonly Image[] targetIcons = new Image[MaxTargets];
        readonly List<FishIndexEntry> targets = new List<FishIndexEntry>();
        FishIndexEntry target;

        readonly Image[] noteDots = new Image[NoteSlots];
        readonly Text[] noteTexts = new Text[NoteSlots];
        Text notesHint;

        // =====================================================================
        // Public API
        // =====================================================================

        /// <summary>Connects the screen to the equipped bait and (optionally) the Fish Index for notes.</summary>
        public void Bind(EquippedBait equippedBait, IFishIndexView fishIndex)
        {
            equipped = equippedBait;
            index = fishIndex;
            if (index != null) index.Changed += OnIndexChanged;

            draft.CopyFrom(equipped.Selection);
            RefreshTargets();
            Refresh();
        }

        void OnIndexChanged()
        {
            if (!IsOpen) return;
            RefreshTargets();
            RefreshNotes();
        }

        // =====================================================================
        // Layout
        // =====================================================================

        protected override void BuildWindow(RectTransform w)
        {
            Image card = UiBuilder.Box(w, "Card", UiPalette.Paper, 34);
            UiBuilder.Stretch(card.rectTransform);
            UiBuilder.DropShadow(card, 14f, 0.35f);

            Text title = UiBuilder.Label(w, "Title", "Bait Builder", 48, UiPalette.Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            UiBuilder.TopLeft(title.rectTransform, 44f, 24f, 700f, 62f);

            Text sub = UiBuilder.Label(w, "Subtitle", "Mix and match every part. Different fish are drawn to different baits.",
                                       22, UiPalette.InkSoft, TextAnchor.UpperLeft);
            UiBuilder.TopLeft(sub.rectTransform, 46f, 84f, 900f, 30f);

            Image closeHint = UiBuilder.Box(w, "CloseHint", UiPalette.PaperShade, 16);
            UiBuilder.TopRight(closeHint.rectTransform, 44f, 34f, 180f, 46f);
            Text closeText = UiBuilder.Label(closeHint.transform, "Text", "Esc   Close", 20, UiPalette.InkSoft,
                                             TextAnchor.MiddleCenter, FontStyle.Bold);
            UiBuilder.Stretch(closeText.rectTransform);

            BuildLeftColumn(w);
            BuildRightColumn(w);

            draft.Changed += Refresh;
        }

        void BuildLeftColumn(RectTransform w)
        {
            const float x = 44f;
            const float width = 820f;
            float y = 128f;
            const float step = 192f;

            // ---- buoyancy ----
            buoyancyValues = (Buoyancy[])System.Enum.GetValues(typeof(Buoyancy));
            buoyancyButtons = new UiButton[buoyancyValues.Length];
            RectTransform row = BuildSection(w, x, y, width, "BUOYANCY", "where it sits in the water");
            for (int i = 0; i < buoyancyValues.Length; i++)
            {
                Buoyancy value = buoyancyValues[i];
                Image icon;
                buoyancyButtons[i] = MakeChip(row, BaitText.Name(value), BaitArt.BuoyancyIcon(value), out icon);
                buoyancyButtons[i].Clicked = delegate { draft.SetBuoyancy(value); };
            }

            // ---- body shape ----
            y += step;
            shapeValues = (BodyShape[])System.Enum.GetValues(typeof(BodyShape));
            shapeButtons = new UiButton[shapeValues.Length];
            shapeIcons = new Image[shapeValues.Length];
            row = BuildSection(w, x, y, width, "BODY SHAPE", "the silhouette fish see");
            for (int i = 0; i < shapeValues.Length; i++)
            {
                BodyShape value = shapeValues[i];
                Image icon;
                shapeButtons[i] = MakeChip(row, BaitText.Name(value), BaitArt.Lure(value, ColorPattern.RedWhite), out icon);
                shapeIcons[i] = icon;
                shapeButtons[i].Clicked = delegate { draft.SetShape(value); };
            }

            // ---- color / pattern ----
            y += step;
            patternValues = (ColorPattern[])System.Enum.GetValues(typeof(ColorPattern));
            patternButtons = new UiButton[patternValues.Length];
            row = BuildSection(w, x, y, width, "COLOR / PATTERN", "what catches their eye");
            for (int i = 0; i < patternValues.Length; i++)
            {
                ColorPattern value = patternValues[i];
                Image icon;
                patternButtons[i] = MakeChip(row, BaitText.Name(value), BaitArt.Swatch(value), out icon);
                patternButtons[i].Clicked = delegate { draft.SetPattern(value); };
            }

            // ---- swim pattern ----
            y += step;
            swimValues = (SwimPattern[])System.Enum.GetValues(typeof(SwimPattern));
            swimButtons = new UiButton[swimValues.Length];
            row = BuildSection(w, x, y, width, "SWIM PATTERN", "how it moves through the water");
            for (int i = 0; i < swimValues.Length; i++)
            {
                SwimPattern value = swimValues[i];
                Image icon;
                swimButtons[i] = MakeChip(row, BaitText.Name(value), BaitArt.SwimIcon(value), out icon);
                swimButtons[i].Clicked = delegate { draft.SetSwim(value); };
            }
        }

        RectTransform BuildSection(RectTransform parent, float x, float y, float width, string title, string hint)
        {
            Text header = UiBuilder.Label(parent, title + " Header",
                                          "<b>" + title + "</b>   <color=#9DB0B8>" + hint + "</color>",
                                          22, UiPalette.Ink, TextAnchor.MiddleLeft);
            UiBuilder.TopLeft(header.rectTransform, x + 4f, y, width, 32f);

            RectTransform row = UiBuilder.Rect(parent, title + " Row");
            UiBuilder.TopLeft(row, x, y + 38f, width, 128f);

            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            return row;
        }

        UiButton MakeChip(RectTransform row, string label, Sprite icon, out Image iconImage)
        {
            Image bg = UiBuilder.Box(row, "Chip", UiPalette.PaperShade, 24, true);
            UiButton button = UiBuilder.Button(bg, UiPalette.PaperShade, UiPalette.Hex("#F8EFD9"),
                                               UiPalette.PaperEdge, UiPalette.Coral);

            iconImage = UiBuilder.Icon(bg.transform, "Icon", icon, Color.white);
            UiBuilder.Fixed(iconImage.rectTransform, 0.5f, 1f, 72f, 72f, 0f, -12f);

            Text text = UiBuilder.Label(bg.transform, "Label", label, 22, UiPalette.Ink,
                                        TextAnchor.MiddleCenter, FontStyle.Bold, true);
            UiBuilder.Anchor(text.rectTransform, 0f, 0f, 1f, 0.28f, 6f, 4f, 6f, 0f);

            button.label = text;
            button.labelNormal = UiPalette.Ink;
            button.labelSelected = UiPalette.White;
            return button;
        }

        void BuildRightColumn(RectTransform w)
        {
            const float x = 904f;
            const float width = 612f;

            // ---- live preview ----
            RectTransform tankRt = UiBuilder.Rect(w, "Tank");
            UiBuilder.TopLeft(tankRt, x, 128f, width, 300f);
            tank = tankRt.gameObject.AddComponent<BaitPreviewTank>();
            tank.Build(tankRt, width, 300f);

            // ---- name + tags ----
            Image tags = UiBuilder.Box(w, "Tags", UiPalette.PaperShade, 24);
            UiBuilder.TopLeft(tags.rectTransform, x, 442f, width, 112f);

            nameText = UiBuilder.Label(tags.transform, "Name", "", 30, UiPalette.Ink, TextAnchor.MiddleLeft,
                                       FontStyle.Bold, true);
            UiBuilder.TopLeft(nameText.rectTransform, 22f, 10f, width - 44f, 44f);

            Color[] tagColors = { UiPalette.Sky, UiPalette.Mint, UiPalette.Sun, UiPalette.Lavender };
            float pillW = (width - 44f - 3f * 10f) / 4f;
            for (int i = 0; i < 4; i++)
            {
                Image pill = UiBuilder.Box(tags.transform, "Tag", UiPalette.WithAlpha(tagColors[i], 0.55f), 16);
                UiBuilder.TopLeft(pill.rectTransform, 22f + i * (pillW + 10f), 62f, pillW, 38f);
                tagTexts[i] = UiBuilder.Label(pill.transform, "Text", "", 19, UiPalette.Ink,
                                              TextAnchor.MiddleCenter, FontStyle.Bold, true);
                UiBuilder.Stretch(tagTexts[i].rectTransform, 6f, 0f, 6f, 0f);
            }

            // ---- tidebook notes ----
            Image notes = UiBuilder.Box(w, "Notes", UiPalette.PaperShade, 24);
            UiBuilder.TopLeft(notes.rectTransform, x, 568f, width, 236f);

            Text notesTitle = UiBuilder.Label(notes.transform, "Title", "Tidebook notes", 24, UiPalette.Ink,
                                              TextAnchor.MiddleLeft, FontStyle.Bold);
            UiBuilder.TopLeft(notesTitle.rectTransform, 22f, 12f, 400f, 32f);

            Text notesSub = UiBuilder.Label(notes.transform, "Sub", "Pick a fish to see what you've learned about it.", 18,
                                            UiPalette.InkSoft, TextAnchor.MiddleLeft);
            UiBuilder.TopLeft(notesSub.rectTransform, 22f, 44f, 560f, 24f);

            for (int i = 0; i < MaxTargets; i++)
            {
                Image bg = UiBuilder.Box(notes.transform, "Target", UiPalette.Paper, 16, true);
                UiButton button = UiBuilder.Button(bg, UiPalette.Paper, UiPalette.Hex("#FFFBF1"),
                                                   UiPalette.PaperEdge, UiPalette.Coral);
                UiBuilder.TopLeft(bg.rectTransform, 22f + i * 63f, 78f, 54f, 54f);

                Image icon = UiBuilder.Icon(bg.transform, "Icon", null, Color.white);
                UiBuilder.Stretch(icon.rectTransform, 4f, 4f, 4f, 4f);

                targetButtons[i] = button;
                targetIcons[i] = icon;
                int slot = i;
                button.Clicked = delegate { SelectTarget(slot); };
            }

            for (int i = 0; i < NoteSlots; i++)
            {
                int col = i / 3;
                int rowIndex = i % 3;
                float nx = 22f + col * 296f;
                float ny = 144f + rowIndex * 28f;

                Image dot = UiBuilder.Icon(notes.transform, "Dot", ProceduralSprites.Circle(), UiPalette.Mint);
                UiBuilder.TopLeft(dot.rectTransform, nx, ny + 7f, 14f, 14f);
                noteDots[i] = dot;

                Text t = UiBuilder.Label(notes.transform, "Note", "", 19, UiPalette.Ink, TextAnchor.MiddleLeft, FontStyle.Normal, true);
                UiBuilder.TopLeft(t.rectTransform, nx + 24f, ny, 262f, 28f);
                noteTexts[i] = t;
            }

            notesHint = UiBuilder.Label(notes.transform, "Hint", "", 20, UiPalette.InkSoft, TextAnchor.UpperLeft);
            UiBuilder.TopLeft(notesHint.rectTransform, 22f, 148f, width - 44f, 76f);

            // ---- buttons ----
            Image revertBg = UiBuilder.Box(w, "Revert", UiPalette.PaperShade, 22, true);
            UiButton revert = UiBuilder.Button(revertBg, UiPalette.PaperShade, UiPalette.Hex("#F8EFD9"),
                                               UiPalette.PaperEdge, UiPalette.PaperEdge);
            UiBuilder.TopLeft(revertBg.rectTransform, x, 818f, 170f, 60f);
            Text revertText = UiBuilder.Label(revertBg.transform, "Text", "Revert", 24, UiPalette.InkSoft,
                                              TextAnchor.MiddleCenter, FontStyle.Bold);
            UiBuilder.Stretch(revertText.rectTransform);
            revert.label = revertText;
            revert.labelNormal = UiPalette.InkSoft;
            revert.labelSelected = UiPalette.InkSoft;
            revert.Clicked = delegate { if (equipped != null) draft.CopyFrom(equipped.Selection); };

            Image equipBg = UiBuilder.Box(w, "Equip", UiPalette.Coral, 22, true);
            equipButton = UiBuilder.Button(equipBg, UiPalette.Coral, UiPalette.Hex("#FF9C80"),
                                           UiPalette.CoralDark, UiPalette.CoralDark);
            equipButton.disabledColor = UiPalette.Mint;
            UiBuilder.TopLeft(equipBg.rectTransform, x + 186f, 818f, width - 186f, 60f);
            UiBuilder.DropShadow(equipBg, 6f, 0.2f);

            equipLabel = UiBuilder.Label(equipBg.transform, "Text", "Equip Bait", 28, UiPalette.White,
                                         TextAnchor.MiddleCenter, FontStyle.Bold);
            UiBuilder.Stretch(equipLabel.rectTransform);
            equipButton.label = equipLabel;
            equipButton.labelNormal = UiPalette.White;
            equipButton.labelSelected = UiPalette.White;
            equipButton.Clicked = Equip;
        }

        // =====================================================================
        // Behaviour
        // =====================================================================

        protected override void OnOpen()
        {
            if (equipped != null) draft.CopyFrom(equipped.Selection);
            RefreshTargets();
            Refresh();
        }

        protected override void OnTick()
        {
            bool enter = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
            if (enter && equipButton.interactable) Equip();
        }

        void Equip()
        {
            if (equipped == null) return;
            equipped.Selection.CopyFrom(draft);
            UiSounds.Play(UiSound.Confirm);
            Close();
        }

        void Refresh()
        {
            if (buoyancyButtons == null) return;

            for (int i = 0; i < buoyancyButtons.Length; i++) buoyancyButtons[i].selected = buoyancyValues[i] == draft.Buoyancy;
            for (int i = 0; i < shapeButtons.Length; i++)
            {
                shapeButtons[i].selected = shapeValues[i] == draft.Shape;
                shapeIcons[i].sprite = BaitArt.Lure(shapeValues[i], draft.Pattern);   // shape chips preview the chosen color
            }
            for (int i = 0; i < patternButtons.Length; i++) patternButtons[i].selected = patternValues[i] == draft.Pattern;
            for (int i = 0; i < swimButtons.Length; i++) swimButtons[i].selected = swimValues[i] == draft.Swim;

            nameText.text = BaitText.FullName(draft);
            tagTexts[0].text = BaitText.Name(draft.Buoyancy);
            tagTexts[1].text = BaitText.Name(draft.Shape);
            tagTexts[2].text = BaitText.Name(draft.Pattern);
            tagTexts[3].text = BaitText.Name(draft.Swim);

            tank.SetBait(draft);
            RefreshNotes();
            RefreshEquipButton();
        }

        void RefreshEquipButton()
        {
            bool same = equipped != null && draft.SameAs(equipped.Selection);
            equipButton.interactable = !same;
            equipLabel.text = same ? "Currently equipped" : "Equip Bait";
        }

        // ---------------------------------------------------------------- notes

        void RefreshTargets()
        {
            targets.Clear();
            if (index != null)
            {
                IReadOnlyList<FishIndexEntry> all = index.Entries;
                for (int i = 0; i < all.Count && targets.Count < MaxTargets; i++)
                {
                    if (all[i].Discovered) targets.Add(all[i]);
                }
            }

            if (target != null && !targets.Contains(target)) target = null;
            if (target == null && targets.Count > 0) target = targets[0];

            for (int i = 0; i < MaxTargets; i++)
            {
                bool has = i < targets.Count;
                targetButtons[i].gameObject.SetActive(has);
                if (has)
                {
                    targetIcons[i].sprite = FishArt.Icon(targets[i].Source);
                    targetButtons[i].selected = targets[i] == target;
                }
            }
        }

        void SelectTarget(int slot)
        {
            if (slot < 0 || slot >= targets.Count) return;
            target = targets[slot];
            for (int i = 0; i < targets.Count; i++) targetButtons[i].selected = targets[i] == target;
            RefreshNotes();
        }

        void RefreshNotes()
        {
            for (int i = 0; i < NoteSlots; i++)
            {
                noteDots[i].gameObject.SetActive(false);
                noteTexts[i].gameObject.SetActive(false);
            }

            if (target == null)
            {
                notesHint.text = index == null
                    ? "Your Tidebook notes will show up here."
                    : "Catch a fish and it will appear here, along with anything you learn about its taste in bait.";
                return;
            }

            PreferenceProfile p = target.Species.profile;
            int slot = 0;

            KnownFactors[] baitFactors = { KnownFactors.Buoyancy, KnownFactors.Shape, KnownFactors.Pattern, KnownFactors.Swim };
            for (int i = 0; i < baitFactors.Length; i++)
            {
                KnownFactors f = baitFactors[i];
                if ((target.Known & f) != 0 && FishInfoText.HasWeight(f, p) && slot < NoteSlots)
                    SetNote(slot++, FishInfoText.Sentence(f, p), MatchesDraft(f, p) ? UiPalette.Mint : UiPalette.Coral);
            }

            KnownFactors[] worldFactors = { KnownFactors.Time, KnownFactors.Weather };
            for (int i = 0; i < worldFactors.Length; i++)
            {
                KnownFactors f = worldFactors[i];
                if ((target.Known & f) != 0 && FishInfoText.HasWeight(f, p) && slot < NoteSlots)
                    SetNote(slot++, FishInfoText.Sentence(f, p), UiPalette.Sky);
            }

            int unknown = FishInfoText.UnknownCount(target);
            if (unknown > 0 && slot < NoteSlots)
                SetNote(slot++, unknown + " more to discover", UiPalette.InkFaint);

            notesHint.text = slot == 0 || (slot == 1 && unknown > 0)
                ? "Catch " + target.Species.displayName + " with different baits to learn what it likes."
                : "";
        }

        void SetNote(int slot, string text, Color dotColor)
        {
            noteDots[slot].color = dotColor;
            noteDots[slot].gameObject.SetActive(true);
            noteTexts[slot].text = text;
            noteTexts[slot].gameObject.SetActive(true);
        }

        bool MatchesDraft(KnownFactors factor, PreferenceProfile p)
        {
            switch (factor)
            {
                case KnownFactors.Buoyancy: return draft.Buoyancy == p.buoyancy;
                case KnownFactors.Shape: return draft.Shape == p.shape;
                case KnownFactors.Pattern: return draft.Pattern == p.color;
                case KnownFactors.Swim: return draft.Swim == p.swim;
            }
            return false;
        }
    }
}
