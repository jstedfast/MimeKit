//
// ParserOptions.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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
using System.Text;
using System.Reflection;
using System.Collections.Generic;

#if ENABLE_CRYPTO
using MimeKit.Cryptography;
#endif

using MimeKit.Tnef;
using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// Parser options as used by <see cref="MimeParser"/> as well as various Parse and TryParse methods in MimeKit.
	/// </summary>
	/// <remarks>
	/// <see cref="ParserOptions"/> allows you to change and/or override default parsing options used by methods such
	/// as <see cref="MimeMessage.Load(ParserOptions,System.IO.Stream,System.Threading.CancellationToken)"/> and others.
	/// </remarks>
	public class ParserOptions
	{
		readonly Dictionary<string, ConstructorInfo> mimeTypes = new Dictionary<string, ConstructorInfo> (MimeUtils.OrdinalIgnoreCase);
		static readonly Type[] ConstructorArgTypes = { typeof (MimeEntityConstructorArgs) };

		/// <summary>
		/// The default parser options.
		/// </summary>
		/// <remarks>
		/// If a <see cref="ParserOptions"/> is not supplied to <see cref="MimeParser"/> or other Parse and TryParse
		/// methods throughout MimeKit, <see cref="ParserOptions.Default"/> will be used.
		/// </remarks>
		public static readonly ParserOptions Default = new ParserOptions ();

		/// <summary>
		/// Get or set the compliance mode that should be used when parsing rfc822 addresses.
		/// </summary>
		/// <remarks>
		/// <para>In general, you'll probably want this value to be <see cref="RfcComplianceMode.Loose"/>
		/// (the default) as it allows maximum interoperability with existing (broken) mail clients
		/// and other mail software such as sloppily written perl scripts (aka spambots).</para>
		/// <note type="tip">Even in <see cref="RfcComplianceMode.Strict"/> mode, the address parser
		/// is fairly liberal in what it accepts. Setting it to <see cref="RfcComplianceMode.Loose"/>
		/// just makes it try harder to deal with garbage input.</note>
		/// </remarks>
		/// <value>The RFC compliance mode.</value>
		public RfcComplianceMode AddressParserComplianceMode { get; set; }

		/// <summary>
		/// Get or set whether the rfc822 address parser should ignore unquoted commas in address names.
		/// </summary>
		/// <remarks>
		/// <para>In general, you'll probably want this value to be <c>true</c> (the default) as it allows
		/// maximum interoperability with existing (broken) mail clients and other mail software such as
		/// sloppily written perl scripts (aka spambots) that do not properly quote the name when it
		/// contains a comma.</para>
		/// </remarks>
		/// <value><c>true</c> if the address parser should ignore unquoted commas in address names; otherwise, <c>false</c>.</value>
		public bool AllowUnquotedCommasInAddresses { get; set; }

		/// <summary>
		/// Get or set whether the rfc822 address parser should allow addresses without a domain.
		/// </summary>
		/// <remarks>
		/// <para>In general, you'll probably want this value to be <c>true</c> (the default) as it allows
		/// maximum interoperability with older email messages that may contain local UNIX addresses.</para>
		/// <para>This option exists in order to allow parsing of mailbox addresses that do not have an
		/// @domain component. These types of addresses are rare and were typically only used when sending
		/// mail to other users on the same UNIX system.</para>
		/// </remarks>
		/// <value><c>true</c> if the address parser should allow mailbox addresses without a domain; otherwise, <c>false</c>.</value>
		public bool AllowAddressesWithoutDomain { get; set; }

		/// <summary>
		/// Get or set the maximum address group depth the parser should accept.
		/// </summary>
		/// <remarks>
		/// <para>This option exists in order to define the maximum recursive depth of an rfc822 group address
		/// that the parser should accept before bailing out with the assumption that the address is maliciously
		/// formed. If the value is set too large, then it is possible that a maliciously formed set of
		/// recursive group addresses could cause a stack overflow.</para>
		/// </remarks>
		/// <value>The maximum address group depth.</value>
		public int MaxAddressGroupDepth { get; set; }

		/// <summary>
		/// Get or set the maximum MIME nesting depth the parser should accept.
		/// </summary>
		/// <remarks>
		/// <para>This option exists in order to define the maximum recursive depth of MIME parts that the parser
		/// should accept before treating further nesting as a leaf-node MIME part and not recursing any further.
		/// If the value is set too large, then it is possible that a maliciously formed set of deeply nested
		/// multipart MIME parts could cause a stack overflow.</para>
		/// </remarks>
		/// <value>The maximum MIME nesting depth.</value>
		public int MaxMimeDepth { get; set; }

		/// <summary>
		/// Get or set the compliance mode that should be used when parsing Content-Type and Content-Disposition parameters.
		/// </summary>
		/// <remarks>
		/// <para>In general, you'll probably want this value to be <see cref="RfcComplianceMode.Loose"/>
		/// (the default) as it allows maximum interoperability with existing (broken) mail clients
		/// and other mail software such as sloppily written perl scripts (aka spambots).</para>
		/// <note type="tip">Even in <see cref="RfcComplianceMode.Strict"/> mode, the parameter parser
		/// is fairly liberal in what it accepts. Setting it to <see cref="RfcComplianceMode.Loose"/>
		/// just makes it try harder to deal with garbage input.</note>
		/// </remarks>
		/// <value>The RFC compliance mode.</value>
		public RfcComplianceMode ParameterComplianceMode { get; set; }

		/// <summary>
		/// Get or set the compliance mode that should be used when decoding rfc2047 encoded words.
		/// </summary>
		/// <remarks>
		/// In general, you'll probably want this value to be <see cref="RfcComplianceMode.Loose"/>
		/// (the default) as it allows maximum interoperability with existing (broken) mail clients
		/// and other mail software such as sloppily written perl scripts (aka spambots).
		/// </remarks>
		/// <value>The RFC compliance mode.</value>
		public RfcComplianceMode Rfc2047ComplianceMode { get; set; }

		/// <summary>
		/// Get or set a value indicating whether the Content-Length value should be
		/// respected when parsing mbox streams.
		/// </summary>
		/// <remarks>
		/// For more details about why this may be useful, you can find more information
		/// at <a href="http://www.jwz.org/doc/content-length.html">
		/// http://www.jwz.org/doc/content-length.html</a>.
		/// </remarks>
		/// <value><c>true</c> if the Content-Length value should be respected;
		/// otherwise, <c>false</c>.</value>
		public bool RespectContentLength { get; set; }

		/// <summary>
		/// Get or set the charset encoding to use as a fallback for 8bit headers.
		/// </summary>
		/// <remarks>
		/// <see cref="Rfc2047.DecodeText(ParserOptions, byte[])"/> and
		/// <see cref="Rfc2047.DecodePhrase(ParserOptions, byte[])"/>
		/// use this charset encoding as a fallback when decoding 8bit text into unicode. The first
		/// charset encoding attempted is UTF-8, followed by this charset encoding, before finally
		/// falling back to iso-8859-1.
		/// </remarks>
		/// <value>The charset encoding.</value>
		public Encoding CharsetEncoding { get; set; }

		/// <summary>
		/// Initialize a new instance of the <see cref="ParserOptions"/> class.
		/// </summary>
		/// <remarks>
		/// By default, new instances of <see cref="ParserOptions"/> enable rfc2047 work-arounds
		/// (which are needed for maximum interoperability with mail software used in the wild)
		/// and do not respect the Content-Length header value.
		/// </remarks>
		public ParserOptions ()
		{
			AddressParserComplianceMode = RfcComplianceMode.Loose;
			ParameterComplianceMode = RfcComplianceMode.Loose;
			Rfc2047ComplianceMode = RfcComplianceMode.Loose;
			CharsetEncoding = CharsetUtils.UTF8;
			AllowUnquotedCommasInAddresses = true;
			AllowAddressesWithoutDomain = true;
			RespectContentLength = false;
			MaxAddressGroupDepth = 3;
			MaxMimeDepth = 1024;
		}

		/// <summary>
		/// Clone an instance of <see cref="ParserOptions"/>.
		/// </summary>
		/// <remarks>
		/// Clones a set of options, allowing you to change a specific option
		/// without requiring you to change the original.
		/// </remarks>
		/// <returns>An identical copy of the current instance.</returns>
		public ParserOptions Clone ()
		{
			var options = new ParserOptions {
				AddressParserComplianceMode = AddressParserComplianceMode,
				AllowUnquotedCommasInAddresses = AllowUnquotedCommasInAddresses,
				AllowAddressesWithoutDomain = AllowAddressesWithoutDomain,
				ParameterComplianceMode = ParameterComplianceMode,
				Rfc2047ComplianceMode = Rfc2047ComplianceMode,
				MaxAddressGroupDepth = MaxAddressGroupDepth,
				RespectContentLength = RespectContentLength,
				CharsetEncoding = CharsetEncoding,
				MaxMimeDepth = MaxMimeDepth
			};

			foreach (var mimeType in mimeTypes)
				options.mimeTypes.Add (mimeType.Key, mimeType.Value);

			return options;
		}

		/// <summary>
		/// Register a <see cref="MimeEntity"/> subclass for the specified mime-type.
		/// </summary>
		/// <param name="mimeType">The MIME type.</param>
		/// <param name="type">A custom subclass of <see cref="MimeEntity"/>.</param>
		/// <remarks>
		/// Your custom <see cref="MimeEntity"/> class should not subclass
		/// <see cref="MimeEntity"/> directly, but rather it should subclass
		/// <see cref="Multipart"/>, <see cref="MimePart"/>,
		/// <see cref="MessagePart"/>, or one of their derivatives.
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="mimeType"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="type"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="type"/> is not a subclass of <see cref="Multipart"/>,
		/// <see cref="MimePart"/>, or <see cref="MessagePart"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="type"/> does not have a constructor that takes
		/// only a <see cref="MimeEntityConstructorArgs"/> argument.</para>
		/// </exception>
		public void RegisterMimeType (string mimeType, Type type)
		{
			if (mimeType is null)
				throw new ArgumentNullException (nameof (mimeType));

			if (type is null)
				throw new ArgumentNullException (nameof (type));

			if (!type.IsSubclassOf (typeof (MessagePart)) &&
				!type.IsSubclassOf (typeof (Multipart)) &&
				!type.IsSubclassOf (typeof (MimePart)))
				throw new ArgumentException ("The specified type must be a subclass of MessagePart, Multipart, or MimePart.", nameof (type));

			var ctor = type.GetConstructor (ConstructorArgTypes);

			if (ctor is null)
				throw new ArgumentException ("The specified type must have a constructor that takes a MimeEntityConstructorArgs argument.", nameof (type));

			mimeType = mimeType.ToLowerInvariant ();

			mimeTypes[mimeType] = ctor;
		}

		internal static bool IsEncoded (ContentEncoding encoding)
		{
			switch (encoding) {
			case ContentEncoding.SevenBit:
			case ContentEncoding.EightBit:
			case ContentEncoding.Binary:
				return false;
			default:
				return true;
			}
		}

		static bool IsEncoded (IList<Header> headers)
		{
			for (int i = 0; i < headers.Count; i++) {
				if (headers[i].Id != HeaderId.ContentTransferEncoding)
					continue;

				MimeUtils.TryParse (headers[i].Value, out ContentEncoding encoding);

				return IsEncoded (encoding);
			}

			return false;
		}

		static bool EqualsAny (string value, params string[] values)
		{
			for (int i = 0; i < values.Length; i++) {
				if (value.Equals (values[i], StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		internal MimeEntity CreateEntity (ContentType contentType, IList<Header> headers, bool toplevel, int depth)
		{
			var args = new MimeEntityConstructorArgs (this, contentType, headers, toplevel);
			var subtype = contentType.MediaSubtype;
			var type = contentType.MediaType;

			if (mimeTypes.Count > 0) {
				var mimeType = $"{type}/{subtype}";

				if (mimeTypes.TryGetValue (mimeType, out var ctor)) {
					// Instantiate the custom type if-and-only-if the current parser depth is < MaxMimeDepth -or- the custom type is a MimePart subclass.
					if (depth < MaxMimeDepth || typeof (MimePart).IsAssignableFrom (ctor.DeclaringType))
						return (MimeEntity) ctor.Invoke (new object[] { args });
				}
			}

			if (type.Equals ("text", StringComparison.OrdinalIgnoreCase)) {
				// text/rfc822-headers
				if (depth < MaxMimeDepth && subtype.Equals ("rfc822-headers", StringComparison.OrdinalIgnoreCase) && !IsEncoded (headers))
					return new TextRfc822Headers (args);

				return new TextPart (args);
			} else if (type.Equals ("multipart", StringComparison.OrdinalIgnoreCase)) {
				if (depth >= MaxMimeDepth) {
					// We don't want to recurse any further, so treat this as a leaf node.
					return new MimePart (args);
				}

				// multipart/alternative
				if (subtype.Equals ("alternative", StringComparison.OrdinalIgnoreCase))
					return new MultipartAlternative (args);

				// multipart/related
				if (subtype.Equals ("related", StringComparison.OrdinalIgnoreCase))
					return new MultipartRelated (args);

				// multipart/report
				if (subtype.Equals ("report", StringComparison.OrdinalIgnoreCase))
					return new MultipartReport (args);

#if ENABLE_CRYPTO
				// multipart/encrypted
				if (subtype.Equals ("encrypted", StringComparison.OrdinalIgnoreCase))
					return new MultipartEncrypted (args);

				// multipart/signed
				if (subtype.Equals ("signed", StringComparison.OrdinalIgnoreCase))
					return new MultipartSigned (args);
#endif

				// multipart/mixed, multipart/parallel, etc.
				return new Multipart (args);
			} else if (type.Equals ("message", StringComparison.OrdinalIgnoreCase)) {
				// Note: message/rfc822 and message/partial are not allowed to be encoded according to rfc2046
				// (sections 5.2.1 and 5.2.2, respectively). Since some broken clients will encode them anyway,
				// it is necessary for us to treat those as opaque blobs instead, and thus the parser should
				// parse them as normal MimeParts instead of MessageParts.
				//
				// Technically message/disposition-notification is only allowed to have use the 7bit encoding
				// as well, but since MessageDispositionNotification is a MImePart subclass rather than a
				// MessagePart subclass, it means that the content won't be parsed until later and so we can
				// actually handle that w/o any problems.

				// message/disposition-notification
				if (EqualsAny (subtype, "disposition-notification", "global-disposition-notification"))
					return new MessageDispositionNotification (args);

				// message/delivery-status
				if (EqualsAny (subtype, "delivery-status", "global-delivery-status"))
					return new MessageDeliveryStatus (args);

				if (subtype.Equals ("feedback-report", StringComparison.OrdinalIgnoreCase))
					return new MessageFeedbackReport (args);

				// message/rfc822
				if (EqualsAny (subtype, "rfc822", "global", "news", "external-body", "rfc2822")) {
					if (depth < MaxMimeDepth && !IsEncoded (headers))
						return new MessagePart (args);
				} else if (subtype.Equals ("partial", StringComparison.OrdinalIgnoreCase)) {
					if (!IsEncoded (headers))
						return new MessagePartial (args);
				} else if (subtype.Equals ("global-headers", StringComparison.OrdinalIgnoreCase)) {
					if (depth < MaxMimeDepth && !IsEncoded (headers))
						return new TextRfc822Headers (args);
				}
			} else if (type.Equals ("application", StringComparison.OrdinalIgnoreCase)) {
#if ENABLE_CRYPTO
				// application/pkcs7-mime
				if (subtype.Equals ("pkcs7-mime", StringComparison.OrdinalIgnoreCase) ||
					subtype.Equals ("x-pkcs7-mime", StringComparison.OrdinalIgnoreCase))
					return new ApplicationPkcs7Mime (args);

				// application/pkcs7-signature
				if (subtype.Equals ("pkcs7-signature", StringComparison.OrdinalIgnoreCase) ||
					subtype.Equals ("x-pkcs7-signature", StringComparison.OrdinalIgnoreCase))
					return new ApplicationPkcs7Signature (args);

				// application/pgp-encrypted
				if (subtype.Equals ("pgp-encrypted", StringComparison.OrdinalIgnoreCase) ||
					subtype.Equals ("x-pgp-encrypted", StringComparison.OrdinalIgnoreCase))
					return new ApplicationPgpEncrypted (args);

				// application/pgp-signature
				if (subtype.Equals ("pgp-signature", StringComparison.OrdinalIgnoreCase) ||
					subtype.Equals ("x-pgp-signature", StringComparison.OrdinalIgnoreCase))
					return new ApplicationPgpSignature (args);
#endif

				// application/ms-tnef
				if (subtype.Equals ("ms-tnef", StringComparison.OrdinalIgnoreCase) ||
					subtype.Equals ("vnd.ms-tnef", StringComparison.OrdinalIgnoreCase))
					return new TnefPart (args);

				// application/rtf
				if (subtype.Equals ("rtf", StringComparison.OrdinalIgnoreCase))
					return new TextPart (args);
			}

			return new MimePart (args);
		}
	}
}
