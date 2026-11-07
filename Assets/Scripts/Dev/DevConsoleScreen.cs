// DevConsoleScreen.cs
// The on-screen text console (a UiScreen, so it dims and pauses the game like the other menus).
// It only reads keys and shows text: whatever you type is handed to DevConsole.Run().
//   Enter = run      Up / Down = earlier commands      Ctrl/Cmd+V = paste      Esc or ` = close
// Typing is read straight from Input.inputString, so there is no focus handling to go wrong.

using System.Collections.Generic;
using Tidebook.Ui;
using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Dev
{
    public class DevConsoleScreen : UiScreen
    {
        const float WindowW = 1240f;
        const float WindowH = 700f;
        const int MaxLines = 15;
        const int MaxRecalled = 50;

        protected override Vector2 WindowSize { get { return new Vector2(WindowW, WindowH); } }
        public override bool CapturesTyping { get { return true; } }

        DevConsole console;
        Text logText;
        Text inputText;

        readonly List<string> lines = new List<string>();
        readonly List<string> recalled = new List<string>();
        string input = "";
        int recallIndex;
        string shown;   // what the input line currently displays (so we only rebuild text when it changes)

        public void Bind(DevConsole devConsole)
        {
            console = devConsole;
            if (lines.Count == 0) Print("Developer console. Type 'help' for the commands. Esc or ` closes.", true);
        }

        /// <summary>Adds text to the log (also used by the hotkeys, so their results show up here too).</summary>
        public void Print(string message, bool ok)
        {
            if (string.IsNullOrEmpty(message)) return;

            string[] split = message.Split('\n');
            for (int i = 0; i < split.Length; i++)
            {
                string clean = split[i].Replace("<", "<​");   // stop typed text being read as rich-text tags
                lines.Add(ok ? clean : "<color=#FF9C80>" + clean + "</color>");
            }
            while (lines.Count > MaxLines) lines.RemoveAt(0);

            if (logText != null) logText.text = string.Join("\n", lines.ToArray());
        }

        // =====================================================================
        // Layout
        // =====================================================================

        protected override void BuildWindow(RectTransform w)
        {
            Image card = UiBuilder.Box(w, "Card", UiPalette.Spine, 34);
            UiBuilder.Stretch(card.rectTransform);
            UiBuilder.DropShadow(card, 14f, 0.35f);

            Text title = UiBuilder.Label(w, "Title", "Developer Console", 40, UiPalette.Sun, TextAnchor.UpperLeft, FontStyle.Bold);
            UiBuilder.TopLeft(title.rectTransform, 44f, 24f, 700f, 52f);

            Text sub = UiBuilder.Label(w, "Subtitle", "Testing tools. Everything here can be undone with 'undo'.",
                                       20, UiPalette.InkFaint, TextAnchor.UpperLeft);
            UiBuilder.TopLeft(sub.rectTransform, 46f, 78f, 1100f, 28f);

            Image logBg = UiBuilder.Box(w, "LogBg", UiPalette.CoverDark, 24);
            UiBuilder.TopLeft(logBg.rectTransform, 44f, 120f, WindowW - 88f, 470f);

            logText = UiBuilder.Label(logBg.transform, "Log", "", 22, UiPalette.Paper, TextAnchor.LowerLeft);
            UiBuilder.Stretch(logText.rectTransform, 24f, 14f, 24f, 14f);
            logText.text = string.Join("\n", lines.ToArray());

            Image inputBg = UiBuilder.Box(w, "InputBg", UiPalette.CoverDark, 20);
            UiBuilder.TopLeft(inputBg.rectTransform, 44f, 606f, WindowW - 88f, 62f);

            inputText = UiBuilder.Label(inputBg.transform, "Input", "", 28, UiPalette.White, TextAnchor.MiddleLeft);
            UiBuilder.Stretch(inputText.rectTransform, 24f, 0f, 24f, 0f);
            inputText.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        // =====================================================================
        // Behaviour
        // =====================================================================

        protected override void OnOpen()
        {
            input = "";
            recallIndex = recalled.Count;
            shown = null;
            RefreshInput();
        }

        protected override void OnTick()
        {
            ReadTyping();

            if (Input.GetKeyDown(KeyCode.UpArrow)) Recall(-1);
            else if (Input.GetKeyDown(KeyCode.DownArrow)) Recall(1);

            RefreshInput();
        }

        void ReadTyping()
        {
            string typed = Input.inputString;
            for (int i = 0; i < typed.Length; i++)
            {
                char c = typed[i];
                if (c == '\b')
                {
                    if (input.Length > 0) input = input.Substring(0, input.Length - 1);
                }
                else if (c == '\n' || c == '\r')
                {
                    Submit();
                }
                else if (!char.IsControl(c) && c != '`' && c != '~')   // ` is the key that closes the console
                {
                    input += c;
                }
            }

            bool mod = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)
                       || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
            if (mod && Input.GetKeyDown(KeyCode.V))
            {
                string clip = GUIUtility.systemCopyBuffer;
                if (!string.IsNullOrEmpty(clip))
                    input += clip.Replace("\r", " ").Replace("\n", " ").Replace("`", "");
            }
        }

        void Submit()
        {
            string line = input.Trim();
            input = "";
            if (line.Length == 0) return;

            Print("> " + line, true);

            if (recalled.Count == 0 || recalled[recalled.Count - 1] != line) recalled.Add(line);
            if (recalled.Count > MaxRecalled) recalled.RemoveAt(0);
            recallIndex = recalled.Count;

            if (line.ToLowerInvariant() == "clear")
            {
                lines.Clear();
                logText.text = "";
                return;
            }

            if (console == null)
            {
                Print("The console is not connected yet.", false);
                return;
            }

            DevResult result = console.Run(line);
            Print(result.Message, result.Ok);
        }

        /// <summary>Up / Down arrow: step through the commands typed earlier.</summary>
        void Recall(int direction)
        {
            if (recalled.Count == 0) return;

            recallIndex = Mathf.Clamp(recallIndex + direction, 0, recalled.Count);
            input = recallIndex < recalled.Count ? recalled[recallIndex] : "";
        }

        void RefreshInput()
        {
            bool caretOn = Mathf.Repeat(Time.unscaledTime, 1f) < 0.5f;
            string text = "> " + input.Replace("<", "<​") + (caretOn ? "_" : " ");
            if (text == shown) return;

            shown = text;
            inputText.text = text;
        }
    }
}
