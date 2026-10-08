using UnityEngine;

namespace Tidebook
{
    public class WorldState : MonoBehaviour
    {
        public static WorldState Instance { get; private set; }

        [Range(0f, 24f)] public float hour = 8f;
        public float dayLengthSeconds = 300f;   // real seconds for a full 24h cycle
        public bool advanceTime = true;
        public Weather weather = Weather.Clear;

        public TimeOfDay CurrentTime
        {
            get { return (hour >= 6f && hour < 18f) ? TimeOfDay.Day : TimeOfDay.Night; }
        }

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (advanceTime && dayLengthSeconds > 0f)
                hour = (hour + 24f * Time.deltaTime / dayLengthSeconds) % 24f;
        }

        /// <summary>Safe to call even when no WorldState exists in the scene (defaults to clear daytime).</summary>
        public static WaterContext ContextFor(string spotId)
        {
            WaterContext ctx = new WaterContext();
            ctx.spotId = spotId;
            if (Instance != null)
            {
                ctx.time = Instance.CurrentTime;
                ctx.weather = Instance.weather;
            }
            else
            {
                ctx.time = TimeOfDay.Day;
                ctx.weather = Weather.Clear;
            }
            return ctx;
        }
    }
}
