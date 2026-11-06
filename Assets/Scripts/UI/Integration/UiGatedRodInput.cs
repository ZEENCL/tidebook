// UiGatedRodInput.cs
// Wraps the rod's input so clicks and keys meant for a menu never cast, hook or reel.
// (Decorator over IRodInput. A charge that was already underway can still be released.)

namespace Tidebook.Ui
{
    public class UiGatedRodInput : IRodInput
    {
        readonly IRodInput inner;

        public UiGatedRodInput(IRodInput inner)
        {
            this.inner = inner;
        }

        public bool ChargePressed { get { return !UiFocus.AnyOpen && inner.ChargePressed; } }
        public bool ChargeReleased { get { return inner.ChargeReleased; } }
        public bool HookPressed { get { return !UiFocus.AnyOpen && inner.HookPressed; } }
        public bool ReelHeld { get { return !UiFocus.AnyOpen && inner.ReelHeld; } }
        public bool CancelPressed { get { return !UiFocus.AnyOpen && inner.CancelPressed; } }
    }
}
