//
// OpenPgpDigitalCertificate.cs
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
using System.Text;

using Org.BouncyCastle.Bcpg.OpenPgp;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An OpenPGP digital certificate.
	/// </summary>
	/// <remarks>
	/// An OpenPGP digital certificate.
	/// </remarks>
	public class OpenPgpDigitalCertificate : IDigitalCertificate
	{
		internal OpenPgpDigitalCertificate (PgpPublicKey pubkey)
		{
			var data = pubkey.GetFingerprint ();
			var builder = new StringBuilder ();

			for (int i = 0; i < data.Length; i++)
				builder.Append (data[i].ToString ("X"));

//			var trust = pubkey.GetTrustData ();
//			if (trust != null) {
//				TrustLevel = (TrustLevel) (trust[0] & 15);
//			} else {
//				TrustLevel = TrustLevel.None;
//			}

			Fingerprint = builder.ToString ();
			PublicKey = pubkey;

			foreach (string userId in pubkey.GetUserIds ()) {
				data = Encoding.UTF8.GetBytes (userId);
				InternetAddress address;
				int index = 0;

				if (!InternetAddress.TryParse (ParserOptions.Default, data, ref index, data.Length, false, out address))
					continue;

				Name = address.Name;

				var mailbox = address as MailboxAddress;
				if (mailbox == null)
					continue;

				Email = mailbox.Address;
				break;
			}
		}

		OpenPgpDigitalCertificate ()
		{
		}

		/// <summary>
		/// Gets the public key.
		/// </summary>
		/// <remarks>
		/// Gets the public key.
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
			get { return CreationDate.AddSeconds ((double) PublicKey.GetValidSeconds ()); }
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
