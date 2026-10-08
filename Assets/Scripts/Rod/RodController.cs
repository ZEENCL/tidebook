using System.Text;
using UnityEngine;

namespace Tidebook
{
    public class RodController : MonoBehaviour
    {
        [Header("Casting")]
        public Camera cam;
        public float minCastRange = 4f;
        public float maxCastRange = 25f;
        public float chargeSeconds = 1.2f;       // time to reach full power

        [Header("Bait (placeholder until the bait-builder UI exists)")]
        public Buoyancy buoyancy = Buoyancy.Sinking;
        public BodyShape shape = BodyShape.Slim;
        public ColorPattern colorPattern = ColorPattern.RedWhite;
        public SwimPattern swim = SwimPattern.Steady;

        [Header("Reeling (tension mini-game tuning)")]
        public float reelTensionRate = 0.7f;     // tension gained per second while holding reel
        public float relaxTensionRate = 0.6f;    // tension lost per second while not holding
        public float progressRate = 0.18f;       // reel progress per second while tension is in the safe band
        public float slipRate = 0.12f;           // reel progress lost per second while outside the band

        // ----- runtime data the states read/write -----
        public StateMachine Machine { get; private set; }
        public Bait CurrentBait;
        public Vector3 BaitPosition;
        public WaterContext Context;
        public Fish Target;
        public float Power;           // 0..1 while charging
        public float Tension;         // 0..1 while reeling (snaps at 1)
        public float ReelProgress;    // 0..1 while reeling (caught at 1, escapes at 0)
        public float BandHalfWidth;   // safe band is 0.5 +/- this
        public int Currency;
        public string Message = "";

        GameObject bobber;

        // ----- injected dependencies (defaults are created in Start if left null) -----
        public IRodInput InputSource { get; set; }
        public IFishRegistry Registry { get; set; }
        public IWaterZones Zones { get; set; }

        // ----- input shortcuts used by the states -----
        public bool ActionPressed { get { return InputSource.ActionPressed; } }
        public bool ActionReleased { get { return InputSource.ActionReleased; } }
        public bool ReelHeld { get { return InputSource.ReelHeld; } }
        public bool CancelPressed { get { return InputSource.CancelPressed; } }

        void Awake()
        {
            Machine = new StateMachine();
        }

        void Start()
        {
            if (cam == null) cam = Camera.main;
            if (InputSource == null) InputSource = new LegacyRodInput();
            if (Registry == null) Registry = new StaticFishRegistry();
            if (Zones == null) Zones = new SpawnerWaterZones();
            Machine.ChangeState(new AimingState(this));
        }

        void Update()
        {
            Machine.Tick(Time.deltaTime);
        }

        // ------------------------------------------------------------------
        // Helpers used by the states
        // ------------------------------------------------------------------

        /// <summary>Builder pattern in action: the placeholder inspector fields become an immutable Bait.</summary>
        public Bait BuildBait()
        {
            return new BaitBuilder()
                .WithBuoyancy(buoyancy)
                .WithShape(shape)
                .WithColor(colorPattern)
                .WithSwim(swim)
                .Build();
        }

        /// <summary>
        /// Crosshair gives the direction, power gives the distance (min..max range).
        /// The cast only works if it lands inside a water zone (a FishSpawner area for now).
        /// </summary>
        public bool TryResolveLanding(float power, out Vector3 point, out IWaterZone zone)
        {
            point = Vector3.zero;
            zone = null;
            if (cam == null) return false;

            Vector3 dir = cam.transform.forward;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return false;   // looking straight up/down
            dir.Normalize();

            float distance = Mathf.Lerp(minCastRange, maxCastRange, power);
            Vector3 flat = cam.transform.position + dir * distance;

            zone = Zones.FindAt(flat);
            if (zone == null) return false;

            point = new Vector3(flat.x, zone.SurfaceY, flat.z);
            return true;
        }

        public void SpawnBobber(Vector3 position)
        {
            ClearBobber();
            bobber = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bobber.name = "Bobber";
            bobber.transform.position = position;
            bobber.transform.localScale = Vector3.one * 0.25f;
            Collider col = bobber.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        public void ClearBobber()
        {
            if (bobber != null) Destroy(bobber);
            bobber = null;
        }

        // ------------------------------------------------------------------
        // Debug HUD (no art yet - just enough to play the loop)
        // ------------------------------------------------------------------
        void OnGUI()
        {
            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;
            GUI.Label(new Rect(cx - 6f, cy - 12f, 30f, 24f), "+");   // crosshair

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Rod state: " + Machine.CurrentName);
            sb.AppendLine(Message);
            sb.AppendLine("Bait: " + BuildBait());
            sb.AppendLine("Currency: " + Currency);

            if (Machine.Current is ChargingState)
                sb.AppendLine("Power   " + Bar(Power));

            if (Machine.Current is ReelingState)
            {
                sb.AppendLine("Tension " + Bar(Tension) + "  (keep near the middle)");
                sb.AppendLine("Progress " + Bar(ReelProgress));
            }

            if (Machine.Current is BiteWindowState && Target != null)
                sb.AppendLine("!!! BITE !!!  time left: " + Target.BiteTimeLeft.ToString("0.0"));

            if (Target != null)
                sb.AppendLine("[debug] target: " + Target.Species.displayName + " | " + Target.StateName +
                              " | match " + Target.MatchRatio.ToString("0.00"));

            GUI.Label(new Rect(10f, 10f, 520f, 240f), sb.ToString());
        }

        static string Bar(float value)
        {
            int filled = Mathf.RoundToInt(Mathf.Clamp01(value) * 20f);
            return "[" + new string('#', filled) + new string('-', 20 - filled) + "]";
        }
    }
}
