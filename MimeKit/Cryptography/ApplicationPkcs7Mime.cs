//
// ApplicationPkcs7Mime.cs
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
using System.Threading;
using System.Threading.Tasks;
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
	public class ApplicationPkcs7Mime : MimePart, IApplicationPkcs7Mime
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
		/// want to use the <see cref="Compress(MimeEntity, CancellationToken)"/>,
		/// <see cref="Encrypt(CmsRecipientCollection, MimeEntity, CancellationToken)"/>, and/or
		/// <see cref="Sign(CmsSigner, MimeEntity, CancellationToken)"/> method to create new instances
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
			case SecureMimeType.AuthEnvelopedData:
				ContentType.Parameters["smime-type"] = "authenveloped-data";
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

		void CheckDisposed ()
		{
			CheckDisposed (nameof (ApplicationPkcs7Mime));
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

				if (type.Equals ("authenveloped-data", StringComparison.OrdinalIgnoreCase))
					return SecureMimeType.AuthEnvelopedData;
				if (type.Equals ("compressed-data", StringComparison.OrdinalIgnoreCase))
					return SecureMimeType.CompressedData;
				if (type.Equals ("enveloped-data", StringComparison.OrdinalIgnoreCase))
					return SecureMimeType.EnvelopedData;
				if (type.Equals ("signed-data", StringComparison.OrdinalIgnoreCase))
					return SecureMimeType.SignedData;
				if (type.Equals ("certs-only", StringComparison.OrdinalIgnoreCase))
					return SecureMimeType.CertsOnly;

				return SecureMimeType.Unknown;
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
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor == null)
				throw new ArgumentNullException (nameof (visitor));

			CheckDisposed ();

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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "compressed-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Decompress (SecureMimeContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			CheckDisposed ();

			if (SecureMimeType != SecureMimeType.CompressedData && SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				Content.DecodeTo (memory, cancellationToken);
				memory.Position = 0;

				return ctx.Decompress (memory, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously decompress the compressed-data.
		/// </summary>
		/// <remarks>
		/// Asynchronously decompresses the compressed-data using the specified <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decompressed <see cref="MimeEntity"/>.</returns>
		/// <param name="ctx">The S/MIME context to use for decompressing.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "compressed-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public async Task<MimeEntity> DecompressAsync (SecureMimeContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			CheckDisposed ();

			if (SecureMimeType != SecureMimeType.CompressedData && SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				await Content.DecodeToAsync (memory, cancellationToken).ConfigureAwait (false);
				memory.Position = 0;

				return await ctx.DecompressAsync (memory, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Decompress the compressed-data.
		/// </summary>
		/// <remarks>
		/// Decompresses the compressed-data using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decompressed <see cref="MimeEntity"/>.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "compressed-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Decompress (CancellationToken cancellationToken = default)
		{
			CheckDisposed ();

			if (SecureMimeType != SecureMimeType.CompressedData && SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Decompress (ctx, cancellationToken);
		}

		/// <summary>
		/// Asynchronously decompress the compressed-data.
		/// </summary>
		/// <remarks>
		/// Asynchronously decompresses the compressed-data using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decompressed <see cref="MimeEntity"/>.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "compressed-data".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public async Task<MimeEntity> DecompressAsync (CancellationToken cancellationToken = default)
		{
			CheckDisposed ();

			if (SecureMimeType != SecureMimeType.CompressedData && SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await DecompressAsync (ctx, cancellationToken).ConfigureAwait (false);
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
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Decrypt (SecureMimeContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			CheckDisposed ();

			if (SecureMimeType != SecureMimeType.EnvelopedData && SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				Content.DecodeTo (memory, cancellationToken);
				memory.Position = 0;

				return ctx.Decrypt (memory, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously decrypt the enveloped-data.
		/// </summary>
		/// <remarks>
		/// Asynchronously decrypts the enveloped-data using the specified <see cref="SecureMimeContext"/>.
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
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public async Task<MimeEntity> DecryptAsync (SecureMimeContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			CheckDisposed ();

			if (SecureMimeType != SecureMimeType.EnvelopedData && SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				await Content.DecodeToAsync (memory, cancellationToken).ConfigureAwait (false);
				memory.Position = 0;

				return await ctx.DecryptAsync (memory, cancellationToken).ConfigureAwait (false);
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
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public MimeEntity Decrypt (CancellationToken cancellationToken = default)
		{
			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Decrypt (ctx, cancellationToken);
		}

		/// <summary>
		/// Asynchronously decrypt the enveloped-data.
		/// </summary>
		/// <remarks>
		/// Asynchronously decrypts the enveloped-data using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The decrypted <see cref="MimeEntity"/>.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "certs-only".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public async Task<MimeEntity> DecryptAsync (CancellationToken cancellationToken = default)
		{
			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await DecryptAsync (ctx, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Import the certificates contained in the application/pkcs7-mime content.
		/// </summary>
		/// <remarks>
		/// Imports the certificates contained in the application/pkcs7-mime content.
		/// </remarks>
		/// <param name="ctx">The S/MIME context to import certificates into.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "certs-only".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public void Import (SecureMimeContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			CheckDisposed ();

			if (SecureMimeType != SecureMimeType.CertsOnly && SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				Content.DecodeTo (memory, cancellationToken);
				memory.Position = 0;

				ctx.Import (memory, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously import the certificates contained in the application/pkcs7-mime content.
		/// </summary>
		/// <remarks>
		/// Asynchronously imports the certificates contained in the application/pkcs7-mime content.
		/// </remarks>
		/// <returns>An asynchronous task context.</returns>
		/// <param name="ctx">The S/MIME context to import certificates into.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="ctx"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The "smime-type" parameter on the Content-Type header is not "certs-only".
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public async Task ImportAsync (SecureMimeContext ctx, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			CheckDisposed ();

			if (SecureMimeType != SecureMimeType.CertsOnly && SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				await Content.DecodeToAsync (memory, cancellationToken).ConfigureAwait (false);
				memory.Position = 0;

				await ctx.ImportAsync (memory, cancellationToken).ConfigureAwait (false);
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
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public DigitalSignatureCollection Verify (SecureMimeContext ctx, out MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			CheckDisposed ();

			if (SecureMimeType != SecureMimeType.SignedData && SecureMimeType != SecureMimeType.Unknown)
				throw new InvalidOperationException ();

			using (var memory = new MemoryBlockStream ()) {
				Content.DecodeTo (memory, cancellationToken);
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
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ApplicationPkcs7Mime"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public DigitalSignatureCollection Verify (out MimeEntity entity, CancellationToken cancellationToken = default)
		{
			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Verify (ctx, out entity, cancellationToken);
		}

		static async Task<ApplicationPkcs7Mime> CompressAsync (SecureMimeContext ctx, MimeEntity entity, bool doAsync, CancellationToken cancellationToken)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				if (doAsync)
					await entity.WriteToAsync (options, memory, cancellationToken).ConfigureAwait (false);
				else
					entity.WriteTo (options, memory, cancellationToken);
				memory.Position = 0;

				if (doAsync)
					return await ctx.CompressAsync (memory, cancellationToken).ConfigureAwait (false);

				return ctx.Compress (memory, cancellationToken);
			}
		}

		/// <summary>
		/// Compress the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Compresses the specified entity using the specified <see cref="SecureMimeContext"/>.</para>
		/// <note type="warning">Most mail clients, even among those that support S/MIME, do not support compression.</note>
		/// </remarks>
		/// <returns>The compressed entity.</returns>
		/// <param name="ctx">The S/MIME context to use for compressing.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Compress (SecureMimeContext ctx, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return CompressAsync (ctx, entity, false, cancellationToken).GetAwaiter ().GetResult ();
		}

		/// <summary>
		/// Asynchronously compress the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously compresses the specified entity using the specified <see cref="SecureMimeContext"/>.</para>
		/// <note type="warning">Most mail clients, even among those that support S/MIME, do not support compression.</note>
		/// </remarks>
		/// <returns>The compressed entity.</returns>
		/// <param name="ctx">The S/MIME context to use for compressing.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static Task<ApplicationPkcs7Mime> CompressAsync (SecureMimeContext ctx, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			return CompressAsync (ctx, entity, true, cancellationToken);
		}

		/// <summary>
		/// Compress the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Compresses the specified entity using the default <see cref="SecureMimeContext"/>.</para>
		/// <note type="warning">Most mail clients, even among those that support S/MIME, do not support compression.</note>
		/// </remarks>
		/// <returns>The compressed entity.</returns>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="entity"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Compress (MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Compress (ctx, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously compress the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously compresses the specified entity using the default <see cref="SecureMimeContext"/>.</para>
		/// <note type="warning">Most mail clients, even among those that support S/MIME, do not support compression.</note>
		/// </remarks>
		/// <returns>The compressed entity.</returns>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="entity"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> CompressAsync (MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await CompressAsync (ctx, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients using the supplied <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for encrypting.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Encrypt (SecureMimeContext ctx, CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory, cancellationToken);
				memory.Position = 0;

				return ctx.Encrypt (recipients, memory, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously encrypts the entity to the specified recipients using the supplied <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for encrypting.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> EncryptAsync (SecureMimeContext ctx, CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				await entity.WriteToAsync (options, memory, cancellationToken).ConfigureAwait (false);
				memory.Position = 0;

				return await ctx.EncryptAsync (recipients, memory, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Encrypt (CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Encrypt (ctx, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously encrypts the entity to the specified recipients using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> EncryptAsync (CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await EncryptAsync (ctx, recipients, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients using the supplied <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for encrypting.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
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
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Encrypt (SecureMimeContext ctx, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory, cancellationToken);
				memory.Position = 0;

				return (ApplicationPkcs7Mime) ctx.Encrypt (recipients, memory, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously encrypts the entity to the specified recipients using the supplied <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for encrypting.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
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
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> EncryptAsync (SecureMimeContext ctx, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				await entity.WriteToAsync (options, memory, cancellationToken).ConfigureAwait (false);
				memory.Position = 0;

				return (ApplicationPkcs7Mime) await ctx.EncryptAsync (recipients, memory, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Encrypts the entity to the specified recipients using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// Valid certificates could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Encrypt (IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Encrypt (ctx, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously encrypts the entity to the specified recipients using the default <see cref="SecureMimeContext"/>.
		/// </remarks>
		/// <returns>The encrypted entity.</returns>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// Valid certificates could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A certificate could not be found for one or more of the <paramref name="recipients"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> EncryptAsync (IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await EncryptAsync (ctx, recipients, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Signs the entity using the supplied signer and <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Sign (SecureMimeContext ctx, CmsSigner signer, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory, cancellationToken);
				memory.Position = 0;

				return ctx.EncapsulatedSign (signer, memory, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously signs the entity using the supplied signer and <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.CreateAsync(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> SignAsync (SecureMimeContext ctx, CmsSigner signer, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				await entity.WriteToAsync (options, memory, cancellationToken).ConfigureAwait (false);
				memory.Position = 0;

				return await ctx.EncapsulatedSignAsync (signer, memory, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Signs the entity using the supplied signer and the default <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Sign (CmsSigner signer, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Sign (ctx, signer, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously signs the entity using the supplied signer and the default <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.CreateAsync(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> SignAsync (CmsSigner signer, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await SignAsync (ctx, signer, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Signs the entity using the supplied signer, digest algorithm and <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Sign (SecureMimeContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				entity.WriteTo (options, memory, cancellationToken);
				memory.Position = 0;

				return ctx.EncapsulatedSign (signer, digestAlgo, memory, cancellationToken);
			}
		}

		/// <summary>
		/// Asynchronously sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously signs the entity using the supplied signer, digest algorithm and <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> SignAsync (SecureMimeContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var memory = new MemoryBlockStream ()) {
				var options = FormatOptions.Default.Clone ();
				options.NewLineFormat = NewLineFormat.Dos;

				await entity.WriteToAsync (options, memory, cancellationToken).ConfigureAwait (false);
				memory.Position = 0;

				return await ctx.EncapsulatedSignAsync (signer, digestAlgo, memory, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Signs the entity using the supplied signer, digest algorithm and the default
		/// <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.Create(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime Sign (MailboxAddress signer, DigestAlgorithm digestAlgo, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return Sign (ctx, signer, digestAlgo, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously sign the specified entity.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously signs the entity using the supplied signer, digest algorithm and the default
		/// <see cref="SecureMimeContext"/>.</para>
		/// <para>For better interoperability with other mail clients, you should use
		/// <see cref="MultipartSigned.CreateAsync(SecureMimeContext, CmsSigner, MimeEntity,CancellationToken)"/>
		/// instead as the multipart/signed format is supported among a much larger
		/// subset of mail client software.</para>
		/// </remarks>
		/// <returns>The signed entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// A signing certificate could not be found for <paramref name="signer"/>.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> SignAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await SignAsync (ctx, signer, digestAlgo, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs entity using the supplied signer and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing and encrypting.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime SignAndEncrypt (SecureMimeContext ctx, CmsSigner signer, CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			var signed = MultipartSigned.Create (ctx, signer, entity, cancellationToken);

			return Encrypt (ctx, recipients, signed, cancellationToken);
		}

		/// <summary>
		/// Asynchronously sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs entity using the supplied signer and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing and encrypting.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> SignAndEncryptAsync (SecureMimeContext ctx, CmsSigner signer, CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			var signed = await MultipartSigned.CreateAsync (ctx, signer, entity, cancellationToken).ConfigureAwait (false);

			return await EncryptAsync (ctx, recipients, signed, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs entity using the supplied signer and the default <see cref="SecureMimeContext"/>
		/// and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime SignAndEncrypt (CmsSigner signer, CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return SignAndEncrypt (ctx, signer, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchroinously sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs entity using the supplied signer and the default <see cref="SecureMimeContext"/>
		/// and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> SignAndEncryptAsync (CmsSigner signer, CmsRecipientCollection recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await SignAndEncryptAsync (ctx, signer, recipients, entity, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs entity using the supplied signer and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing and encrypting.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// <para>A signing certificate could not be found for <paramref name="signer"/>.</para>
		/// <para>-or-</para>
		/// <para>A certificate could not be found for one or more of the <paramref name="recipients"/>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime SignAndEncrypt (SecureMimeContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			var signed = MultipartSigned.Create (ctx, signer, digestAlgo, entity, cancellationToken);

			return Encrypt (ctx, recipients, signed, cancellationToken);
		}

		/// <summary>
		/// Asynchronously sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs entity using the supplied signer and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="ctx">The S/MIME context to use for signing and encrypting.</param>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="ctx"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// <para>A signing certificate could not be found for <paramref name="signer"/>.</para>
		/// <para>-or-</para>
		/// <para>A certificate could not be found for one or more of the <paramref name="recipients"/>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> SignAndEncryptAsync (SecureMimeContext ctx, MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (ctx == null)
				throw new ArgumentNullException (nameof (ctx));

			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			var signed = await MultipartSigned.CreateAsync (ctx, signer, digestAlgo, entity, cancellationToken).ConfigureAwait (false);

			return await EncryptAsync (ctx, recipients, signed, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Signs entity using the supplied signer and the default <see cref="SecureMimeContext"/>
		/// and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// <para>A signing certificate could not be found for <paramref name="signer"/>.</para>
		/// <para>-or-</para>
		/// <para>A certificate could not be found for one or more of the <paramref name="recipients"/>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static ApplicationPkcs7Mime SignAndEncrypt (MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return SignAndEncrypt (ctx, signer, digestAlgo, recipients, entity, cancellationToken);
		}

		/// <summary>
		/// Asynchronously sign and encrypt the specified entity.
		/// </summary>
		/// <remarks>
		/// Asynchronously signs entity using the supplied signer and the default <see cref="SecureMimeContext"/>
		/// and then encrypts the result to the specified recipients.
		/// </remarks>
		/// <returns>The signed and encrypted entity.</returns>
		/// <param name="signer">The signer.</param>
		/// <param name="digestAlgo">The digest algorithm to use for signing.</param>
		/// <param name="recipients">The recipients.</param>
		/// <param name="entity">The entity.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="signer"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="recipients"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entity"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="entity"/> has been disposed.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="CertificateNotFoundException">
		/// <para>A signing certificate could not be found for <paramref name="signer"/>.</para>
		/// <para>-or-</para>
		/// <para>A certificate could not be found for one or more of the <paramref name="recipients"/>.</para>
		/// </exception>
		/// <exception cref="Org.BouncyCastle.Cms.CmsException">
		/// An error occurred in the cryptographic message syntax subsystem.
		/// </exception>
		public static async Task<ApplicationPkcs7Mime> SignAndEncryptAsync (MailboxAddress signer, DigestAlgorithm digestAlgo, IEnumerable<MailboxAddress> recipients, MimeEntity entity, CancellationToken cancellationToken = default)
		{
			if (signer == null)
				throw new ArgumentNullException (nameof (signer));

			if (recipients == null)
				throw new ArgumentNullException (nameof (recipients));

			if (entity == null)
				throw new ArgumentNullException (nameof (entity));

			using (var ctx = (SecureMimeContext) CryptographyContext.Create ("application/pkcs7-mime"))
				return await SignAndEncryptAsync (ctx, signer, digestAlgo, recipients, entity, cancellationToken).ConfigureAwait (false);
		}
	}
}
