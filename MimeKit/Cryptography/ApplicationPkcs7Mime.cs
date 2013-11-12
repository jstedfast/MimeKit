//
// ApplicationPkcs7Mime.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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
using System.Collections.Generic;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME part with a Content-Type of application/pkcs7-mime.
	/// </summary>
	public sealed class ApplicationPkcs7Mime : MimePart
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> class.
		/// </summary>
		/// <remarks>This constructor is used by <see cref="MimeKit.MimeParser"/>.</remarks>
		/// <param name="entity">Information used by the constructor.</param>
		public ApplicationPkcs7Mime (MimeEntityConstructorInfo entity) : base (entity)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> class.
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
			ContentObject = new ContentObject (stream, ContentEncoding.Default);
			ContentDisposition = new ContentDisposition ("attachment");
			ContentTransferEncoding = ContentEncoding.Base64;

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
				throw new ArgumentOutOfRangeException ("type");
			}
		}

		/// <summary>
		/// Gets the value of the "smime-type" parameter.
		/// </summary>
		/// <value>The value of the "smime-type" parameter.</value>
		public SecureMimeType SecureMimeType {
			get {
				var type = ContentType.Parameters["smime-type"];

				if (type == null)
					return SecureMimeType.Unknown;

				switch (type.ToLowerInvariant ()) {
				case "compressed-data": return SecureMimeType.CompressedData;
				case "enveloped-data": return SecureMimeType.EnvelopedData;
				case "signed-data": return SecureMimeType.SignedData;
				case "certs-only": return SecureMimeType.CertsOnly;
				default: return SecureMimeType.Unknown;
				}
			}
		}

		/// <summary>
		/// Decompress using the specified <see cref="SecureMimeContext"/>.
		/// </summary>
		/// <returns>The decompressed <see cref="MimeKit.MimeEntity"/>.</returns>
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
				throw new ArgumentNullException ("ctx");

			if (SecureMimeType != SecureMimeType.CompressedData)
				throw new InvalidOperationException ();

			using (var memory = new MemoryStream ()) {
				ContentObject.DecodeTo (memory);
				memory.Position = 0;

				return ctx.Decompress (memory);
			}
		}

		/// <summary>
		/// Decompress using the specified <see cref="SecureMimeContext"/>.
		/// </summary>
		/// <returns>The decompressed <see cref="MimeKit.MimeEntity"/>.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "compressed-data".
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Decompress ()
		{
			if (SecureMimeType != SecureMimeType.CompressedData)
				throw new InvalidOperationException ();

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime")) {
				return Decompress (ctx);
			}
		}

		/// <summary>
		/// Decrypt using the specified <see cref="CryptographyContext"/>.
		/// </summary>
		/// <returns>The decrypted <see cref="MimeKit.MimeEntity"/>.</returns>
		/// <param name="ctx">The S/MIME context to use for decrypting.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "enveloped-data".
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Decrypt (SecureMimeContext ctx)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (SecureMimeType != SecureMimeType.EnvelopedData)
				throw new InvalidOperationException ();

			using (var memory = new MemoryStream ()) {
				ContentObject.DecodeTo (memory);
				memory.Position = 0;

				return ctx.Decrypt (memory);
			}
		}

		/// <summary>
		/// Decrypt the content.
		/// </summary>
		/// <returns>The decrypted <see cref="MimeKit.MimeEntity"/>.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "certs-only".
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Decrypt ()
		{
			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime")) {
				return Decrypt (ctx);
			}
		}

		/// <summary>
		/// Import the certificates contained in the content.
		/// </summary>
		/// <param name="ctx">The S/MIME context to import certificates into.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "certs-only".
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public void Import (SecureMimeContext ctx)
		{
			if (SecureMimeType != SecureMimeType.CertsOnly)
				throw new InvalidOperationException ();

			using (var memory = new MemoryStream ()) {
				ContentObject.DecodeTo (memory);
				memory.Position = 0;

				ctx.Import (memory);
			}
		}

		/// <summary>
		/// Verify the signed-data and return the unencapsulated <see cref="MimeKit.MimeEntity"/>.
		/// </summary>
		/// <returns>The unencapsulated <see cref="MimeEntity"/>.</returns>
		/// <param name="ctx">The S/MIME context to use for verifying the signature.</param>
		/// <param name="signatures">The digital signatures.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "signed-data".
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Verify (SecureMimeContext ctx, out IList<IDigitalSignature> signatures)
		{
			if (SecureMimeType != SecureMimeType.SignedData)
				throw new InvalidOperationException ();

			using (var memory = new MemoryStream ()) {
				ContentObject.DecodeTo (memory);
				memory.Position = 0;

				return ctx.Verify (memory, out signatures);
			}
		}

		/// <summary>
		/// Verify the signed-data and return the unencapsulated <see cref="MimeKit.MimeEntity"/>.
		/// </summary>
		/// <returns>The unencapsulated <see cref="MimeEntity"/>.</returns>
		/// <param name="signatures">The digital signatures.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "signed-data".
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Verify (out IList<IDigitalSignature> signatures)
		{
			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime")) {
				return Verify (ctx, out signatures);
			}
		}

		static void PrepareEntity (MimeEntity entity)
		{
			if (entity is Multipart) {
				// Note: we do not want to modify multipart/signed parts
				if (entity is MultipartSigned)
					return;

				var multipart = (Multipart) entity;

				foreach (var subpart in multipart)
					PrepareEntity (subpart);
			} else if (entity is MessagePart) {
				var mpart = (MessagePart) entity;

				if (mpart.Message != null && mpart.Message.Body != null)
					PrepareEntity (mpart.Message.Body);
			} else {
				var part = (MimePart) entity;

				if (part.ContentTransferEncoding == ContentEncoding.Binary)
					part.ContentTransferEncoding = ContentEncoding.Base64;
				else if (part.ContentTransferEncoding != ContentEncoding.Base64)
					part.ContentTransferEncoding = ContentEncoding.QuotedPrintable;
			}
		}

		/// <summary>
		/// Compress the specified entity.
		/// </summary>
		/// <remarks>
		/// It should be noted that this feature is not supported by most mail clients,
		/// even among those that support S/MIME.
		/// </remarks>
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
				throw new ArgumentNullException ("ctx");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				PrepareEntity (entity);
				entity.WriteTo (options, memory);
				memory.Position = 0;

				return ctx.Compress (memory);
			}
		}

		/// <summary>
		/// Compress the specified entity.
		/// </summary>
		/// <remarks>
		/// It should be noted that this feature is not supported by most mail clients,
		/// even among those that support S/MIME.
		/// </remarks>
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
				throw new ArgumentNullException ("entity");

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime")) {
				return Compress (ctx, entity);
			}
		}

		/// <summary>
		/// Encrypt the specified entity.
		/// </summary>
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
				throw new ArgumentNullException ("ctx");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				PrepareEntity (entity);
				entity.WriteTo (options, memory);
				memory.Position = 0;

				return ctx.Encrypt (recipients, memory);
			}
		}

		/// <summary>
		/// Encrypt the specified entity.
		/// </summary>
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
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime")) {
				return Encrypt (ctx, recipients, entity);
			}
		}

		/// <summary>
		/// Encrypt the specified entity.
		/// </summary>
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
				throw new ArgumentNullException ("ctx");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				PrepareEntity (entity);
				entity.WriteTo (options, memory);
				memory.Position = 0;

				return (ApplicationPkcs7Mime) ctx.Encrypt (recipients, memory);
			}
		}

		/// <summary>
		/// Encrypt the specified entity.
		/// </summary>
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
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime")) {
				return Encrypt (ctx, recipients, entity);
			}
		}

		/// <summary>
		/// Sign the specified entity.
		/// </summary>
		/// <remarks>
		/// For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.
		/// </remarks>
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
				throw new ArgumentNullException ("ctx");

			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				PrepareEntity (entity);
				entity.WriteTo (options, memory);
				memory.Position = 0;

				return ctx.EncapsulatedSign (signer, memory);
			}
		}

		/// <summary>
		/// Sign the specified entity.
		/// </summary>
		/// <remarks>
		/// For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.
		/// </remarks>
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
				throw new ArgumentNullException ("signer");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime")) {
				return Sign (ctx, signer, entity);
			}
		}

		/// <summary>
		/// Sign the specified entity.
		/// </summary>
		/// <remarks>
		/// For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(CryptographyContext, MailboxAddress, DigestAlgorithm, MimeEntity)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.
		/// </remarks>
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
				throw new ArgumentNullException ("ctx");

			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				PrepareEntity (entity);
				entity.WriteTo (options, memory);
				memory.Position = 0;

				return ctx.EncapsulatedSign (signer, digestAlgo, memory);
			}
		}

		/// <summary>
		/// Sign the specified entity.
		/// </summary>
		/// <remarks>
		/// For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(CryptographyContext, MailboxAddress, DigestAlgorithm, MimeEntity)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.
		/// </remarks>
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
				throw new ArgumentNullException ("signer");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime")) {
				return Sign (ctx, signer, digestAlgo, entity);
			}
		}

		/// <summary>
		/// Sign and Encrypt the specified entity.
		/// </summary>
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
				throw new ArgumentNullException ("ctx");

			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			return Encrypt (ctx, recipients, MultipartSigned.Create (ctx, signer, entity));
		}

		/// <summary>
		/// Sign and Encrypt the specified entity.
		/// </summary>
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
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime")) {
				return SignAndEncrypt (ctx, signer, recipients, entity);
			}
		}

		/// <summary>
		/// Sign and Encrypt the specified entity.
		/// </summary>
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
				throw new ArgumentNullException ("ctx");

			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			return Encrypt (ctx, recipients, MultipartSigned.Create (ctx, signer, digestAlgo, entity));
		}

		/// <summary>
		/// Sign and Encrypt the specified entity.
		/// </summary>
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
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime")) {
				return SignAndEncrypt (ctx, signer, digestAlgo, recipients, entity);
			}
		}
	}
}
