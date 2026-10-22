// FishingHud.cs
// The gameplay HUD. It reads the rod's read-only IRodStatus and draws, with one small view per concern:
//   CastView        trajectory line, landing reticle, power meter
//   BobberView      waiting / nibbling / bite alerts above the bobber
//   CrosshairView   the crosshair
//   FightView       tension bar with pull telegraph, progress, coaching
//   HintView        controls prompt + messages
//   StatusView      time of day, weather, coins
//   CatchBannerView result card after each cast
// Plays UiSounds for the key moments so you can plug audio in later (IUiSound).

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook.Ui
{
    public class FishingHud : MonoBehaviour
    {
        RodController rod;
        HudContext ctx;
        CatchBannerView banner;
        readonly List<HudView> views = new List<HudView>();

        RodPhase lastPhase;
        float phaseStart;

        public static FishingHud Create(RectTransform layer, RodController rod, IFishIndexView index,
                                        Transform castOrigin, bool showTrajectory)
        {
            RectTransform root = UiBuilder.Rect(layer, "Fishing HUD");
            UiBuilder.Stretch(root);
            FishingHud hud = root.gameObject.AddComponent<FishingHud>();
            hud.Init(layer, rod, index, castOrigin, showTrajectory);
            return hud;
        }

        void Init(RectTransform layer, RodController rodController, IFishIndexView index, Transform castOrigin, bool showTrajectory)
        {
            rod = rodController;

            ctx = new HudContext();
            ctx.Status = rod.Status;
            ctx.Index = index;
            ctx.Layer = layer;
            ctx.CastOrigin = castOrigin;
            ctx.ShowTrajectory = showTrajectory;

            RectTransform root = (RectTransform)transform;

            // creation order = draw order (later views draw on top)
            Add(root, new CastView());
            Add(root, new BobberView());
            Add(root, new CrosshairView());
            Add(root, new FightView());
            Add(root, new HintView());
            Add(root, new StatusView());

            banner = new CatchBannerView();
            Add(root, banner);
            rod.CatchEvents.Subscribe(banner);

            ctx.Status.Cue += OnCue;
            lastPhase = ctx.Status.Phase;
        }

        void Add(RectTransform parent, HudView view)
        {
            view.Init(parent, ctx);
            views.Add(view);
        }

        void Update()
        {
            // these are created by the rod in its Start(), so pick them up lazily
            if (ctx.Cam == null) ctx.Cam = rod.cam != null ? rod.cam : Camera.main;
            if (ctx.Wallet == null) ctx.Wallet = rod.Wallet;

            ctx.Now = Time.unscaledTime;
            ctx.Dt = Time.unscaledDeltaTime;

            RodPhase phase = ctx.Status.Phase;
            if (phase != lastPhase)
            {
                lastPhase = phase;
                phaseStart = ctx.Now;
                for (int i = 0; i < views.Count; i++) views[i].OnPhaseChanged(phase);
            }
            ctx.Phase = phase;
            ctx.PhaseTime = ctx.Now - phaseStart;

            for (int i = 0; i < views.Count; i++) views[i].Tick();
        }

        void OnCue(RodCue cue)
        {
            switch (cue)
            {
                case RodCue.CastLaunched: UiSounds.Play(UiSound.Cast); break;
                case RodCue.CastRejected: UiSounds.Play(UiSound.Reject); break;
                case RodCue.BiteStarted: UiSounds.Play(UiSound.Bite); break;
                case RodCue.HookSet: UiSounds.Play(UiSound.HookSet); break;
                case RodCue.PullWarning: UiSounds.Play(UiSound.PullWarning); break;
                case RodCue.Pulled: UiSounds.Play(UiSound.Pull); break;
            }

            for (int i = 0; i < views.Count; i++) views[i].OnCue(cue);
        }

        void OnDestroy()
        {
            if (ctx != null && ctx.Status != null) ctx.Status.Cue -= OnCue;
            if (rod != null && banner != null) rod.CatchEvents.Unsubscribe(banner);
        }
    }
}
