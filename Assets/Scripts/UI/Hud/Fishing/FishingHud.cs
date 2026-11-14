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
//
// MVVM wiring for catch rewards lives here (this class is where Model, ViewModel and View are connected):
//   PlayerWallet  -> WalletViewModel      -> StatusView       coin counter and the "+N" pop
//   CatchReport   -> CatchResultViewModel -> CatchBannerView  the result card

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook.Ui
{
    public class FishingHud : MonoBehaviour
    {
        RodController rod;
        HudContext ctx;
        StatusView status;
        CatchBannerView banner;
        WalletViewModel walletViewModel;
        CatchResultViewModel catchViewModel;
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
            status = new StatusView();
            Add(root, status);

            // Catch card: the ViewModel listens to the rod's catch events, the View shows what it produces.
            // (Subscribed after the Tidebook log, so "new entry" is already up to date when the card is built.)
            catchViewModel = new CatchResultViewModel(ctx.EntryFor);
            banner = new CatchBannerView();
            Add(root, banner);
            banner.Bind(catchViewModel);
            rod.CatchEvents.Subscribe(catchViewModel);

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
            if (walletViewModel == null && rod.Wallet != null)
            {
                walletViewModel = new WalletViewModel(rod.Wallet);
                status.Bind(walletViewModel);
            }

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
            if (rod != null && catchViewModel != null) rod.CatchEvents.Unsubscribe(catchViewModel);
            if (banner != null) banner.Unbind();
            if (status != null) status.Unbind();
            if (walletViewModel != null) walletViewModel.Dispose();
        }
    }
}
