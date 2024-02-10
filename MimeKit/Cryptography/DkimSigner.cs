//
// DkimSigner.cs
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
using System.Collections.Generic;

using Org.BouncyCastle.Crypto;

using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A DKIM signer.
	/// </summary>
	/// <remarks>
	/// A DKIM signer.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
	/// </example>
	public class DkimSigner : DkimSignerBase
	{
		static readonly string[] DkimShouldNotInclude = { "return-path", "received", "comments", "keywords", "bcc", "resent-bcc", "dkim-signature" };

		/// <summary>
		/// Initialize a new instance of the <see cref="DkimSigner"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="DkimSigner"/>.</para>
		/// <note type="security">Due to the recognized weakness of the SHA-1 hash algorithm
		/// and the wide availability of the SHA-256 hash algorithm (it has been a required
		/// part of DKIM since it was originally standardized in 2007), it is recommended
		/// that <see cref="DkimSignatureAlgorithm.RsaSha1"/> NOT be used.</note>
		/// </remarks>
		/// <param name="domain">The domain that the signer represents.</param>
		/// <param name="selector">The selector subdividing the domain.</param>
		/// <param name="algorithm">The signature algorithm.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="domain"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="selector"/> is <c>null</c>.</para>
		/// </exception>
		protected DkimSigner (string domain, string selector, DkimSignatureAlgorithm algorithm = DkimSignatureAlgorithm.RsaSha256) : base (domain, selector, algorithm)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="DkimSigner"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="DkimSigner"/>.</para>
		/// <note type="security">Due to the recognized weakness of the SHA-1 hash algorithm
		/// and the wide availability of the SHA-256 hash algorithm (it has been a required
		/// part of DKIM since it was originally standardized in 2007), it is recommended
		/// that <see cref="DkimSignatureAlgorithm.RsaSha1"/> NOT be used.</note>
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
		public DkimSigner (AsymmetricKeyParameter key, string domain, string selector, DkimSignatureAlgorithm algorithm = DkimSignatureAlgorithm.RsaSha256) : this (domain, selector, algorithm)
		{
			if (key == null)
				throw new ArgumentNullException (nameof (key));

			if (!key.IsPrivate)
				throw new ArgumentException ("The key must be a private key.", nameof (key));

			PrivateKey = key;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="DkimSigner"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="DkimSigner"/>.</para>
		/// <note type="security">Due to the recognized weakness of the SHA-1 hash algorithm
		/// and the wide availability of the SHA-256 hash algorithm (it has been a required
		/// part of DKIM since it was originally standardized in 2007), it is recommended
		/// that <see cref="DkimSignatureAlgorithm.RsaSha1"/> NOT be used.</note>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
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
		public DkimSigner (string fileName, string domain, string selector, DkimSignatureAlgorithm algorithm = DkimSignatureAlgorithm.RsaSha256) : this (domain, selector, algorithm)
		{
			if (fileName == null)
				throw new ArgumentNullException (nameof (fileName));

			if (fileName.Length == 0)
				throw new ArgumentException ("The file name cannot be empty.", nameof (fileName));

			using (var stream = File.OpenRead (fileName))
				PrivateKey = LoadPrivateKey (stream);
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="DkimSigner"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="DkimSigner"/>.</para>
		/// <note type="security">Due to the recognized weakness of the SHA-1 hash algorithm
		/// and the wide availability of the SHA-256 hash algorithm (it has been a required
		/// part of DKIM since it was originally standardized in 2007), it is recommended
		/// that <see cref="DkimSignatureAlgorithm.RsaSha1"/> NOT be used.</note>
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
		public DkimSigner (Stream stream, string domain, string selector, DkimSignatureAlgorithm algorithm = DkimSignatureAlgorithm.RsaSha256) : this (domain, selector, algorithm)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			PrivateKey = LoadPrivateKey (stream);
		}

		/// <summary>
		/// Get or set the agent or user identifier.
		/// </summary>
		/// <remarks>
		/// Gets or sets the agent or user identifier.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
		/// </example>
		/// <value>The agent or user identifier.</value>
		public string AgentOrUserIdentifier {
			get; set;
		}

		/// <summary>
		/// Get or set the public key query method.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the public key query method.</para>
		/// <para>The value should be a colon-separated list of query methods used to
		/// retrieve the public key (plain-text; OPTIONAL, default is "dns/txt"). Each
		/// query method is of the form "type[/options]", where the syntax and
		/// semantics of the options depend on the type and specified options.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
		/// </example>
		/// <value>The public key query method.</value>
		public string QueryMethod {
			get; set;
		}

		/// <summary>
		/// Get the timestamp value.
		/// </summary>
		/// <remarks>
		/// Gets the timestamp to use as the <c>t=</c> value in the DKIM-Signature header.
		/// </remarks>
		/// <returns>A value representing the timestamp value.</returns>
		protected virtual long GetTimestamp ()
		{
			return (long) (DateTime.UtcNow - DateUtils.UnixEpoch).TotalSeconds;
		}

		void DkimSign (FormatOptions options, MimeMessage message, IList<string> headers)
		{
			using var builder = new ValueStringBuilder (256);
			var t = GetTimestamp ();
			byte[] signature, hash;
			Header dkim;

			options = options.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.EnsureNewLine = true;

			builder.Append ("v=1");

			switch (SignatureAlgorithm) {
			case DkimSignatureAlgorithm.Ed25519Sha256:
				builder.Append ("; a=ed25519-sha256");
				break;
			case DkimSignatureAlgorithm.RsaSha256:
				builder.Append ("; a=rsa-sha256");
				break;
			default:
				builder.Append ("; a=rsa-sha1");
				break;
			}

			builder.Append ("; d=");
			builder.Append (Domain);
			builder.Append ("; s=");
			builder.Append (Selector);
			builder.Append ("; c=");
			builder.Append (HeaderCanonicalizationAlgorithm.ToString ().ToLowerInvariant ());
			builder.Append ('/');
			builder.Append (BodyCanonicalizationAlgorithm.ToString ().ToLowerInvariant ());

			if (!string.IsNullOrEmpty (QueryMethod)) {
				builder.Append ("; q=");
				builder.Append (QueryMethod);
			}
			if (!string.IsNullOrEmpty (AgentOrUserIdentifier)) {
				builder.Append ("; i=");
				builder.Append (AgentOrUserIdentifier);
			}

			builder.Append ("; t=");
			builder.AppendInvariant (t);

			if (SignaturesExpireAfter.HasValue) {
				var x = t + SignaturesExpireAfter.Value.TotalSeconds;
				builder.Append ("; x=");
				builder.AppendInvariant (x);
			}

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

					dkim = new Header (HeaderId.DkimSignature, builder.ToString ());
					message.Headers.Insert (0, dkim);

					switch (HeaderCanonicalizationAlgorithm) {
					case DkimCanonicalizationAlgorithm.Relaxed:
						DkimVerifierBase.WriteHeaderRelaxed (options, filtered, dkim, true);
						break;
					default:
						DkimVerifierBase.WriteHeaderSimple (options, filtered, dkim, true);
						break;
					}

					filtered.Flush ();
				}

				signature = stream.GenerateSignature ();

				dkim.Value += Convert.ToBase64String (signature);
			}
		}

		/// <summary>
		/// Digitally sign the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </summary>
		/// <remarks>
		/// Digitally signs the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
		/// </example>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to sign.</param>
		/// <param name="headers">The list of header fields to sign.</param>
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
		public void Sign (FormatOptions options, MimeMessage message, IList<string> headers)
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

				if (DkimShouldNotInclude.Contains (fields[i]))
					throw new ArgumentException (string.Format ("The list of headers to sign SHOULD NOT include the '{0}' header.", headers[i]), nameof (headers));

				if (fields[i] == "from")
					containsFrom = true;
			}

			if (!containsFrom)
				throw new ArgumentException ("The list of headers to sign MUST include the 'From' header.", nameof (headers));

			DkimSign (options, message, fields);
		}

		/// <summary>
		/// Digitally sign the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </summary>
		/// <remarks>
		/// Digitally signs the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
		/// </example>
		/// <param name="message">The message to sign.</param>
		/// <param name="headers">The headers to sign.</param>
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
		public void Sign (MimeMessage message, IList<string> headers)
		{
			Sign (FormatOptions.Default, message, headers);
		}

		/// <summary>
		/// Digitally sign the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </summary>
		/// <remarks>
		/// Digitally signs the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
		/// </example>
		/// <param name="options">The formatting options.</param>
		/// <param name="message">The message to sign.</param>
		/// <param name="headers">The list of header fields to sign.</param>
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
		public void Sign (FormatOptions options, MimeMessage message, IList<HeaderId> headers)
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

				if (DkimShouldNotInclude.Contains (fields[i]))
					throw new ArgumentException (string.Format ("The list of headers to sign SHOULD NOT include the '{0}' header.", headers[i].ToHeaderName ()), nameof (headers));

				if (headers[i] == HeaderId.From)
					containsFrom = true;
			}

			if (!containsFrom)
				throw new ArgumentException ("The list of headers to sign MUST include the 'From' header.", nameof (headers));

			DkimSign (options, message, fields);
		}

		/// <summary>
		/// Digitally sign the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </summary>
		/// <remarks>
		/// Digitally signs the message using a DomainKeys Identified Mail (DKIM) signature.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
		/// </example>
		/// <param name="message">The message to sign.</param>
		/// <param name="headers">The headers to sign.</param>
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
		public void Sign (MimeMessage message, IList<HeaderId> headers)
		{
			Sign (FormatOptions.Default, message, headers);
		}
	}
}
