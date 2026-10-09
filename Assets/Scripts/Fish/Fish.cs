// Fish.cs
// The Unity-facing part of a fish: lifecycle, pooling hook, and the small role interfaces
// other systems use (IBaitReactive, IHookableFish). All behaviour lives in FishBrain + FishStates,
// all movement in FishMotor.

using System.Collections.Generic;
using UnityEngine;

namespace Tidebook
{
    public class Fish : MonoBehaviour, IBaitReactive, IHookableFish
    {
        /// <summary>All currently active (spawned, not pooled) fish.</summary>
        public static readonly List<Fish> Active = new List<Fish>();

        [Header("Tuning (prototype)")]
        public float detectionRadius = 10f;   // horizontal distance at which a fish can notice bait
        public float reachDistance = 0.6f;    // how close counts as "at the bait"

        public FishSpecies Species { get; private set; }

        public event System.Action<IHookableFish> Bit;

        FishBrain brain;
        System.Action<Fish> release;

        void OnEnable() { Active.Add(this); }
        void OnDisable() { Active.Remove(this); }

        public void Init(FishSpecies species, IBiteStrategy strategy, Bounds wanderArea,
                         float layerY, System.Action<Fish> releaseCallback)
        {
            Species = species;
            release = releaseCallback;

            if (brain != null) brain.Bitten -= OnBrainBitten;   // drop the previous life (pooled fish)
            FishMotor motor = new FishMotor(transform, wanderArea, layerY, reachDistance);
            brain = new FishBrain(species, strategy, motor, detectionRadius);
            brain.Bitten += OnBrainBitten;
        }

        void Update()
        {
            if (brain != null) brain.Machine.Tick(Time.deltaTime);
        }

        void OnBrainBitten()
        {
            System.Action<IHookableFish> handler = Bit;
            if (handler != null) handler(this);
        }

        // ---- extra (non-interface) info used by the spawner ----
        public bool IsBusy { get { return brain != null && brain.IsBusy; } }

        // ---- IBaitReactive ----
        public bool TryNotice(Bait bait, Vector3 position, WaterContext ctx)
        {
            return brain != null && brain.TryNotice(bait, position, ctx);
        }

        public void LoseInterest()
        {
            if (brain != null) brain.LoseInterest();
        }

        // ---- IBiteTarget ----
        public bool IsBiting { get { return brain != null && brain.IsBiting; } }

        public bool Hook()
        {
            return brain != null && brain.Hook();
        }

        // ---- IFightTarget ----
        public float FightStrength { get { return Species.fightStrength; } }
        public int Reward { get { return Species.currencyValue; } }

        public void Escape()
        {
            if (brain != null) brain.Escape();
        }

        public void Catch()
        {
            if (release != null) release(this);
            else Destroy(gameObject);
        }

        // ---- IFishInfo ----
        public string DisplayName { get { return Species != null ? Species.displayName : "-"; } }
        public string StateName { get { return brain != null ? brain.StateName : "-"; } }
        public float MatchRatio { get { return brain != null ? brain.MatchRatio : 0f; } }
        public float BiteTimeLeft { get { return brain != null ? brain.BiteTimeLeft : 0f; } }
    }
}
