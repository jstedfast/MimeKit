﻿//
// OpenPgpDigitalSignature.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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
using System.Diagnostics;
using System.Globalization;

using Org.BouncyCastle.Bcpg.OpenPgp;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An OpenPGP digital signature.
	/// </summary>
	/// <remarks>
	/// An OpenPGP digital signature.
	/// </remarks>
	public class OpenPgpDigitalSignature : IDigitalSignature
	{
		DigitalSignatureVerifyException? vex;
		bool? valid;

		internal OpenPgpDigitalSignature (PgpPublicKeyRing? keyring, PgpPublicKey? pubkey, PgpOnePassSignature signature)
		{
			if (pubkey != null) {
				Debug.Assert (keyring != null, "keyring must not be null if pubkey is not null");
				SignerCertificate = new OpenPgpDigitalCertificate (keyring, pubkey);
			}

			OnePassSignature = signature;
		}

		internal OpenPgpDigitalSignature (PgpPublicKeyRing? keyring, PgpPublicKey? pubkey, PgpSignature signature)
		{
			if (pubkey != null) {
				Debug.Assert (keyring != null, "keyring must not be null if pubkey is not null");
				SignerCertificate = new OpenPgpDigitalCertificate (keyring, pubkey);
			}

			Signature = signature;
		}

		internal PgpOnePassSignature? OnePassSignature {
			get; private set;
		}

		internal PgpSignature? Signature {
			get; set;
		}

		#region IDigitalSignature implementation

		/// <summary>
		/// Gets certificate used by the signer.
		/// </summary>
		/// <remarks>
		/// Gets certificate used by the signer.
		/// </remarks>
		/// <value>The signer's certificate.</value>
		public IDigitalCertificate? SignerCertificate {
			get; private set;
		}

		/// <summary>
		/// Gets the public key algorithm used for the signature.
		/// </summary>
		/// <remarks>
		/// Gets the public key algorithm used for the signature.
		/// </remarks>
		/// <value>The public key algorithm.</value>
		public PublicKeyAlgorithm PublicKeyAlgorithm {
			get; internal set;
		}

		/// <summary>
		/// Gets the digest algorithm used for the signature.
		/// </summary>
		/// <remarks>
		/// Gets the digest algorithm used for the signature.
		/// </remarks>
		/// <value>The digest algorithm.</value>
		public DigestAlgorithm DigestAlgorithm {
			get; internal set;
		}

		/// <summary>
		/// Gets the creation date of the digital signature.
		/// </summary>
		/// <remarks>
		/// Gets the creation date of the digital signature.
		/// </remarks>
		/// <value>The creation date.</value>
		public DateTime CreationDate {
			get; internal set;
		}

		/// <summary>
		/// Verifies the digital signature.
		/// </summary>
		/// <remarks>
		/// Verifies the digital signature.
		/// </remarks>
		/// <returns><see langword="true" /> if the signature is valid; otherwise, <see langword="false" />.</returns>
		/// <exception cref="DigitalSignatureVerifyException">
		/// An error verifying the signature has occurred.
		/// </exception>
		public bool Verify ()
		{
			if (valid.HasValue)
				return valid.Value;

			if (vex != null)
				throw vex;

			if (SignerCertificate == null) {
				var message = string.Format (CultureInfo.InvariantCulture, "Failed to verify digital signature: no public key found for {0:X8}", (int) Signature.KeyId);
				vex = new DigitalSignatureVerifyException (Signature.KeyId, message);
				throw vex;
			}

			try {
				if (OnePassSignature != null)
					valid = OnePassSignature.Verify (Signature);
				else
					valid = Signature!.Verify (); // Either OnePassSignature or Signature must be non-null
				return valid.Value;
			} catch (Exception ex) {
				var message = string.Format ("Failed to verify digital signature: {0}", ex.Message);
				vex = new DigitalSignatureVerifyException (Signature.KeyId, message, ex);
				throw vex;
			}
		}

		/// <summary>
		/// Verifies the digital signature.
		/// </summary>
		/// <remarks>
		/// Verifies the digital signature.
		/// </remarks>
		/// <param name="verifySignatureOnly">This option is ignored for OpenPGP digital signatures.</param>
		/// <returns><see langword="true" /> if the signature is valid; otherwise, <see langword="false" />.</returns>
		/// <exception cref="DigitalSignatureVerifyException">
		/// An error verifying the signature has occurred.
		/// </exception>
		public bool Verify (bool verifySignatureOnly)
		{
			return Verify ();
		}

		#endregion
	}
}
