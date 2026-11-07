// DevToolsController.cs
// OPTIONAL developer tooling. Drop this on the same GameObject as TidebookUiController and press Play.
// It adds a text console (press ` to open it, type "help") and a few hotkeys. Every action is a Command that
// goes through one CommandHistory, so weather, time, coins, spawned fish and Tidebook unlocks can all be undone.
//
// It switches itself off in release builds (it only runs in the Editor and Development Builds).
// Remove this component and nothing else in the game changes.

using Tidebook.Ui;
using UnityEngine;

namespace Tidebook.Dev
{
    [AddComponentMenu("Tidebook/Dev Tools")]
    public class DevToolsController : MonoBehaviour
    {
        [Tooltip("Found automatically if left empty.")]
        public TidebookUiController ui;

        [Header("Hotkeys (only while no menu is open)")]
        public KeyCode consoleKey = KeyCode.BackQuote;
        [Tooltip("Next weather: clear > rain > storm > fog.")]
        public KeyCode nextWeatherKey = KeyCode.F6;
        [Tooltip("Jump to night, or back to day.")]
        public KeyCode dayNightKey = KeyCode.F7;
        public KeyCode addCoinsKey = KeyCode.F8;
        public int coinsPerPress = 100;
        public KeyCode unlockAllKey = KeyCode.F9;
        [Tooltip("Undo the last dev command. Hold Shift to redo.")]
        public KeyCode undoKey = KeyCode.F10;

        DevConsole console;
        DevConsoleScreen screen;

        void Start()
        {
            if (!Debug.isDebugBuild)
            {
                enabled = false;   // release build: no cheats
                return;
            }

            if (ui == null) ui = UiFind.First<TidebookUiController>();
            if (ui == null || ui.Root == null)
            {
                Debug.LogError("DevToolsController needs a TidebookUiController in the scene.");
                enabled = false;
                return;
            }

            console = new DevConsole(new DevContext(ui.rod, ui.Log));
            screen = ui.Root.AddScreen<DevConsoleScreen>("Dev Console");
            screen.Bind(console);
        }

        void Update()
        {
            if (console == null) return;

            if (Input.GetKeyDown(consoleKey))
            {
                ToggleConsole();
                return;
            }

            if (UiFocus.AnyOpen) return;

            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (Input.GetKeyDown(nextWeatherKey)) Quick("weather next");
            else if (Input.GetKeyDown(dayNightKey)) Quick(IsDay() ? "hour night" : "hour day");
            else if (Input.GetKeyDown(addCoinsKey)) Quick("coins " + Mathf.Max(1, coinsPerPress));
            else if (Input.GetKeyDown(unlockAllKey)) Quick("unlockall");
            else if (Input.GetKeyDown(undoKey)) Quick(shift ? "redo" : "undo");
        }

        bool IsDay()
        {
            WorldState world = console.Context.World;
            return world == null || world.CurrentTime == TimeOfDay.Day;
        }

        void ToggleConsole()
        {
            if (screen.IsOpen)
            {
                screen.Close();
                return;
            }

            ui.CloseAll();
            screen.Open();
        }

        /// <summary>Runs a console line from a hotkey and shows the outcome as a toast (and in the console log).</summary>
        void Quick(string line)
        {
            DevResult result = console.Run(line);
            screen.Print("> " + line, true);
            screen.Print(result.Message, result.Ok);
            if (ui.Toast != null) ui.Toast.Show(result.Ok ? "Dev" : "Dev - failed", result.Message, null);
        }
    }
}
