// HudView.cs
// One self-contained piece of the fishing HUD (crosshair, cast meter, bobber alerts, fight bar...).
// FishingHud builds each view, ticks it every frame and forwards rod cues to it.

using UnityEngine;

namespace Tidebook.Ui
{
    public abstract class HudView
    {
        protected HudContext ctx;

        public void Init(RectTransform layer, HudContext context)
        {
            ctx = context;
            Build(layer);
        }

        protected abstract void Build(RectTransform layer);
        public abstract void Tick();

        public virtual void OnCue(RodCue cue) { }
        public virtual void OnPhaseChanged(RodPhase phase) { }
    }
}
