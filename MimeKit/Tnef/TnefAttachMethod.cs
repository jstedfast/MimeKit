// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MimeKit.Tnef {
	/// <summary>
	/// The TNEF attach method.
	/// </summary>
	/// <remarks>
	/// The <see cref="TnefAttachMethod"/> enum contains a list of possible values for
	/// the <see cref="TnefPropertyId.AttachMethod"/> property.
	/// </remarks>
	public enum TnefAttachMethod {
		/// <summary>
		/// No AttachMethod specified.
		/// </summary>
		None            = 0,

		/// <summary>
		/// The attachment is a binary blob and SHOULD appear in the
		/// <see cref="TnefAttributeTag.AttachData"/> attribute.
		/// </summary>
		ByValue         = 1,

		/// <summary>
		/// The attachment is an embedded TNEF message stream and MUST appear
		/// in the <see cref="TnefPropertyId.AttachData"/> property of the
		/// <see cref="TnefAttributeTag.Attachment"/> attribute.
		/// </summary>
		EmbeddedMessage = 5,

		/// <summary>
		/// The attachment is an OLE stream and MUST appear
		/// in the <see cref="TnefPropertyId.AttachData"/> property of the
		/// <see cref="TnefAttributeTag.Attachment"/> attribute.
		/// </summary>
		Ole             = 6
	}
}
