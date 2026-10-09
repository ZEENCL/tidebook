# Tidebook - UML Diagram

## 1. Module map

```mermaid
flowchart LR
    subgraph Core["Core"]
        Types["FishingTypes<br/>(enums, WaterContext)"]
        SM["StateMachine"]
        BaitB["Bait + BaitBuilder"]
        Ifaces["Interfaces (25)"]
        Defaults["DefaultServices"]
    end

    subgraph FishM["Fish"]
        Fish["Fish (Unity shell)"]
        Brain["FishBrain + FishStates"]
        Motor["FishMotor"]
        Spawner["FishSpawner"]
        Factory["FishFactory"]
        Strat["BiteStrategies + MatchFactors"]
        Species["FishSpecies (data)"]
    end

    subgraph RodM["Rod"]
        Rod["RodController<br/>(composition root)"]
        RodSF["RodStateFactory"]
        RodStates["RodStates (7)"]
        Session["RodSession"]
        Hud["RodHud"]
        Services["ZoneCastResolver, PrimitiveBobber,<br/>PlayerWallet, InspectorBaitLoadout"]
    end

    subgraph WorldM["World"]
        Zone["WaterZone"]
        WS["WorldState"]
    end

    Rod --> RodSF --> RodStates
    RodStates --> Session
    Rod --> Services
    Rod --> Hud
    Hud -. reads .-> Session
    RodStates -. "role interfaces" .-> Fish
    Spawner --> Factory --> Fish
    Spawner --> Zone
    Fish --> Brain --> Motor
    Brain --> Strat --> Species
    Zone --> WS
    Services -. uses .-> Zone
    RodM -. depends on .-> Ifaces
    FishM -. depends on .-> Ifaces
    Fish --> SM
    RodStates --> SM
```

---

## 2. Fish behaviour (Unity shell, brain, motor, states)

```mermaid
classDiagram
    direction LR

    class Fish {
        <<MonoBehaviour>>
        +List~Fish~ Active$
        +FishSpecies Species
        +float detectionRadius
        +float reachDistance
        +event Bit
        +bool IsBusy
        +Init(species, strategy, area, layerY, release)
        +TryNotice(bait, pos, ctx) bool
        +Hook() bool
        +LoseInterest()
        +Escape()
        +Catch()
    }

    class FishBrain {
        +StateMachine Machine
        +FishMotor Motor
        +FishSpecies Species
        +float MatchRatio
        +Vector3 BaitPosition
        +float BiteTimeLeft
        +event Bitten
        +TryNotice(bait, pos, ctx) bool
        +Hook() bool
        +LoseInterest()
        +Escape()
        +GoWander(float cooldown)
        +RaiseBitten()
    }

    class FishMotor {
        +MoveToward(target, speed, dt) bool
        +RandomPointInArea() Vector3
    }

    class StateMachine {
        +IState Current
        +ChangeState(IState next)
        +Tick(float dt)
    }

    class IState {
        <<interface>>
        +Enter()
        +Tick(float dt)
        +Exit()
    }

    class FishState {
        <<abstract>>
        #FishBrain brain
    }

    class WanderState
    class CuriousState
    class InterestedState
    class BitingState
    class HookedState

    class IBaitReactive {
        <<interface>>
        +event Bit
        +TryNotice(bait, pos, ctx) bool
        +LoseInterest()
    }
    class IHookableFish {
        <<interface>>
    }

    Fish ..|> IBaitReactive
    Fish ..|> IHookableFish
    Fish *-- FishBrain : brain
    FishBrain *-- StateMachine : Machine
    FishBrain *-- FishMotor : Motor
    StateMachine o-- IState : Current
    FishState ..|> IState
    FishState --> FishBrain : brain
    FishState <|-- WanderState
    FishState <|-- CuriousState
    FishState <|-- InterestedState
    FishState <|-- BitingState
    FishState <|-- HookedState
    FishBrain ..> WanderState : creates
```

---

## 3. Spawning, factory and species selection

```mermaid
classDiagram
    direction LR

    class FishSpawner {
        <<MonoBehaviour>>
        +List~FishSpecies~ species
        +int targetPopulation
        +int maxSpawnPerTick
        +float tickInterval
        +Transform player
        +float activeRadius
        +IFishFactory Factory
        +ISpeciesSelector Selector
    }

    class WaterZone {
        <<MonoBehaviour>>
        +List~WaterZone~ All$
        +string spotId
        +Vector3 areaSize
        +float depth
        +float SurfaceY
        +WaterContext Context
        +Bounds Area
        +LayerY(DepthLayer layer) float
        +Contains(Vector3 pos) bool
        +FindAt(Vector3 pos)$ WaterZone
    }

    class IWaterZone {
        <<interface>>
        +float SurfaceY
        +WaterContext Context
    }

    class IFishFactory {
        <<interface>>
        +Create(species, pos, area, layerY) Fish
        +Release(Fish fish)
    }

    class FishFactory {
        -Dictionary pool
        +Create(species, pos, area, layerY) Fish
        +Release(Fish fish)
    }

    class IBiteStrategyResolver {
        <<interface>>
        +Resolve(FishSpecies species) IBiteStrategy
    }

    class BiteStrategyRegistry {
        +Register(BiteStrategyType t, IBiteStrategy s)
        +Resolve(FishSpecies species) IBiteStrategy
    }

    class ISpeciesSelector {
        <<interface>>
        +Pick(candidates, ctx) FishSpecies
    }

    class WeightedSpeciesSelector {
        +Pick(candidates, ctx) FishSpecies
    }

    class FishSpecies {
        <<ScriptableObject>>
        +string displayName
        +Rarity rarity
        +int currencyValue
        +DepthLayer layer
        +string[] allowedSpotIds
        +float spawnWeight
        +float swimSpeed
        +float fightStrength
        +BiteStrategyType biteStrategy
        +GameObject prefab
        +CanExistIn(WaterContext ctx) bool
    }

    class Fish {
        <<MonoBehaviour>>
    }

    FishSpawner ..> WaterZone : requires
    WaterZone ..|> IWaterZone
    FishSpawner o-- FishSpecies : species list
    FishSpawner --> IFishFactory : Factory
    FishSpawner --> ISpeciesSelector : Selector
    FishFactory ..|> IFishFactory
    FishFactory --> IBiteStrategyResolver : resolves strategy
    FishFactory ..> Fish : builds and pools
    BiteStrategyRegistry ..|> IBiteStrategyResolver
    WeightedSpeciesSelector ..|> ISpeciesSelector
    ISpeciesSelector ..> FishSpecies : picks
```

---

## 4. Bite strategy, match factors and species data

```mermaid
classDiagram
    direction LR

    class IBiteStrategy {
        <<interface>>
        +Evaluate(PreferenceProfile p, Bait bait, WaterContext ctx) BiteEvaluation
    }

    class WeightedBiteStrategy {
        -IReadOnlyList factors
        +Evaluate(p, bait, ctx) BiteEvaluation
    }

    class GatedBiteStrategy {
        -IBiteStrategy inner
        -IReadOnlyList factors
        +Evaluate(p, bait, ctx) BiteEvaluation
    }

    class IMatchFactor {
        <<interface>>
        +WeightFor(PreferenceProfile p) FactorWeight
        +Matches(p, bait, ctx) bool
    }

    class MatchFactor {
        -Func weight
        -Func matches
    }

    class MatchFactors {
        <<static>>
        +IReadOnlyList Default$
    }

    class BiteEvaluation {
        <<struct>>
        +bool eligible
        +float matchRatio
    }

    class BiteStrategyType {
        <<enumeration>>
        Weighted
        Gated
    }

    class FishSpecies {
        <<ScriptableObject>>
        +BiteStrategyType biteStrategy
        +PreferenceProfile profile
    }

    class PreferenceProfile {
        +Buoyancy buoyancy
        +BodyShape shape
        +ColorPattern color
        +SwimPattern swim
        +TimeOfDay time
        +Weather weather
        +FactorWeight buoyancyW to weatherW
    }

    class FactorWeight {
        <<struct>>
        +float weight
        +bool required
    }

    class Bait {
        <<immutable>>
        +Buoyancy Buoyancy
        +BodyShape Shape
        +ColorPattern Color
        +SwimPattern Swim
    }

    class BaitBuilder {
        +WithBuoyancy(v) BaitBuilder
        +WithShape(v) BaitBuilder
        +WithColor(v) BaitBuilder
        +WithSwim(v) BaitBuilder
        +Build() Bait
    }

    WeightedBiteStrategy ..|> IBiteStrategy
    GatedBiteStrategy ..|> IBiteStrategy
    GatedBiteStrategy o-- IBiteStrategy : wraps (inner)
    WeightedBiteStrategy --> IMatchFactor : loops over
    GatedBiteStrategy --> IMatchFactor : checks required
    MatchFactor ..|> IMatchFactor
    MatchFactors o-- MatchFactor : Default list
    IBiteStrategy ..> BiteEvaluation : returns
    IBiteStrategy ..> PreferenceProfile : reads
    IBiteStrategy ..> Bait : reads
    FishSpecies *-- PreferenceProfile : profile
    FishSpecies --> BiteStrategyType
    PreferenceProfile *-- FactorWeight
    BaitBuilder ..> Bait : builds
```

---

## 5. Fish role interfaces (Interface Segregation)

```mermaid
classDiagram
    direction TB

    class IBaitReactive {
        <<interface>>
        +event Bit
        +TryNotice(bait, pos, ctx) bool
        +LoseInterest()
    }
    class IBiteTarget {
        <<interface>>
        +bool IsBiting
        +Hook() bool
    }
    class IFightTarget {
        <<interface>>
        +float FightStrength
        +int Reward
        +Escape()
        +Catch()
    }
    class IFishInfo {
        <<interface>>
        +string DisplayName
        +string StateName
        +float MatchRatio
        +float BiteTimeLeft
    }
    class IHookableFish {
        <<interface>>
    }
    class Fish {
        <<MonoBehaviour>>
    }
    class WaitingState
    class BiteWindowState
    class ReelingState
    class RodHud

    IBiteTarget <|-- IHookableFish
    IFightTarget <|-- IHookableFish
    IFishInfo <|-- IHookableFish
    Fish ..|> IBaitReactive
    Fish ..|> IHookableFish

    WaitingState --> IBaitReactive : notices bait
    BiteWindowState --> IBiteTarget : hooks
    ReelingState --> IFightTarget : fights
    ReelingState --> IFishInfo : name for message
    RodHud --> IFishInfo : debug readout
```

---

## 6. Rod wiring (composition root, factory, injected services)

```mermaid
classDiagram
    direction LR

    class RodController {
        <<MonoBehaviour>>
        +Camera cam
        +CastSettings cast
        +ReelSettings reel
        +IRodInput InputSource
        +IFishRegistry Registry
        +IWaterZones Zones
        +IBaitProvider BaitProvider
        +IBobberView Bobber
        +IWallet Wallet
        +IRodStatus Status
    }

    class RodStateFactory {
        +Aiming() IState
        +Charging() IState
        +Settling() IState
        +Waiting() IState
        +BiteWindow() IState
        +Reeling() IState
        +Resolved(CatchResult r) IState
    }

    class IRodStateFactory {
        <<interface>>
    }

    class RodSession {
        +RodPhase Phase
        +string Message
        +float Power
        +float Tension
        +float ReelProgress
        +float BandHalfWidth
        +Bait Bait
        +Vector3 BaitPosition
        +WaterContext Water
        +IHookableFish Target
    }

    class IRodStatus {
        <<interface>>
        +RodPhase Phase
        +string Message
        +float Power
        +float Tension
        +float ReelProgress
        +IFishInfo Target
    }

    class RodHud {
        <<MonoBehaviour>>
        +Bind(status, wallet, baits)
    }

    class StateMachine

    class ICastLandingResolver { <<interface>> }
    class ZoneCastResolver
    class IBobberView { <<interface>> }
    class PrimitiveBobber
    class IBaitProvider { <<interface>> }
    class InspectorBaitLoadout { <<MonoBehaviour>> }
    class IWallet { <<interface>> }
    class IWalletView { <<interface>> }
    class PlayerWallet
    class IFishRegistry { <<interface>> }
    class StaticFishRegistry
    class IWaterZones { <<interface>> }
    class WaterZoneLookup
    class IRodInput { <<interface>> }
    class LegacyRodInput

    RodController *-- StateMachine
    RodController *-- RodSession
    RodController ..> RodStateFactory : creates
    RodStateFactory ..|> IRodStateFactory
    RodSession ..|> IRodStatus
    RodController --> IRodStatus : exposes
    RodController ..> RodHud : Bind()
    RodHud --> IRodStatus
    RodHud --> IWalletView

    RodController --> IRodInput
    RodController --> IFishRegistry
    RodController --> IWaterZones
    RodController --> IBaitProvider
    RodController --> IBobberView
    RodController --> IWallet
    RodController ..> ICastLandingResolver : creates

    LegacyRodInput ..|> IRodInput
    StaticFishRegistry ..|> IFishRegistry
    WaterZoneLookup ..|> IWaterZones
    InspectorBaitLoadout ..|> IBaitProvider
    PrimitiveBobber ..|> IBobberView
    PlayerWallet ..|> IWallet
    ZoneCastResolver ..|> ICastLandingResolver
    IWallet --|> IWalletView
```

---

## 7. Rod states and what each one depends on (Interface Segregation)

```mermaid
classDiagram
    direction LR

    class RodState {
        <<abstract>>
        #RodSession session
        #StateMachine machine
        #IRodStateFactory states
    }

    class AimingState
    class ChargingState
    class SettlingState
    class WaitingState
    class BiteWindowState
    class ReelingState
    class ResolvedState

    class ICastInput {
        <<interface>>
        +ChargePressed
        +ChargeReleased
        +CancelPressed
    }
    class IHookInput {
        <<interface>>
        +HookPressed
    }
    class IReelInput {
        <<interface>>
        +ReelHeld
    }
    class IRodInput {
        <<interface>>
    }

    class ICastLandingResolver { <<interface>> }
    class IBaitProvider { <<interface>> }
    class IBobberView { <<interface>> }
    class IFishRegistry { <<interface>> }
    class IWallet { <<interface>> }
    class CastSettings
    class ReelSettings

    RodState <|-- AimingState
    RodState <|-- ChargingState
    RodState <|-- SettlingState
    RodState <|-- WaitingState
    RodState <|-- BiteWindowState
    RodState <|-- ReelingState
    RodState <|-- ResolvedState

    ICastInput <|-- IRodInput
    IHookInput <|-- IRodInput
    IReelInput <|-- IRodInput

    AimingState --> ICastInput
    ChargingState --> ICastInput
    ChargingState --> CastSettings
    ChargingState --> ICastLandingResolver
    ChargingState --> IBaitProvider
    ChargingState --> IBobberView
    SettlingState --> ICastInput
    WaitingState --> ICastInput
    WaitingState --> IFishRegistry
    BiteWindowState --> IHookInput
    ReelingState --> IReelInput
    ReelingState --> ReelSettings
    ReelingState --> IWallet
    ResolvedState --> IBobberView
```

---

## 8. Rod state flow

```mermaid
stateDiagram-v2
    [*] --> Aiming
    Aiming --> Charging : LMB down
    Charging --> Aiming : RMB cancel, or cast outside a zone
    Charging --> Settling : LMB up, lands in a zone
    Settling --> Waiting : settle timer ends
    Settling --> Resolved : RMB, Retrieved
    Waiting --> BiteWindow : a fish bites
    Waiting --> Resolved : RMB, Retrieved
    BiteWindow --> Reeling : click in time, fish hooked
    BiteWindow --> Resolved : too slow, Missed
    Reeling --> Resolved : Caught, Snapped or Escaped
    Resolved --> Aiming : after 1.5s
```

---

## 9. Fish state flow (one instance per spawned fish)

```mermaid
stateDiagram-v2
    [*] --> Wander
    Wander --> Curious : bait noticed, gates pass, MatchRatio above 0
    Curious --> Interested : timer ends, shorter with better match
    Interested --> Biting : bite roll succeeds at the bait
    Interested --> Wander : gave up, 5s cooldown
    Biting --> Hooked : player clicks inside bite window
    Biting --> Wander : window expired, 8s cooldown
    Curious --> Wander : LoseInterest, bait removed
    Interested --> Wander : LoseInterest, bait removed
    Hooked --> Wander : Escape, 8s cooldown
    Hooked --> [*] : Catch, returned to pool
```
