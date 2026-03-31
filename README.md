# UtilityAI

A Unity 2D framework for building AI-driven characters using **Utility AI** — weighted action scoring with short-term memory, ScriptableObject-driven behavior, and a lightweight ECS-style entity/component registry.

Built with Unity **6000.3.10f1**.

---

## Why it exists

This was extracted from [Divrom](https://github.com/keshavprasadneupane/Divrom), a larger personal game project. The goal was to isolate the AI system into a clean, self-contained reference with the minimum supporting code needed to actually run it — no unrelated gameplay logic, no third-party dependencies.

It is not a general-purpose game framework. It is a focused implementation you can study, drop into a project, or build on top of.

---

## Sample scene

A ready-to-run demo scene is included at `Assets/_Project/_Game/Scenes/SampleScene`. Open it and press Play to see the Utility AI system running on a live entity — idle, wander, and move-toward behaviors driven by health and distance considerations.

---

## What's included

| System | Description |
|---|---|
| `EntityManager` | Root identity component for an entity |
| `EntityComponentsRegistry` | Runtime component container and registry |
| `Context` / `IReadOnlyContext` | World state passed to AI; read-only for planners |
| `AIBrain` | Drives execution — ticks actions, handles interrupts |
| `AIBrainAlgorithm` | Abstract planner base; swappable for GOAP, BTs, etc. |
| `UtilityAiAlgorithm` | Scores and selects actions using weighted considerations |
| `ActionSO` / `BaseActionSO` | ScriptableObject-based executable actions |
| `ConsiderationSO` | Scoring pieces attached to actions |
| `MovementComponentBase` | Physics-driven 2D movement via intent pattern |
| `HealthComponentBase` | Health storage, damage, events |
| `InitializableBase` / `InitManager` | Controlled initialization order |
| Timers, PriorityQueue | Lightweight utilities used internally |

Included actions: `IdleAction`, `MoveTowardAction`, `RandomWanderActionSO`

Included considerations: `HealthConsideration`, `TargetDistanceConsideration`, `CompositeConsideration`

---

## How to use it

### Drop into a project

1. Copy `Assets/_Project/Scripts` into your Unity project.
2. Attach `EntityManager` and `EntityComponentsRegistry` to your entity GameObject.
3. Attach `AIBrain` and assign a `UtilityAiAlgorithm` instance as the planner.
4. Create `ActionSO` assets and configure considerations in the editor.
5. Assign actions to the `UtilityAiAlgorithm`.

### Add a new action

1. Inherit from `ActionSO`.
2. Implement the template functions.
3. Add considerations to score it.
4. Assign to a `UtilityAiAlgorithm` config.

### Add a new consideration

1. Inherit from `ConsiderationSO`.
2. Implement `Evaluate(IReadOnlyContext context)`.
3. Optionally override `GetSelectedTargetRegistry(...)` if the action needs a target.
4. Attach to an action.

### Swap the planner

1. Inherit from `AIBrainAlgorithm`.
2. Implement `GetDecisionPlan(IReadOnlyContext context)` returning a list of `BaseActionSO`.
3. Assign to `AIBrain`.

---

## Architecture overview

```
EntityManager
  └── EntityComponentsRegistry
        ├── MovementComponentBase
        ├── HealthComponentBase
        └── AIBrain
              ├── Context  (reads registry + nearby entities)
              └── UtilityAiAlgorithm
                    └── ActionSO[]
                          └── ConsiderationSO[]
```

Each layer has one job:

- **EntityManager** — what the entity is
- **Registry** — what components it owns
- **Context** — what the AI can see
- **AIBrainAlgorithm** — how to choose
- **ActionSO** — what to do
- **ConsiderationSO** — how desirable it is

See [ARCHITECTURE.md](./ARCHITECTURE.md) for a full breakdown.

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

## License

MIT — see [LICENSE](./LICENSE) for details.