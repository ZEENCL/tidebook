// Default implementation of the rod input interface.
using UnityEngine;

namespace Tidebook
{
    public class LegacyRodInput : IRodInput
    {
        public bool ChargePressed { get { return Input.GetMouseButtonDown(0); } }
        public bool ChargeReleased { get { return Input.GetMouseButtonUp(0); } }
        public bool HookPressed { get { return Input.GetMouseButtonDown(0); } }
        public bool ReelHeld { get { return Input.GetMouseButton(0); } }
        public bool CancelPressed { get { return Input.GetMouseButtonDown(1); } }
    }
}
