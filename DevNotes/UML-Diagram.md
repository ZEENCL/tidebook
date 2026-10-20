# Tidebook - UML Diagram

This document reflects the current runtime structure in the Unity project as it exists in the codebase today. It focuses on the modules that are actually wired together: world/spot data, bait matching, fish spawning and behavior, the rod state machine, and catch outcome listeners.

## 1. Runtime module map

```mermaid
flowchart LR
    subgraph World["World + local conditions"]
        WorldState["WorldState<br/>hour / weather / spot context"]
        WaterZone["WaterZone<br/>geometry + Context"]
        Lookup["WaterZoneLookup<br/>FindAt()"]
    end

    subgraph Bait["Bait + matching"]
        BaitData["Bait / BaitBuilder"]
        Pref["PreferenceProfile<br/>FactorWeight"]
        Match["MatchFactors<br/>weighted factor rules"]
        Strategies["BiteStrategyRegistry<br/>Weighted / Gated"]
    end

    subgraph Fish["Fish module"]
        FishSpawner["FishSpawner"]
        Factory["FishFactory"]
        Selector["WeightedSpeciesSelector"]
        Fish["Fish<br/>MonoBehaviour"]
        Brain["FishBrain"]
        Motor["FishMotor"]
        Species["FishSpecies<br/>ScriptableObject"]
        Styles["FightStyle<br/>IPullPattern"]
    end

    subgraph Rod["Rod module"]
        RodController["RodController"]
        Session["RodSession<br/>Phase / Message / Meters / Target"]
        FactoryState["RodStateFactory"]
        States["RodState subclasses<br/>Aiming / Charging / Waiting / BiteWindow / Reeling / Resolved"]
        Hud["RodHud"]
        Input["LegacyRodInput"]
        Bobber["PrimitiveBobber"]
        Wallet["PlayerWallet"]
        Baits["InspectorBaitLoadout"]
    end

    subgraph Catch["Outcome pipeline"]
        CatchReporter["CatchReporter<br/>observer list"]
        Listeners["CatchRewardListener<br/>FishOutcomeListener<br/>CatchMessageListener"]
    end

    WorldState --> WaterZone
    WaterZone --> Lookup
    WaterZone --> FishSpawner

    FishSpawner --> Factory
    FishSpawner --> Selector
    Factory --> Fish
    Fish --> Brain
    Brain --> Motor
    Fish --> Species
    Species --> Pref
    Species --> Styles
    Species --> Strategies
    Strategies --> Match
    Baits --> BaitData
    Fish --> BaitData

    RodController --> Session
    RodController --> FactoryState
    RodController --> Input
    RodController --> Bobber
    RodController --> Wallet
    RodController --> Baits
    AssemblyState["StateMachine"] --- RodController
    FactoryState --> States
    States --> Session
    States --> Input
    States --> Bobber
    States --> Baits
    States --> CatchReporter

    RodController --> Hud
    Hud --> Session
    Hud --> Wallet

    CatchReporter --> Listeners
    Listeners --> Wallet
    Listeners --> Session
```

---

## 2. Rod controller and state wiring

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
        +ICatchEvents CatchEvents
        +Start()
        +Update()
    }

    class StateMachine {
        +IState Current
        +ChangeState(IState state)
        +Tick(float dt)
    }

    class RodSession {
        +RodPhase Phase
        +string Message
        +RodMeters Meters
        +Bait Bait
        +Vector3 BaitPosition
        +WaterContext Water
        +IHookableFish Target
        +RaiseCue(RodCue cue)
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

    class IRodStatus {
        <<interface>>
        +RodPhase Phase
        +string Message
        +IReadOnlyList~IMeterView~ Meters
        +IFishInfo Target
    }

    class RodHud {
        <<MonoBehaviour>>
        +Bind(session, wallet, baits)
    }

    class CatchReporter {
        +Subscribe(ICatchListener listener)
        +Publish(CatchReport report)
    }

    class LegacyRodInput
    class StaticFishRegistry
    class WaterZoneLookup
    class InspectorBaitLoadout
    class PrimitiveBobber
    class PlayerWallet
    class ZoneCastResolver

    RodController *-- StateMachine
    RodController *-- RodSession
    RodController --> RodStateFactory : creates
    RodController --> IRodInput
    RodController --> IFishRegistry
    RodController --> IWaterZones
    RodController --> IBaitProvider
    RodController --> IBobberView
    RodController --> IWallet
    RodController --> ICatchEvents
    RodController --> RodHud : optional bind

    RodSession ..|> IRodStatus
    RodStateFactory ..|> IRodStateFactory
    RodStateFactory --> IRodInput
    RodStateFactory --> ICastLandingResolver
    RodStateFactory --> IBaitProvider
    RodStateFactory --> IBobberView
    RodStateFactory --> IFishRegistry
    RodStateFactory --> ICatchPublisher

    LegacyRodInput ..|> IRodInput
    StaticFishRegistry ..|> IFishRegistry
    WaterZoneLookup ..|> IWaterZones
    InspectorBaitLoadout ..|> IBaitProvider
    PrimitiveBobber ..|> IBobberView
    PlayerWallet ..|> IWallet
    ZoneCastResolver ..|> ICastLandingResolver

    RodHud --> IRodStatus
    RodHud --> IWalletView
    CatchReporter ..|> ICatchPublisher
    CatchReporter ..|> ICatchEvents
```

---

## 3. Fish lifecycle and gameplay behavior

```mermaid
classDiagram
    direction LR

    class Fish {
        <<MonoBehaviour>>
        +static List~Fish~ Active
        +float detectionRadius
        +float reachDistance
        +FishSpecies Species
        +event Bit
        +bool IsBusy
        +Init(species, strategy, area, layerY, releaseCallback)
        +TryNotice(bait, pos, ctx) bool
        +LoseInterest()
        +Hook() bool
        +Escape()
        +Catch()
        +DisplayName
        +StateName
        +MatchRatio
        +BiteTimeLeft
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
        +TryNotice(bait, pos, ctx) bool
        +LoseInterest()
        +event Bit
    }

    class IHookableFish {
        <<interface>>
        +bool IsBiting
        +bool Hook()
    }

    class IFishInfo {
        <<interface>>
        +string DisplayName
        +string StateName
        +float MatchRatio
        +float BiteTimeLeft
    }

    class IFightTarget {
        <<interface>>
        +float FightStrength
        +IPullPattern CreatePullPattern()
    }

    Fish ..|> IBaitReactive
    Fish ..|> IHookableFish
    Fish ..|> IFishInfo
    Fish ..|> IFightTarget
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

## 4. Spawning, factory, registry, and world placement

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
        +Start()
        +Update()
    }

    class WaterZone {
        <<MonoBehaviour>>
        +static List~WaterZone~ All
        +string spotId
        +Vector3 areaSize
        +float depth
        +float SurfaceY
        +WaterContext Context
        +Bounds Area
        +float LayerY(DepthLayer layer)
        +bool Contains(Vector3 pos)
        +static WaterZone FindAt(Vector3 worldPos)
    }

    class WorldState {
        <<MonoBehaviour>>
        +static WorldState Instance
        +float hour
        +float dayLengthSeconds
        +bool advanceTime
        +Weather weather
        +TimeOfDay CurrentTime
        +static WaterContext ContextFor(string spotId)
    }

    class IWaterZones {
        <<interface>>
        +IWaterZone FindAt(Vector3 worldPos)
    }

    class IWaterZone {
        <<interface>>
        +float SurfaceY
        +WaterContext Context
    }

    class IFishFactory {
        <<interface>>
        +Fish Create(species, pos, area, layerY)
        +void Release(Fish fish)
    }

    class FishFactory {
        -Dictionary pool
        +Fish Create(species, pos, area, layerY)
        +void Release(Fish fish)
    }

    class ISpeciesSelector {
        <<interface>>
        +FishSpecies Pick(candidates, ctx)
    }

    class WeightedSpeciesSelector {
        +FishSpecies Pick(candidates, ctx)
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
        +FightStyle fightStyle
        +GameObject prefab
        +PreferenceProfile profile
        +bool CanExistIn(WaterContext ctx)
        +IPullPattern CreatePullPattern()
    }

    class WaterContext {
        <<struct>>
        +TimeOfDay time
        +Weather weather
        +string spotId
    }

    class Fish { <<MonoBehaviour>> }

    FishSpawner ..> WaterZone : requires
    WaterZone ..|> IWaterZone
    WorldState --> WaterContext : produces
    FishSpawner --> IFishFactory : Factory
    FishSpawner --> ISpeciesSelector : Selector
    FishSpawner --> FishSpecies : candidate list
    FishFactory ..|> IFishFactory
    FishFactory ..> Fish : builds + pools
    WeightedSpeciesSelector ..|> ISpeciesSelector
    ISpeciesSelector ..> FishSpecies : chooses
    Fish --> FishSpecies : data + visuals
```

---

## 5. Bait matching and bite evaluation

```mermaid
classDiagram
    direction LR

    class FishSpecies {
        <<ScriptableObject>>
        +BiteStrategyType biteStrategy
        +PreferenceProfile profile
        +FightStyle fightStyle
    }

    class PreferenceProfile {
        +Buoyancy buoyancy
        +FactorWeight buoyancyW
        +BodyShape shape
        +FactorWeight shapeW
        +ColorPattern color
        +FactorWeight colorW
        +SwimPattern swim
        +FactorWeight swimW
        +TimeOfDay time
        +FactorWeight timeW
        +Weather weather
        +FactorWeight weatherW
    }

    class FactorWeight {
        <<struct>>
        +float weight
        +bool required
    }

    class Bait {
        <<sealed>>
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

    class IBiteStrategy {
        <<interface>>
        +Evaluate(PreferenceProfile p, Bait bait, WaterContext ctx) BiteEvaluation
    }

    class WeightedBiteStrategy {
        -IReadOnlyList~IMatchFactor~ factors
        +Evaluate(p, bait, ctx) BiteEvaluation
    }

    class GatedBiteStrategy {
        -IBiteStrategy inner
        -IReadOnlyList~IMatchFactor~ factors
        +Evaluate(p, bait, ctx) BiteEvaluation
    }

    class IMatchFactor {
        <<interface>>
        +FactorWeight WeightFor(PreferenceProfile p)
        +bool Matches(PreferenceProfile p, Bait bait, WaterContext ctx)
    }

    class MatchFactor {
        +WeightFor(profile) FactorWeight
        +Matches(profile, bait, ctx) bool
    }

    class MatchFactors {
        <<static>>
        +IReadOnlyList Default
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

    class WaterContext {
        <<struct>>
        +TimeOfDay time
        +Weather weather
        +string spotId
    }

    WeightedBiteStrategy ..|> IBiteStrategy
    GatedBiteStrategy ..|> IBiteStrategy
    GatedBiteStrategy o-- IBiteStrategy : delegates to inner
    WeightedBiteStrategy --> IMatchFactor : iterates all factors
    GatedBiteStrategy --> IMatchFactor : checks required gates
    MatchFactor ..|> IMatchFactor
    MatchFactors o-- MatchFactor : Default list
    IBiteStrategy ..> BiteEvaluation : returns
    IBiteStrategy ..> PreferenceProfile : reads
    IBiteStrategy ..> Bait : reads
    IBiteStrategy ..> WaterContext : reads
    FishSpecies *-- PreferenceProfile : profile
    FishSpecies --> BiteStrategyType
    PreferenceProfile *-- FactorWeight
    BaitBuilder ..> Bait : builds
```

---

## 6. Rod state machine and shared cast/session data

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

    class RodSession {
        +RodMeters Meters
        +Bait Bait
        +Vector3 BaitPosition
        +WaterContext Water
        +IHookableFish Target
        +string Message
        +RodPhase Phase
        +event Cue
        +RaiseCue(RodCue cue)
    }

    class RodMeters {
        +IReadOnlyList~IMeterView~ Items
        +Clear()
        +Set(label, value)
        +Set(label, value, bandMin, bandMax)
    }

    class Meter {
        +string Label
        +float Value
        +bool HasBand
        +float BandMin
        +float BandMax
    }

    class IRodInput {
        <<interface>>
    }

    class ICastInput {
        <<interface>>
        +bool ChargePressed
        +bool ChargeReleased
        +bool CancelPressed
    }

    class IHookInput {
        <<interface>>
        +bool HookPressed
    }

    class IReelInput {
        <<interface>>
        +bool ReelHeld
    }

    class ICastLandingResolver {
        <<interface>>
        +TryResolve(float power, out Vector3 landing, out IWaterZone zone) bool
    }

    class IBaitProvider {
        <<interface>>
        +Bait BuildBait()
    }

    class IBobberView {
        <<interface>>
        +Show(Vector3 pos)
        +Hide()
    }

    class IFishRegistry {
        <<interface>>
        +IReadOnlyList~IBaitReactive~ ActiveFish
    }

    class IWallet {
        <<interface>>
    }

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

    RodSession *-- RodMeters
    RodMeters o-- Meter

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
    ResolvedState --> IBobberView
```

---

## 7. Catch outcomes and UI listeners

```mermaid
classDiagram
    direction LR

    class RodController {
        <<MonoBehaviour>>
        +ICatchEvents CatchEvents
        +IRodStatus Status
    }

    class CatchReporter {
        +Subscribe(ICatchListener listener)
        +Publish(CatchReport report)
    }

    class CatchReport {
        +CatchResult Result
        +Fish Fish
        +float Reward
        +string Message
    }

    class ICatchPublisher {
        <<interface>>
        +Publish(CatchReport report)
    }

    class ICatchEvents {
        <<interface>>
        +Subscribe(ICatchListener listener)
    }

    class ICatchListener {
        <<interface>>
        +OnCatchResolved(CatchReport report)
    }

    class CatchRewardListener {
        +OnCatchResolved(report)
    }

    class FishOutcomeListener {
        +OnCatchResolved(report)
    }

    class CatchMessageListener {
        +OnCatchResolved(report)
    }

    class RodHud {
        <<MonoBehaviour>>
        +Bind(session, wallet, baits)
    }

    class RodSession {
        +RodPhase Phase
        +string Message
        +RodMeters Meters
        +IHookableFish Target
    }

    class PlayerWallet {
        +Spend()
        +AddCoins()
    }

    class Fish {
        <<MonoBehaviour>>
        +Escape()
        +Catch()
    }

    RodController --> CatchReporter : creates
    CatchReporter ..|> ICatchPublisher
    CatchReporter ..|> ICatchEvents
    CatchReporter --> ICatchListener : notifies in subscription order
    CatchRewardListener ..|> ICatchListener
    FishOutcomeListener ..|> ICatchListener
    CatchMessageListener ..|> ICatchListener

    RodController --> RodHud : optional UI
    RodHud --> RodSession : reads phase/message/target/meters
    RodHud --> IWalletView : displays wallet state

    CatchRewardListener --> PlayerWallet : pays rewards
    FishOutcomeListener --> Fish : escapes / catches / resolves
    CatchMessageListener --> RodSession : writes status messages
```

---

## 8. Rod state flow

```mermaid
stateDiagram-v2
    [*] --> Aiming
    Aiming --> Charging : LMB down
    Charging --> Aiming : RMB cancel or invalid cast
    Charging --> Settling : release and cast lands in a zone
    Settling --> Waiting : timer completes
    Settling --> Resolved : RMB / retrieved
    Waiting --> BiteWindow : fish notices bait and bites
    Waiting --> Resolved : RMB / retrieved
    BiteWindow --> Reeling : click while target is biting
    BiteWindow --> Resolved : bite window expires / fish missed
    Reeling --> Resolved : caught, snapped, escaped, or missed
    Resolved --> Aiming : after resolve period
```

---

## 9. Fish state flow

```mermaid
stateDiagram-v2
    [*] --> Wander
    Wander --> Curious : bait noticed and evaluated as eligible
    Curious --> Interested : attention timer ends
    Interested --> Biting : bite roll succeeds
    Interested --> Wander : lose interest / cooldown
    Curious --> Wander : lose interest / bait removed
    Biting --> Hooked : player clicks within window
    Biting --> Wander : window expires / cooldown
    Hooked --> Wander : escape / cooldown
    Hooked --> [*] : catch, returned to pool
```
