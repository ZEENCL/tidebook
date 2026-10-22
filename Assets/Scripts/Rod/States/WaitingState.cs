using System.Collections.Generic;

namespace Tidebook
{
    /// <summary>Bait is in place. Every fish nearby gets a chance to notice it; we wait for the first bite.</summary>
    public class WaitingState : RodState
    {
        readonly ICastInput input;
        readonly IFishRegistry registry;
        readonly List<IBaitReactive> noticed = new List<IBaitReactive>();

        public WaitingState(RodSession session, StateMachine machine, IRodStateFactory states,
                            ICastInput input, IFishRegistry registry)
            : base(session, machine, states)
        {
            this.input = input;
            this.registry = registry;
        }

        public override void Enter()
        {
            session.Phase = RodPhase.Waiting;
            session.Message = "Waiting for a bite... (RMB reels in)";

            IReadOnlyList<IBaitReactive> fish = registry.ActiveFish;
            for (int i = 0; i < fish.Count; i++)
            {
                if (fish[i].TryNotice(session.Bait, session.BaitPosition, session.Water))
                {
                    fish[i].Bit += OnFishBit;
                    noticed.Add(fish[i]);
                    session.AddWatcher(fish[i].Cue);
                }
            }
        }

        public override void Tick(float dt)
        {
            if (input.CancelPressed) machine.ChangeState(states.Resolved(CatchResult.Retrieved));
        }

        void OnFishBit(IHookableFish fish)
        {
            // First fish to bite wins.
            session.Target = fish;
            machine.ChangeState(states.BiteWindow());
        }

        public override void Exit()
        {
            for (int i = 0; i < noticed.Count; i++)
            {
                noticed[i].Bit -= OnFishBit;
                if (!object.ReferenceEquals(noticed[i], session.Target)) noticed[i].LoseInterest();   // everyone else wanders off
            }
            noticed.Clear();
        }
    }
}
