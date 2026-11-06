// UiFind.cs - version-safe object lookup (FindObjectOfType was renamed in Unity 2023).

using UnityEngine;

namespace Tidebook.Ui
{
    public static class UiFind
    {
        public static T First<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }

        public static T[] All<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindObjectsByType<T>(FindObjectsSortMode.None);
#else
            return Object.FindObjectsOfType<T>();
#endif
        }
    }
}
