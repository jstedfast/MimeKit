//
// DkimVerifier.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2019 Xamarin Inc. (www.xamarin.com)
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
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

#if PORTABLE
using Encoding = Portable.Text.Encoding;
#endif

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;

using MimeKit;
using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A DKIM-Signature verifier.
	/// </summary>
	/// <remarks>
	/// Verifies DomainKeys Identified Mail (DKIM) signatures.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
	/// </example>
	public class DkimVerifier
	{
		readonly IDkimPublicKeyLocator publicKeyLocator;
		int enabledSignatureAlgorithms;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DkimVerifier"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DkimVerifier"/>.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <param name="publicKeyLocator">The public key locator.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="publicKeyLocator"/> is <c>null</c>.
		/// </exception>
		public DkimVerifier (IDkimPublicKeyLocator publicKeyLocator)
		{
			if (publicKeyLocator == null)
				throw new ArgumentNullException (nameof (publicKeyLocator));

			this.publicKeyLocator = publicKeyLocator;

			Enable (DkimSignatureAlgorithm.RsaSha256);
			//Enable (DkimSignatureAlgorithm.RsaSha1);
			MinimumRsaKeyLength = 1024;
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

				var name = signature.Substring (startIndex, index - startIndex).TrimEnd ();

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
				if (!int.TryParse (t, NumberStyles.Integer, CultureInfo.InvariantCulture, out int timestamp) || timestamp < 0)
					throw new FormatException (string.Format ("Malformed {0} header: invalid timestamp parameter: t={1}.", header, t));
			}
		}

		internal static void ValidateCommonSignatureParameters (string header, IDictionary<string, string> parameters, out DkimSignatureAlgorithm algorithm, out DkimCanonicalizationAlgorithm headerAlgorithm,
			out DkimCanonicalizationAlgorithm bodyAlgorithm, out string d, out string s, out string q, out string[] headers, out string bh, out string b, out int maxLength)
		{
			ValidateCommonParameters (header, parameters, out algorithm, out d, out s, out q, out b);

			if (parameters.TryGetValue ("l", out string l)) {
				if (!int.TryParse (l, out maxLength))
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

		static void ValidateDkimSignatureParameters (IDictionary<string, string> parameters, out DkimSignatureAlgorithm algorithm, out DkimCanonicalizationAlgorithm headerAlgorithm,
			out DkimCanonicalizationAlgorithm bodyAlgorithm, out string d, out string s, out string q, out string[] headers, out string bh, out string b, out int maxLength)
		{
			bool containsFrom = false;

			if (!parameters.TryGetValue ("v", out string v))
				throw new FormatException ("Malformed DKIM-Signature header: no version parameter detected.");

			if (v != "1")
				throw new FormatException (string.Format ("Unrecognized DKIM-Signature version: v={0}", v));

			ValidateCommonSignatureParameters ("DKIM-Signature", parameters, out algorithm, out headerAlgorithm, out bodyAlgorithm, out d, out s, out q, out headers, out bh, out b, out maxLength);

			for (int i = 0; i < headers.Length; i++) {
				if (headers[i].Equals ("from", StringComparison.OrdinalIgnoreCase)) {
					containsFrom = true;
					break;
				}
			}

			if (!containsFrom)
				throw new FormatException ("Malformed DKIM-Signature header: From header not signed.");

			if (parameters.TryGetValue ("i", out string id)) {
				string ident;
				int at;

				if ((at = id.LastIndexOf ('@')) == -1)
					throw new FormatException ("Malformed DKIM-Signature header: no @ in the AUID value.");

				ident = id.Substring (at + 1);

				if (!ident.Equals (d, StringComparison.OrdinalIgnoreCase) && !ident.EndsWith ("." + d, StringComparison.OrdinalIgnoreCase))
					throw new FormatException ("Invalid DKIM-Signature header: the domain in the AUID does not match the domain parameter.");
			}
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

		internal static ISigner GetDigestSigner (DkimSignatureAlgorithm algorithm, AsymmetricKeyParameter key)
		{
#if ENABLE_NATIVE_DKIM
			return new SystemSecuritySigner (algorithm, key.AsAsymmetricAlgorithm ());
#else
			DerObjectIdentifier id;

			if (algorithm == DkimSignatureAlgorithm.RsaSha256)
				id = PkcsObjectIdentifiers.Sha256WithRsaEncryption;
			else
				id = PkcsObjectIdentifiers.Sha1WithRsaEncryption;

			var signer = SignerUtilities.GetSigner (id);

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
				int index, count, n = 0;

				if (!counts.TryGetValue (name, out count))
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

			return new Header (header.Options, header.RawField, rawValue);
		}

		async Task<bool> VerifyAsync (FormatOptions options, MimeMessage message, Header dkimSignature, bool doAsync, CancellationToken cancellationToken)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (message == null)
				throw new ArgumentNullException (nameof (message));

			if (dkimSignature == null)
				throw new ArgumentNullException (nameof (dkimSignature));

			if (dkimSignature.Id != HeaderId.DkimSignature)
				throw new ArgumentException ("The signature parameter MUST be a DKIM-Signature header.", nameof (dkimSignature));

			var parameters = ParseParameterTags (dkimSignature.Id, dkimSignature.Value);
			DkimCanonicalizationAlgorithm headerAlgorithm, bodyAlgorithm;
			DkimSignatureAlgorithm signatureAlgorithm;
			AsymmetricKeyParameter key;
			string d, s, q, bh, b;
			string[] headers;
			int maxLength;

			ValidateDkimSignatureParameters (parameters, out signatureAlgorithm, out headerAlgorithm, out bodyAlgorithm,
				out d, out s, out q, out headers, out bh, out b, out maxLength);

			if (!IsEnabled (signatureAlgorithm))
				return false;

			if (doAsync)
				key = await publicKeyLocator.LocatePublicKeyAsync (q, d, s, cancellationToken).ConfigureAwait (false);
			else
				key = publicKeyLocator.LocatePublicKey (q, d, s, cancellationToken);

			if (!(key is RsaKeyParameters rsa) || rsa.Modulus.BitLength < MinimumRsaKeyLength)
				return false;

			options = options.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;

			// first check the body hash (if that's invalid, then the entire signature is invalid)
			var hash = Convert.ToBase64String (message.HashBody (options, signatureAlgorithm, bodyAlgorithm, maxLength));

			if (hash != bh)
				return false;

			using (var stream = new DkimSignatureStream (GetDigestSigner (signatureAlgorithm, key))) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (options.CreateNewLineFilter ());

					WriteHeaders (options, message, headers, headerAlgorithm, filtered);

					// now include the DKIM-Signature header that we are verifying,
					// but only after removing the "b=" signature value.
					var header = GetSignedSignatureHeader (dkimSignature);

					switch (headerAlgorithm) {
					case DkimCanonicalizationAlgorithm.Relaxed:
						WriteHeaderRelaxed (options, filtered, header, true);
						break;
					default:
						WriteHeaderSimple (options, filtered, header, true);
						break;
					}

					filtered.Flush ();
				}

				return stream.VerifySignature (b);
			}
		}

		/// <summary>
		/// Verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// Verifies the specified DKIM-Signature header.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns><c>true</c> if the DKIM-Signature is valid; otherwise, <c>false</c>.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to verify.</param>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimSignature"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="dkimSignature"/> is not a DKIM-Signature header.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The DKIM-Signature header value is malformed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public bool Verify (FormatOptions options, MimeMessage message, Header dkimSignature, CancellationToken cancellationToken = default (CancellationToken))
		{
			return VerifyAsync (options, message, dkimSignature, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// Verifies the specified DKIM-Signature header.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns><c>true</c> if the DKIM-Signature is valid; otherwise, <c>false</c>.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to verify.</param>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimSignature"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="dkimSignature"/> is not a DKIM-Signature header.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The DKIM-Signature header value is malformed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public Task<bool> VerifyAsync (FormatOptions options, MimeMessage message, Header dkimSignature, CancellationToken cancellationToken = default (CancellationToken))
		{
			return VerifyAsync (options, message, dkimSignature, true, cancellationToken);
		}

		/// <summary>
		/// Verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// Verifies the specified DKIM-Signature header.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns><c>true</c> if the DKIM-Signature is valid; otherwise, <c>false</c>.</returns>
		/// <param name="message">The message to verify.</param>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimSignature"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="dkimSignature"/> is not a DKIM-Signature header.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The DKIM-Signature header value is malformed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public bool Verify (MimeMessage message, Header dkimSignature, CancellationToken cancellationToken = default (CancellationToken))
		{
			return Verify (FormatOptions.Default, message, dkimSignature, cancellationToken);
		}

		/// <summary>
		/// Asynchronously verify the specified DKIM-Signature header.
		/// </summary>
		/// <remarks>
		/// Verifies the specified DKIM-Signature header.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimVerifierExample.cs" />
		/// </example>
		/// <returns><c>true</c> if the DKIM-Signature is valid; otherwise, <c>false</c>.</returns>
		/// <param name="message">The message to verify.</param>
		/// <param name="dkimSignature">The DKIM-Signature header.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dkimSignature"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="dkimSignature"/> is not a DKIM-Signature header.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The DKIM-Signature header value is malformed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public Task<bool> VerifyAsync (MimeMessage message, Header dkimSignature, CancellationToken cancellationToken = default (CancellationToken))
		{
			return VerifyAsync (FormatOptions.Default, message, dkimSignature, cancellationToken);
		}
	}
}
