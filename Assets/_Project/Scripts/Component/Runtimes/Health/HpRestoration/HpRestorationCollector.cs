
// Copyright (c) 2026 Keshav Prasad Neupane (Kope)
// Licensed under the MIT License. See LICENSE in the repository root for details.
using Kope.Component.Health;
using Kope.Component.Health.Temp;
using Kope.Core.Sensor;
using UnityEngine;

namespace Kope.Component {
	public class HpRestorationCollector : SensorBase {
		[SerializeField] private HealthComponentBase healthComponent;

		public override void OnStart() {
			base.OnStart();
			if (this.healthComponent == null) {
				Debug.LogWarning($"[HpRestorationCollector] No IHealthComponent assigned on {gameObject.name}.");
			}
		}

		public override void OnDetect(Collider2D other) {
			if (this.healthComponent == null) return;
			if (!other.TryGetComponent<EntityManager>(out var mgr)) {
				Debug.LogWarning($"[HpRestorationCollector] Detected collider {other.name} does not have an EntityManager component. Cannot restore HP." + this._parentGOHiearchPathMessage, other.gameObject);
				return;
			}

			if (!mgr.EntityDetail.ComponentRegistry.TryGetReadOnlyComponent(out HpRestoration healthComp)) {
				Debug.LogWarning($"[HpRestorationCollector] Detected collider {other.name} does not have an HpRestoration component. Cannot restore HP." + this._parentGOHiearchPathMessage, other.gameObject);
				return;
			}
			float currentHp = this.healthComponent.CurrentHealth;
			float maxHp = this.healthComponent.MaxHealth;
			if (currentHp < maxHp) {
				float amountToRestore = healthComp.RestoreAmount;
				if (healthComp.IsPercentage) {
					amountToRestore *= maxHp;
				}
				this.healthComponent.Heal(amountToRestore);
				mgr.NotifyEntityDiedOrPooled();
				Destroy(other.gameObject);
				return;
			}
			Debug.Log($"[HpRestorationCollector] Detected collider {other.name} has full health. No need to restore HP." + this._parentGOHiearchPathMessage, other.gameObject);
		}
	}
}
