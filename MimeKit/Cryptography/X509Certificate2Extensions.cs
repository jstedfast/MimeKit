//
// X509Certificate2Extensions.cs
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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.X509;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;

namespace MimeKit.Cryptography {
	/// <summary>
	/// Extension methods for X509Certificate2.
	/// </summary>
	/// <remarks>
	/// Extension methods for X509Certificate2.
	/// </remarks>
	public static class X509Certificate2Extensions
	{
		/// <summary>
		/// Convert an X509Certificate2 into a BouncyCastle X509Certificate.
		/// </summary>
		/// <remarks>
		/// Converts an X509Certificate2 into a BouncyCastle X509Certificate.
		/// </remarks>
		/// <returns>The bouncy castle certificate.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public static X509Certificate AsBouncyCastleCertificate (this X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			var rawData = certificate.GetRawCertData ();
			var parser = new X509CertificateParser ();
			var cert = parser.ReadCertificate (rawData);

			if (cert == null)
				throw new ArgumentException ("Cannot convert X509Certificate2 to a BouncyCastle X509Certificate.", nameof (certificate));

			return cert;
		}

		/// <summary>
		/// Gets the public key algorithm for the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the public key algorithm for the ceretificate.
		/// </remarks>
		/// <returns>The public key algorithm.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public static PublicKeyAlgorithm GetPublicKeyAlgorithm (this X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			var identifier = certificate.GetKeyAlgorithm ();
			var oid = new Oid (identifier);

			switch (oid.FriendlyName) {
			case "DSA": return PublicKeyAlgorithm.Dsa;
			case "RSA": return PublicKeyAlgorithm.RsaGeneral;
			case "ECC": return PublicKeyAlgorithm.EllipticCurve;
			case "DH": return PublicKeyAlgorithm.DiffieHellman;
			default: return PublicKeyAlgorithm.None;
			}
		}

		static EncryptionAlgorithm[] DecodeEncryptionAlgorithms (byte[] rawData)
		{
			using (var memory = new MemoryStream (rawData, false)) {
				using (var asn1 = new Asn1InputStream (memory)) {
					if (asn1.ReadObject () is not Asn1Sequence sequence)
						return null;

					var algorithms = new List<EncryptionAlgorithm> ();

					for (int i = 0; i < sequence.Count; i++) {
						var identifier = AlgorithmIdentifier.GetInstance (sequence[i]);

						if (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (identifier, out var algorithm))
							algorithms.Add (algorithm);
					}

					return algorithms.ToArray ();
				}
			}
		}

		/// <summary>
		/// Get the encryption algorithms that can be used with an X.509 certificate.
		/// </summary>
		/// <remarks>
		/// <para>Scans the X.509 certificate for the S/MIME capabilities extension. If found,
		/// the supported encryption algorithms will be decoded and returned.</para>
		/// <para>If no extension can be found, the <see cref="EncryptionAlgorithm.TripleDes"/>
		/// algorithm is returned.</para>
		/// </remarks>
		/// <returns>The encryption algorithms.</returns>
		/// <param name="certificate">The X.509 certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public static EncryptionAlgorithm[] GetEncryptionAlgorithms (this X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			foreach (var extension in certificate.Extensions) {
				if (extension.Oid.Value == "1.2.840.113549.1.9.15") {
					var algorithms = DecodeEncryptionAlgorithms (extension.RawData);

					if (algorithms != null)
						return algorithms;

					break;
				}
			}

			return new EncryptionAlgorithm[] { EncryptionAlgorithm.TripleDes };
		}

		/// <summary>
		/// Get the PrivateKey property as a BouncyCastle AsymmetricKeyParameter.
		/// </summary>
		/// <remarks>
		/// Gets the PrivateKey property as a BouncyCastle AsymmetricKeyParameter.
		/// </remarks>
		/// <returns>The asymmetric key parameter.</returns>
		/// <param name="certificate">The X.509 certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public static AsymmetricKeyParameter GetPrivateKeyAsAsymmetricKeyParameter (this X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

#if NET6_0_OR_GREATER
			AsymmetricAlgorithm privateKey = null;

			if (certificate.HasPrivateKey) {
				switch (GetPublicKeyAlgorithm (certificate)) {
				case PublicKeyAlgorithm.Dsa:
					privateKey = certificate.GetDSAPrivateKey ();
					break;
				case PublicKeyAlgorithm.RsaGeneral:
					privateKey = certificate.GetRSAPrivateKey ();
					break;
				case PublicKeyAlgorithm.EllipticCurve:
					//privateKey = certificate.GetECDsaPrivateKey ();
					privateKey = certificate.GetECDiffieHellmanPrivateKey ();
					break;
				}
			}

			return privateKey?.AsAsymmetricKeyParameter ();
#else
			return certificate.PrivateKey?.AsAsymmetricKeyParameter ();
#endif
		}
	}
}
