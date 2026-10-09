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
	/// <example>
	/// <code language="c#" source="Examples\TnefExamples.cs" region="ReadProperties"/>
	/// </example>
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
		/// <para>Gets the codepage that was used to decode the message's 8-bit strings.</para>
		/// <para>As described in [MS-OXTNEF] section 2.3.3.2, this is the codepage specified by a nonzero
		/// <see cref="TnefAttributeTag.OemCodepage"/> attribute or, if the TNEF stream did not contain one, the
		/// codepage specified by the message's <see cref="TnefPropertyId.InternetCodepage"/> property (unless it
		/// specifies a UTF-16 or UTF-32 codepage) or, failing that, <see cref="TnefOptions.DefaultCodepage"/>.</para>
		/// <para>When the codepage comes from the <see cref="TnefPropertyId.InternetCodepage"/> property, 8-bit
		/// strings that preceded that property in the TNEF stream were decoded using
		/// <see cref="TnefOptions.DefaultCodepage"/>. The 8-bit message bodies always use this codepage.</para>
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
		/// <example>
		/// <code language="c#" source="Examples\TnefExamples.cs" region="ReadProperties"/>
		/// </example>
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
		/// <example>
		/// <code language="c#" source="Examples\TnefExamples.cs" region="ReadRecipients"/>
		/// </example>
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
		/// <example>
		/// <code language="c#" source="Examples\TnefExamples.cs" region="ExtractAttachments"/>
		/// </example>
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
		/// <example>
		/// <code language="c#" source="Examples\TnefExamples.cs" region="ReadBodies"/>
		/// </example>
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
		/// <example>
		/// <code language="c#" source="Examples\TnefExamples.cs" region="ReadBodies"/>
		/// </example>
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
		/// <example>
		/// <code language="c#" source="Examples\TnefExamples.cs" region="ReadBodies"/>
		/// </example>
		/// <value>The compressed RTF body, or <see langword="null"/> if the message does not have one.</value>
		public TnefMessageBody? RtfBody {
			get;
		}

		/// <summary>
		/// Get the message class.
		/// </summary>
		/// <remarks>
		/// <para>Gets the message class from the <see cref="TnefPropertyId.MessageClass"/> property, such as <c>"IPM.Note"</c>.</para>
		/// <para>Leading and trailing whitespace is removed from the value.</para>
		/// </remarks>
		/// <value>The message class, or <see langword="null"/> if it is not known.</value>
		public string? MessageClass {
			get {
				var messageClass = Properties.GetString (TnefPropertyTag.MessageClassW)?.Trim ();

				return string.IsNullOrEmpty (messageClass) ? null : messageClass;
			}
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
		/// <example>
		/// <code language="c#" source="Examples\TnefExamples.cs" region="ReadProperties"/>
		/// </example>
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
		/// <example>
		/// <code language="c#" source="Examples\TnefExamples.cs" region="ComplianceLogger"/>
		/// </example>
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
		/// Convert the message to MIME.
		/// </summary>
		/// <remarks>
		/// <para>Converts the TNEF message to a <see cref="MimeMessage"/>.</para>
		/// <para>If the message has a <see cref="TnefPropertyId.MimeSkeleton"/> property, the MIME structure and headers
		/// are taken from the skeleton and its empty parts are filled in with the message's bodies and attachments. If the
		/// skeleton cannot be parsed or does not match the message's bodies and attachments, it is discarded and a
		/// <see cref="TnefConversionLossKind.InvalidMimeSkeleton"/> loss is reported.</para>
		/// <para>Otherwise, the MIME message is built from the message's properties:</para>
		/// <list type="bullet">
		/// <item>The Received headers are taken from the <see cref="TnefPropertyId.TransportMessageHeaders"/>, followed
		/// by the headers that correspond to the message's properties (such as From, Date, Subject, Message-Id, To and
		/// Cc), and then the headers stored in the <see cref="TnefPropertySetGuid.InternetHeaders"/> named properties.
		/// No header is generated that does not correspond to a property.</item>
		/// <item>The plain text and HTML bodies become a <c>multipart/alternative</c>, in that order, if there is
		/// more than one. When the best body ([MS-OXBBODY]) is the compressed RTF body, both are generated from the
		/// RTF using <see cref="MimeKit.Text.RtfToText"/> and <see cref="MimeKit.Text.RtfToHtml"/>, as recommended
		/// by [MS-OXCMAIL] 2.1.3.3.5. The RTF itself is never added as a <c>text/rtf</c> part.</item>
		/// <item>If the message is a calendar item or a meeting message and
		/// <see cref="TnefConversionOptions.GenerateCalendar"/> is enabled, an iCalendar <c>text/calendar</c> part is
		/// generated from its properties and added as the last alternative of the body. The embedded messages that
		/// hold the exceptions to a recurring appointment become part of the calendar rather than attachments.</item>
		/// <item>The attachments follow the body within a <c>multipart/mixed</c>.</item>
		/// </list>
		/// <para>The information that could not be represented in MIME is listed in
		/// <see cref="TnefConversionResult.Losses"/>.</para>
		/// <para>The converted message is independent of the <see cref="TnefMessage"/>, which may be disposed
		/// afterwards.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\TnefExamples.cs" region="ConvertToMime"/>
		/// </example>
		/// <returns>The result of the conversion.</returns>
		/// <param name="options">The conversion options, or <see langword="null"/> to use
		/// <see cref="TnefConversionOptions.Default"/>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefMessage"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public TnefConversionResult ConvertToMime (TnefConversionOptions? options = null, CancellationToken cancellationToken = default)
		{
			if (disposed)
				throw new ObjectDisposedException (nameof (TnefMessage));

			return TnefMessageConverter.Convert (this, options ?? TnefConversionOptions.Default, cancellationToken);
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
