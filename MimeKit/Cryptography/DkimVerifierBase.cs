//
// DkimVerifierBase.cs
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
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Signers;

using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A base class for DKIM and ARC verifiers.
	/// </summary>
	/// <remarks>
	/// The base class for <see cref="DkimVerifier"/> and <see cref="ArcVerifier"/>.
	/// </remarks>
	public abstract class DkimVerifierBase
	{
		int enabledSignatureAlgorithms;

		/// <summary>
		/// Initialize a new instance of the <see cref="DkimVerifierBase"/> class.
		/// </summary>
		/// <remarks>
		/// Initializes the <see cref="DkimVerifierBase"/>.
		/// </remarks>
		/// <param name="publicKeyLocator">The public key locator.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="publicKeyLocator"/> is <c>null</c>.
		/// </exception>
		protected DkimVerifierBase (IDkimPublicKeyLocator publicKeyLocator)
		{
			if (publicKeyLocator == null)
				throw new ArgumentNullException (nameof (publicKeyLocator));

			PublicKeyLocator = publicKeyLocator;

			Enable (DkimSignatureAlgorithm.Ed25519Sha256);
			Enable (DkimSignatureAlgorithm.RsaSha256);
			//Enable (DkimSignatureAlgorithm.RsaSha1);
			MinimumRsaKeyLength = 1024;
		}

		/// <summary>
		/// Get the public key locator.
		/// </summary>
		/// <remarks>
		/// Gets the public key locator.
		/// </remarks>
		/// <value>The public key locator.</value>
		protected IDkimPublicKeyLocator PublicKeyLocator {
			get; private set;
		}

		/// <summary>
		/// Get or set the minimum allowed RSA key length.
		/// </summary>
		/// <remarks>
		/// <para>Gets the minimum allowed RSA key length.</para>
		/// <note type="security">The DKIM specifications specify a single signing algorithm, RSA,
		/// and recommend key sizes of 1024 to 2048 bits (but require verification of 512-bit keys).
		/// As discussed in US-CERT Vulnerability Note VU#268267, the operational community has
		/// recognized that shorter keys compromise the effectiveness of DKIM. While 1024-bit
		/// signatures are common, stronger signatures are not. Widely used DNS configuration
		/// software places a practical limit on key sizes, because the software only handles a
		/// single 256-octet string in a TXT record, and RSA keys significantly longer than 1024
		/// bits don't fit in 256 octets.</note>
		/// </remarks>
		/// <value>The minimum allowed RSA key length.</value>
		public int MinimumRsaKeyLength {
			get; set;
		}

		/// <summary>
		/// Enable a DKIM signature algorithm.
		/// </summary>
		/// <remarks>
		/// <para>Enables the specified DKIM signature algorithm.</para>
		/// <note type="security">Due to the recognized weakness of the SHA-1 hash algorithm
		/// and the wide availability of the SHA-256 hash algorithm (it has been a required
		/// part of DKIM since it was originally standardized in 2007), it is recommended
		/// that <see cref="DkimSignatureAlgorithm.RsaSha1"/> NOT be enabled.</note>
		/// </remarks>
		/// <param name="algorithm">The DKIM signature algorithm.</param>
		public void Enable (DkimSignatureAlgorithm algorithm)
		{
			enabledSignatureAlgorithms |= 1 << (int) algorithm;
		}

		/// <summary>
		/// Disable a DKIM signature algorithm.
		/// </summary>
		/// <remarks>
		/// <para>Disables the specified DKIM signature algorithm.</para>
		/// <note type="security">Due to the recognized weakness of the SHA-1 hash algorithm
		/// and the wide availability of the SHA-256 hash algorithm (it has been a required
		/// part of DKIM since it was originally standardized in 2007), it is recommended
		/// that <see cref="DkimSignatureAlgorithm.RsaSha1"/> NOT be enabled.</note>
		/// </remarks>
		/// <param name="algorithm">The DKIM signature algorithm.</param>
		public void Disable (DkimSignatureAlgorithm algorithm)
		{
			enabledSignatureAlgorithms &= ~(1 << (int) algorithm);
		}

		/// <summary>
		/// Check whether a DKIM signature algorithm is enabled.
		/// </summary>
		/// <remarks>
		/// <para>Determines whether the specified DKIM signature algorithm is enabled.</para>
		/// <note type="security">Due to the recognized weakness of the SHA-1 hash algorithm
		/// and the wide availability of the SHA-256 hash algorithm (it has been a required
		/// part of DKIM since it was originally standardized in 2007), it is recommended
		/// that <see cref="DkimSignatureAlgorithm.RsaSha1"/> NOT be enabled.</note>
		/// </remarks>
		/// <returns><c>true</c> if the specified DKIM signature algorithm is enabled; otherwise, <c>false</c>.</returns>
		/// <param name="algorithm">The DKIM signature algorithm.</param>
		public bool IsEnabled (DkimSignatureAlgorithm algorithm)
		{
			return (enabledSignatureAlgorithms & (1 << (int) algorithm)) != 0;
		}

		static bool IsWhiteSpace (char c)
		{
			return c == ' ' || c == '\t';
		}

		static bool IsAlpha (char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
		}

		internal static Dictionary<string, string> ParseParameterTags (HeaderId header, string signature)
		{
			var parameters = new Dictionary<string, string> ();
			var value = new StringBuilder ();
			int index = 0;

			while (index < signature.Length) {
				while (index < signature.Length && IsWhiteSpace (signature[index]))
					index++;

				if (index >= signature.Length)
					break;

				if (signature[index] == ';' || !IsAlpha (signature[index]))
					throw new FormatException (string.Format ("Malformed {0} value.", header.ToHeaderName ()));

				int startIndex = index++;

				while (index < signature.Length && signature[index] != '=')
					index++;

				if (index >= signature.Length)
					continue;

				var name = signature.AsSpan (startIndex, index - startIndex).TrimEnd ().ToString ();

				// skip over '=' and clear value buffer
				value.Length = 0;
				index++;

				while (index < signature.Length && signature[index] != ';') {
					if (!IsWhiteSpace (signature[index]))
						value.Append (signature[index]);
					index++;
				}

				if (parameters.ContainsKey (name))
					throw new FormatException (string.Format ("Malformed {0} value: duplicate parameter '{1}'.", header.ToHeaderName (), name));

				parameters.Add (name, value.ToString ());

				// skip over ';'
				index++;
			}

			return parameters;
		}

		internal static void ValidateCommonParameters (string header, IDictionary<string, string> parameters, out DkimSignatureAlgorithm algorithm,
			out string d, out string s, out string q, out string b)
		{
			if (!parameters.TryGetValue ("a", out string a))
				throw new FormatException (string.Format ("Malformed {0} header: no signature algorithm parameter detected.", header));

			switch (a.ToLowerInvariant ()) {
			case "ed25519-sha256": algorithm = DkimSignatureAlgorithm.Ed25519Sha256; break;
			case "rsa-sha256": algorithm = DkimSignatureAlgorithm.RsaSha256; break;
			case "rsa-sha1": algorithm = DkimSignatureAlgorithm.RsaSha1; break;
			default: throw new FormatException (string.Format ("Unrecognized {0} algorithm parameter: a={1}", header, a));
			}

			if (!parameters.TryGetValue ("d", out d))
				throw new FormatException (string.Format ("Malformed {0} header: no domain parameter detected.", header));

			if (d.Length == 0)
				throw new FormatException (string.Format ("Malformed {0} header: empty domain parameter detected.", header));

			if (!parameters.TryGetValue ("s", out s))
				throw new FormatException (string.Format ("Malformed {0} header: no selector parameter detected.", header));

			if (s.Length == 0)
				throw new FormatException (string.Format ("Malformed {0} header: empty selector parameter detected.", header));

			if (!parameters.TryGetValue ("q", out q))
				q = "dns/txt";

			if (!parameters.TryGetValue ("b", out b))
				throw new FormatException (string.Format ("Malformed {0} header: no signature parameter detected.", header));

			if (b.Length == 0)
				throw new FormatException (string.Format ("Malformed {0} header: empty signature parameter detected.", header));

			if (parameters.TryGetValue ("t", out string t)) {
				if (!int.TryParse (t, NumberStyles.None, CultureInfo.InvariantCulture, out int timestamp) || timestamp < 0)
					throw new FormatException (string.Format ("Malformed {0} header: invalid timestamp parameter: t={1}.", header, t));
			}
		}

		internal static void ValidateCommonSignatureParameters (string header, IDictionary<string, string> parameters, out DkimSignatureAlgorithm algorithm, out DkimCanonicalizationAlgorithm headerAlgorithm,
			out DkimCanonicalizationAlgorithm bodyAlgorithm, out string d, out string s, out string q, out string[] headers, out string bh, out string b, out int maxLength)
		{
			ValidateCommonParameters (header, parameters, out algorithm, out d, out s, out q, out b);

			if (parameters.TryGetValue ("l", out string l)) {
				if (!int.TryParse (l, NumberStyles.None, CultureInfo.InvariantCulture, out maxLength) || maxLength < 0)
					throw new FormatException (string.Format ("Malformed {0} header: invalid length parameter: l={1}", header, l));
			} else {
				maxLength = -1;
			}

			if (parameters.TryGetValue ("c", out string c)) {
				var tokens = c.ToLowerInvariant ().Split ('/');

				if (tokens.Length == 0 || tokens.Length > 2)
					throw new FormatException (string.Format ("Malformed {0} header: invalid canonicalization parameter: c={1}", header, c));

				switch (tokens[0]) {
				case "relaxed": headerAlgorithm = DkimCanonicalizationAlgorithm.Relaxed; break;
				case "simple": headerAlgorithm = DkimCanonicalizationAlgorithm.Simple; break;
				default: throw new FormatException (string.Format ("Malformed {0} header: invalid canonicalization parameter: c={1}", header, c));
				}

				if (tokens.Length == 2) {
					switch (tokens[1]) {
					case "relaxed": bodyAlgorithm = DkimCanonicalizationAlgorithm.Relaxed; break;
					case "simple": bodyAlgorithm = DkimCanonicalizationAlgorithm.Simple; break;
					default: throw new FormatException (string.Format ("Malformed {0} header: invalid canonicalization parameter: c={1}", header, c));
					}
				} else {
					bodyAlgorithm = DkimCanonicalizationAlgorithm.Simple;
				}
			} else {
				headerAlgorithm = DkimCanonicalizationAlgorithm.Simple;
				bodyAlgorithm = DkimCanonicalizationAlgorithm.Simple;
			}

			if (!parameters.TryGetValue ("h", out string h))
				throw new FormatException (string.Format ("Malformed {0} header: no signed header parameter detected.", header));

			headers = h.Split (':');

			if (!parameters.TryGetValue ("bh", out bh))
				throw new FormatException (string.Format ("Malformed {0} header: no body hash parameter detected.", header));
		}

		internal static void WriteHeaderRelaxed (FormatOptions options, Stream stream, Header header, bool isDkimSignature)
		{
			// o  Convert all header field names (not the header field values) to
			//    lowercase.  For example, convert "SUBJect: AbC" to "subject: AbC".
			var name = Encoding.ASCII.GetBytes (header.Field.ToLowerInvariant ());
			var rawValue = header.GetRawValue (options);
			int index = 0;

			// o  Delete any WSP characters remaining before and after the colon
			//    separating the header field name from the header field value.  The
			//    colon separator MUST be retained.
			stream.Write (name, 0, name.Length);
			stream.WriteByte ((byte) ':');

			// trim leading whitespace...
			while (index < rawValue.Length && rawValue[index].IsWhitespace ())
				index++;

			while (index < rawValue.Length) {
				int startIndex = index;

				// look for the first non-whitespace character
				while (index < rawValue.Length && rawValue[index].IsWhitespace ())
					index++;

				// o  Delete all WSP characters at the end of each unfolded header field
				//    value.
				if (index >= rawValue.Length)
					break;

				// o  Convert all sequences of one or more WSP characters to a single SP
				//    character.  WSP characters here include those before and after a
				//    line folding boundary.
				if (index > startIndex)
					stream.WriteByte ((byte) ' ');

				startIndex = index;

				while (index < rawValue.Length && !rawValue[index].IsWhitespace ())
					index++;

				if (index > startIndex)
					stream.Write (rawValue, startIndex, index - startIndex);
			}

			if (!isDkimSignature)
				stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
		}

		internal static void WriteHeaderSimple (FormatOptions options, Stream stream, Header header, bool isDkimSignature)
		{
			var rawValue = header.GetRawValue (options);
			int rawLength = rawValue.Length;

			if (isDkimSignature && rawLength > 0) {
				if (rawValue[rawLength - 1] == (byte) '\n') {
					rawLength--;

					if (rawLength > 0 && rawValue[rawLength - 1] == (byte) '\r')
						rawLength--;
				}
			}

			stream.Write (header.RawField, 0, header.RawField.Length);
			stream.Write (Header.Colon, 0, Header.Colon.Length);
			stream.Write (rawValue, 0, rawLength);
		}

		/// <summary>
		/// Create the digest signing context.
		/// </summary>
		/// <remarks>
		/// Creates a new digest signing context that uses the specified algorithm.
		/// </remarks>
		/// <param name="algorithm">The DKIM signature algorithm.</param>
		/// <param name="key">The public key.</param>
		/// <returns>The digest signer.</returns>
		internal virtual ISigner CreateVerifyContext (DkimSignatureAlgorithm algorithm, AsymmetricKeyParameter key)
		{
#if ENABLE_NATIVE_DKIM
			return new SystemSecuritySigner (algorithm, key.AsAsymmetricAlgorithm ());
#else
			ISigner signer;

			switch (algorithm) {
			case DkimSignatureAlgorithm.RsaSha1:
				signer = new RsaDigestSigner (new Sha1Digest ());
				break;
			case DkimSignatureAlgorithm.RsaSha256:
				signer = new RsaDigestSigner (new Sha256Digest ());
				break;
			case DkimSignatureAlgorithm.Ed25519Sha256:
				signer = new Ed25519DigestSigner (new Sha256Digest ());
				break;
			default:
				throw new NotSupportedException (string.Format ("{0} is not supported.", algorithm));
			}

			signer.Init (key.IsPrivate, key);

			return signer;
#endif
		}

		internal static void WriteHeaders (FormatOptions options, MimeMessage message, IList<string> fields, DkimCanonicalizationAlgorithm headerCanonicalizationAlgorithm, Stream stream)
		{
			var counts = new Dictionary<string, int> (StringComparer.Ordinal);

			for (int i = 0; i < fields.Count; i++) {
				var headers = fields[i].StartsWith ("Content-", StringComparison.OrdinalIgnoreCase) ? message.Body.Headers : message.Headers;
				var name = fields[i].ToLowerInvariant ();
				int index, n = 0;

				if (!counts.TryGetValue (name, out var count))
					count = 0;

				// Note: signers choosing to sign an existing header field that occurs more
				// than once in the message (such as Received) MUST sign the physically last
				// instance of that header field in the header block. Signers wishing to sign
				// multiple instances of such a header field MUST include the header field
				// name multiple times in the list of header fields and MUST sign such header
				// fields in order from the bottom of the header field block to the top.
				index = headers.LastIndexOf (name);

				// find the n'th header with this name
				while (n < count && --index >= 0) {
					if (headers[index].Field.Equals (name, StringComparison.OrdinalIgnoreCase))
						n++;
				}

				if (index < 0)
					continue;

				var header = headers[index];

				switch (headerCanonicalizationAlgorithm) {
				case DkimCanonicalizationAlgorithm.Relaxed:
					WriteHeaderRelaxed (options, stream, header, false);
					break;
				default:
					WriteHeaderSimple (options, stream, header, false);
					break;
				}

				counts[name] = ++count;
			}
		}

		internal static Header GetSignedSignatureHeader (Header header)
		{
			// modify the raw DKIM-Signature header value by chopping off the signature value after the "b="
			var rawValue = (byte[]) header.RawValue.Clone ();
			int length = 0, index = 0;

			do {
				while (index < rawValue.Length && rawValue[index].IsWhitespace ())
					index++;

				if (index + 2 < rawValue.Length) {
					var param = (char) rawValue[index++];

					while (index < rawValue.Length && rawValue[index].IsWhitespace ())
						index++;

					if (index < rawValue.Length && rawValue[index] == (byte) '=' && param == 'b') {
						length = ++index;

						while (index < rawValue.Length && rawValue[index] != (byte) ';')
							index++;

						if (index == rawValue.Length && rawValue[index - 1] == (byte) '\n') {
							index--;

							if (rawValue[index - 1] == (byte) '\r')
								index--;
						}

						break;
					}
				}

				while (index < rawValue.Length && rawValue[index] != (byte) ';')
					index++;

				if (index < rawValue.Length)
					index++;
			} while (index < rawValue.Length);

			if (index == rawValue.Length)
				throw new FormatException (string.Format ("Malformed {0} header: missing signature parameter.", header.Id.ToHeaderName ()));

			while (index < rawValue.Length)
				rawValue[length++] = rawValue[index++];

			Array.Resize (ref rawValue, length);

			return new Header (header.Options, header.RawField, rawValue, false);
		}

		/// <summary>
		/// Verify the hash of the message body.
		/// </summary>
		/// <remarks>
		/// Verifies the hash of the message body.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The signed MIME message.</param>
		/// <param name="signatureAlgorithm">The algorithm used to sign the message.</param>
		/// <param name="canonicalizationAlgorithm">The algorithm used to canonicalize the message body.</param>
		/// <param name="maxLength">The max length of the message body to hash or <c>-1</c> to hash the entire message body.</param>
		/// <param name="bodyHash">The expected message body hash encoded in base64.</param>
		/// <returns><c>true</c> if the calculated body hash matches <paramref name="bodyHash"/>; otherwise, <c>false</c>.</returns>
		protected static bool VerifyBodyHash (FormatOptions options, MimeMessage message, DkimSignatureAlgorithm signatureAlgorithm, DkimCanonicalizationAlgorithm canonicalizationAlgorithm, int maxLength, string bodyHash)
		{
			var hash = Convert.ToBase64String (message.HashBody (options, signatureAlgorithm, canonicalizationAlgorithm, maxLength));

			return hash == bodyHash;
		}

		/// <summary>
		/// Verify the signature of the message headers.
		/// </summary>
		/// <remarks>
		/// Verifies the signature of the message headers.
		/// </remarks>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The signed MIME message.</param>
		/// <param name="dkimSignature">The DKIM-Signature or ARC-Message-Signature header.</param>
		/// <param name="signatureAlgorithm">The algorithm used to sign the message headers.</param>
		/// <param name="key">The public key used to verify the signature.</param>
		/// <param name="headers">The list of headers that were signed.</param>
		/// <param name="canonicalizationAlgorithm">The algorithm used to canonicalize the headers.</param>
		/// <param name="signature">The expected signature of the headers encoded in base64.</param>
		/// <returns><c>true</c> if the calculated signature matches <paramref name="signature"/>; otherwise, <c>false</c>.</returns>
		protected bool VerifySignature (FormatOptions options, MimeMessage message, Header dkimSignature, DkimSignatureAlgorithm signatureAlgorithm, AsymmetricKeyParameter key, string[] headers, DkimCanonicalizationAlgorithm canonicalizationAlgorithm, string signature)
		{
			using (var stream = new DkimSignatureStream (CreateVerifyContext (signatureAlgorithm, key))) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (options.CreateNewLineFilter ());

					WriteHeaders (options, message, headers, canonicalizationAlgorithm, filtered);

					// now include the DKIM-Signature header that we are verifying,
					// but only after removing the "b=" signature value.
					var header = GetSignedSignatureHeader (dkimSignature);

					switch (canonicalizationAlgorithm) {
					case DkimCanonicalizationAlgorithm.Relaxed:
						WriteHeaderRelaxed (options, filtered, header, true);
						break;
					default:
						WriteHeaderSimple (options, filtered, header, true);
						break;
					}

					filtered.Flush ();
				}

				return stream.VerifySignature (signature);
			}
		}
	}
}
