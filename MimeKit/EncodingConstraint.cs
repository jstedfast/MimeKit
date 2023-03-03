// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MimeKit {
	/// <summary>
	/// A content encoding constraint.
	/// </summary>
	/// <remarks>
	/// Not all message transports support binary or 8-bit data, so it becomes
	/// necessary to constrain the content encoding to a subset of the possible
	/// Content-Transfer-Encoding values.
	/// </remarks>
	public enum EncodingConstraint {
		/// <summary>
		/// There are no encoding constraints, the content may contain any byte.
		/// </summary>
		None,

		/// <summary>
		/// The content may contain bytes with the high bit set, but must not contain any zero-bytes.
		/// </summary>
		EightBit,

		/// <summary>
		/// The content may only contain bytes within the 7-bit ASCII range.
		/// </summary>
		SevenBit,
	}
}
