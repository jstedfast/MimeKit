// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MimeKit.Text {
	/// <summary>
	/// An enumeration of text encoding confidences.
	/// </summary>
	/// <remarks>
	/// An enumeration of text encoding confidences.
	/// </remarks>
	public enum TextEncodingConfidence
	{
		/// <summary>
		/// The text encoding confidence is undefined.
		/// </summary>
		Undefined,

		/// <summary>
		/// The text encoding confidence is tentative.
		/// </summary>
		Tentative,

		/// <summary>
		/// The text encoding confidence is certain.
		/// </summary>
		Certain,

		/// <summary>
		/// The text encoding confidence is irrelevant.
		/// </summary>
		Irrelevant
	}
}
