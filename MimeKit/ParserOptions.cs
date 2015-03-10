//
// ParserOptions.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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

#if PORTABLE
using Encoding = Portable.Text.Encoding;
#endif

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
		readonly Dictionary<string, ConstructorInfo> mimeTypes = new Dictionary<string, ConstructorInfo> ();
		static readonly Type[] ConstructorArgTypes = { typeof (MimeEntityConstructorInfo) };

		/// <summary>
		/// The default parser options.
		/// </summary>
		/// <remarks>
		/// If a <see cref="ParserOptions"/> is not supplied to <see cref="MimeParser"/> or other Parse and TryParse
		/// methods throughout MimeKit, <see cref="ParserOptions.Default"/> will be used.
		/// </remarks>
		public static readonly ParserOptions Default = new ParserOptions ();

		/// <summary>
		/// Gets or sets the compliance mode that should be used when parsing rfc822 addresses.
		/// </summary>
		/// <remarks>
		/// <para>In general, you'll probably want this value to be <see cref="RfcComplianceMode.Loose"/>
		/// (the default) as it allows maximum interoperability with existing (broken) mail clients
		/// and other mail software such as sloppily written perl scripts (aka spambots).</para>
		/// <para>It should be noted that even in <see cref="RfcComplianceMode.Strict"/> mode, the address
		/// parser is fairly liberal in what it accepts. Setting it to <see cref="RfcComplianceMode.Loose"/>
		/// just makes it try harder to deal with garbage input.</para>
		/// </remarks>
		/// <value>The RFC compliance mode.</value>
		public RfcComplianceMode AddressParserComplianceMode { get; set; }

		/// <summary>
		/// Gets or sets the compliance mode that should be used when parsing Content-Type and Content-Disposition parameters.
		/// </summary>
		/// <remarks>
		/// <para>In general, you'll probably want this value to be <see cref="RfcComplianceMode.Loose"/>
		/// (the default) as it allows maximum interoperability with existing (broken) mail clients
		/// and other mail software such as sloppily written perl scripts (aka spambots).</para>
		/// <para>It should be noted that even in <see cref="RfcComplianceMode.Strict"/> mode, the parameter
		/// parser is fairly liberal in what it accepts. Setting it to <see cref="RfcComplianceMode.Loose"/>
		/// just makes it try harder to deal with garbage input.</para>
		/// </remarks>
		/// <value>The RFC compliance mode.</value>
		public RfcComplianceMode ParameterComplianceMode { get; set; }

		/// <summary>
		/// Gets or sets the compliance mode that should be used when decoding rfc2047 encoded words.
		/// </summary>
		/// <remarks>
		/// In general, you'll probably want this value to be <see cref="RfcComplianceMode.Loose"/>
		/// (the default) as it allows maximum interoperability with existing (broken) mail clients
		/// and other mail software such as sloppily written perl scripts (aka spambots).
		/// </remarks>
		/// <value>The RFC compliance mode.</value>
		public RfcComplianceMode Rfc2047ComplianceMode { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Content-Length value should be
		/// respected when parsing mbox streams.
		/// </summary>
		/// <remarks>
		/// For more details about why this may be useful, you can find more information
		/// at http://www.jwz.org/doc/content-length.html
		/// </remarks>
		/// <value><c>true</c> if the Content-Length value should be respected;
		/// otherwise, <c>false</c>.</value>
		public bool RespectContentLength { get; set; }

		/// <summary>
		/// Gets or sets the charset encoding to use as a fallback for 8bit headers.
		/// </summary>
		/// <remarks>
		/// <see cref="MimeKit.Utils.Rfc2047.DecodeText(ParserOptions, byte[])"/> and
		/// <see cref="MimeKit.Utils.Rfc2047.DecodePhrase(ParserOptions, byte[])"/>
		/// use this charset encoding as a fallback when decoding 8bit text into unicode. The first
		/// charset encoding attempted is UTF-8, followed by this charset encoding, before finally
		/// falling back to iso-8859-1.
		/// </remarks>
		/// <value>The charset encoding.</value>
		public Encoding CharsetEncoding { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.ParserOptions"/> class.
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
			CharsetEncoding = Encoding.Default;
			RespectContentLength = false;
		}

		/// <summary>
		/// Clones an instance of <see cref="MimeKit.ParserOptions"/>.
		/// </summary>
		/// <remarks>
		/// Clones a set of options, allowing you to change a specific option
		/// without requiring you to change the original.
		/// </remarks>
		/// <returns>An identical copy of the current instance.</returns>
		public ParserOptions Clone ()
		{
			var options = new ParserOptions ();
			options.AddressParserComplianceMode = AddressParserComplianceMode;
			options.ParameterComplianceMode = ParameterComplianceMode;
			options.Rfc2047ComplianceMode = Rfc2047ComplianceMode;
			options.RespectContentLength = RespectContentLength;
			options.CharsetEncoding = CharsetEncoding;

			foreach (var mimeType in mimeTypes)
				options.mimeTypes.Add (mimeType.Key, mimeType.Value);

			return options;
		}

#if PORTABLE
		static ConstructorInfo GetConstructor (TypeInfo type, Type[] argTypes)
		{
			foreach (var ctor in type.DeclaredConstructors) {
				var args = ctor.GetParameters ();

				if (args.Length != ConstructorArgTypes.Length)
					continue;

				bool matched = true;
				for (int i = 0; i < argTypes.Length && matched; i++)
					matched = matched && args[i].ParameterType == argTypes[i];

				if (matched)
					return ctor;
			}

			return null;
		}
#endif

		/// <summary>
		/// Registers the <see cref="MimeEntity"/> subclass for the specified mime-type.
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
		/// only a <see cref="MimeEntityConstructorInfo"/> argument.</para>
		/// </exception>
		public void RegisterMimeType (string mimeType, Type type)
		{
			if (mimeType == null)
				throw new ArgumentNullException ("mimeType");

			if (type == null)
				throw new ArgumentNullException ("type");

			mimeType = mimeType.ToLowerInvariant ();

#if PORTABLE
			var info = type.GetTypeInfo ();
#else
			var info = type;
#endif

			if (!info.IsSubclassOf (typeof (MessagePart)) &&
				!info.IsSubclassOf (typeof (Multipart)) &&
				!info.IsSubclassOf (typeof (MimePart)))
				throw new ArgumentException ("The specified type must be a subclass of MessagePart, Multipart, or MimePart.", "type");

#if PORTABLE
			var ctor = GetConstructor (info, ConstructorArgTypes);
#else
			var ctor = type.GetConstructor (ConstructorArgTypes);
#endif

			if (ctor == null)
				throw new ArgumentException ("The specified type must have a constructor that takes a MimeEntityConstructorInfo argument.", "type");

			mimeTypes[mimeType] = ctor;
		}

		static bool IsEncoded (IList<Header> headers)
		{
			ContentEncoding encoding;

			for (int i = 0; i < headers.Count; i++) {
				if (headers[i].Id != HeaderId.ContentTransferEncoding)
					continue;

				MimeUtils.TryParse (headers[i].Value, out encoding);

				switch (encoding) {
				case ContentEncoding.SevenBit:
				case ContentEncoding.EightBit:
				case ContentEncoding.Binary:
					return false;
				default:
					return true;
				}
			}

			return false;
		}

		internal MimeEntity CreateEntity (ContentType contentType, IList<Header> headers, bool toplevel)
		{
			var entity = new MimeEntityConstructorInfo (this, contentType, headers, toplevel);
			var subtype = contentType.MediaSubtype.ToLowerInvariant ();
			var type = contentType.MediaType.ToLowerInvariant ();

			if (mimeTypes.Count > 0) {
				var mimeType = string.Format ("{0}/{1}", type, subtype);
				ConstructorInfo ctor;

				if (mimeTypes.TryGetValue (mimeType, out ctor))
					return (MimeEntity) ctor.Invoke (new object[] { entity });
			}

			// Note: message/rfc822 and message/partial are not allowed to be encoded according to rfc2046
			// (sections 5.2.1 and 5.2.2, respectively). Since some broken clients will encode them anyway,
			// it is necessary for us to treat those as opaque blobs instead, and thus the parser should
			// parse them as normal MimeParts instead of MessageParts.
			//
			// Technically message/disposition-notification is only allowed to have use the 7bit encoding
			// as well, but since MessageDispositionNotification is a MImePart subclass rather than a
			// MessagePart subclass, it means that the content won't be parsed until later and so we can
			// actually handle that w/o any problems.
			if (type == "message") {
				switch (subtype) {
				case "disposition-notification":
					return new MessageDispositionNotification (entity);
				case "partial":
					if (!IsEncoded (headers))
						return new MessagePartial (entity);
					break;
				case "external-body":
				case "rfc2822":
				case "rfc822":
				case "news":
					if (!IsEncoded (headers))
						return new MessagePart (entity);
					break;
				}
			}

			if (type == "multipart") {
				if (subtype == "related")
					return new MultipartRelated (entity);

#if ENABLE_CRYPTO
				if (subtype == "encrypted")
					return new MultipartEncrypted (entity);

				if (subtype == "signed")
					return new MultipartSigned (entity);
#endif

				return new Multipart (entity);
			}

#if ENABLE_CRYPTO
			if (type == "application") {
				switch (subtype) {
				case "x-pkcs7-signature":
				case "pkcs7-signature":
					return new ApplicationPkcs7Signature (entity);
				case "x-pgp-encrypted":
				case "pgp-encrypted":
					return new ApplicationPgpEncrypted (entity);
				case "x-pgp-signature":
				case "pgp-signature":
					return new ApplicationPgpSignature (entity);
				case "x-pkcs7-mime":
				case "pkcs7-mime":
					return new ApplicationPkcs7Mime (entity);
				case "vnd.ms-tnef":
				case "ms-tnef":
					return new TnefPart (entity);
				}
			}
#endif

			if (type == "text")
				return new TextPart (entity);

			return new MimePart (entity);
		}
	}
}
