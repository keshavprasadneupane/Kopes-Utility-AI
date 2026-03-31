// Copyright (c) 2026 Keshav Prasad Neupane (Kope)
// Licensed under the MIT License. See LICENSE in the repository root for details.

namespace Kope.AI.Utility {

	public enum ActionType {
		None = 0,
		Idle = 20,
		Move = 40,
		RandomWalk = 41, // a sub type of move action, used for wandering around without a specific target, just random movement.
		MoveToTarget = 42, // a sub type of move action, used for moving towards a specific target, with pathfinding and obstacle avoidance.
		Attack = 60,
		Flee = 80,
		Patrol = 100,
		Gather = 120,
		Heal = 140,
		SpecialAbility = 160
	}

}