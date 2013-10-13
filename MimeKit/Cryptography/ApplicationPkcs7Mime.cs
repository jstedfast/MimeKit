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
			}
		}

		/// <summary>
		/// Decrypt using the specified <see cref="SecureMimeContext"/>.
		/// </summary>
		/// <param name="ctx">The S/MIME context.</param>
		/// <param name="recipients">The list of recipients that can decrypt this application/pkcs7-mime part.</param>
		/// <param name="signers">The list of signers that signed this application/pkcs7-mime part.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		public MimeEntity Decrypt (SecureMimeContext ctx, out RecipientInfoCollection recipients, out SignerInfoCollection signers)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			using (var memory = new MemoryStream ()) {
				ContentObject.WriteTo (memory);

				return ctx.Decrypt (memory.ToArray (), out recipients, out signers);
			}
		}

		/// <summary>
		/// Decrypt using the specified <see cref="SecureMimeContext"/>.
		/// </summary>
		/// <param name="ctx">The S/MIME context.</param>
		/// <param name="signers">The list of signers that signed this application/pkcs7-mime part.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		public MimeEntity Decrypt (SecureMimeContext ctx, out SignerInfoCollection signers)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			using (var memory = new MemoryStream ()) {
				RecipientInfoCollection recipients;

				ContentObject.WriteTo (memory);

				return ctx.Decrypt (memory.ToArray (), out recipients, out signers);
			}
		}

		/// <summary>
		/// Decrypt using the specified <see cref="SecureMimeContext"/>.
		/// </summary>
		/// <param name="ctx">The S/MIME context.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		public MimeEntity Decrypt (SecureMimeContext ctx)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			using (var memory = new MemoryStream ()) {
				RecipientInfoCollection recipients;
				SignerInfoCollection signers;

				ContentObject.WriteTo (memory);

				return ctx.Decrypt (memory.ToArray (), out recipients, out signers);
			}
		}

		/// <summary>
		/// Decrypt the content.
		/// </summary>
		public MimeEntity Decrypt ()
		{
			var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime");

			using (var memory = new MemoryStream ()) {
				RecipientInfoCollection recipients;
				SignerInfoCollection signers;

				ContentObject.WriteTo (memory);

				return ctx.Decrypt (memory.ToArray (), out recipients, out signers);
			}
		}

		/// <summary>
		/// Encrypt the specified entity.
		/// </summary>
		/// <param name="ctx">The context.</param>
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
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory);

				return ctx.Encrypt (recipients, memory.ToArray ());
			}
		}

		/// <summary>
		/// Encrypt the specified entity.
		/// </summary>
		/// <param name="ctx">The context.</param>
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

				entity.WriteTo (options, memory);

				return (ApplicationPkcs7Mime) ctx.Encrypt (recipients, memory.ToArray ());
			}
		}

		/// <summary>
		/// Sign and Encrypt the specified entity.
		/// </summary>
		/// <param name="ctx">The context.</param>
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
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory);

				return ctx.SignAndEncrypt (signer, recipients, memory.ToArray ());
			}
		}

		/// <summary>
		/// Sign and Encrypt the specified entity.
		/// </summary>
		/// <param name="ctx">The context.</param>
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
		public static ApplicationPkcs7Mime SignAndEncrypt (SecureMimeContext ctx, MailboxAddress signer, IEnumerable<MailboxAddress> recipients, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory);

				return (ApplicationPkcs7Mime) ctx.SignAndEncrypt (signer, recipients, memory.ToArray ());
			}
		}
	}
}
