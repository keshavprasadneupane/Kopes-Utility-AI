// Copyright (c) 2026 Keshav Prasad Neupane (Kope)
// Licensed under the MIT License. See LICENSE in the repository root for details.

using UnityEngine;
using Kope.AI.Utility;
[CreateAssetMenu(fileName = "ConstantConsideration", menuName = "Scriptable Objects/AI/Utility/Considerations/ConstantConsideration")]
public class ConstantConsideration : ConsiderationSO {
	[SerializeField] private string considerationName;
	[SerializeField, Min(0f)] private float constantValue = 1f;

	public override string ConsiderationName => this.considerationName;


	/// <summary>
	/// Evaluates the consideration and returns a constant score.
	/// Also returns the incremented total multiplication count.
	/// used for compensated utility calculation.
	/// 
	/// </summary>
	/// <returns></returns>
	public override (float, int) Evaluate(IReadOnlyContext context) {
		return (this.constantValue, 0); // no mult happened
	}


}
