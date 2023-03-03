// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace MimeKit {
	/// <summary>
	/// Header list changed action.
	/// </summary>
    /// <remarks>
    /// Specifies the way that a <see cref="HeaderList"/> was changed.
    /// </remarks>
	public enum HeaderListChangedAction {
		/// <summary>
		/// A header was added.
		/// </summary>
		Added,

		/// <summary>
		/// A header was changed.
		/// </summary>
		Changed,

		/// <summary>
		/// A header was removed.
		/// </summary>
		Removed,

		/// <summary>
		/// The header list was cleared.
		/// </summary>
		Cleared
	}

	class HeaderListChangedEventArgs : EventArgs
	{
		internal HeaderListChangedEventArgs (Header header, HeaderListChangedAction action)
		{
			Header = header;
			Action = action;
		}

		public HeaderListChangedAction Action {
			get; private set;
		}

		public Header Header {
			get; private set;
		}
	}
}
