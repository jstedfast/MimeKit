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
using System.Security.Cryptography.Pkcs;

// FIXME: Implement a Verify() method for parts tagged with signed-data?
// FIXME: Implement a Decompress() method for parts tagged with compressed-data?

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME part with a Content-Type of application/pkcs7-mime.
	/// </summary>
	public class ApplicationPkcs7Mime : MimePart
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> class.
		/// </summary>
		/// <param name="entity">Information used by the constructor.</param>
		public ApplicationPkcs7Mime (MimeEntityConstructorInfo entity) : base (entity)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.ApplicationPkcs7Mime"/> class.
		/// </summary>
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
		/// Decrypt using the specified <see cref="CryptographyContext"/>.
		/// </summary>
		/// <returns>The decrypted <see cref="MimeKit.MimeEntity"/>.</returns>
		/// <param name="ctx">The S/MIME context to use for decrypting.</param>
		/// <param name="signatures">The list of digital signatures for this application/pkcs7-mime part.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header does not support decryption.
		/// </exception>
		public MimeEntity Decrypt (SecureMimeContext ctx, out IList<IDigitalSignature> signatures)
		{
			// FIXME: find out what exceptions BouncyCastle can throw...
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (SecureMimeType != SecureMimeType.EnvelopedData)
				throw new InvalidOperationException ();

			using (var memory = new MemoryStream ()) {
				ContentObject.DecodeTo (memory);
				memory.Position = 0;

				return ctx.Decrypt (memory, out signatures);
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
		/// The "smime-type" parameter on the Content-Type header does not support decryption.
		/// </exception>
		public MimeEntity Decrypt (SecureMimeContext ctx)
		{
			// FIXME: find out what exceptions BouncyCastle can throw...
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (SecureMimeType != SecureMimeType.EnvelopedData)
				throw new InvalidOperationException ();

			IList<IDigitalSignature> signatures;

			return Decrypt (ctx, out signatures);
		}

		/// <summary>
		/// Decrypt the content.
		/// </summary>
		/// <returns>The decrypted <see cref="MimeKit.MimeEntity"/>.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header does not support decryption.
		/// </exception>
		public MimeEntity Decrypt ()
		{
			// FIXME: find out what exceptions BouncyCastle can throw...
			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime")) {
				return Decrypt (ctx);
			}
		}

		/// <summary>
		/// Import the certificates contained in the content.
		/// </summary>
		/// <param name="ctx">The S/MIME context to import certificates into.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header does not support decryption.
		/// </exception>
		public void Import (SecureMimeContext ctx)
		{
			// FIXME: find out what exceptions BouncyCastle can throw...
			if (SecureMimeType != SecureMimeType.CertsOnly)
				throw new InvalidOperationException ();

			using (var memory = new MemoryStream ()) {
				ContentObject.WriteTo (memory);
				memory.Position = 0;

				ctx.Import (memory);
			}
		}

		static void PrepareEntityForEncrypting (MimeEntity entity)
		{
			if (entity is Multipart) {
				// Note: we do not want to modify multipart/signed parts
				if (entity is MultipartSigned)
					return;

				var multipart = (Multipart) entity;

				foreach (var subpart in multipart)
					PrepareEntityForEncrypting (subpart);
			} else if (entity is MessagePart) {
				var mpart = (MessagePart) entity;

				if (mpart.Message != null && mpart.Message.Body != null)
					PrepareEntityForEncrypting (mpart.Message.Body);
			} else {
				var part = (MimePart) entity;

				if (part.ContentTransferEncoding == ContentEncoding.Binary)
					part.ContentTransferEncoding = ContentEncoding.Base64;
				else if (part.ContentTransferEncoding != ContentEncoding.Base64)
					part.ContentTransferEncoding = ContentEncoding.QuotedPrintable;
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
		/// <para><paramref name="recipients"/> is<c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is<c>null</c>.</para>
		/// </exception>
		public static ApplicationPkcs7Mime Encrypt (SecureMimeContext ctx, CmsRecipientCollection recipients, MimeEntity entity)
		{
			// FIXME: find out what exceptions BouncyCastle can throw...
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				PrepareEntityForEncrypting (entity);
				entity.WriteTo (options, memory);
				memory.Position = 0;

				return ctx.Encrypt (recipients, memory);
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
		/// <para><paramref name="recipients"/> is<c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is<c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// Valid certificates could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		public static ApplicationPkcs7Mime Encrypt (SecureMimeContext ctx, IEnumerable<MailboxAddress> recipients, MimeEntity entity)
		{
			// FIXME: find out what exceptions BouncyCastle can throw...
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				PrepareEntityForEncrypting (entity);
				entity.WriteTo (options, memory);
				memory.Position = 0;

				return (ApplicationPkcs7Mime) ctx.Encrypt (recipients, memory);
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
		/// <para><paramref name="signer"/> is<c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is<c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is<c>null</c>.</para>
		/// </exception>
		public static ApplicationPkcs7Mime SignAndEncrypt (SecureMimeContext ctx, CmsSigner signer, CmsRecipientCollection recipients, MimeEntity entity)
		{
			// FIXME: find out what exceptions BouncyCastle can throw...
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
		/// <param name="ctx">The S/MIME context to use for signing and encrypting.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is<c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is<c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is<c>null</c>.</para>
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// <para>A signing certificate could not be found for <paramref name="signer"/>.</para>
		/// <para>-or-</para>
		/// <para>A certificate could not be found for one or more of the <paramref name="recipients"/>.</para>
		/// </exception>
		public static ApplicationPkcs7Mime SignAndEncrypt (SecureMimeContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity)
		{
			// FIXME: find out what exceptions BouncyCastle can throw...
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
	}
}
