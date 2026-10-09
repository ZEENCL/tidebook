// PrimitiveBobber.cs
// Placeholder bobber: a small sphere. Replace with a real model by writing another IBobberView.

using UnityEngine;

namespace Tidebook
{
    public class PrimitiveBobber : IBobberView
    {
        GameObject bobber;

        public void Show(Vector3 position)
        {
            Hide();
            bobber = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bobber.name = "Bobber";
            bobber.transform.position = position;
            bobber.transform.localScale = Vector3.one * 0.25f;

            Collider col = bobber.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
        }

        public void Hide()
        {
            if (bobber != null) Object.Destroy(bobber);
            bobber = null;
        }
    }
}
