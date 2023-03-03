// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MimeKit.Tnef {
	/// <summary>
	/// A TNEF attribute level.
	/// </summary>
	/// <remarks>
	/// A TNEF attribute level.
	/// </remarks>
	public enum TnefAttributeLevel {
		/// <summary>
		/// The attribute is a message-level attribute.
		/// </summary>
		Message    = 1,

		/// <summary>
		/// The attribute is an attachment-level attribute.
		/// </summary>
		Attachment = 2,
	}
}
