using UnityEngine;

namespace Kope.Component {
	public struct FieldOfViewData {
		private readonly float fieldOfViewAngle;
		private readonly float viewDistance;
		public readonly float FieldOfViewAngle => this.fieldOfViewAngle;
		public readonly float ViewDistance => this.viewDistance;
		public readonly float CosineOfAngleThreshold;
		public readonly float SquareCosineOfAngleThreshold;
		public readonly float SquareViewDistance;

		public FieldOfViewData(float fieldOfViewAngle = 90f, float viewDistance = 10f) {
			this.fieldOfViewAngle = fieldOfViewAngle;
			this.viewDistance = viewDistance;

			this.SquareViewDistance = this.viewDistance * this.viewDistance;
			this.CosineOfAngleThreshold = Mathf.Cos(fieldOfViewAngle * 0.5f * Mathf.Deg2Rad);
			this.SquareCosineOfAngleThreshold = CosineOfAngleThreshold * CosineOfAngleThreshold;
		}
	}
}
