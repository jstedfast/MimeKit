//
// TnefRecipient.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

namespace MimeKit.Tnef {
	/// <summary>
	/// A recipient of a TNEF message.
	/// </summary>
	/// <remarks>
	/// <para>A <see cref="TnefRecipient"/> is a single row of the <see cref="TnefAttributeTag.RecipientTable"/>
	/// attribute of a <see cref="TnefMessage"/>.</para>
	/// <para>The convenience properties are derived from <see cref="Properties"/>, which contains all of the
	/// recipient's MAPI properties.</para>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\TnefExamples.cs" region="ReadRecipients"/>
	/// </example>
	public sealed class TnefRecipient
	{
		// Note: The high-order bits of PidTagRecipientType may be set to indicate that the recipient was resent to
		// ([MS-OXOMSG] section 2.2.3.1).
		const int RecipientTypeMask = 0x0FFFFFFF;

		internal TnefRecipient (TnefPropertySet properties)
		{
			Properties = properties;
		}

		/// <summary>
		/// Get the recipient's MAPI properties.
		/// </summary>
		/// <remarks>
		/// Gets the recipient's MAPI properties.
		/// </remarks>
		/// <value>The properties.</value>
		public TnefPropertySet Properties {
			get;
		}

		/// <summary>
		/// Get the type of the recipient.
		/// </summary>
		/// <remarks>
		/// <para>Gets the type of the recipient from the <see cref="TnefPropertyId.RecipientType"/> property, without
		/// any of the flags that may accompany it.</para>
		/// <para>If the recipient does not have a <see cref="TnefPropertyId.RecipientType"/> property, the recipient
		/// is assumed to be a <see cref="TnefRecipientType.To"/> recipient.</para>
		/// </remarks>
		/// <value>The recipient type.</value>
		public TnefRecipientType RecipientType {
			get {
				var value = Properties.GetInt32 (TnefPropertyTag.RecipientType);

				return value.HasValue ? (TnefRecipientType) (value.Value & RecipientTypeMask) : TnefRecipientType.To;
			}
		}

		/// <summary>
		/// Get the display name of the recipient.
		/// </summary>
		/// <remarks>
		/// Gets the display name of the recipient from the first of the <see cref="TnefPropertyId.RecipientDisplayName"/>,
		/// <see cref="TnefPropertyId.TransmitableDisplayName"/> and <see cref="TnefPropertyId.DisplayName"/> properties
		/// that is present.
		/// </remarks>
		/// <value>The display name, or <see langword="null"/> if it is not known.</value>
		public string? DisplayName {
			get {
				return Properties.GetString (TnefPropertyTag.RecipientDisplayNameW)
					?? Properties.GetString (TnefPropertyTag.TransmitableDisplayNameW)
					?? Properties.GetString (TnefPropertyTag.DisplayNameW);
			}
		}

		/// <summary>
		/// Get the email address of the recipient.
		/// </summary>
		/// <remarks>
		/// Gets the email address of the recipient from the <see cref="TnefPropertyId.SmtpAddress"/> property or, if
		/// that is not present, from the <see cref="TnefPropertyId.EmailAddress"/> property. The address type of the
		/// latter is given by <see cref="AddressType"/>.
		/// </remarks>
		/// <value>The email address, or <see langword="null"/> if it is not known.</value>
		public string? EmailAddress {
			get {
				return Properties.GetString (TnefPropertyTag.SmtpAddressW)
					?? Properties.GetString (TnefPropertyTag.EmailAddressW);
			}
		}

		/// <summary>
		/// Get the address type of the recipient.
		/// </summary>
		/// <remarks>
		/// Gets the address type of the recipient's <see cref="TnefPropertyId.EmailAddress"/> from the
		/// <see cref="TnefPropertyId.Addrtype"/> property, such as <c>"SMTP"</c> or <c>"EX"</c>.
		/// </remarks>
		/// <value>The address type, or <see langword="null"/> if it is not known.</value>
		public string? AddressType {
			get { return Properties.GetString (TnefPropertyTag.AddrtypeW); }
		}
	}
}
