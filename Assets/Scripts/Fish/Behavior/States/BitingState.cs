using UnityEngine;

namespace Tidebook
{
    /// <summary>The bite window. Better match = longer (more forgiving) window.</summary>
    public class BitingState : FishState
    {
        float timeLeft;

        public BitingState(FishBrain brain) : base(brain) { }

        public override void Enter()
        {
            timeLeft = Mathf.Lerp(0.5f, 2f, brain.MatchRatio);
            brain.BiteTimeLeft = timeLeft;
            brain.RaiseBitten();   // tells whoever is listening (the rod) "I'm biting!"
        }

        public override void Tick(float dt)
        {
            timeLeft -= dt;
            brain.BiteTimeLeft = Mathf.Max(0f, timeLeft);
            if (timeLeft <= 0f) brain.GoWander(8f);   // player missed it
        }
    }
}
