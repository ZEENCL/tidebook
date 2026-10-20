// RodController.cs
// Composition root for the rod: creates the default services, wires them into the state factory,
// subscribes the default catch listeners, and ticks the state machine. It holds no gameplay logic.
//
// Every dependency is an interface with a sensible default. To swap one (new Input System, a real
// bait-builder UI, a model for the bobber...) assign your own before Start().
// Other systems (Fish Index, achievements, audio) can react to outcomes through CatchEvents.

using UnityEngine;

namespace Tidebook
{
    public class RodController : MonoBehaviour
    {
        [Header("Aim")]
        public Camera cam;

        [Header("Casting")]
        public CastSettings cast = new CastSettings();

        [Header("Reeling")]
        public ReelSettings reel = new ReelSettings();

        // ---- injection points (defaults are created in Start if left null) ----
        public IRodInput InputSource { get; set; }
        public IFishRegistry Registry { get; set; }
        public IWaterZones Zones { get; set; }
        public IBaitProvider BaitProvider { get; set; }
        public IBobberView Bobber { get; set; }
        public IWallet Wallet { get; set; }

        /// <summary>Read-only view of the rod for UI.</summary>
        public IRodStatus Status { get { return session; } }

        /// <summary>Subscribe here to react when a cast is resolved (caught, snapped, missed...).</summary>
        public ICatchEvents CatchEvents { get { return catches; } }

        readonly StateMachine machine = new StateMachine();
        readonly RodSession session = new RodSession();
        readonly CatchReporter catches = new CatchReporter();

        void Start()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("RodController needs a Camera (assign one or tag one MainCamera).");
                enabled = false;
                return;
            }

            if (InputSource == null) InputSource = new LegacyRodInput();
            if (Registry == null) Registry = new StaticFishRegistry();
            if (Zones == null) Zones = new WaterZoneLookup();
            if (Bobber == null) Bobber = new PrimitiveBobber();
            if (Wallet == null) Wallet = new PlayerWallet();
            if (BaitProvider == null)
            {
                InspectorBaitLoadout loadout = GetComponent<InspectorBaitLoadout>();
                if (loadout == null) loadout = gameObject.AddComponent<InspectorBaitLoadout>();
                BaitProvider = loadout;
            }

            // Default reactions to a resolved cast (order matters: pay first, then release the fish, then message).
            catches.Subscribe(new CatchRewardListener(Wallet));
            catches.Subscribe(new FishOutcomeListener());
            catches.Subscribe(new CatchMessageListener(session));

            ICastLandingResolver resolver = new ZoneCastResolver(cam.transform, Zones, cast);
            RodStateFactory states = new RodStateFactory(
                machine, session, InputSource, cast, reel, resolver, BaitProvider, Bobber, Registry, catches);

            machine.ChangeState(states.Aiming());

            RodHud hud = GetComponent<RodHud>();
            if (hud != null) hud.Bind(session, Wallet, BaitProvider);
        }

        void Update()
        {
            machine.Tick(Time.deltaTime);
        }
    }
}
