// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MimeKit.Tnef {
	/// <summary>
	/// A TNEF compliance mode.
	/// </summary>
	/// <remarks>
	/// A TNEF compliance mode.
	/// </remarks>
	public enum TnefComplianceMode {
		/// <summary>
		/// Use a loose compliance mode, attempting to ignore invalid or corrupt data.
		/// </summary>
		Loose,

		/// <summary>
		/// Use a very strict compliance mode, aborting the parser at the first sign of
		/// invalid or corrupted data.
		/// </summary>
		Strict
	}
}
