// Copyright (c) 2026 Keshav Prasad Neupane (Kope)
// Licensed under the MIT License. See LICENSE in the repository root for details.

using System;
using Kope.Core.Attribute;
using Kope.Core.Init;
using UnityEngine;


namespace Kope.Component.Health {

	public interface IHealthComponent {
		float CurrentHealth { get; }
		float MaxHealth { get; }

		event Action<float> OnMaxHealthChanged;
		event Action<float> OnCurrentHealthChanged;
		void Heal(float amount);
		void TakeHit(float amount);
	}

	public class HealthComponentBase : InitializableBase, IHealthComponent {
		[SerializeField, Tooltip("The maximum health value for this entity.")]
		protected float maxHealth = 100;

		[SerializeField, ReadOnly, Tooltip("The current health value for this entity." +
		"It is updated based on healing and damage taken, and should not exceed maxHealth.")]
		protected float currentHealth = 100;

		[SerializeField, Tooltip("The defense value for this entity, which reduces incoming damage.")]
		protected float defence;

		public float CurrentHealth => this.currentHealth;
		public float MaxHealth => this.maxHealth;

		public event Action<float> OnMaxHealthChanged;

		// In your code Implementation, you can invoke this event whenever the maxHealth value changes to 
		// notify any listeners about the update. but for this example this is unused.
		public event Action<float> OnCurrentHealthChanged;



		protected override bool OnInit() {
			this.currentHealth = this.maxHealth;
			//	Debug.Log($"HealthComponent initialized with MaxHealth: {this.maxHealth} and CurrentHealth: {this.currentHealth}");
			return true;
		}


		public void Heal(float amount) {
			this.currentHealth = Mathf.Clamp(this.currentHealth + amount, 0, this.maxHealth);
			this.OnCurrentHealthChanged?.Invoke(this.currentHealth);
		}

		/// <summary>
		/// Used for dubugging purposes, directly reduces HP by a specified amount, ignoring all calculations.
		/// </summary>
		/// <param name="amount"></param>
		public void ReduceHp(float amount, float minHealthAmount = 0.2f) {
			this.currentHealth = Mathf.Clamp(
				this.currentHealth - amount,
				this.maxHealth * minHealthAmount,
				this.maxHealth
			);
			this.OnCurrentHealthChanged?.Invoke(this.currentHealth);
		}

		public void TakeHit(float amount) {
			float effectiveDamage = Mathf.Max(amount - this.defence, 0);
			this.currentHealth = Mathf.Clamp(this.currentHealth - effectiveDamage, 0, this.maxHealth);

			this.OnCurrentHealthChanged?.Invoke(this.currentHealth);
		}
	}
}