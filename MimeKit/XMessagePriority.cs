// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MimeKit {
	/// <summary>
	/// An enumeration of X-Priority header values.
	/// </summary>
	/// <remarks>
	/// Indicates the priority of a message.
	/// </remarks>
	public enum XMessagePriority {
		/// <summary>
		/// The message is of the highest priority.
		/// </summary>
		Highest = 1,

		/// <summary>
		/// The message is high priority.
		/// </summary>
		High    = 2,

		/// <summary>
		/// The message is of normal priority.
		/// </summary>
		Normal  = 3,

		/// <summary>
		/// The message is of low priority.
		/// </summary>
		Low     = 4,

		/// <summary>
		/// The message is of lowest priority.
		/// </summary>
		Lowest  = 5
	}
}

