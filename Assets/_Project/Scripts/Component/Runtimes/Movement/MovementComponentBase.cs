// Copyright (c) 2026 Keshav Prasad Neupane (Kope)
// Licensed under the MIT License. See LICENSE in the repository root for details.

using UnityEngine;
using Kope.Core.Init;

namespace Kope.Component.Movement {
	public enum Dimension {
		TwoD,
		ThreeD,
	}
	/// <summary>
	/// Defines the type of movement intent.
	/// Just a simple enum to indicate what kind of movement is intended.
	/// </summary>
	public enum MovementIntentType {
		Stop = 0,
		Move = 10,
		Attacking = 20,
	}

	public struct MovementIntent {
		public Vector3 Direction;
		public MovementIntentType IntentType;
		public MovementIntent(Vector3 direction, MovementIntentType intentType = MovementIntentType.Stop) {
			this.Direction = direction;
			this.IntentType = intentType;
		}
	}



	public interface IMovementComponent {
		Vector3 Direction { get; }
		Vector3 Position { get; }
		void SetMovementIntent(MovementIntent intent);
		Vector3 GetLookingAtDirection();
	}

	public class MovementComponentBase : InitializableBase, IMovementComponent {
		[SerializeField] protected Dimension dimension = Dimension.TwoD;
		[SerializeField] protected Rigidbody2D rb;
		[SerializeField] protected float defaultMovementSpeed = 2f;
		/// <summary>
		/// this is universal threshold to determine if direction is significant enough to consider.
		/// so no need to square it every time.
		/// </summary>
		public const float MOVEMENT_EPSILON = 0.1f;

		protected MovementIntent _currentIntent;
		public float Mass => this.rb.mass;
		public Vector3 Direction => this._currentIntent.Direction;
		public Vector3 Position => this.rb.position;

		private Vector3 lastDirection = Vector3.right;

		/// <summary>
		/// Gets the current looking direction of the entity based on its movement intent and dimension.
		/// For 2D movement, it projects the last movement direction onto the XY plane.
		/// For 3D movement, it uses the Rigidbody's forward direction as the looking direction.
		/// If we implement strafing or other movement mechanics in the future, we may
		///  need to adjust this logic to account for those cases.
		/// </summary>
		/// <returns></returns>
		public virtual Vector3 GetLookingAtDirection() {
			if (this.dimension == Dimension.TwoD) {
				// we only care about x and y for 2D movement, so we project the
				//  lastDirection onto the XY plane.
				return new Vector3(this.lastDirection.x, this.lastDirection.y, 0f);
			} else {
				// for 3D movement, we can use the Rigidbody's forward direction as the looking direction.
				// we could change this if we are implementing some kind of strafing movement, 
				// but for now we will just assume the looking direction is the same as the movement direction.
				return this.rb.transform.forward;
			}
		}

		/// <summary>
		/// Gets the Rigidbody2D associated with this movement component.
		/// Highly discouraged to use this reference to manipulate movement directly.
		/// Use SetMovementIntent instead to ensure proper movement handling.
		/// </summary>
		public Rigidbody2D Rigidbody => this.rb;
		protected override bool OnInit() {
			this._currentIntent = new MovementIntent(Vector3.zero, MovementIntentType.Stop);
			return true;
		}

		protected virtual void OnDisable() {
			StopMovement();
		}

		/// <summary>
		/// Sets the default movement speed.
		/// This is usually called by the CharacterStatsSystem when the SPD stat changes.
		/// </summary>
		public virtual void SetDefaultMovementSpeed(float speed) {
			this.defaultMovementSpeed = speed;
		}

		/// <summary>
		/// Sets the movement intent for this component.
		/// The intent direction will be normalized if its magnitude is greater than the direction epsilon.
		/// if u wanna do some fancy with direction, just lerp or slerp or whatever before passing it here.
		/// this function will just assign the direction to velocity after normalization. 
		/// it does not do any smoothing or interpolation.
		/// </summary>
		/// <param name="intent"></param>
		public virtual void SetMovementIntent(MovementIntent intent) {
			if (intent.Direction.sqrMagnitude > MOVEMENT_EPSILON) {
				intent.Direction.Normalize();
				// we only update lastDirection when we have a significant movement intent,
				//  to avoid jittery lastDirection when we are trying to stop or have very minor movement.
				this.lastDirection = intent.Direction;
			} else {
				intent.Direction = Vector3.zero;
			}
			this._currentIntent = intent;

		}

		public void StopMovement() {
			this._currentIntent = default;
		}

		/// <summary>
		/// For sake of the example , we are calling ApplyPhysic than using stateMachine state's TickPhysicUpdate to call it
		/// but in real implementation, we would want to give more control to stateMachine states on when to apply movement.
		/// Since this project is about Utility AI and not about state machine implementation,
		/// we will just call ApplyPhysics in FixedUpdate for simplicity.
		/// </summary>
		protected override void OnFixedUpdate() {
			base.OnFixedUpdate();
			ApplyPhysics();
		}


		/// <summary>
		/// Applies physics-based movement based on the current movement intent and a speed multiplier.
		/// The speed multiplier can be used to implement effects like slowing down the entity during attacks or debuffs.
		/// The method blends the desired velocity from the movement intent with the current physics velocity to allow for
		/// responsive control while still respecting collisions and other physics interactions.
		/// Must be called by State themselves in their TickPhysicUpdate to take effect,
		///  giving them control over when movement is applied during the update cycle.
		/// </summary>
		/// <param name="speedMultiplier"></param>
		public virtual void ApplyPhysics(float speedMultiplier = 1f) {
			Vector3 targetVelocity = Vector3.zero;
			if (this._currentIntent.IntentType != MovementIntentType.Stop) {
				targetVelocity = speedMultiplier * this.defaultMovementSpeed * this._currentIntent.Direction;
			}
			this.rb.linearVelocity = targetVelocity;
		}
	}

}