//
// ArcSigner.cs
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Org.BouncyCastle.Crypto;

using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An ARC signer.
	/// </summary>
	/// <remarks>
	/// An ARC signer.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\ArcSignerExample.cs" />
	/// </example>
	public abstract class ArcSigner : DkimSignerBase
	{
		static readonly string[] ArcShouldNotInclude = { "return-path", "received", "comments", "keywords", "bcc", "resent-bcc", "arc-seal" };

		/// <summary>
		/// Initialize a new instance of the <see cref="ArcSigner"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ArcSigner"/>.
		/// </remarks>
		/// <param name="domain">The domain that the signer represents.</param>
		/// <param name="selector">The selector subdividing the domain.</param>
		/// <param name="algorithm">The signature algorithm.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="domain"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="selector"/> is <c>null</c>.</para>
		/// </exception>
		protected ArcSigner (string domain, string selector, DkimSignatureAlgorithm algorithm = DkimSignatureAlgorithm.RsaSha256) : base (domain, selector, algorithm)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="ArcSigner"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ArcSigner"/>.
		/// </remarks>
		/// <param name="key">The signer's private key.</param>
		/// <param name="domain">The domain that the signer represents.</param>
		/// <param name="selector">The selector subdividing the domain.</param>
		/// <param name="algorithm">The signature algorithm.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="key"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="domain"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="selector"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="key"/> is not a private key.
		/// </exception>
		protected ArcSigner (AsymmetricKeyParameter key, string domain, string selector, DkimSignatureAlgorithm algorithm = DkimSignatureAlgorithm.RsaSha256) : this (domain, selector, algorithm)
		{
			if (key == null)
				throw new ArgumentNullException (nameof (key));

			if (!key.IsPrivate)
				throw new ArgumentException ("The key must be a private key.", nameof (key));

			PrivateKey = key;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="ArcSigner"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ArcSigner"/>.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcSignerExample.cs" />
		/// </example>
		/// <param name="fileName">The file containing the private key.</param>
		/// <param name="domain">The domain that the signer represents.</param>
		/// <param name="selector">The selector subdividing the domain.</param>
		/// <param name="algorithm">The signature algorithm.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="domain"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="selector"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The file did not contain a private key.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		protected ArcSigner (string fileName, string domain, string selector, DkimSignatureAlgorithm algorithm = DkimSignatureAlgorithm.RsaSha256) : this (domain, selector, algorithm)
		{
			if (fileName == null)
				throw new ArgumentNullException (nameof (fileName));

			if (fileName.Length == 0)
				throw new ArgumentException ("The file name cannot be empty.", nameof (fileName));

			using (var stream = File.OpenRead (fileName))
				PrivateKey = LoadPrivateKey (stream);
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="ArcSigner"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ArcSigner"/>.
		/// </remarks>
		/// <param name="stream">The stream containing the private key.</param>
		/// <param name="domain">The domain that the signer represents.</param>
		/// <param name="selector">The selector subdividing the domain.</param>
		/// <param name="algorithm">The signature algorithm.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="domain"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="selector"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The file did not contain a private key.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		protected ArcSigner (Stream stream, string domain, string selector, DkimSignatureAlgorithm algorithm = DkimSignatureAlgorithm.RsaSha256) : this (domain, selector, algorithm)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			PrivateKey = LoadPrivateKey (stream);
		}

		/// <summary>
		/// Generate an ARC-Authentication-Results header.
		/// </summary>
		/// <remarks>
		/// <para>Generates an ARC-Authentication-Results header.</para>
		/// <para>If the returned <see cref="AuthenticationResults"/> contains a <see cref="AuthenticationMethodResult"/>
		/// with a <see cref="AuthenticationMethodResult.Method"/> equal to <c>"arc"</c>, then the
		/// <see cref="AuthenticationMethodResult.Result"/> will be used as the <c>cv=</c> tag value
		/// in the <c>ARC-Seal</c> header generated by the <see cref="ArcSigner"/>.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcSignerExample.cs" />
		/// </example>
		/// <param name="options">The format options.</param>
		/// <param name="message">The message to create the ARC-Authentication-Results header for.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The ARC-Authentication-Results header or <c>null</c> if the <see cref="ArcSigner"/> should not sign the message.</returns>
		protected abstract AuthenticationResults GenerateArcAuthenticationResults (FormatOptions options, MimeMessage message, CancellationToken cancellationToken);

		/// <summary>
		/// Asynchronously generate an ARC-Authentication-Results header.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously generates an ARC-Authentication-Results header.</para>
		/// <para>If the returned <see cref="AuthenticationResults"/> contains a <see cref="AuthenticationMethodResult"/>
		/// with a <see cref="AuthenticationMethodResult.Method"/> equal to <c>"arc"</c>, then the
		/// <see cref="AuthenticationMethodResult.Result"/> will be used as the <c>cv=</c> tag value
		/// in the <c>ARC-Seal</c> header generated by the <see cref="ArcSigner"/>.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcSignerExample.cs" />
		/// </example>
		/// <param name="options">The format options.</param>
		/// <param name="message">The message to create the ARC-Authentication-Results header for.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The ARC-Authentication-Results header or <c>null</c> if the <see cref="ArcSigner"/> should not sign the message.</returns>
		protected abstract Task<AuthenticationResults> GenerateArcAuthenticationResultsAsync (FormatOptions options, MimeMessage message, CancellationToken cancellationToken);

		/// <summary>
		/// Get the timestamp value.
		/// </summary>
		/// <remarks>
		/// Gets the timestamp to use as the <c>t=</c> value in the ARC-Message-Signature and ARC-Seal headers.
		/// </remarks>
		/// <returns>A value representing the timestamp value.</returns>
		protected virtual long GetTimestamp ()
		{
			return (long) (DateTime.UtcNow - DateUtils.UnixEpoch).TotalSeconds;
		}

		static void AppendInstanceAndSignatureAlgorithm (ref ValueStringBuilder value, int instance, DkimSignatureAlgorithm signatureAlgorithm)
		{
			value.Append ("i=");
			value.AppendInvariant (instance);

			switch (signatureAlgorithm) {
			case DkimSignatureAlgorithm.Ed25519Sha256:
				value.Append ("; a=ed25519-sha256");
				break;
			case DkimSignatureAlgorithm.RsaSha256:
				value.Append ("; a=rsa-sha256");
				break;
			default:
				value.Append ("; a=rsa-sha1");
				break;
			}
		}

		Header GenerateArcMessageSignature (FormatOptions options, MimeMessage message, int instance, long t, IList<string> headers)
		{
			var builder = new ValueStringBuilder (256);
			byte[] signature, hash;
			Header ams;

			AppendInstanceAndSignatureAlgorithm (ref builder, instance, SignatureAlgorithm);

			builder.Append ("; d=");
			builder.Append (Domain);
			builder.Append ("; s="); 
			builder.Append (Selector);
			builder.Append ("; c=");
			builder.Append (HeaderCanonicalizationAlgorithm.ToString ().ToLowerInvariant ());
			builder.Append ('/');
			builder.Append (BodyCanonicalizationAlgorithm.ToString ().ToLowerInvariant ());
			builder.Append ("; t=");
			builder.AppendInvariant (t);

			using (var stream = new DkimSignatureStream (CreateSigningContext ())) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (options.CreateNewLineFilter ());

					// write the specified message headers
					DkimVerifierBase.WriteHeaders (options, message, headers, HeaderCanonicalizationAlgorithm, filtered);

					builder.Append ("; h="); 
					builder.AppendJoin (':', headers);

					hash = message.HashBody (options, SignatureAlgorithm, BodyCanonicalizationAlgorithm, -1);
					builder.Append ("; bh="); 
					builder.Append (Convert.ToBase64String (hash));
					builder.Append ("; b=");

					ams = new Header (HeaderId.ArcMessageSignature, builder.ToString ());

					switch (HeaderCanonicalizationAlgorithm) {
					case DkimCanonicalizationAlgorithm.Relaxed:
						DkimVerifierBase.WriteHeaderRelaxed (options, filtered, ams, true);
						break;
					default:
						DkimVerifierBase.WriteHeaderSimple (options, filtered, ams, true);
						break;
					}

					filtered.Flush ();
				}

				signature = stream.GenerateSignature ();

				ams.Value += Convert.ToBase64String (signature);

				return ams;
			}
		}

		Header GenerateArcSeal (FormatOptions options, int instance, string cv, long t, ArcHeaderSet[] sets, int count, Header aar, Header ams)
		{
			var builder = new ValueStringBuilder (256);
			byte[] signature;
			Header seal;

			AppendInstanceAndSignatureAlgorithm (ref builder, instance, SignatureAlgorithm);

			builder.Append ("; cv=");
			builder.Append (cv);
			builder.Append ("; d=");
			builder.Append (Domain);
			builder.Append ("; s=");
			builder.Append (Selector);
			builder.Append ("; t=");
			builder.AppendInvariant (t);

			using (var stream = new DkimSignatureStream (CreateSigningContext ())) {
				using (var filtered = new FilteredStream (stream)) {
					filtered.Add (options.CreateNewLineFilter ());

					for (int i = 0; i < count; i++) {
						DkimVerifierBase.WriteHeaderRelaxed (options, filtered, sets[i].ArcAuthenticationResult, false);
						DkimVerifierBase.WriteHeaderRelaxed (options, filtered, sets[i].ArcMessageSignature, false);
						DkimVerifierBase.WriteHeaderRelaxed (options, filtered, sets[i].ArcSeal, false);
					}

					DkimVerifierBase.WriteHeaderRelaxed (options, filtered, aar, false);
					DkimVerifierBase.WriteHeaderRelaxed (options, filtered, ams, false);

					builder.Append ("; b=");

					seal = new Header (HeaderId.ArcSeal, builder.ToString ());
					DkimVerifierBase.WriteHeaderRelaxed (options, filtered, seal, true);

					filtered.Flush ();
				}

				signature = stream.GenerateSignature ();

				seal.Value += Convert.ToBase64String (signature);

				return seal;
			}
		}

		async Task ArcSignAsync (FormatOptions options, MimeMessage message, IList<string> headers, bool doAsync, CancellationToken cancellationToken)
		{
			ArcVerifier.GetArcHeaderSets (message, true, out ArcHeaderSet[] sets, out int count, out var errors);
			AuthenticationResults authres;
			int instance = count + 1;
			string cv;

			// do not sign if there is already a failed/invalid ARC-Seal.
			if (count > 0 && (errors & ArcValidationErrors.InvalidArcSealChainValidationValue) != 0)
				return;

			options = options.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.EnsureNewLine = true;

			if (doAsync)
				authres = await GenerateArcAuthenticationResultsAsync (options, message, cancellationToken).ConfigureAwait (false);
			else
				authres = GenerateArcAuthenticationResults (options, message, cancellationToken);

			if (authres == null)
				return;

			authres.Instance = instance;

			var aar = new Header (HeaderId.ArcAuthenticationResults, authres.ToString ());
			cv = "none";

			if (count > 0) {
				cv = "pass";

				foreach (var method in authres.Results) {
					if (method.Method.Equals ("arc", StringComparison.OrdinalIgnoreCase)) {
						cv = method.Result;
						break;
					}
				}
			}

			var t = GetTimestamp ();
			var ams = GenerateArcMessageSignature (options, message, instance, t, headers);
			var seal = GenerateArcSeal (options, instance, cv, t, sets, count, aar, ams);

			message.Headers.Insert (0, aar);
			message.Headers.Insert (0, ams);
			message.Headers.Insert (0, seal);
		}

		Task SignAsync (FormatOptions options, MimeMessage message, IList<string> headers, bool doAsync, CancellationToken cancellationToken)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (message == null)
				throw new ArgumentNullException (nameof (message));

			if (headers == null)
				throw new ArgumentNullException (nameof (headers));

			var fields = new string[headers.Count];
			var containsFrom = false;

			for (int i = 0; i < headers.Count; i++) {
				if (headers[i] == null)
					throw new ArgumentException ("The list of headers cannot contain null.", nameof (headers));

				if (headers[i].Length == 0)
					throw new ArgumentException ("The list of headers cannot contain empty string.", nameof (headers));

				fields[i] = headers[i].ToLowerInvariant ();

				if (ArcShouldNotInclude.Contains (fields[i]))
					throw new ArgumentException (string.Format ("The list of headers to sign SHOULD NOT include the '{0}' header.", headers[i]), nameof (headers));

				if (fields[i] == "from")
					containsFrom = true;
			}

			if (!containsFrom)
				throw new ArgumentException ("The list of headers to sign MUST include the 'From' header.", nameof (headers));

			return ArcSignAsync (options, message, fields, doAsync, cancellationToken);
		}

		Task SignAsync (FormatOptions options, MimeMessage message, IList<HeaderId> headers, bool doAsync, CancellationToken cancellationToken)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (message == null)
				throw new ArgumentNullException (nameof (message));

			if (headers == null)
				throw new ArgumentNullException (nameof (headers));

			var fields = new string[headers.Count];
			var containsFrom = false;

			for (int i = 0; i < headers.Count; i++) {
				if (headers[i] == HeaderId.Unknown)
					throw new ArgumentException ("The list of headers to sign cannot include the 'Unknown' header.", nameof (headers));

				fields[i] = headers[i].ToHeaderName ().ToLowerInvariant ();

				if (ArcShouldNotInclude.Contains (fields[i]))
					throw new ArgumentException (string.Format ("The list of headers to sign SHOULD NOT include the '{0}' header.", headers[i].ToHeaderName ()), nameof (headers));

				if (headers[i] == HeaderId.From)
					containsFrom = true;
			}

			if (!containsFrom)
				throw new ArgumentException ("The list of headers to sign MUST include the 'From' header.", nameof (headers));

			return ArcSignAsync (options, message, fields, doAsync, cancellationToken);
		}

		/// <summary>
		/// Digitally sign and seal a message using ARC.
		/// </summary>
		/// <remarks>
		/// Digitally signs and seals a message using ARC.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcSignerExample.cs" />
		/// </example>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to sign.</param>
		/// <param name="headers">The list of header fields to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="headers"/> does not contain the 'From' header.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> contains one or more of the following headers: Return-Path,
		/// Received, Comments, Keywords, Bcc, Resent-Bcc, or DKIM-Signature.</para>
		/// </exception>
		/// <exception cref="System.FormatException">
		/// One or more ARC headers either did not contain an instance tag or the instance tag was invalid.
		/// </exception>
		public void Sign (FormatOptions options, MimeMessage message, IList<string> headers, CancellationToken cancellationToken = default)
		{
			SignAsync (options, message, headers, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously digitally sign and seal a message using ARC.
		/// </summary>
		/// <remarks>
		/// Asynchronously digitally signs and seals a message using ARC.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcSignerExample.cs" />
		/// </example>
		/// <returns>An awaitable task.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to sign.</param>
		/// <param name="headers">The list of header fields to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="headers"/> does not contain the 'From' header.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> contains one or more of the following headers: Return-Path,
		/// Received, Comments, Keywords, Bcc, Resent-Bcc, or DKIM-Signature.</para>
		/// </exception>
		/// <exception cref="System.FormatException">
		/// One or more ARC headers either did not contain an instance tag or the instance tag was invalid.
		/// </exception>
		public Task SignAsync (FormatOptions options, MimeMessage message, IList<string> headers, CancellationToken cancellationToken = default)
		{
			return SignAsync (options, message, headers, true, cancellationToken);
		}

		/// <summary>
		/// Digitally sign and seal a message using ARC.
		/// </summary>
		/// <remarks>
		/// Digitally signs and seals a message using ARC.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcSignerExample.cs" />
		/// </example>
		/// <param name="message">The message to sign.</param>
		/// <param name="headers">The list of header fields to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="headers"/> does not contain the 'From' header.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> contains one or more of the following headers: Return-Path,
		/// Received, Comments, Keywords, Bcc, Resent-Bcc, or DKIM-Signature.</para>
		/// </exception>
		/// <exception cref="System.FormatException">
		/// One or more ARC headers either did not contain an instance tag or the instance tag was invalid.
		/// </exception>
		public void Sign (MimeMessage message, IList<string> headers, CancellationToken cancellationToken = default)
		{
			Sign (FormatOptions.Default, message, headers, cancellationToken);
		}

		/// <summary>
		/// Asynchronously digitally sign and seal a message using ARC.
		/// </summary>
		/// <remarks>
		/// Asynchronously digitally signs and seals a message using ARC.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcSignerExample.cs" />
		/// </example>
		/// <returns>An awaitable task.</returns>
		/// <param name="message">The message to sign.</param>
		/// <param name="headers">The list of header fields to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="headers"/> does not contain the 'From' header.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> contains one or more of the following headers: Return-Path,
		/// Received, Comments, Keywords, Bcc, Resent-Bcc, or DKIM-Signature.</para>
		/// </exception>
		/// <exception cref="System.FormatException">
		/// One or more ARC headers either did not contain an instance tag or the instance tag was invalid.
		/// </exception>
		public Task SignAsync (MimeMessage message, IList<string> headers, CancellationToken cancellationToken = default)
		{
			return SignAsync (FormatOptions.Default, message, headers, cancellationToken);
		}

		/// <summary>
		/// Digitally sign and seal a message using ARC.
		/// </summary>
		/// <remarks>
		/// Digitally signs and seals a message using ARC.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcSignerExample.cs" />
		/// </example>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to sign.</param>
		/// <param name="headers">The list of header fields to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="headers"/> does not contain the 'From' header.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> contains one or more of the following headers: Return-Path,
		/// Received, Comments, Keywords, Bcc, Resent-Bcc, or DKIM-Signature.</para>
		/// </exception>
		/// <exception cref="System.FormatException">
		/// One or more ARC headers either did not contain an instance tag or the instance tag was invalid.
		/// </exception>
		public void Sign (FormatOptions options, MimeMessage message, IList<HeaderId> headers, CancellationToken cancellationToken = default)
		{
			SignAsync (options, message, headers, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously digitally sign and seal a message using ARC.
		/// </summary>
		/// <remarks>
		/// Asynchronously digitally signs and seals a message using ARC.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcSignerExample.cs" />
		/// </example>
		/// <returns>An awaitable task.</returns>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to sign.</param>
		/// <param name="headers">The list of header fields to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="headers"/> does not contain the 'From' header.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> contains one or more of the following headers: Return-Path,
		/// Received, Comments, Keywords, Bcc, Resent-Bcc, or DKIM-Signature.</para>
		/// </exception>
		/// <exception cref="System.FormatException">
		/// One or more ARC headers either did not contain an instance tag or the instance tag was invalid.
		/// </exception>
		public Task SignAsync (FormatOptions options, MimeMessage message, IList<HeaderId> headers, CancellationToken cancellationToken = default)
		{
			return SignAsync (options, message, headers, true, cancellationToken);
		}

		/// <summary>
		/// Digitally sign and seal a message using ARC.
		/// </summary>
		/// <remarks>
		/// Digitally signs and seals a message using ARC.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcSignerExample.cs" />
		/// </example>
		/// <param name="message">The message to sign.</param>
		/// <param name="headers">The list of header fields to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="headers"/> does not contain the 'From' header.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> contains one or more of the following headers: Return-Path,
		/// Received, Comments, Keywords, Bcc, Resent-Bcc, or DKIM-Signature.</para>
		/// </exception>
		/// <exception cref="System.FormatException">
		/// One or more ARC headers either did not contain an instance tag or the instance tag was invalid.
		/// </exception>
		public void Sign (MimeMessage message, IList<HeaderId> headers, CancellationToken cancellationToken = default)
		{
			Sign (FormatOptions.Default, message, headers, cancellationToken);
		}

		/// <summary>
		/// Asynchronously digitally sign and seal a message using ARC.
		/// </summary>
		/// <remarks>
		/// Asynchronously digitally signs and seals a message using ARC.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\ArcSignerExample.cs" />
		/// </example>
		/// <returns>An awaitable task.</returns>
		/// <param name="message">The message to sign.</param>
		/// <param name="headers">The list of header fields to sign.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="message"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="headers"/> does not contain the 'From' header.</para>
		/// <para>-or-</para>
		/// <para><paramref name="headers"/> contains one or more of the following headers: Return-Path,
		/// Received, Comments, Keywords, Bcc, Resent-Bcc, or DKIM-Signature.</para>
		/// </exception>
		/// <exception cref="System.FormatException">
		/// One or more ARC headers either did not contain an instance tag or the instance tag was invalid.
		/// </exception>
		public Task SignAsync (MimeMessage message, IList<HeaderId> headers, CancellationToken cancellationToken = default)
		{
			return SignAsync (FormatOptions.Default, message, headers, cancellationToken);
		}
	}
}
