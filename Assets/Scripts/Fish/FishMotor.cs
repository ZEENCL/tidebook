// FishMotor.cs
// Movement only: how a fish swims inside its zone at its depth layer.

using UnityEngine;

namespace Tidebook
{
    public class FishMotor
    {
        readonly Transform transform;
        readonly Bounds area;
        readonly float layerY;
        readonly float reachDistance;

        public FishMotor(Transform transform, Bounds area, float layerY, float reachDistance)
        {
            this.transform = transform;
            this.area = area;
            this.layerY = layerY;
            this.reachDistance = reachDistance;
        }

        public Vector3 Position { get { return transform.position; } }

        /// <summary>Swims horizontally toward target at this fish's layer. Returns true when arrived.</summary>
        public bool MoveToward(Vector3 target, float speed, float dt)
        {
            target.y = layerY;
            Vector3 toTarget = target - transform.position;
            if (toTarget.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(toTarget);

            transform.position = Vector3.MoveTowards(transform.position, target, speed * dt);

            Vector3 remaining = target - transform.position;
            remaining.y = 0f;
            return remaining.magnitude <= reachDistance;
        }

        public Vector3 RandomPointInArea()
        {
            return new Vector3(
                Random.Range(area.min.x, area.max.x),
                layerY,
                Random.Range(area.min.z, area.max.z));
        }
    }
}
