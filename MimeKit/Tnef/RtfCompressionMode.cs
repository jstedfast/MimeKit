// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MimeKit.Tnef {
	/// <summary>
	/// A RTF compression mode.
	/// </summary>
	/// <remarks>
	/// A RTF compression mode.
	/// </remarks>
	public enum RtfCompressionMode {
		/// <summary>
		/// The compression mode is not known.
		/// </summary>
		Unknown      = 0,

		/// <summary>
		/// The RTF stream is not compressed.
		/// </summary>
		Uncompressed = 0x414C454D,

		/// <summary>
		/// The RTF stream is compressed.
		/// </summary>
		Compressed   = 0x75465A4C
	}
}
