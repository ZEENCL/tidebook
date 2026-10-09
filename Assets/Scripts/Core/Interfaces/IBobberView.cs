// IBobberView.cs - whatever shows the bait in the water (a sphere now, a model later).
using UnityEngine;

namespace Tidebook
{
    public interface IBobberView
    {
        void Show(Vector3 position);
        void Hide();
    }
}
