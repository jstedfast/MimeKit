// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MimeKit {
	/// <summary>
	/// The format of the MIME stream.
	/// </summary>
	/// <remarks>
	/// The format of the MIME stream.
	/// </remarks>
	public enum MimeFormat : byte {
		/// <summary>
		/// The stream contains a single MIME entity or message.
		/// </summary>
		Entity,

		/// <summary>
		/// The stream is in the Unix mbox format and may contain
		/// more than a single message.
		/// </summary>
		Mbox,

		/// <summary>
		/// The default stream format.
		/// </summary>
		Default = Entity,
	}
}
