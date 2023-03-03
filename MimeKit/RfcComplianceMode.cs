// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MimeKit {
	/// <summary>
	/// An RFC compliance mode.
	/// </summary>
	/// <remarks>
	/// An RFC compliance mode.
	/// </remarks>
	public enum RfcComplianceMode {
		/// <summary>
		/// Attempt to be even more liberal in accepting broken and/or invalid formatting.
		/// </summary>
		Looser = -1,

		/// <summary>
		/// Attempt to be more liberal accepting broken and/or invalid formatting.
		/// </summary>
		Loose,

		/// <summary>
		/// Do not attempt to be overly liberal in accepting broken and/or invalid formatting.
		/// </summary>
		Strict
	}
}
