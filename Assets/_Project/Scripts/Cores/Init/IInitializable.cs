// Copyright (c) 2026 Keshav Prasad Neupane (Kope)
// Licensed under the MIT License. See LICENSE in the repository root for details.

namespace Kope.Core.Init {
	public interface IInitializable {

		bool IsInitialized { get; }

		/// <summary>
		/// Called once after dependencies are injected.
		/// </summary>
		void Init();

		/// <summary>
		/// Called when the object is being destroyed, for cleanup.
		/// </summary>
		void Shutdown();
	}
}