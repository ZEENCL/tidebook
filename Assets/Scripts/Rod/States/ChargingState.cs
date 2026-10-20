using UnityEngine;

namespace Tidebook
{
    /// <summary>Holding the button: the power meter fills. Release to cast.</summary>
    public class ChargingState : RodState
    {
        readonly ICastInput input;
        readonly CastSettings cast;
        readonly ICastLandingResolver resolver;
        readonly IBaitProvider baits;
        readonly IBobberView bobber;

        float power;   // 0..1, owned by this state and published as a meter

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
            session.Target = null;
            session.Message = "Charging... release to cast (RMB cancels).";
            power = 0f;
            session.Meters.Set("Power", power);
        }

        public override void Tick(float dt)
        {
            power = Mathf.Clamp01(power + dt / cast.chargeSeconds);
            session.Meters.Set("Power", power);

            if (input.CancelPressed)
            {
                session.Message = "Cast cancelled.";
                machine.ChangeState(states.Aiming());
                return;
            }

            if (!input.ChargeReleased) return;

            Vector3 landing;
            IWaterZone zone;
            if (resolver.TryResolve(power, out landing, out zone))
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
