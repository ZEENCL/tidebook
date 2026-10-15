using UnityEngine;

namespace Tidebook
{
    /// <summary>Noticed the bait. Better match = shorter hesitation.</summary>
    public class CuriousState : FishState
    {
        float timer;

        public CuriousState(FishBrain brain) : base(brain) { }

        public override void Enter()
        {
            timer = Mathf.Lerp(4f, 0.5f, brain.MatchRatio);
        }

        public override void Tick(float dt)
        {
            timer -= dt;
            if (timer <= 0f) brain.Machine.ChangeState(new InterestedState(brain));
        }
    }
}
