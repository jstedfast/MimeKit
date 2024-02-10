//
// OpenPgpDigitalCertificate.cs
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
using System.Text;

using Org.BouncyCastle.Bcpg.OpenPgp;

using MimeKit.Utils;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An OpenPGP digital certificate.
	/// </summary>
	/// <remarks>
	/// An OpenPGP digital certificate.
	/// </remarks>
	public class OpenPgpDigitalCertificate : IDigitalCertificate
	{
		internal OpenPgpDigitalCertificate (PgpPublicKeyRing keyring, PgpPublicKey pubkey)
		{
			var bytes = pubkey.GetFingerprint ();
			var builder = new ValueStringBuilder (bytes.Length * 2);

			for (int i = 0; i < bytes.Length; i++)
				builder.Append (bytes[i].ToString ("X2"));

//			var trust = pubkey.GetTrustData ();
//			if (trust != null) {
//				TrustLevel = (TrustLevel) (trust[0] & 15);
//			} else {
//				TrustLevel = TrustLevel.None;
//			}

			Fingerprint = builder.ToString ();
			PublicKey = pubkey;
			KeyRing = keyring;

			if (!UpdateUserId (pubkey) && !pubkey.IsMasterKey) {
				foreach (PgpPublicKey key in keyring.GetPublicKeys ()) {
					if (key.IsMasterKey) {
						UpdateUserId (key);
						break;
					}
				}
			}
		}

		bool UpdateUserId (PgpPublicKey pubkey)
		{
			foreach (string userId in pubkey.GetUserIds ()) {
				var bytes = Encoding.UTF8.GetBytes (userId);
				int index = 0;

				if (!MailboxAddress.TryParse (ParserOptions.Default, bytes, ref index, bytes.Length, false, out var mailbox))
					continue;

				Email = mailbox.Address;
				Name = mailbox.Name;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the public key ring.
		/// </summary>
		/// <remarks>
		/// Get the public key ring that <see cref="PublicKey"/> is associated with.
		/// </remarks>
		/// <value>The key ring.</value>
		public PgpPublicKeyRing KeyRing {
			get; private set;
		}

		/// <summary>
		/// Gets the public key.
		/// </summary>
		/// <remarks>
		/// Get the public key.
		/// </remarks>
		/// <value>The public key.</value>
		public PgpPublicKey PublicKey {
			get; private set;
		}

		#region IDigitalCertificate implementation

		/// <summary>
		/// Gets the public key algorithm supported by the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the public key algorithm supported by the certificate.
		/// </remarks>
		/// <value>The public key algorithm.</value>
		public PublicKeyAlgorithm PublicKeyAlgorithm {
			get { return OpenPgpContext.GetPublicKeyAlgorithm (PublicKey.Algorithm); }
		}

		/// <summary>
		/// Gets the date that the certificate was created.
		/// </summary>
		/// <remarks>
		/// Gets the date that the certificate was created.
		/// </remarks>
		/// <value>The creation date.</value>
		public DateTime CreationDate {
			get { return PublicKey.CreationTime; }
		}

		/// <summary>
		/// Gets the expiration date of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the expiration date of the certificate.
		/// </remarks>
		/// <value>The expiration date.</value>
		public DateTime ExpirationDate {
			get {
				long seconds = PublicKey.GetValidSeconds ();

				return seconds > 0 ? CreationDate.AddSeconds ((double) seconds) : DateTime.MaxValue;
			}
		}

		/// <summary>
		/// Gets the fingerprint of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the fingerprint of the certificate.
		/// </remarks>
		/// <value>The fingerprint.</value>
		public string Fingerprint {
			get; private set;
		}

		/// <summary>
		/// Gets the email address of the owner of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the email address of the owner of the certificate.
		/// </remarks>
		/// <value>The email address.</value>
		public string Email {
			get; private set;
		}

		/// <summary>
		/// Gets the name of the owner of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the name of the owner of the certificate.
		/// </remarks>
		/// <value>The name of the owner.</value>
		public string Name {
			get; private set;
		}

		#endregion
	}
}
