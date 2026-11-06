// WorldFeedback.cs
// The in-world half of the GDD's feedback (3.2 and 3.3), drawn with flat sprites lying on the water:
//   - Pre-cast ripples      : small rings on the surface where fish are swimming nearby (look for them!)
//   - Approaching shadows   : a dark fish silhouette for each fish that has noticed your bait. Fainter when
//                             it is only curious, darker when it nibbles, strong when it bites. Bigger
//                             shadow = rarer fish.
//   - Splash and nibble rings at the bobber, and rings when a fish pulls or is hooked
// Uses SpriteRenderers with the default sprite material, so it works in the built-in and URP pipelines.

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook.Ui
{
    public class WorldFeedback : MonoBehaviour
    {
        const int RippleCount = 14;
        const int ShadowCount = 8;
        const float RippleSpriteSize = 1.28f;   // ring sprite is 128px at 100 pixels-per-unit
        const float ShadowSpriteWidth = 1.6f;   // fish sprite is 160px wide at 100 pixels-per-unit

        class Ripple
        {
            public SpriteRenderer sr;
            public bool active;
            public float age;
            public float life;
            public float size;
            public float alpha;
        }

        class Shadow
        {
            public SpriteRenderer sr;
            public float alpha;
            public float yaw;
        }

        RodController rod;
        IFishLocator locator;
        IRodStatus status;
        Ripple[] ripples;
        Shadow[] shadows;

        float rippleTimer = 1f;
        float nibbleTimer = 0.4f;

        public static WorldFeedback Create(RodController rod, IFishLocator locator)
        {
            GameObject go = new GameObject("Tidebook World Feedback");
            WorldFeedback feedback = go.AddComponent<WorldFeedback>();
            feedback.Init(rod, locator);
            return feedback;
        }

        void Init(RodController rodController, IFishLocator fishLocator)
        {
            rod = rodController;
            locator = fishLocator;
            status = rod.Status;

            Sprite ring = HudSprites.Ring(5);
            ripples = new Ripple[RippleCount];
            for (int i = 0; i < RippleCount; i++)
            {
                Ripple r = new Ripple();
                r.sr = MakeRenderer("Ripple", ring);
                r.sr.transform.rotation = Quaternion.Euler(90f, 0f, 0f);   // lie flat on the water
                ripples[i] = r;
            }

            Sprite fish = FishArt.Silhouette(new FishCatalogEntry());
            shadows = new Shadow[ShadowCount];
            for (int i = 0; i < ShadowCount; i++)
            {
                Shadow s = new Shadow();
                s.sr = MakeRenderer("Fish Shadow", fish);
                shadows[i] = s;
            }

            status.Cue += OnCue;
        }

        SpriteRenderer MakeRenderer(string name, Sprite sprite)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 5;
            go.SetActive(false);
            return sr;
        }

        void OnDestroy()
        {
            if (status != null) status.Cue -= OnCue;
        }

        // =====================================================================

        void OnCue(RodCue cue)
        {
            Vector3 bait = status.BaitPosition + Vector3.up * 0.03f;
            switch (cue)
            {
                case RodCue.CastLaunched:
                    SpawnRipple(bait, 1.8f, 1.5f, 0.85f);
                    UiSounds.Play(UiSound.Splash);
                    break;
                case RodCue.BiteStarted: SpawnRipple(bait, 1.0f, 0.8f, 0.8f); break;
                case RodCue.HookSet: SpawnRipple(bait, 1.4f, 1.0f, 0.85f); break;
                case RodCue.Pulled: SpawnRipple(bait, 1.2f, 0.9f, 0.8f); break;
            }
        }

        void Update()
        {
            Camera cam = rod.cam != null ? rod.cam : Camera.main;
            if (cam == null) return;

            float dt = Time.deltaTime;
            RodPhase phase = status.Phase;

            UpdateRipples(dt);

            // ---- hints of fish before the cast ----
            if (phase == RodPhase.Aiming || phase == RodPhase.Charging || phase == RodPhase.Resolved)
            {
                rippleTimer -= dt;
                if (rippleTimer <= 0f)
                {
                    rippleTimer = Random.Range(0.8f, 2.0f);
                    SpawnFishRipple(cam);
                }
            }

            // ---- after the cast: nibble rings and approaching shadows ----
            bool inWater = phase == RodPhase.Settling || phase == RodPhase.Waiting
                           || phase == RodPhase.BiteWindow || phase == RodPhase.Reeling;

            if (phase == RodPhase.Waiting || phase == RodPhase.BiteWindow) UpdateNibbles(dt);
            UpdateShadows(inWater, dt);
        }

        // ---------------------------------------------------------------- ripples

        void SpawnFishRipple(Camera cam)
        {
            IReadOnlyList<IFishCue> fish = locator.ActiveFish;
            IWaterZones zones = rod.Zones;
            if (fish.Count == 0 || zones == null) return;

            for (int tries = 0; tries < 6; tries++)
            {
                IFishCue f = fish[Random.Range(0, fish.Count)];
                Vector3 p = f.Position;
                Vector3 toFish = p - cam.transform.position;
                if (toFish.magnitude > 32f) continue;                                  // too far to notice
                if (Vector3.Dot(cam.transform.forward, toFish.normalized) < 0.2f) continue;   // behind or beside the player

                IWaterZone zone = zones.FindAt(p);
                if (zone == null) continue;

                float size = Random.Range(0.5f, 0.9f) * Mathf.Max(0.8f, f.SizeHint);
                SpawnRipple(new Vector3(p.x, zone.SurfaceY + 0.03f, p.z), size, Random.Range(1.2f, 1.8f), 0.6f);
                return;
            }
        }

        void SpawnRipple(Vector3 position, float size, float life, float alpha)
        {
            Ripple r = null;
            for (int i = 0; i < ripples.Length; i++)
            {
                if (!ripples[i].active) { r = ripples[i]; break; }
            }
            if (r == null) return;

            r.active = true;
            r.age = 0f;
            r.life = life;
            r.size = size;
            r.alpha = alpha;
            r.sr.transform.position = position;
            r.sr.transform.localScale = new Vector3(0.001f, 0.001f, 1f);
            r.sr.gameObject.SetActive(true);
        }

        void UpdateRipples(float dt)
        {
            for (int i = 0; i < ripples.Length; i++)
            {
                Ripple r = ripples[i];
                if (!r.active) continue;

                r.age += dt;
                float k = r.age / r.life;
                if (k >= 1f)
                {
                    r.active = false;
                    r.sr.gameObject.SetActive(false);
                    continue;
                }

                float grow = 1f - (1f - k) * (1f - k);   // fast at first, slowing down
                float scale = Mathf.Lerp(0.25f, 1f, grow) * r.size / RippleSpriteSize;
                r.sr.transform.localScale = new Vector3(scale, scale, 1f);
                r.sr.color = new Color(1f, 1f, 1f, r.alpha * (1f - k) * (1f - k));
            }
        }

        void UpdateNibbles(float dt)
        {
            FishCueState top = FishCueState.None;
            IReadOnlyList<IFishCue> watchers = status.Watchers;
            for (int i = 0; i < watchers.Count; i++)
            {
                if (watchers[i].CueState > top) top = watchers[i].CueState;
            }

            if (top != FishCueState.Interested && top != FishCueState.Biting)
            {
                nibbleTimer = 0.3f;
                return;
            }

            nibbleTimer -= dt;
            if (nibbleTimer > 0f) return;

            bool biting = top == FishCueState.Biting;
            nibbleTimer = biting ? 0.35f : Random.Range(0.7f, 1.2f);
            SpawnRipple(status.BaitPosition + Vector3.up * 0.03f, biting ? 0.9f : 0.5f, 0.9f, 0.7f);
            if (!biting) UiSounds.Play(UiSound.Nibble);
        }

        // ---------------------------------------------------------------- shadows

        static float AlphaFor(FishCueState state)
        {
            switch (state)
            {
                case FishCueState.Curious: return 0.28f;
                case FishCueState.Interested: return 0.45f;
                case FishCueState.Biting: return 0.70f;
                case FishCueState.Hooked: return 0.80f;
                default: return 0f;
            }
        }

        void UpdateShadows(bool inWater, float dt)
        {
            IReadOnlyList<IFishCue> watchers = status.Watchers;
            float surface = status.BaitPosition.y + 0.02f;
            float time = Time.time;

            for (int i = 0; i < shadows.Length; i++)
            {
                Shadow s = shadows[i];
                IFishCue cue = (inWater && i < watchers.Count) ? watchers[i] : null;

                if (cue == null)
                {
                    s.alpha = 0f;
                    s.sr.gameObject.SetActive(false);
                    continue;
                }

                float target = AlphaFor(cue.CueState);
                s.alpha = Mathf.MoveTowards(s.alpha, target, dt * 2.5f);
                if (s.alpha <= 0.01f)
                {
                    s.sr.gameObject.SetActive(false);
                    continue;
                }

                Vector3 forward = cue.Forward;
                forward.y = 0f;
                if (forward.sqrMagnitude > 0.0001f)
                {
                    forward.Normalize();
                    s.yaw = Mathf.Atan2(-forward.z, forward.x) * Mathf.Rad2Deg;
                }

                float alpha = s.alpha;
                float yaw = s.yaw;
                if (cue.CueState == FishCueState.Biting) alpha += 0.12f * Mathf.Sin(time * 14f);
                if (cue.CueState == FishCueState.Hooked) yaw += Mathf.Sin(time * 18f) * 22f;   // thrashing on the line

                Vector3 position = cue.Position;
                s.sr.transform.position = new Vector3(position.x, surface, position.z);
                s.sr.transform.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);

                float scale = Mathf.Max(0.3f, cue.SizeHint) / ShadowSpriteWidth;
                s.sr.transform.localScale = new Vector3(scale, scale, 1f);
                s.sr.color = new Color(0.04f, 0.12f, 0.17f, Mathf.Clamp01(alpha));
                s.sr.gameObject.SetActive(true);
            }
        }
    }
}
