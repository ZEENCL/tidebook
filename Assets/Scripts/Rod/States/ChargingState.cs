using UnityEngine;

namespace Tidebook
{
    /// <summary>Holding the button: power meter fills. Release to cast.</summary>
    public class ChargingState : RodState
    {
        readonly ICastInput input;
        readonly CastSettings cast;
        readonly ICastLandingResolver resolver;
        readonly IBaitProvider baits;
        readonly IBobberView bobber;

        public ChargingState(RodSession session, StateMachine machine, IRodStateFactory states,
                             ICastInput input, CastSettings cast, ICastLandingResolver resolver,
                             IBaitProvider baits, IBobberView bobber)
            : base(session, machine, states)
        {
            this.input = input;
            this.cast = cast;
            this.resolver = resolver;
            this.baits = baits;
            this.bobber = bobber;
        }

        public override void Enter()
        {
            session.Phase = RodPhase.Charging;
            session.Power = 0f;
            session.Target = null;
            session.Message = "Charging... release to cast (RMB cancels).";
        }

        public override void Tick(float dt)
        {
            session.Power = Mathf.Clamp01(session.Power + dt / cast.chargeSeconds);

            if (input.CancelPressed)
            {
                session.Message = "Cast cancelled.";
                machine.ChangeState(states.Aiming());
                return;
            }

            if (!input.ChargeReleased) return;

            Vector3 landing;
            IWaterZone zone;
            if (resolver.TryResolve(session.Power, out landing, out zone))
            {
                session.BaitPosition = landing;
                session.Water = zone.Context;
                session.Bait = baits.BuildBait();
                bobber.Show(landing);
                machine.ChangeState(states.Settling());
            }
            else
            {
                session.Message = "That cast didn't land in any fishing zone.";
                machine.ChangeState(states.Aiming());
            }
        }
    }
}
