namespace Tidebook
{
    public abstract class FishState : IState
    {
        protected readonly FishBrain brain;

        protected FishState(FishBrain brain) { this.brain = brain; }

        public virtual void Enter() { }
        public virtual void Tick(float dt) { }
        public virtual void Exit() { }
    }
}
