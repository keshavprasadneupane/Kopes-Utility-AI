// Copyright (c) 2026 Keshav Prasad Neupane (Kope)
// Licensed under the MIT License. See LICENSE in the repository root for details.

using System;

namespace Kope.Core.Extensions {
	public static class FloatExtension {
		/// <summary>
		/// Applies a compensation to the utility score based on the number of considerations.
		/// This helps to prevent very low scores when multiple considerations are multiplied together.
		/// Uses the Algorithm Written byt Mr.Dave Mark in his book 
		/// Behavioral Mathematics for Game AI (Applied Mathematics).<br/>
		/// Highly Recommended! https://www.amazon.com/Behavioral-Mathematics-Game-AI-Applied/dp/1584506849
		/// To read more about the technique.
		/// Formula :
		/// finalScore = original + ((1 - original) * (1 - (1 / size))) * original
		/// </summary>
		/// <param name="value"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static float GetCompensatedUtility(this float value, int size) {
			// Just return the original value if it's less than or equal to 0,
			// or if there is only one consideration (size <= 1).
			if (value <= 0f || size <= 1) return value;

			float orginal = value;
			float modFactor = 1f - (1f / size);
			float makeup = (1f - orginal) * modFactor;
			float finalScore = orginal + (makeup * orginal);
			// just return the final score, it can be >1, since we want to have
			//  more dynamic range for better differentiation between actions.
			// and not allowing x<0, since utility should not be negative.
			return Math.Max(0.0f, finalScore);
		}

	}
}