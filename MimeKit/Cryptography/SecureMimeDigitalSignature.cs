﻿//
// SecureMimeDigitalSignature.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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
using System.Collections.Generic;

using Org.BouncyCastle.Cms;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Smime;
using Org.BouncyCastle.Asn1.X509;

using MimeKit.Utils;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME digital signature.
	/// </summary>
	/// <remarks>
	/// An S/MIME digital signature.
	/// </remarks>
	public class SecureMimeDigitalSignature : IDigitalSignature
	{
		DigitalSignatureVerifyException vex;
		bool? valid;

		static DateTime ToAdjustedDateTime (DerUtcTime time)
		{
			//try {
			//	return time.ToAdjustedDateTime ();
			//} catch {
			return DateUtils.Parse (time.AdjustedTimeString, "yyyyMMddHHmmsszzz");
			//}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.SecureMimeDigitalSignature"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="SecureMimeDigitalSignature"/>.
		/// </remarks>
		/// <param name="signerInfo">The information about the signer.</param>
		/// <param name="certificate">The signer's certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="signerInfo"/> is <c>null</c>.
		/// </exception>
		public SecureMimeDigitalSignature (SignerInformation signerInfo, X509Certificate certificate)
		{
			if (signerInfo == null)
				throw new ArgumentNullException (nameof (signerInfo));

			SignerInfo = signerInfo;

			var algorithms = new List<EncryptionAlgorithm> ();
			DigestAlgorithm digestAlgo;

			if (signerInfo.SignedAttributes != null) {
				Asn1EncodableVector vector = signerInfo.SignedAttributes.GetAll (CmsAttributes.SigningTime);
				foreach (Org.BouncyCastle.Asn1.Cms.Attribute attr in vector) {
					var signingTime = (DerUtcTime) ((DerSet) attr.AttrValues)[0];
					CreationDate = ToAdjustedDateTime (signingTime);
					break;
				}

				vector = signerInfo.SignedAttributes.GetAll (SmimeAttributes.SmimeCapabilities);
				foreach (Org.BouncyCastle.Asn1.Cms.Attribute attr in vector) {
					foreach (Asn1Sequence sequence in attr.AttrValues) {
						for (int i = 0; i < sequence.Count; i++) {
							var identifier = AlgorithmIdentifier.GetInstance (sequence[i]);
							EncryptionAlgorithm algorithm;

							if (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (identifier, out algorithm))
								algorithms.Add (algorithm);
						}
					}
				}
			}

			EncryptionAlgorithms = algorithms.ToArray ();

			if (BouncyCastleSecureMimeContext.TryGetDigestAlgorithm (signerInfo.DigestAlgorithmID, out digestAlgo))
				DigestAlgorithm = digestAlgo;

			if (certificate != null)
				SignerCertificate = new SecureMimeDigitalCertificate (certificate);
		}

		/// <summary>
		/// Gets the signer info.
		/// </summary>
		/// <remarks>
		/// Gets the signer info.
		/// </remarks>
		/// <value>The signer info.</value>
		public SignerInformation SignerInfo {
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

		/// <summary>
		/// Gets the certificate chain.
		/// </summary>
		/// <remarks>
		/// If building the certificate chain failed, this value will be <c>null</c> and
		/// <see cref="ChainException"/> will be set.
		/// </remarks>
		/// <value>The certificate chain.</value>
		public PkixCertPath Chain {
			get; internal set;
		}

		/// <summary>
		/// The exception that occurred, if any, while building the certificate chain.
		/// </summary>
		/// <remarks>
		/// This will only be set if building the certificate chain failed.
		/// </remarks>
		/// <value>The exception.</value>
		public Exception ChainException {
			get; internal set;
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
			if (valid.HasValue)
				return valid.Value;

			if (vex != null)
				throw vex;

			if (SignerCertificate == null) {
				var message = string.Format ("Failed to verify digital signature: missing certificate.");
				vex = new DigitalSignatureVerifyException (message);
				throw vex;
			}

			if (ChainException != null) {
				var message = string.Format ("Failed to verify digital signature: {0}", ChainException.Message);
				vex = new DigitalSignatureVerifyException (message, ChainException);
				throw vex;
			}

			try {
				var certificate = ((SecureMimeDigitalCertificate) SignerCertificate).Certificate;
				valid = SignerInfo.Verify (certificate);
				return valid.Value;
			} catch (Exception ex) {
				var message = string.Format ("Failed to verify digital signature: {0}", ex.Message);
				vex = new DigitalSignatureVerifyException (message, ex);
				throw vex;
			}
		}

		#endregion
	}
}
