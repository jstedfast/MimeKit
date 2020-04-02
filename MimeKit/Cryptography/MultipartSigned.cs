﻿//
// MultipartSigned.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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
using System.Threading.Tasks;

using Org.BouncyCastle.Bcpg.OpenPgp;

using MimeKit.IO;
using MimeKit.IO.Filters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A signed multipart, as used by both S/MIME and PGP/MIME protocols.
	/// </summary>
	/// <remarks>
	/// The first child of a multipart/signed is the content while the second child
	/// is the detached signature data. Any other children are not defined and could
	/// be anything.
	/// </remarks>
	public class MultipartSigned : Multipart
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.MultipartSigned"/> class.
		/// </summary>
		/// <remarks>This constructor is used by <see cref="MimeKit.MimeParser"/>.</remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public MultipartSigned (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.MultipartSigned"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MultipartSigned"/>.
		/// </remarks>
		public MultipartSigned () : base ("signed")
		{
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="MimeKit.Cryptography.MultipartSigned"/> nodes
		/// calls <see cref="MimeKit.MimeVisitor.VisitMultipartSigned"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeKit.MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeKit.MimeVisitor.VisitMultipartSigned"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor == null)
				throw new ArgumentNullException (nameof (visitor));

			visitor.VisitMultipartSigned (this);
		}

		static MimeEntity Prepare (MimeEntity entity, Stream memory)
		{
			entity.Prepare (EncodingConstraint.SevenBit, 78);

			using (var filtered = new FilteredStream (memory)) {
				// Note: see rfc3156, section 3 - second note
				filtered.Add (new ArmoredFromFilter ());

				// Note: see rfc3156, section 5.4 (this is the main difference between rfc2015 and rfc3156)
				filtered.Add (new TrailingWhitespaceFilter ());

				// Note: see rfc2015 or rfc3156, section 5.1
				filtered.Add (new Unix2DosFilter ());

				entity.WriteTo (filtered);
				filtered.Flush ();
			}

			memory.Position = 0;

			// Note: we need to parse the modified entity structure to preserve any modifications
			var parser = new MimeParser (memory, MimeFormat.Entity);

			return parser.ParseEntity ();
		}

		static MultipartSigned Create (CryptographyContext ctx, DigestAlgorithm digestAlgo, MimeEntity entity, MimeEntity signature)
		{
			var micalg = ctx.GetDigestAlgorithmName (digestAlgo);
			var signed = new MultipartSigned ();

			// set the protocol and micalg Content-Type parameters
			signed.ContentType.Parameters["protocol"] = ctx.SignatureProtocol;
			signed.ContentType.Parameters["micalg"] = micalg;

			// add the modified/parsed entity as our first part
			signed.Add (entity);

			// add the detached signature as the second part
			signed.Add (signature);

			return signed;
		}

		/// <summary>
		/// Creates a new <see cref="MultipartSigned"/>.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the entity using the supplied signer and digest algorithm in
		/// order to generate a detached signature and then adds the entity along with the
		/// detached signature data to a new multipart/signed part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartSigned"/> instance.</returns>
		/// <param name="ctx">The cryptography context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity to sign.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="digestAlgo"/> is not supported.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static MultipartSigned Create (CryptographyContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var prepared = Prepare (entity, memory);

				memory.Position = 0;

				// sign the cleartext content
				var signature = ctx.Sign (signer, digestAlgo, memory);

				return Create (ctx, digestAlgo, prepared, signature);
			}
		}

		/// <summary>
		/// Creates a new <see cref="MultipartSigned"/>.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the entity using the supplied signer and digest algorithm in
		/// order to generate a detached signature and then adds the entity along with the
		/// detached signature data to a new multipart/signed part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartSigned"/> instance.</returns>
		/// <param name="ctx">The OpenPGP context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity to sign.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="signer"/> cannot be used for signing.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="digestAlgo"/> is not supported.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Bcpg.OpenPgp.PgpException">
		/// An error occurred in the OpenPGP subsystem.
		/// </exception>
		public static MultipartSigned Create (OpenPgpContext ctx, PgpSecretKey signer, DigestAlgorithm digestAlgo, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var prepared = Prepare (entity, memory);

				memory.Position = 0;

				// sign the cleartext content
				var signature = ctx.Sign (signer, digestAlgo, memory);

				return Create (ctx, digestAlgo, prepared, signature);
			}
		}

		/// <summary>
		/// Creates a new <see cref="MultipartSigned"/>.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the entity using the supplied signer and digest algorithm in
		/// order to generate a detached signature and then adds the entity along with the
		/// detached signature data to a new multipart/signed part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartSigned"/> instance.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity to sign.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="signer"/> cannot be used for signing.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A cryptography context suitable for signing could not be found.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Bcpg.OpenPgp.PgpException">
		/// An error occurred in the OpenPGP subsystem.
		/// </exception>
		public static MultipartSigned Create (PgpSecretKey signer, DigestAlgorithm digestAlgo, MimeEntity entity)
		{
			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-signature"))
				return Create (ctx, signer, digestAlgo, entity);
		}

		/// <summary>
		/// Creates a new <see cref="MultipartSigned"/>.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the entity using the supplied signer in order
		/// to generate a detached signature and then adds the entity along with
		/// the detached signature data to a new multipart/signed part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartSigned"/> instance.</returns>
		/// <param name="ctx">The S/MIME context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity to sign.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static MultipartSigned Create (SecureMimeContext ctx, CmsSigner signer, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var prepared = Prepare (entity, memory);

				memory.Position = 0;

				// sign the cleartext content
				var signature = ctx.Sign (signer, memory);

				return Create (ctx, signer.DigestAlgorithm, prepared, signature);
			}
		}

		/// <summary>
		/// Creates a new <see cref="MultipartSigned"/>.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs the entity using the supplied signer in order
		/// to generate a detached signature and then adds the entity along with
		/// the detached signature data to a new multipart/signed part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartSigned"/> instance.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity to sign.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A cryptography context suitable for signing could not be found.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static MultipartSigned Create (CmsSigner signer, MimeEntity entity)
		{
			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-signature"))
				return Create (ctx, signer, entity);
		}

		/// <summary>
		/// Prepare the MIME entity for transport using the specified encoding constraints.
		/// </summary>
		/// <remarks>
		/// Prepares the MIME entity for transport using the specified encoding constraints.
		/// </remarks>
		/// <param name="constraint">The encoding constraint.</param>
		/// <param name="maxLineLength">The maximum number of octets allowed per line (not counting the CRLF). Must be between <c>60</c> and <c>998</c> (inclusive).</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="maxLineLength"/> is not between <c>60</c> and <c>998</c> (inclusive).</para>
		/// <para>-or-</para>
		/// <para><paramref name="constraint"/> is not a valid value.</para>
		/// </exception>
		public override void Prepare (EncodingConstraint constraint, int maxLineLength = 78)
		{
			if (maxLineLength < FormatOptions.MinimumLineLength || maxLineLength > FormatOptions.MaximumLineLength)
				throw new ArgumentOutOfRangeException (nameof (maxLineLength));

			// Note: we do not iterate over our children because they are already signed
			// and changing them would break the signature. They should already be
			// properly prepared, anyway.
		}

		/// <summary>
		/// Verify the multipart/signed part.
		/// </summary>
		/// <remarks>
		/// Verifies the multipart/signed part using the supplied cryptography context.
		/// </remarks>
		/// <returns>A signer info collection.</returns>
		/// <param name="ctx">The cryptography context to use for verifying the signature.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The multipart is malformed in some way.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="ctx"/> does not support verifying the signature part.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public DigitalSignatureCollection Verify (CryptographyContext ctx, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			var protocol = ContentType.Parameters["protocol"]?.Trim ();
			if (string.IsNullOrEmpty (protocol))
				throw new FormatException ("The multipart/signed part did not specify a protocol.");

			if (!ctx.Supports (protocol))
				throw new NotSupportedException ("The specified cryptography context does not support the signature protocol.");

			if (Count < 2)
				throw new FormatException ("The multipart/signed part did not contain the expected children.");

			var signature = this[1] as MimePart;
			if (signature == null || signature.Content == null)
				throw new FormatException ("The signature part could not be found.");

			var ctype = signature.ContentType;
			var value = string.Format ("{0}/{1}", ctype.MediaType, ctype.MediaSubtype);
			if (!ctx.Supports (value))
				throw new NotSupportedException (string.Format ("The specified cryptography context does not support '{0}'.", value));

			using (var signatureData = new MemoryBlockStream ()) {
				signature.Content.DecodeTo (signatureData, cancellationToken);
				signatureData.Position = 0;

				using (var cleartext = new MemoryBlockStream ()) {
					// Note: see rfc2015 or rfc3156, section 5.1
					var options = FormatOptions.CloneDefault ();
					options.NewLineFormat = NewLineFormat.Dos;

					this[0].WriteTo (options, cleartext);
					cleartext.Position = 0;

					return ctx.Verify (cleartext, signatureData, cancellationToken);
				}
			}
		}

		/// <summary>
		/// Verify the multipart/signed part.
		/// </summary>
		/// <remarks>
		/// Verifies the multipart/signed part using the supplied cryptography context.
		/// </remarks>
		/// <returns>A signer info collection.</returns>
		/// <param name="ctx">The cryptography context to use for verifying the signature.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The multipart is malformed in some way.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <paramref name="ctx"/> does not support verifying the signature part.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public async Task<DigitalSignatureCollection> VerifyAsync (CryptographyContext ctx, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			var protocol = ContentType.Parameters["protocol"]?.Trim ();
			if (string.IsNullOrEmpty (protocol))
				throw new FormatException ("The multipart/signed part did not specify a protocol.");

			if (!ctx.Supports (protocol))
				throw new NotSupportedException ("The specified cryptography context does not support the signature protocol.");

			if (Count < 2)
				throw new FormatException ("The multipart/signed part did not contain the expected children.");

			var signature = this[1] as MimePart;
			if (signature == null || signature.Content == null)
				throw new FormatException ("The signature part could not be found.");

			var ctype = signature.ContentType;
			var value = string.Format ("{0}/{1}", ctype.MediaType, ctype.MediaSubtype);
			if (!ctx.Supports (value))
				throw new NotSupportedException (string.Format ("The specified cryptography context does not support '{0}'.", value));

			using (var signatureData = new MemoryBlockStream ()) {
				await signature.Content.DecodeToAsync (signatureData, cancellationToken).ConfigureAwait (false);
				signatureData.Position = 0;

				using (var cleartext = new MemoryBlockStream ()) {
					// Note: see rfc2015 or rfc3156, section 5.1
					var options = FormatOptions.CloneDefault ();
					options.NewLineFormat = NewLineFormat.Dos;

					await this[0].WriteToAsync (options, cleartext, cancellationToken);
					cleartext.Position = 0;

					return await ctx.VerifyAsync (cleartext, signatureData, cancellationToken).ConfigureAwait (false);
				}
			}
		}

		/// <summary>
		/// Verify the multipart/signed part.
		/// </summary>
		/// <remarks>
		/// Verifies the multipart/signed part using the default cryptography context.
		/// </remarks>
		/// <returns>A signer info collection.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A cryptography context suitable for verifying the signature could not be found.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public DigitalSignatureCollection Verify (CancellationToken cancellationToken = default (CancellationToken))
		{
			var protocol = ContentType.Parameters["protocol"]?.Trim ();

			if (string.IsNullOrEmpty (protocol))
				throw new FormatException ("The multipart/signed part did not specify a protocol.");

			using (var ctx = CryptographyContext.Create (protocol))
				return Verify (ctx, cancellationToken);
		}

		/// <summary>
		/// Asynchronously verify the multipart/signed part.
		/// </summary>
		/// <remarks>
		/// Verifies the multipart/signed part using the default cryptography context.
		/// </remarks>
		/// <returns>A signer info collection.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A cryptography context suitable for verifying the signature could not be found.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public Task<DigitalSignatureCollection> VerifyAsync (CancellationToken cancellationToken = default (CancellationToken))
		{
			var protocol = ContentType.Parameters["protocol"]?.Trim ();

			if (string.IsNullOrEmpty (protocol))
				throw new FormatException ("The multipart/signed part did not specify a protocol.");

			using (var ctx = CryptographyContext.Create (protocol))
				return VerifyAsync (ctx, cancellationToken);
		}
	}
}
