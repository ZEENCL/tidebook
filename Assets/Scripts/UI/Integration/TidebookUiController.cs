// TidebookUiController.cs
// Drop this on ONE GameObject in your scene (for example "Game") and press Play. It:
//   - builds the Canvas, the Bait Builder, the Tidebook, the HUD strip and the toast banner,
//   - connects the Bait Builder to the rod (rod.BaitProvider) so the equipped bait is what gets cast,
//   - listens to rod catches (rod.CatchEvents) so caught fish are logged in the Tidebook,
//   - opens/closes the menus with hotkeys and pauses the game while one is open.
//
// With no RodController in the scene it still works as a standalone UI preview.
// With no fish assigned anywhere it shows 12 demo fish so you can see the Tidebook right away.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Tidebook.Ui
{
    public class TidebookUiController : MonoBehaviour
    {
        [Header("Wiring")]
        [Tooltip("Found automatically if left empty.")]
        public RodController rod;

        [Tooltip("Optional. Which fish appear in the Tidebook, in order. If empty, the species used by FishSpawners are collected automatically.")]
        public FishCatalog catalog;

        [Header("Hotkeys")]
        public KeyCode baitKey = KeyCode.B;
        public KeyCode indexKey = KeyCode.T;
        public KeyCode closeKey = KeyCode.Escape;

        [Header("Behaviour")]
        [Tooltip("Freeze the game (Time.timeScale = 0) while a menu is open. Menus keep animating.")]
        public bool pauseWhileOpen = true;
        public bool persistProgress = true;
        public bool showHud = true;
        [Tooltip("Optional: use your own font for all UI text.")]
        public Font customFont;

        [Header("Fishing HUD (gameplay feedback)")]
        [Tooltip("Crosshair, cast trajectory, bite alerts, fight bar, catch banner, time / weather and coins.")]
        public bool showFishingHud = true;
        [Tooltip("Switch off the old debug text HUD (RodHud) while the fishing HUD is on.")]
        public bool replaceDebugHud = true;
        public bool showCastTrajectory = true;
        [Tooltip("Ripples on the water, approaching fish shadows and nibble rings.")]
        public bool showWorldFeedback = true;
        [Tooltip("Optional: where the trajectory line starts (e.g. a rod tip). Empty = just in front of the camera.")]
        public Transform castOrigin;

        [Header("Preview / demo")]
        [Tooltip("When nothing else provides fish, fill the Tidebook with 12 sample species.")]
        public bool useDemoFishIfNoSpecies = true;
        [Tooltip("For screenshots: mark this many fish as already found (not saved). 0 = off.")]
        [Min(0)] public int previewUnlockCount = 0;

        [Header("Events (hook your player controller here)")]
        public UnityEvent onMenuOpened = new UnityEvent();
        public UnityEvent onMenuClosed = new UnityEvent();

        UiRoot root;
        BaitBuilderScreen baitScreen;
        TidebookScreen indexScreen;
        UiToast toast;
        EquippedBait equipped;
        FishIndexLog log;
        FishingHud fishingHud;
        WorldFeedback worldFeedback;

        bool menuWasOpen;
        CursorLockMode previousLock;
        bool previousCursorVisible;
        float previousTimeScale = 1f;

        public EquippedBait Equipped { get { return equipped; } }
        public IFishIndexView Index { get { return log; } }

        // =====================================================================
        // Setup
        // =====================================================================

        void Awake()
        {
            if (customFont != null) UiFonts.Override = customFont;
            if (rod == null) rod = UiFind.First<RodController>();

            equipped = new EquippedBait();
            SeedFromInspectorLoadout();

            log = new FishIndexLog(ResolveCatalog(), persistProgress);
            if (previewUnlockCount > 0) log.PreviewUnlock(previewUnlockCount);

            root = UiRoot.Create(100);

            baitScreen = root.AddScreen<BaitBuilderScreen>("Bait Builder");
            baitScreen.Bind(equipped, log);

            indexScreen = root.AddScreen<TidebookScreen>("Tidebook");
            indexScreen.Bind(log);

            toast = root.CreateToast();
            if (showHud) root.CreateHud(equipped, baitKey.ToString(), indexKey.ToString());

            log.Discovered += OnDiscovered;
            UiFocus.Changed += OnFocusChanged;

            if (rod != null)
            {
                rod.BaitProvider = equipped;                                   // the rod now casts what the builder equips
                rod.InputSource = new UiGatedRodInput(new LegacyRodInput());   // menus never trigger casts
                rod.CatchEvents.Subscribe(log);                                // catches are logged in the Tidebook

                // Gameplay feedback. Created after the log subscribed, so the catch banner sees updated progress.
                if (showFishingHud)
                {
                    RodHud debugHud = rod.GetComponent<RodHud>();
                    if (replaceDebugHud && debugHud != null) debugHud.enabled = false;
                    fishingHud = FishingHud.Create(root.HudLayer, rod, log, castOrigin, showCastTrajectory);
                }

                if (showWorldFeedback) worldFeedback = WorldFeedback.Create(rod, new StaticFishLocator());
            }
        }

        void OnDestroy()
        {
            UiFocus.Changed -= OnFocusChanged;
            if (log != null) log.Discovered -= OnDiscovered;
            if (rod != null && log != null) rod.CatchEvents.Unsubscribe(log);
            if (worldFeedback != null) Destroy(worldFeedback.gameObject);

            if (menuWasOpen)
            {
                Time.timeScale = previousTimeScale;
                menuWasOpen = false;
            }
        }

        void SeedFromInspectorLoadout()
        {
            // If you already set a bait on the old InspectorBaitLoadout, start from that.
            if (rod == null) return;
            InspectorBaitLoadout loadout = rod.GetComponent<InspectorBaitLoadout>();
            if (loadout != null)
                equipped.Selection.Set(loadout.buoyancy, loadout.shape, loadout.colorPattern, loadout.swim);
        }

        List<FishCatalogEntry> ResolveCatalog()
        {
            if (catalog != null && catalog.entries.Count > 0) return FishCatalogBuilder.FromCatalog(catalog);

            List<FishSpecies> found = new List<FishSpecies>();
            FishSpawner[] spawners = UiFind.All<FishSpawner>();
            for (int i = 0; i < spawners.Length; i++)
            {
                for (int k = 0; k < spawners[i].species.Count; k++)
                {
                    FishSpecies s = spawners[i].species[k];
                    if (s != null && !found.Contains(s)) found.Add(s);
                }
            }

            if (found.Count == 0 && useDemoFishIfNoSpecies) found = DemoFish.Create();
            return FishCatalogBuilder.FromSpecies(found);
        }

        // =====================================================================
        // Input
        // =====================================================================

        void Update()
        {
            if (Input.GetKeyDown(closeKey) && UiFocus.AnyOpen)
            {
                UiFocus.Top.Close();
                return;
            }

            if (Input.GetKeyDown(baitKey)) Toggle(baitScreen, indexScreen);
            else if (Input.GetKeyDown(indexKey)) Toggle(indexScreen, baitScreen);
        }

        static void Toggle(UiScreen screen, UiScreen other)
        {
            if (screen.IsOpen)
            {
                screen.Close();
                return;
            }

            if (other.IsOpen) other.Close();
            screen.Open();
        }

        // =====================================================================
        // Public helpers (wire these to buttons or other scripts if you like)
        // =====================================================================

        public void OpenBaitBuilder() { Toggle(baitScreen, indexScreen); }
        public void OpenTidebook() { Toggle(indexScreen, baitScreen); }

        public void CloseAll()
        {
            if (baitScreen.IsOpen) baitScreen.Close();
            if (indexScreen.IsOpen) indexScreen.Close();
        }

        [ContextMenu("Reset Tidebook Progress")]
        public void ResetProgress()
        {
            if (log != null) log.ClearProgress();
        }

        // =====================================================================
        // Reactions
        // =====================================================================

        void OnDiscovered(FishIndexEntry entry)
        {
            toast.Show("New entry!", entry.Species.displayName + " added to your Tidebook", FishArt.Icon(entry.Source));
        }

        void OnFocusChanged()
        {
            bool open = UiFocus.AnyOpen;
            if (open == menuWasOpen) return;
            menuWasOpen = open;

            if (open)
            {
                previousLock = Cursor.lockState;
                previousCursorVisible = Cursor.visible;
                previousTimeScale = Time.timeScale;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (pauseWhileOpen) Time.timeScale = 0f;
                onMenuOpened.Invoke();
            }
            else
            {
                Cursor.lockState = previousLock;
                Cursor.visible = previousCursorVisible;
                if (pauseWhileOpen) Time.timeScale = previousTimeScale;
                onMenuClosed.Invoke();
            }
        }
    }
}
