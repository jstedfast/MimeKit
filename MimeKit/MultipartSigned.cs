//
// MultipartSigned.cs
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
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace MimeKit {
	public class MultipartSigned : Multipart
	{
		internal MultipartSigned (ParserOptions options, ContentType type, IEnumerable<Header> headers, bool toplevel) : base (options, type, headers, toplevel)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MultipartSigned"/> class.
		/// </summary>
		public MultipartSigned () : base ("signed")
		{
		}

		static void PrepareEntityForSigning (MimeEntity entity)
		{
			if (entity is Multipart) {
				// Note: we do not want to modify multipart/signed parts
				if (entity is MultipartSigned)
					return;

				var multipart = (Multipart) entity;

				foreach (var subpart in multipart)
					PrepareEntityForSigning (subpart);
			} else if (entity is MessagePart) {
				var mpart = (MessagePart) entity;

				if (mpart.Message != null && mpart.Message.Body != null)
					PrepareEntityForSigning (mpart.Message.Body);
			} else {
				var part = (MimePart) entity;

				if (part.ContentTransferEncoding != ContentEncoding.Base64)
					part.ContentTransferEncoding = ContentEncoding.QuotedPrintable;
			}
		}

		/// <summary>
		/// Creates a new <see cref="MimeKit.MultipartSigned"/> instance with the entity as the content.
		/// </summary>
		/// <param name="signer">The signer.</param>
		/// <param name="entity">The entity to sign.</param>
		public static MultipartSigned Create (CmsSigner signer, MimeEntity entity)
		{
			if (signer == null)
				throw new ArgumentNullException ("signer");

			if (entity == null)
				throw new ArgumentNullException ("entity");

			PrepareEntityForSigning (entity);

			// FIXME: support PGP/MIME as well
			var ctx = new SecureMimeContext ();
			MimeEntity parsed;
			byte[] cleartext;

			using (var memory = new MemoryStream ()) {
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
				parsed = parser.ParseEntity ();

				cleartext = memory.ToArray ();
			}

			// sign the cleartext content
			var signature = ctx.Sign (signer, cleartext);
			var signed = new MultipartSigned ();

			// set the protocol and micalg Content-Type parameters
			signed.ContentType.Parameters["protocol"] = ctx.SignatureProtocol;
			signed.ContentType.Parameters["micalg"] = signer.DigestAlgorithm.FriendlyName;

			// add the modified/parsed entity as our first part
			signed.Add (parsed);

			// add the detached signature as the second part
			signed.Add (signature);

			return signed;
		}

		/// <summary>
		/// Verify the multipart/signed content.
		/// </summary>
		/// <returns>A signer info collection.</returns>
		public SignerInfoCollection Verify ()
		{
			// FIXME: support PGP/MIME as well
			if (Count < 2 || !(this[1] is ApplicationPkcs7Signature))
				return null;

			var signature = (ApplicationPkcs7Signature) this[1];
			if (signature.ContentObject == null)
				return null;

			byte[] cleartext, signatureData;
			var ctx = new SecureMimeContext ();

			using (var memory = new MemoryStream ()) {
				using (var filtered = new FilteredStream (memory)) {
					// Note: see rfc2015 or rfc3156, section 5.1
					filtered.Add (new Unix2DosFilter ());

					this[0].WriteTo (filtered);
					filtered.Flush ();
				}

				cleartext = memory.ToArray ();
			}

			using (var memory = new MemoryStream ()) {
				signature.ContentObject.DecodeTo (memory);
				signatureData = memory.ToArray ();
			}

			return ctx.Verify (cleartext, signatureData);
		}
	}
}
