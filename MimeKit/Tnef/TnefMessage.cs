//
// TnefMessage.cs
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

using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace MimeKit.Tnef {
	/// <summary>
	/// A TNEF message.
	/// </summary>
	/// <remarks>
	/// <para>A <see cref="TnefMessage"/> is an in-memory representation of a TNEF stream, as specified by [MS-OXTNEF]:
	/// the message's MAPI properties, its recipients, its bodies and its attachments.</para>
	/// <para>The legacy TNEF attributes (such as <see cref="TnefAttributeTag.Subject"/> and
	/// <see cref="TnefAttributeTag.From"/>) are decoded into the MAPI properties that they correspond to, as specified by
	/// [MS-OXTNEF] section 2.1.3.4. When a TNEF stream contains both a legacy attribute and the corresponding MAPI property,
	/// the MAPI property takes precedence.</para>
	/// <para>The bodies and attachment content are buffered in memory, subject to the limits specified by
	/// <see cref="TnefOptions"/>. A <see cref="TnefMessage"/> should be disposed once it is no longer needed.</para>
	/// <para>Malformed TNEF streams are read on a best-effort basis. The problems that were encountered are reported to the
	/// <see cref="TnefReader.ComplianceLogger"/>, if any.</para>
	/// </remarks>
	public sealed partial class TnefMessage : IDisposable
	{
		readonly List<TnefAttachment> attachments;
		bool disposed;

		internal TnefMessage (int codepage, ushort legacyKey, TnefPropertySet properties, List<TnefRecipient> recipients, List<TnefAttachment> attachments, TnefMessageBody? textBody, TnefMessageBody? htmlBody, TnefMessageBody? rtfBody)
		{
			this.attachments = attachments;
			Attachments = attachments.AsReadOnly ();
			Recipients = recipients.AsReadOnly ();
			Properties = properties;
			LegacyKey = legacyKey;
			Codepage = codepage;
			TextBody = textBody;
			HtmlBody = htmlBody;
			RtfBody = rtfBody;
		}

		/// <summary>
		/// Get the codepage of the message.
		/// </summary>
		/// <remarks>
		/// Gets the codepage that was used to decode the message's 8-bit strings. This is the codepage specified by the
		/// <see cref="TnefAttributeTag.OemCodepage"/> attribute or, if the TNEF stream did not contain one,
		/// <see cref="TnefOptions.DefaultCodepage"/>.
		/// </remarks>
		/// <value>The codepage.</value>
		public int Codepage {
			get;
		}

		/// <summary>
		/// Get the legacy key from the TNEF stream header.
		/// </summary>
		/// <remarks>
		/// Gets the legacy key from the TNEF stream header, which is used to match attachments with the
		/// attachment placeholders in a legacy RTF body.
		/// </remarks>
		/// <value>The legacy key.</value>
		public ushort LegacyKey {
			get;
		}

		/// <summary>
		/// Get the message's MAPI properties.
		/// </summary>
		/// <remarks>
		/// <para>Gets the message's MAPI properties, including the properties that were decoded from the legacy message
		/// attributes.</para>
		/// <para>The message bodies (the <see cref="TnefPropertyId.Body"/>, <see cref="TnefPropertyId.BodyHtml"/> and
		/// <see cref="TnefPropertyId.RtfCompressed"/> properties and the legacy <see cref="TnefAttributeTag.Body"/>
		/// attribute) are not included; use <see cref="TextBody"/>, <see cref="HtmlBody"/> and <see cref="RtfBody"/>
		/// instead.</para>
		/// </remarks>
		/// <value>The properties.</value>
		public TnefPropertySet Properties {
			get;
		}

		/// <summary>
		/// Get the recipients of the message.
		/// </summary>
		/// <remarks>
		/// Gets the recipients of the message, in the order in which they appeared in the TNEF stream.
		/// </remarks>
		/// <value>The recipients.</value>
		public IReadOnlyList<TnefRecipient> Recipients {
			get;
		}

		/// <summary>
		/// Get the attachments of the message.
		/// </summary>
		/// <remarks>
		/// Gets the attachments of the message, in the order in which they appeared in the TNEF stream.
		/// </remarks>
		/// <value>The attachments.</value>
		public IReadOnlyList<TnefAttachment> Attachments {
			get;
		}

		/// <summary>
		/// Get the plain text body of the message.
		/// </summary>
		/// <remarks>
		/// Gets the plain text body of the message, from the <see cref="TnefPropertyId.Body"/> property or, if the
		/// message does not have that property, from the legacy <see cref="TnefAttributeTag.Body"/> attribute.
		/// </remarks>
		/// <value>The plain text body, or <see langword="null"/> if the message does not have one.</value>
		public TnefMessageBody? TextBody {
			get;
		}

		/// <summary>
		/// Get the HTML body of the message.
		/// </summary>
		/// <remarks>
		/// Gets the HTML body of the message, from the <see cref="TnefPropertyId.BodyHtml"/> property.
		/// </remarks>
		/// <value>The HTML body, or <see langword="null"/> if the message does not have one.</value>
		public TnefMessageBody? HtmlBody {
			get;
		}

		/// <summary>
		/// Get the compressed RTF body of the message.
		/// </summary>
		/// <remarks>
		/// Gets the compressed RTF body of the message, from the <see cref="TnefPropertyId.RtfCompressed"/> property.
		/// Use <see cref="TnefMessageBody.OpenDecodedRead"/> to read the decompressed RTF.
		/// </remarks>
		/// <value>The compressed RTF body, or <see langword="null"/> if the message does not have one.</value>
		public TnefMessageBody? RtfBody {
			get;
		}

		/// <summary>
		/// Get the message class.
		/// </summary>
		/// <remarks>
		/// Gets the message class from the <see cref="TnefPropertyId.MessageClass"/> property, such as <c>"IPM.Note"</c>.
		/// </remarks>
		/// <value>The message class, or <see langword="null"/> if it is not known.</value>
		public string? MessageClass {
			get { return Properties.GetString (TnefPropertyTag.MessageClassW); }
		}

		/// <summary>
		/// Get the subject of the message.
		/// </summary>
		/// <remarks>
		/// Gets the subject of the message from the <see cref="TnefPropertyId.Subject"/> property.
		/// </remarks>
		/// <value>The subject, or <see langword="null"/> if the message does not have one.</value>
		public string? Subject {
			get { return Properties.GetString (TnefPropertyTag.SubjectW); }
		}

		/// <summary>
		/// Get the Internet message identifier of the message.
		/// </summary>
		/// <remarks>
		/// Gets the Internet message identifier of the message from the <see cref="TnefPropertyId.InternetMessageId"/>
		/// property, exactly as it was stored in the TNEF stream.
		/// </remarks>
		/// <value>The message identifier, or <see langword="null"/> if the message does not have one.</value>
		public string? InternetMessageId {
			get { return Properties.GetString (TnefPropertyTag.InternetMessageIdW); }
		}

		static TnefReader CreateReader (Stream stream, TnefOptions? options)
		{
			if (stream is null)
				throw new ArgumentNullException (nameof (stream));

			return new TnefReader (stream, options, true);
		}

		static void CheckReader (TnefReader reader)
		{
			if (reader is null)
				throw new ArgumentNullException (nameof (reader));

			if (reader.HasStarted)
				throw new InvalidOperationException ("The reader has already been advanced.");
		}

		/// <summary>
		/// Load a TNEF message from the specified stream.
		/// </summary>
		/// <remarks>
		/// <para>Loads a TNEF message from the specified stream.</para>
		/// <para>The stream is left open.</para>
		/// </remarks>
		/// <returns>The TNEF message.</returns>
		/// <param name="stream">The TNEF stream.</param>
		/// <param name="options">The options to use, or <see langword="null"/> to use <see cref="TnefOptions.Default"/>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="TnefException">
		/// The stream does not begin with the TNEF signature.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static TnefMessage Load (Stream stream, TnefOptions? options = null, CancellationToken cancellationToken = default)
		{
			using (var reader = CreateReader (stream, options))
				return new TnefMessageLoader (reader).Load (cancellationToken);
		}

		/// <summary>
		/// Load a TNEF message using the specified reader.
		/// </summary>
		/// <remarks>
		/// <para>Loads a TNEF message using the specified reader, which allows the caller to configure the reader's
		/// <see cref="TnefReader.ComplianceLogger"/>.</para>
		/// <para>The reader must not have been advanced. It is not disposed.</para>
		/// </remarks>
		/// <returns>The TNEF message.</returns>
		/// <param name="reader">The TNEF reader.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="reader"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="reader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <paramref name="reader"/> has already been advanced.
		/// </exception>
		/// <exception cref="TnefException">
		/// The stream does not begin with the TNEF signature.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static TnefMessage Load (TnefReader reader, CancellationToken cancellationToken = default)
		{
			CheckReader (reader);

			return new TnefMessageLoader (reader).Load (cancellationToken);
		}

		/// <summary>
		/// Release all resources used by the <see cref="TnefMessage"/> object.
		/// </summary>
		/// <remarks>
		/// Releases the buffered bodies and attachment content of the message.
		/// </remarks>
		public void Dispose ()
		{
			if (disposed)
				return;

			disposed = true;

			TextBody?.Dispose ();
			HtmlBody?.Dispose ();
			RtfBody?.Dispose ();

			for (int i = 0; i < attachments.Count; i++)
				attachments[i].Dispose ();
		}
	}
}
