# Architecture

This document covers the internal design of the UtilityAI framework in detail.

---

## 1. Entity layer

The project uses a custom ECS-style setup to organize runtime components.

### `EntityManager`

The root identity component for any entity in the system.

- Stores a common name (used for tag-style lookup) and a unique ID.
- Holds a reference to the entity's `EntityComponentsRegistry`.
- Validates required setup in the editor.
- Produces an `EntityDetail` object consumed by other systems.
- Broadcasts death and pool events to registered listeners.

This is what makes a GameObject a "valid entity" from the framework's perspective.

### `EntityComponentsRegistry`

Runtime container for all components belonging to an entity.

- Stores a list of components.
- Registers them into a `ComponentRegistry` keyed by type.
- Initializes them in declaration order via `InitManager`.
- Exposes the registry so AI, movement, health, and other systems can resolve each other by interface.

This is the single lookup point for everything an entity owns.

### `Context` / `IReadOnlyContext`

The world state object passed into the AI system.

`Context`:
- Gives mutable access to the current entity's registry.
- Gives read-only access to target registries.
- Caches targets by common name and unique ID for fast lookup.
- Automatically removes targets when they die or are pooled.

`IReadOnlyContext`:
- Read-only interface exposed to planners and considerations.
- Prevents AI evaluation code from mutating world state.

This is the data source passed to the AI planner and all actions.

---

## 2. Initialization layer

### `InitializableBase`

Base class for any component that needs managed initialization.

- Provides `OnInit()`, `OnFixedUpdate()`, and related lifecycle hooks.
- Tracks `IsInitialized` state.
- Intended to be called by `InitManager` rather than Unity's `Awake`/`Start`.

### `InitManager`

Controls initialization order across components.

- Ensures dependent systems initialize after their dependencies.
- Solves Unity's unreliable `Awake`/`Start` ordering when components depend on each other.

---

## 3. AI layer

### `AIBrain`

The runtime controller that drives behavior each frame.

- Builds the `Context` from the entity registry.
- Registers component-based interrupters (e.g. health reaching zero).
- Initializes the planner.
- Refreshes decisions on a configurable timer interval.
- Executes one action at a time.
- Ticks the active action in both `Update` and `FixedUpdate`.
- Handles three interrupt levels: soft (finish current), hard (stop now), death.

### `AIBrainAlgorithm`

Abstract base class for AI planners.

- Standardizes init, cleanup, and display name.
- Defines `GetDecisionPlan(IReadOnlyContext)` for returning an action sequence.
- Intentionally open — a GOAP or behavior tree planner can replace `UtilityAiAlgorithm` by inheriting this class and implementing `GetDecisionPlan`.

---

## 4. Utility AI implementation

### `UtilityAiAlgorithm`

The default planner. Selects one action per decision cycle.

**Scoring:**
- Each action scores itself by multiplying its consideration scores together.
- A compensation factor prevents actions with many considerations from being unfairly punished relative to actions with fewer.
- A configurable bias weight is applied per action.

**Action memory:**
- Recently selected actions are pushed into a short-term memory queue (bounded capacity).
- Their bias weights decay after selection, making repetition less likely.
- Weights regenerate over time for actions that are not being selected.
- This produces more varied, less robotic behavior without a separate state machine.

**Anti-starvation:**
- An idle rescue triggers if no action scores above a minimum threshold, preventing the entity from locking up.

Returns a single-action plan. Multi-step planning is not the goal here.

---

## Actions

Actions are `ScriptableObject` assets configured in the editor and shared across entity instances. Each instance gets its own runtime state via `ScriptableObject.Instantiate` to avoid shared state bugs.

### `BaseActionSO`

Base class for executable actions. Defines the common action lifecycle:

| Method | When called |
|---|---|
| `OnInitialize` | When the action is selected and started |
| `TickUpdate` | Each `Update` tick while active |
| `TickFixedUpdate` | Each `FixedUpdate` tick while active |
| `OnEndOrAbort` | When the action completes normally or interrupted|
| `MarkCompleted`| Mark action is completed|

### `ActionSO`

Extends `BaseActionSO` with Utility AI-specific fields.

- Stores the action type identifier.
- Stores a list of `ConsiderationSO` instances.
- Exposes decay, momentum, and regeneration parameters used by `UtilityAiAlgorithm`.
- `EvaluateScore(IReadOnlyContext)` — multiplies consideration scores together, then compensates the final score so actions with many considerations are not unfairly punished.

### `IdleAction`

A simple wait action. Starts a countdown timer and completes after a configured duration. Used as a fallback when no other action scores high enough.

### `MoveTowardAction`

Moves the entity toward a target selected by its considerations.

- Resolves the target registry from the consideration that cached it.
- Reads the target's transform each physics tick.
- Stops when inside the dead zone radius.
- Aborts after a max duration to prevent stalling.

### `RandomWanderActionSO`

Moves toward a randomly generated point near the entity's current position.

- Picks a point each time the action starts.
- Steers using `MovementComponentBase.SetMovementIntent`.
- Completes when the point is reached.

Useful for idle roaming or non-combat behavior.

---

## Considerations

Considerations are the per-action scoring units. They are also `ScriptableObject` assets, reusable across multiple actions.

### `ConsiderationSO`

Base class for all considerations.

- Defines which action types the consideration applies to.
- Initializes itself on enable/validate.
- Provides `Evaluate(IReadOnlyContext)` returning a `[0, 1]` score.
- Optionally exposes `GetSelectedTargetRegistry(...)` for actions that need a target reference.

### `CompositeConsideration`

Combines multiple child considerations into one.

- Multiplies their scores together.
- Accumulates the multiplication count so the parent action can apply the right compensation factor.
- Forwards the first valid target registry from a child.

Useful when an action should depend on several conditions at once.

### `HealthConsideration`

Scores based on the entity's current health percentage evaluated through an `AnimationCurve`.

- Finds the entity's health component and converts current health to a percentage.
- Curve shape controls behavior: concave for low-health retreat, convex for aggressive-when-healthy, etc.

### `TargetDistanceConsideration`

The main targeting consideration for movement or combat-style actions.

- Finds all targets matching a configured common name via `IReadOnlyContext`.
- Filters by max range and dead zone.
- Optionally filters by FOV angle using a squared-cosine check (avoids `Mathf.Acos` entirely).
- Selects the closest valid target.
- Scores using a distance curve.
- Caches the selected target registry so `MoveTowardAction` (or similar) can retrieve it later.

---

## Components

### `MovementComponentBase`

Physics-based 2D movement using an intent pattern.

- AI actions call `SetMovementIntent(MovementIntent)` — they never touch the `Rigidbody` directly.
- The component normalizes direction and blends desired velocity with current physics velocity each `FixedUpdate`.
- Tracks `lastDirection` for facing queries, only updating when movement is above `MOVEMENT_EPSILON`.
- Exposes `GetLookingAtDirection()` for both 2D (XY plane projection) and 3D (Rigidbody forward) modes.

The intent separation keeps actions decoupled from physics and makes it straightforward to layer multipliers (slow effects, knockback) at the component level without touching action code.

### `HealthComponentBase`

Stores and manages entity health.

- Tracks `MaxHealth` and `CurrentHealth`.
- Supports damage, healing, and direct HP set (debug).
- Fires `OnHealthChanged` events consumed by `HealthConsideration` and the UI.

### `HealthBar`

Simple UI script for a slider-based health bar.

- Reads health values from `HealthComponentBase`.
- Updates a UI slider fill value.
- Subscribes to health change events and unsubscribes on destroy.

### `ReduceHp`

Debug helper. Reduces health manually from the inspector or a button — intended for testing UI and damage behavior.

---

## Support systems

### `CountdownTimer` and `StopwatchTimer`

Reusable timers used by AI actions and brain refresh logic — idle action duration, move action time limit, AI decision refresh interval.

### `PriorityQueueSimple`

A Unity-friendly priority queue used by the Utility AI memory system.

- Stores actions by cost/priority.
- Supports efficient priority updates.
- Prevents repeated boilerplate queue logic.

### `CreateCheckerBoardFlooring`

Editor utility that generates a checkerboard floor in the scene. Used as a zero-dependency ground for the demo scene.

---

## Decision loop

Each frame in `AIBrain`:

```
1. Validate initialized state
2. Tick refresh timer
3. If current action is complete → end it
4. If no action is active and refresh interval elapsed:
     a. Call UtilityAiAlgorithm.GetDecisionPlan(context)
     b. Score all actions via considerations
     c. Apply bias weights and memory decay
     d. Select highest-scoring action
     e. Initialize and start the action
5. Tick active action (Update + FixedUpdate)
6. Check interrupt flags — soft / hard / death
```

Interrupts bypass the normal loop immediately and route to the appropriate cleanup path.

---

## Responsibility map

| Component | Responsibility |
|---|---|
| `EntityManager` | What the entity is |
| `EntityComponentsRegistry` | What components it owns |
| `Context` | What the AI can see |
| `AIBrainAlgorithm` | How to choose |
| `ActionSO` | What to do |
| `ConsiderationSO` | How desirable an action is |
| `MovementComponentBase` | How the body actually moves |
| `HealthComponentBase` | How health is stored and changed |

---

## Project structure

```
Assets/_Project/Scripts/
  AI/          — brain, planner, actions, considerations, editor helpers
  Component/   — movement, health, UI
  Cores/       — ECS, init, identity utilities
  Context/     — AI context and read-only world access
  Floor/       — scene debug helpers
Assets/_ThirdParty/
               — timer and priority queue utilities
```

---

## Design notes

**Why ScriptableObjects for actions and considerations?**
Editor configurability without prefab overhead. Multiple entities can share the same action asset; each gets its own runtime instance via `Instantiate` so state doesn't bleed between entities.

**Why one action at a time?**
Utility AI is not naturally a planning algorithm — it is a selection algorithm. Forcing multi-step plans adds complexity without benefit for the behaviors this system targets. If multi-step planning is needed, swap the planner for GOAP via `AIBrainAlgorithm`.

**Why the intent pattern for movement?**
Actions should describe *what* they want, not *how* the body achieves it. This keeps actions portable across different movement implementations and lets the component handle physics concerns (blending, epsilon checks, facing) in one place.

**Why `IReadOnlyContext` in considerations?**
Considerations run during scoring, which happens potentially every refresh cycle. Giving them write access to the world would make the scoring pass a side-effect minefield. Read-only enforces that evaluation is pure inspection.

**Why verbose namespaces and file names?**
Debugging a running AI system means reading stack traces and log output under pressure. Verbose names make the source of a problem obvious without cross-referencing code.
