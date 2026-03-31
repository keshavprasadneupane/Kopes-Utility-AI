
// Copyright (c) 2026 Keshav Prasad Neupane (Kope)
// Licensed under the MIT License. See LICENSE in the repository root for details.
using UnityEngine;
using UnityEngine.InputSystem;
using Kope.Component.Movement;

public class PlayerMovementComponent : MovementComponentBase {
	public void MoveForInputSystem(InputAction.CallbackContext context) {
		var value = context.ReadValue<Vector2>();
		SetMovementIntent(new MovementIntent(value, MovementIntentType.Move));
	}
}
