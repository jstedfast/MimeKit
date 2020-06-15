//
// ApplicationPkcs7Mime.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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

using MimeKit.IO;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME part with a Content-Type of application/pkcs7-mime.
	/// </summary>
	/// <remarks>
	/// An application/pkcs7-mime is an S/MIME part and may contain encrypted,
	/// signed or compressed data (or any combination of the above).
	/// </remarks>
	public class ApplicationPkcs7Mime : MimePart
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="ApplicationPkcs7Mime"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public ApplicationPkcs7Mime (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="ApplicationPkcs7Mime"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new MIME part with a Content-Type of application/pkcs7-mime
		/// and the <paramref name="stream"/> as its content.</para>
		/// <para>Unless you are writing your own pkcs7 implementation, you'll probably
		/// want to use the <see cref="Compress(MimeEntity)"/>,
		/// <see cref="Encrypt(CmsRecipientCollection, MimeEntity)"/>, and/or
		/// <see cref="Sign(CmsSigner, MimeEntity)"/> method to create new instances
		/// of this class.</para>
		/// </remarks>
		/// <param name="type">The S/MIME type.</param>
		/// <param name="stream">The content stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="type"/> is not a valid value.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="stream"/> does not support reading.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> does not support seeking.</para>
		/// </exception>
		public ApplicationPkcs7Mime (SecureMimeType type, Stream stream) : base ("application", "pkcs7-mime")
		{
			ContentDisposition = new ContentDisposition (ContentDisposition.Attachment);
			ContentTransferEncoding = ContentEncoding.Base64;
			Content = new MimeContent (stream);

			switch (type) {
			case SecureMimeType.CompressedData:
				ContentType.Parameters["smime-type"] = "compressed-data";
				ContentDisposition.FileName = "smime.p7z";
				ContentType.Name = "smime.p7z";
				break;
			case SecureMimeType.EnvelopedData:
				ContentType.Parameters["smime-type"] = "enveloped-data";
				ContentDisposition.FileName = "smime.p7m";
				ContentType.Name = "smime.p7m";
				break;
			case SecureMimeType.SignedData:
				ContentType.Parameters["smime-type"] = "signed-data";
				ContentDisposition.FileName = "smime.p7m";
				ContentType.Name = "smime.p7m";
				break;
			case SecureMimeType.CertsOnly:
				ContentType.Parameters["smime-type"] = "certs-only";
				ContentDisposition.FileName = "smime.p7c";
				ContentType.Name = "smime.p7c";
				break;
			default:
				throw new ArgumentOutOfRangeException (nameof (type));
			}
		}

		/// <summary>
		/// Gets the value of the "smime-type" parameter.
		/// </summary>
		/// <remarks>
		/// Gets the value of the "smime-type" parameter.
		/// </remarks>
		/// <value>The value of the "smime-type" parameter.</value>
		public SecureMimeType SecureMimeType {
			get {
				var type = ContentType.Parameters["smime-type"];

				if (type == null)
					return SecureMimeType.Unknown;

				switch (type.ToLowerInvariant ()) {
				case "authenveloped-data": return SecureMimeType.AuthEnvelopedData;
				case "compressed-data": return SecureMimeType.CompressedData;
				case "enveloped-data": return SecureMimeType.EnvelopedData;
				case "signed-data": return SecureMimeType.SignedData;
				case "certs-only": return SecureMimeType.CertsOnly;
				default: return SecureMimeType.Unknown;
				}
			}
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="ApplicationPkcs7Mime"/> nodes
		/// calls <see cref="MimeVisitor.VisitApplicationPkcs7Mime"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitApplicationPkcs7Mime"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor == null)
				throw new ArgumentNullException (nameof (visitor));

			visitor.VisitApplicationPkcs7Mime (this);
		}

		/// <summary>
		/// Decompress the compressed-data.
		/// </summary>
		/// <remarks>
		/// Decompresses the compressed-data using the specified <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decompressed <see cref="MimeEntity"/>.</returns>
		/// <param name="ctx">The S/MIME context to use for decompressing.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "compressed-data".
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Decompress (SecureMimeContext ctx)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (SecureMimeType != SecureMimeType.CompressedData && SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				Content.DecodeTo (memory);
				memory.Position = 0;

				return ctx.Decompress (memory);
			}
		}

		/// <summary>
		/// Decompress the compressed-data.
		/// </summary>
		/// <remarks>
		/// Decompresses the compressed-data using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decompressed <see cref="MimeEntity"/>.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "compressed-data".
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Decompress ()
		{
			if (SecureMimeType != SecureMimeType.CompressedData && SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Decompress (ctx);
		}

		/// <summary>
		/// Decrypt the enveloped-data.
		/// </summary>
		/// <remarks>
		/// Decrypts the enveloped-data using the specified <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeEntity"/>.</returns>
		/// <param name="ctx">The S/MIME context to use for decrypting.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "enveloped-data".
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Decrypt (SecureMimeContext ctx, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (SecureMimeType != SecureMimeType.EnvelopedData && SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				Content.DecodeTo (memory);
				memory.Position = 0;

				return ctx.Decrypt (memory, cancellationToken);
			}
		}

		/// <summary>
		/// Decrypt the enveloped-data.
		/// </summary>
		/// <remarks>
		/// Decrypts the enveloped-data using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeEntity"/>.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "certs-only".
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Decrypt (CancellationToken cancellationToken = default (CancellationToken))
		{
			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Decrypt (ctx, cancellationToken);
		}

		/// <summary>
		/// Import the certificates contained in the application/pkcs7-mime content.
		/// </summary>
		/// <remarks>
		/// Imports the certificates contained in the application/pkcs7-mime content.
		/// </remarks>
		/// <param name="ctx">The S/MIME context to import certificates into.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "certs-only".
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public void Import (SecureMimeContext ctx)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (SecureMimeType != SecureMimeType.CertsOnly && SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				Content.DecodeTo (memory);
				memory.Position = 0;

				ctx.Import (memory);
			}
		}

		/// <summary>
		/// Verify the signed-data and return the unencapsulated <see cref="MimeEntity"/>.
		/// </summary>
		/// <remarks>
		/// Verifies the signed-data and returns the unencapsulated <see cref="MimeEntity"/>.
		/// </remarks>
		/// <returns>The list of digital signatures.</returns>
		/// <param name="ctx">The S/MIME context to use for verifying the signature.</param>
		/// <param name="entity">The unencapsulated entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "signed-data".
		/// </exception>
		/// <exception cref="System.FormatException">
		/// The extracted content could not be parsed as a MIME entity.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public DigitalSignatureCollection Verify (SecureMimeContext ctx, out MimeEntity entity, CancellationToken cancellationToken = default (CancellationToken))
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (SecureMimeType != SecureMimeType.SignedData && SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				Content.DecodeTo (memory);
				memory.Position = 0;

				return ctx.Verify (memory, out entity, cancellationToken);
			}
		}

		/// <summary>
		/// Verifies the signed-data and returns the unencapsulated <see cref="MimeEntity"/>.
		/// </summary>
		/// <remarks>
		/// Verifies the signed-data using the default <see cref="SecureMimeContext"/> and returns the
		/// unencapsulated <see cref="MimeEntity"/>.
		/// </remarks>
		/// <returns>The list of digital signatures.</returns>
		/// <param name="entity">The unencapsulated entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "signed-data".
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was cancelled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public DigitalSignatureCollection Verify (out MimeEntity entity, CancellationToken cancellationToken = default (CancellationToken))
		{
			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Verify (ctx, out entity, cancellationToken);
		}

		/// <summary>
		/// Compresses the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Compresses the specified entity using the specified <see cref="SecureMimeContext"/>.</para>
		/// <note type="warning">Most mail clients, even among those that support S/MIME, do not support compression.</note>
		/// </remarks>
		/// <returns>The compressed entity.</returns>
		/// <param name="ctx">The S/MIME context to use for compressing.</param>
		/// <param name="entity">The entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Compress (SecureMimeContext ctx, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.CloneDefault ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory);
				memory.Position = 0;

				return ctx.Compress (memory);
			}
		}

		/// <summary>
		/// Compresses the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Compresses the specified entity using the default <see cref="SecureMimeContext"/>.</para>
		/// <note type="warning">Most mail clients, even among those that support S/MIME, do not support compression.</note>
		/// </remarks>
		/// <returns>The compressed entity.</returns>
		/// <param name="entity">The entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="entity"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Compress (MimeEntity entity)
		{
			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Compress (ctx, entity);
		}

		/// <summary>
		/// Encrypts the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients using the supplied <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for encrypting.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Encrypt (SecureMimeContext ctx, CmsRecipientCollection recipients, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.CloneDefault ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory);
				memory.Position = 0;

				return ctx.Encrypt (recipients, memory);
			}
		}

		/// <summary>
		/// Encrypts the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Encrypt (CmsRecipientCollection recipients, MimeEntity entity)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Encrypt (ctx, recipients, entity);
		}

		/// <summary>
		/// Encrypts the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients using the supplied <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for encrypting.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// Valid certificates could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Encrypt (SecureMimeContext ctx, IEnumerable<MailboxAddress> recipients, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.CloneDefault ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory);
				memory.Position = 0;

				return (ApplicationPkcs7Mime) ctx.Encrypt (recipients, memory);
			}
		}

		/// <summary>
		/// Encrypts the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// Valid certificates could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Encrypt (IEnumerable<MailboxAddress> recipients, MimeEntity entity)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Encrypt (ctx, recipients, entity);
		}

		/// <summary>
		/// Cryptographically signs the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Signs the entity using the supplied signer and <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity.</param>
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
		public static ApplicationPkcs7Mime Sign (SecureMimeContext ctx, CmsSigner signer, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.CloneDefault ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory);
				memory.Position = 0;

				return ctx.EncapsulatedSign (signer, memory);
			}
		}

		/// <summary>
		/// Cryptographically signs the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Signs the entity using the supplied signer and the default <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Sign (CmsSigner signer, MimeEntity entity)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Sign (ctx, signer, entity);
		}

		/// <summary>
		/// Cryptographically signs the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Signs the entity using the supplied signer, digest algorithm and <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Sign (SecureMimeContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.CloneDefault ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory);
				memory.Position = 0;

				return ctx.EncapsulatedSign (signer, digestAlgo, memory);
			}
		}

		/// <summary>
		/// Cryptographically signs the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Signs the entity using the supplied signer, digest algorithm and the default
		/// <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Sign (MailboxAddress signer, DigestAlgorithm digestAlgo, MimeEntity entity)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Sign (ctx, signer, digestAlgo, entity);
		}

		/// <summary>
		/// Cryptographically signs and encrypts the specified entity.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs entity using the supplied signer and then
		/// encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing and encrypting.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime SignAndEncrypt (SecureMimeContext ctx, CmsSigner signer, CmsRecipientCollection recipients, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			return Encrypt (ctx, recipients, MultipartSigned.Create (ctx, signer, entity));
		}

		/// <summary>
		/// Cryptographically signs and encrypts the specified entity.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs entity using the supplied signer and the default <see cref="SecureMimeContext"/>
		/// and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime SignAndEncrypt (CmsSigner signer, CmsRecipientCollection recipients, MimeEntity entity)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return SignAndEncrypt (ctx, signer, recipients, entity);
		}

		/// <summary>
		/// Cryptographically signs and encrypts the specified entity.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs entity using the supplied signer and then
		/// encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing and encrypting.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// <para>A signing certificate could not be found for <paramref name="signer"/>.</para>
		/// <para>-or-</para>
		/// <para>A certificate could not be found for one or more of the <paramref name="recipients"/>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime SignAndEncrypt (SecureMimeContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			return Encrypt (ctx, recipients, MultipartSigned.Create (ctx, signer, digestAlgo, entity));
		}

		/// <summary>
		/// Cryptographically signs and encrypts the specified entity.
		/// </summary>
		/// <remarks>
		/// Cryptographically signs entity using the supplied signer and the default <see cref="SecureMimeContext"/>
		/// and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// <para>A signing certificate could not be found for <paramref name="signer"/>.</para>
		/// <para>-or-</para>
		/// <para>A certificate could not be found for one or more of the <paramref name="recipients"/>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime SignAndEncrypt (MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return SignAndEncrypt (ctx, signer, digestAlgo, recipients, entity);
		}
	}
}
