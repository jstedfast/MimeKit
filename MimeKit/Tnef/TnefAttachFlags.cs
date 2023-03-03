// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace MimeKit.Tnef {
	/// <summary>
	/// The TNEF attach flags.
	/// </summary>
	/// <remarks>
	/// The <see cref="TnefAttachFlags"/> enum contains a list of possible values for
	/// the <see cref="TnefPropertyId.AttachFlags"/> property.
	/// </remarks>
	[Flags]
	public enum TnefAttachFlags {
		/// <summary>
		/// No AttachFlags set.
		/// </summary>
		None            = 0,

		/// <summary>
		/// The attachment is invisible in HTML bodies.
		/// </summary>
		InvisibleInHtml = 1,

		/// <summary>
		/// The attachment is invisible in RTF bodies.
		/// </summary>
		InvisibleInRtf  = 2,

		/// <summary>
		/// The attachment is referenced (and rendered) by the HTML body.
		/// </summary>
		RenderedInBody  = 4
	}
}
