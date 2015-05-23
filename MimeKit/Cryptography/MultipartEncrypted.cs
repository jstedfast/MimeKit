//
// MultipartEncrypted.cs
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
using System.IO;
using System.Collections.Generic;

using Org.BouncyCastle.Bcpg.OpenPgp;

using MimeKit.IO;
using MimeKit.IO.Filters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A multipart MIME part with a ContentType of multipart/encrypted containing an encrypted MIME part.
	/// </summary>
	/// <remarks>
	/// This mime-type is common when dealing with PGP/MIME but is not used for S/MIME.
	/// </remarks>
	public class MultipartEncrypted : Multipart
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.MultipartEncrypted"/> class.
		/// </summary>
		/// <remarks>This constructor is used by <see cref="MimeKit.MimeParser"/>.</remarks>
		/// <param name="entity">Information used by the constructor.</param>
		public MultipartEncrypted (MimeEntityConstructorInfo entity) : base (entity)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.MultipartEncrypted"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MultipartEncrypted"/>.
		/// </remarks>
		public MultipartEncrypted () : base ("encrypted")
		{
		}

		/// <summary>
		/// Creates a new <see cref="MultipartEncrypted"/>.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for signing and encrypting.</param>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key for <paramref name="signer"/> could not be found.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static MultipartEncrypted Create (OpenPgpContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.GetDefault ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory);
				memory.Position = 0;

				var encrypted = new MultipartEncrypted ();
				encrypted.ContentType.Parameters["protocol"] = ctx.EncryptionProtocol;

				// add the protocol version part
				encrypted.Add (new ApplicationPgpEncrypted ());

				// add the encrypted entity as the second part
				encrypted.Add (ctx.SignAndEncrypt (signer, digestAlgo, recipients, memory));

				return encrypted;
			}
		}

		/// <summary>
		/// Creates a new <see cref="MultipartEncrypted"/>.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A default <see cref="OpenPgpContext"/> has not been registered.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key for <paramref name="signer"/> could not be found.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static MultipartEncrypted Create (MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted")) {
				return Create (ctx, signer, digestAlgo, recipients, entity);
			}
		}

		/// <summary>
		/// Creates a new <see cref="MultipartEncrypted"/>.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for singing and encrypting.</param>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="cipherAlgo">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="cipherAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static MultipartEncrypted Create (OpenPgpContext ctx, PgpSecretKey signer, DigestAlgorithm digestAlgo, EncryptionAlgorithm cipherAlgo, IEnumerable<PgpPublicKey> recipients, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.GetDefault ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory);
				memory.Position = 0;

				var encrypted = new MultipartEncrypted ();
				encrypted.ContentType.Parameters["protocol"] = ctx.EncryptionProtocol;

				// add the protocol version part
				encrypted.Add (new ApplicationPgpEncrypted ());

				// add the encrypted entity as the second part
				encrypted.Add (ctx.SignAndEncrypt (signer, digestAlgo, cipherAlgo, recipients, memory));

				return encrypted;
			}
		}

		/// <summary>
		/// Creates a new <see cref="MultipartEncrypted"/>.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for singing and encrypting.</param>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The <paramref name="digestAlgo"/> is not supported.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static MultipartEncrypted Create (OpenPgpContext ctx, PgpSecretKey signer, DigestAlgorithm digestAlgo, IEnumerable<PgpPublicKey> recipients, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.GetDefault ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory);
				memory.Position = 0;

				var encrypted = new MultipartEncrypted ();
				encrypted.ContentType.Parameters["protocol"] = ctx.EncryptionProtocol;

				// add the protocol version part
				encrypted.Add (new ApplicationPgpEncrypted ());

				// add the encrypted entity as the second part
				encrypted.Add (ctx.SignAndEncrypt (signer, digestAlgo, recipients, memory));

				return encrypted;
			}
		}

		/// <summary>
		/// Creates a new <see cref="MultipartEncrypted"/>.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="cipherAlgo">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A default <see cref="OpenPgpContext"/> has not been registered.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="cipherAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static MultipartEncrypted Create (PgpSecretKey signer, DigestAlgorithm digestAlgo, EncryptionAlgorithm cipherAlgo, IEnumerable<PgpPublicKey> recipients, MimeEntity entity)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted")) {
				return Create (ctx, signer, digestAlgo, cipherAlgo, recipients, entity);
			}
		}

		/// <summary>
		/// Creates a new <see cref="MultipartEncrypted"/>.
		/// </summary>
		/// <remarks>
		/// Signs the entity using the supplied signer and digest algorithm and then encrypts to
		/// the specified recipients, encapsulating the result in a new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.MultipartEncrypted"/> instance containing
		/// the signed and encrypted version of the specified entity.</returns>
		/// <param name="signer">The signer to use to sign the entity.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="signer"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para>One or more of the recipient keys cannot be used for encrypting.</para>
		/// <para>-or-</para>
		/// <para>No recipients were specified.</para>
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The <paramref name="digestAlgo"/> was out of range.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A default <see cref="OpenPgpContext"/> has not been registered.</para>
		/// <para>-or-</para>
		/// <para>The <paramref name="digestAlgo"/> is not supported.</para>
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public static MultipartEncrypted Create (PgpSecretKey signer, DigestAlgorithm digestAlgo, IEnumerable<PgpPublicKey> recipients, MimeEntity entity)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted")) {
				return Create (ctx, signer, digestAlgo, recipients, entity);
			}
		}

		/// <summary>
		/// Creates a new <see cref="MultipartEncrypted"/>.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for encrypting.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		public static MultipartEncrypted Create (OpenPgpContext ctx, IEnumerable<MailboxAddress> recipients, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryBlockStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Unix2DosFilter ());

					entity.WriteTo (filtered);
					filtered.Flush ();
				}

				memory.Position = 0;

				var encrypted = new MultipartEncrypted ();
				encrypted.ContentType.Parameters["protocol"] = ctx.EncryptionProtocol;

				// add the protocol version part
				encrypted.Add (new ApplicationPgpEncrypted ());

				// add the encrypted entity as the second part
				encrypted.Add (ctx.Encrypt (recipients, memory));

				return encrypted;
			}
		}

		/// <summary>
		/// Creates a new <see cref="MultipartEncrypted"/>.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A default <see cref="OpenPgpContext"/> has not been registered.
		/// </exception>
		/// <exception cref="PublicKeyNotFoundException">
		/// A public key for one or more of the <paramref name="recipients"/> could not be found.
		/// </exception>
		public static MultipartEncrypted Create (IEnumerable<MailboxAddress> recipients, MimeEntity entity)
		{
			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted")) {
				return Create (ctx, recipients, entity);
			}
		}

		/// <summary>
		/// Creates a new <see cref="MultipartEncrypted"/>.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for encrypting.</param>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// THe specified encryption algorithm is not supported.
		/// </exception>
		public static MultipartEncrypted Create (OpenPgpContext ctx, EncryptionAlgorithm algorithm, IEnumerable<PgpPublicKey> recipients, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryBlockStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Unix2DosFilter ());

					entity.WriteTo (filtered);
					filtered.Flush ();
				}

				memory.Position = 0;

				var encrypted = new MultipartEncrypted ();
				encrypted.ContentType.Parameters["protocol"] = ctx.EncryptionProtocol;

				// add the protocol version part
				encrypted.Add (new ApplicationPgpEncrypted ());

				// add the encrypted entity as the second part
				encrypted.Add (ctx.Encrypt (algorithm, recipients, memory));

				return encrypted;
			}
		}

		/// <summary>
		/// Creates a new <see cref="MultipartEncrypted"/>.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for encrypting.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		public static MultipartEncrypted Create (OpenPgpContext ctx, IEnumerable<PgpPublicKey> recipients, MimeEntity entity)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var memory = new MemoryBlockStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					filtered.Add (new Unix2DosFilter ());

					entity.WriteTo (filtered);
					filtered.Flush ();
				}

				memory.Position = 0;

				var encrypted = new MultipartEncrypted ();
				encrypted.ContentType.Parameters["protocol"] = ctx.EncryptionProtocol;

				// add the protocol version part
				encrypted.Add (new ApplicationPgpEncrypted ());

				// add the encrypted entity as the second part
				encrypted.Add (ctx.Encrypt (recipients, memory));

				return encrypted;
			}
		}

		/// <summary>
		/// Creates a new <see cref="MultipartEncrypted"/>.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="algorithm">The encryption algorithm.</param>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// <para>A default <see cref="OpenPgpContext"/> has not been registered.</para>
		/// <para>-or-</para>
		/// <para>The specified encryption algorithm is not supported.</para>
		/// </exception>
		public static MultipartEncrypted Create (EncryptionAlgorithm algorithm, IEnumerable<PgpPublicKey> recipients, MimeEntity entity)
		{
			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted")) {
				return Create (ctx, algorithm, recipients, entity);
			}
		}

		/// <summary>
		/// Creates a new <see cref="MultipartEncrypted"/>.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients, encapsulating the result in a
		/// new multipart/encrypted part.
		/// </remarks>
		/// <returns>A new <see cref="MimeKit.Cryptography.MultipartEncrypted"/> instance containing
		/// the encrypted version of the specified entity.</returns>
		/// <param name="recipients">The recipients for the encrypted entity.</param>
		/// <param name="entity">The entity to sign and encrypt.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// One or more of the recipient keys cannot be used for encrypting.
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A default <see cref="OpenPgpContext"/> has not been registered.
		/// </exception>
		public static MultipartEncrypted Create (IEnumerable<PgpPublicKey> recipients, MimeEntity entity)
		{
			if (recipients == null)
				throw new ArgumentNullException ("recipients");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			using (var ctx = (OpenPgpContext) CryptographyContext.Create ("application/pgp-encrypted")) {
				return Create (ctx, recipients, entity);
			}
		}

		/// <summary>
		/// Decrypts the <see cref="MultipartEncrypted"/> part.
		/// </summary>
		/// <remarks>
		/// Decrypts the <see cref="MultipartEncrypted"/> and extracts any digital signatures in cases
		/// where the content was also signed.
		/// </remarks>
		/// <returns>The decrypted entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for decrypting.</param>
		/// <param name="signatures">A list of digital signatures if the data was both signed and encrypted.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The provided <see cref="OpenPgpContext"/> does not support the protocol parameter.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the encrypted data.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public MimeEntity Decrypt (OpenPgpContext ctx, out DigitalSignatureCollection signatures)
		{
			if (ctx == null)
				throw new ArgumentNullException ("ctx");

			var protocol = ContentType.Parameters["protocol"];
			if (string.IsNullOrEmpty (protocol))
				throw new FormatException ();

			protocol = protocol.Trim ().ToLowerInvariant ();
			if (!ctx.Supports (protocol))
				throw new NotSupportedException ();

			if (Count < 2)
				throw new FormatException ();

			var version = this[0] as MimePart;
			if (version == null)
				throw new FormatException ();

			var ctype = version.ContentType;
			var value = string.Format ("{0}/{1}", ctype.MediaType, ctype.MediaSubtype);
			if (value.ToLowerInvariant () != protocol)
				throw new FormatException ();

			var encrypted = this[1] as MimePart;
			if (encrypted == null || encrypted.ContentObject == null)
				throw new FormatException ();

			if (!encrypted.ContentType.Matches ("application", "octet-stream"))
				throw new FormatException ();

			using (var memory = new MemoryBlockStream ()) {
				encrypted.ContentObject.DecodeTo (memory);
				memory.Position = 0;

				return ctx.Decrypt (memory, out signatures);
			}
		}

		/// <summary>
		/// Decrypts the <see cref="MultipartEncrypted"/> part.
		/// </summary>
		/// <remarks>
		/// Decrypts the <see cref="MultipartEncrypted"/> part.
		/// </remarks>
		/// <returns>The decrypted entity.</returns>
		/// <param name="ctx">The OpenPGP cryptography context to use for decrypting.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// The provided <see cref="OpenPgpContext"/> does not support the protocol parameter.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the encrypted data.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public MimeEntity Decrypt (OpenPgpContext ctx)
		{
			DigitalSignatureCollection signatures;

			return Decrypt (ctx, out signatures);
		}

		/// <summary>
		/// Decrypts the <see cref="MultipartEncrypted"/> part.
		/// </summary>
		/// <remarks>
		/// Decrypts the <see cref="MultipartEncrypted"/> and extracts any digital signatures in cases
		/// where the content was also signed.
		/// </remarks>
		/// <returns>The decrypted entity.</returns>
		/// <param name="signatures">A list of digital signatures if the data was both signed and encrypted.</param>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A suitable <see cref="MimeKit.Cryptography.CryptographyContext"/> for
		/// decrypting could not be found.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the encrypted data.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public MimeEntity Decrypt (out DigitalSignatureCollection signatures)
		{
			var protocol = ContentType.Parameters["protocol"];
			if (string.IsNullOrEmpty (protocol))
				throw new FormatException ();

			protocol = protocol.Trim ().ToLowerInvariant ();

			if (Count < 2)
				throw new FormatException ();

			var version = this[0] as MimePart;
			if (version == null)
				throw new FormatException ();

			var ctype = version.ContentType;
			var value = string.Format ("{0}/{1}", ctype.MediaType, ctype.MediaSubtype);
			if (value.ToLowerInvariant () != protocol)
				throw new FormatException ();

			var encrypted = this[1] as MimePart;
			if (encrypted == null || encrypted.ContentObject == null)
				throw new FormatException ();

			if (!encrypted.ContentType.Matches ("application", "octet-stream"))
				throw new FormatException ();

			using (var ctx = CryptographyContext.Create (protocol)) {
				using (var memory = new MemoryBlockStream ()) {
					var pgp = ctx as OpenPgpContext;

					encrypted.ContentObject.DecodeTo (memory);
					memory.Position = 0;

					if (pgp != null)
						return pgp.Decrypt (memory, out signatures);

					signatures = null;

					return ctx.Decrypt (memory);
				}
			}
		}

		/// <summary>
		/// Decrypts the <see cref="MultipartEncrypted"/> part.
		/// </summary>
		/// <remarks>
		/// Decrypts the <see cref="MultipartEncrypted"/> part.
		/// </remarks>
		/// <returns>The decrypted entity.</returns>
		/// <exception cref="System.FormatException">
		/// <para>The <c>protocol</c> parameter was not specified.</para>
		/// <para>-or-</para>
		/// <para>The multipart is malformed in some way.</para>
		/// </exception>
		/// <exception cref="System.NotSupportedException">
		/// A suitable <see cref="MimeKit.Cryptography.CryptographyContext"/> for
		/// decrypting could not be found.
		/// </exception>
		/// <exception cref="PrivateKeyNotFoundException">
		/// The private key could not be found to decrypt the encrypted data.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The user chose to cancel the password prompt.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// 3 bad attempts were made to unlock the secret key.
		/// </exception>
		public MimeEntity Decrypt ()
		{
			DigitalSignatureCollection signatures;

			return Decrypt (out signatures);
		}
	}
}
