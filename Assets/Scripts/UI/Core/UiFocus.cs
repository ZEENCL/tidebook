// UiFocus.cs
// Tracks which UI screens are open. Gameplay code can check UiFocus.AnyOpen
// (e.g. to stop mouse-look while a menu is showing).

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook.Ui
{
    public static class UiFocus
    {
        static readonly List<UiScreen> open = new List<UiScreen>();

        public static event System.Action Changed;

        public static bool AnyOpen { get { return open.Count > 0; } }
        public static UiScreen Top { get { return open.Count > 0 ? open[open.Count - 1] : null; } }

        public static void Push(UiScreen screen)
        {
            if (!open.Contains(screen)) open.Add(screen);
            Raise();
        }

        public static void Pop(UiScreen screen)
        {
            if (open.Remove(screen)) Raise();
        }

        static void Raise()
        {
            System.Action handler = Changed;
            if (handler != null) handler();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            open.Clear();
            Changed = null;
        }
    }
}
