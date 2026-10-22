// HudContext.cs
// Shared, read-mostly data every HUD view needs: the rod status, the camera, timing, and a few helpers
// (project a world point to the screen, find a meter, look up a species in the Tidebook).

using UnityEngine;

namespace Tidebook.Ui
{
    public class HudContext
    {
        public IRodStatus Status;
        public IFishIndexView Index;
        public IWalletView Wallet;
        public RectTransform Layer;
        public Camera Cam;
        public Transform CastOrigin;
        public bool ShowTrajectory = true;

        public RodPhase Phase;
        public float PhaseTime;   // seconds since the phase began
        public float Now;         // unscaled time (the HUD keeps animating while the game is paused)
        public float Dt;

        /// <summary>World point -> HUD layer coordinates. False if the point is behind the camera.</summary>
        public bool TryProject(Vector3 world, out Vector2 local)
        {
            local = Vector2.zero;
            if (Cam == null) return false;

            Vector3 screen = Cam.WorldToScreenPoint(world);
            if (screen.z <= 0.05f) return false;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(Layer, new Vector2(screen.x, screen.y), null, out local);
        }

        /// <summary>Like TryProject, but keeps the point on screen (used so the bite alert is never lost).</summary>
        public Vector2 ProjectClamped(Vector3 world, float margin, out bool offscreen)
        {
            Vector2 local = Vector2.zero;
            offscreen = true;
            if (Cam == null) return local;

            Vector3 screen = Cam.WorldToScreenPoint(world);
            if (screen.z < 0f)
            {
                screen.x = Screen.width - screen.x;     // behind the camera: mirror so it pins to the far edge
                screen.y = Screen.height - screen.y;
            }

            float cx = Mathf.Clamp(screen.x, margin, Screen.width - margin);
            float cy = Mathf.Clamp(screen.y, margin, Screen.height - margin);
            offscreen = screen.z < 0f || cx != screen.x || cy != screen.y;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(Layer, new Vector2(cx, cy), null, out local);
            return local;
        }

        /// <summary>Roughly where the rod tip is (a point in front of and below the camera) unless a CastOrigin is assigned.</summary>
        public Vector3 CastOriginPosition()
        {
            if (CastOrigin != null) return CastOrigin.position;
            Transform t = Cam.transform;
            return t.position + t.forward * 0.5f + t.right * 0.28f - t.up * 0.34f;
        }

        public IMeterView Meter(string label)
        {
            System.Collections.Generic.IReadOnlyList<IMeterView> meters = Status.Meters;
            for (int i = 0; i < meters.Count; i++)
            {
                if (meters[i].Label == label) return meters[i];
            }
            return null;
        }

        public FishIndexEntry EntryFor(FishSpecies species)
        {
            if (Index == null || species == null) return null;
            System.Collections.Generic.IReadOnlyList<FishIndexEntry> all = Index.Entries;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].Species == species) return all[i];
            }
            return null;
        }
    }
}
