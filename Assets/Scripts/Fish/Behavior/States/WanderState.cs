using UnityEngine;

namespace Tidebook
{
    /// <summary>Idle swimming around the zone, unaware of any bait.</summary>
    public class WanderState : FishState
    {
        Vector3 target;
        float pause;

        public WanderState(FishBrain brain) : base(brain) { }

        public override void Enter()
        {
            target = brain.Motor.RandomPointInArea();
            pause = 0f;
        }

        public override void Tick(float dt)
        {
            if (pause > 0f)
            {
                pause -= dt;
                return;
            }

            if (brain.Motor.MoveToward(target, brain.Species.swimSpeed, dt))
            {
                pause = Random.Range(0.5f, 2.5f);
                target = brain.Motor.RandomPointInArea();
            }
        }
    }
}
