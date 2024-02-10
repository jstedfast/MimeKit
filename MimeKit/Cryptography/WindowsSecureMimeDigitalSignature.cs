//
// WindowsSecureMimeDigitalSignature.cs
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
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography.Pkcs;

using Org.BouncyCastle.Asn1;

using CmsAttributes = Org.BouncyCastle.Asn1.Cms.CmsAttributes;
using SmimeAttributes = Org.BouncyCastle.Asn1.Smime.SmimeAttributes;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME digital signature.
	/// </summary>
	/// <remarks>
	/// An S/MIME digital signature that is used with the <see cref="WindowsSecureMimeContext"/>.
	/// </remarks>
	public class WindowsSecureMimeDigitalSignature : IDigitalSignature
	{
		DigitalSignatureVerifyException vex;

		/// <summary>
		/// Initialize a new instance of the <see cref="WindowsSecureMimeDigitalSignature"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="WindowsSecureMimeDigitalSignature"/>.
		/// </remarks>
		/// <param name="signerInfo">The information about the signer.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signerInfo"/> is <c>null</c>.
		/// </exception>
		public WindowsSecureMimeDigitalSignature (SignerInfo signerInfo)
		{
			if (signerInfo == null)
				throw new ArgumentNullException (nameof (signerInfo));

			SignerInfo = signerInfo;

			var algorithms = new List<EncryptionAlgorithm> ();

			if (signerInfo.SignedAttributes != null) {
				for (int i = 0; i < signerInfo.SignedAttributes.Count; i++) {
					if (signerInfo.SignedAttributes[i].Oid.Value == CmsAttributes.SigningTime.Id) {
						if (signerInfo.SignedAttributes[i].Values[0] is Pkcs9SigningTime signingTime)
							CreationDate = signingTime.SigningTime;
					} else if (signerInfo.SignedAttributes[i].Oid.Value == SmimeAttributes.SmimeCapabilities.Id) {
						foreach (var value in signerInfo.SignedAttributes[i].Values) {
							var sequences = (DerSequence) Asn1Object.FromByteArray (value.RawData);

							foreach (var sequence in sequences.OfType<Asn1Sequence> ()) {
								var identifier = Org.BouncyCastle.Asn1.X509.AlgorithmIdentifier.GetInstance (sequence);

								if (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (identifier, out var algorithm))
									algorithms.Add (algorithm);
							}
						}
					}
				}
			}

			EncryptionAlgorithms = algorithms.ToArray ();

			if (WindowsSecureMimeContext.TryGetDigestAlgorithm (signerInfo.DigestAlgorithm, out var digestAlgo))
				DigestAlgorithm = digestAlgo;

			if (signerInfo.Certificate != null)
				SignerCertificate = new WindowsSecureMimeDigitalCertificate (signerInfo.Certificate);
		}

		/// <summary>
		/// Gets the signer info.
		/// </summary>
		/// <remarks>
		/// Gets the signer info.
		/// </remarks>
		/// <value>The signer info.</value>
		public SignerInfo SignerInfo {
			get; private set;
		}

		/// <summary>
		/// Gets the list of encryption algorithms, in preferential order,
		/// that the signer's client supports.
		/// </summary>
		/// <remarks>
		/// Gets the list of encryption algorithms, in preferential order,
		/// that the signer's client supports.
		/// </remarks>
		/// <value>The S/MIME encryption algorithms.</value>
		public EncryptionAlgorithm[] EncryptionAlgorithms {
			get; private set;
		}

		#region IDigitalSignature implementation

		/// <summary>
		/// Gets certificate used by the signer.
		/// </summary>
		/// <remarks>
		/// Gets certificate used by the signer.
		/// </remarks>
		/// <value>The signer's certificate.</value>
		public IDigitalCertificate SignerCertificate {
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
			get { return SignerCertificate != null ? SignerCertificate.PublicKeyAlgorithm : PublicKeyAlgorithm.None; }
		}

		/// <summary>
		/// Gets the digest algorithm used for the signature.
		/// </summary>
		/// <remarks>
		/// Gets the digest algorithm used for the signature.
		/// </remarks>
		/// <value>The digest algorithm.</value>
		public DigestAlgorithm DigestAlgorithm {
			get; private set;
		}

		/// <summary>
		/// Gets the creation date of the digital signature.
		/// </summary>
		/// <remarks>
		/// Gets the creation date of the digital signature.
		/// </remarks>
		/// <value>The creation date in coordinated universal time (UTC).</value>
		public DateTime CreationDate {
			get; private set;
		}

		/// <summary>
		/// Verifies the digital signature.
		/// </summary>
		/// <remarks>
		/// Verifies the digital signature.
		/// </remarks>
		/// <returns><c>true</c> if the signature is valid; otherwise <c>false</c>.</returns>
		/// <exception cref="DigitalSignatureVerifyException">
		/// An error verifying the signature has occurred.
		/// </exception>
		public bool Verify ()
		{
			return Verify (false);
		}

		/// <summary>
		/// Verifies the digital signature.
		/// </summary>
		/// <remarks>
		/// Verifies the digital signature.
		/// </remarks>
		/// <param name="verifySignatureOnly"><c>true</c> if only the signature itself should be verified; otherwise, both the signature and the certificate chain are validated.</param>
		/// <returns><c>true</c> if the signature is valid; otherwise, <c>false</c>.</returns>
		/// <exception cref="DigitalSignatureVerifyException">
		/// An error verifying the signature has occurred.
		/// </exception>
		public bool Verify (bool verifySignatureOnly)
		{
			if (vex != null)
				throw vex;

			try {
				SignerInfo.CheckSignature (verifySignatureOnly);
				return true;
			} catch (Exception ex) {
				var message = string.Format ("Failed to verify digital signature: {0}", ex.Message);
				vex = new DigitalSignatureVerifyException (message, ex);
				throw vex;
			}
		}

		#endregion
	}
}
