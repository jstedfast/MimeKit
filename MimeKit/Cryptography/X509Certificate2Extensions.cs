//
// X509Certificate2Extensions.cs
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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;

using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;
using X509Extension = System.Security.Cryptography.X509Certificates.X509Extension;

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
		/// <paramref name="certificate"/> is <see langword="null"/>.
		/// </exception>
		public static X509Certificate AsBouncyCastleCertificate (this X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			try {
				return DotNetUtilities.FromX509Certificate (certificate);
			} catch {
				throw new ArgumentException ("Cannot convert X509Certificate2 to a BouncyCastle X509Certificate.", nameof (certificate));
			}
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
		/// <paramref name="certificate"/> is <see langword="null"/>.
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

		static string[] GetSubjectAlternativeNames (X509Certificate2 certificate, int tagNo)
		{
			X509Extension alt = null;

			foreach (var extension in certificate.Extensions) {
				if (extension.Oid.Value == X509Extensions.SubjectAlternativeName.Id) {
					alt = extension;
					break;
				}
			}

			if (alt == null)
				return Array.Empty<string> ();

			var seq = Asn1Sequence.GetInstance (alt.RawData);
			var names = new string[seq.Count];
			int count = 0;

			foreach (Asn1Encodable encodable in seq) {
				var name = GeneralName.GetInstance (encodable);
				if (name.TagNo == tagNo)
					names[count++] = ((IAsn1String) name.Name).GetString ();
			}

			if (count == 0)
				return Array.Empty<string> ();

			if (count < names.Length)
				Array.Resize (ref names, count);

			return names;
		}

		/// <summary>
		/// Get the subject domain names of the certificate.
		/// </summary>
		/// <remarks>
		/// <para>Gets the subject DNS names of the certificate.</para>
		/// <para>Some S/MIME certificates are domain-bound instead of being bound to a
		/// particular email address.</para>
		/// </remarks>
		/// <returns>The subject DNS names.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <param name="idnEncode">If set to <see langword="true" />, international domain names will be IDN encoded.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <see langword="null"/>.
		/// </exception>
		public static string[] GetSubjectDnsNames (this X509Certificate2 certificate, bool idnEncode = false)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			var domains = GetSubjectAlternativeNames (certificate, GeneralName.DnsName);

			if (idnEncode) {
				for (int i = 0; i < domains.Length; i++)
					domains[i] = MailboxAddress.IdnMapping.Encode (domains[i]);
			} else {
				for (int i = 0; i < domains.Length; i++)
					domains[i] = MailboxAddress.IdnMapping.Decode (domains[i]);
			}

			return domains;
		}

		static EncryptionAlgorithm[] DecodeEncryptionAlgorithms (byte[] rawData)
		{
			AlgorithmIdentifier[] capabilities;
			try {
				// TODO Ideally would use SmimeCapabilities (containing SmimeCapability)
				capabilities = Asn1Sequence.GetInstance (rawData).MapElements (AlgorithmIdentifier.GetInstance);
			} catch {
				return null;
			}

			var algorithms = new List<EncryptionAlgorithm> ();

			foreach (AlgorithmIdentifier capability in capabilities) {
				if (BouncyCastleSecureMimeContext.TryGetEncryptionAlgorithm (capability, out var algorithm))
					algorithms.Add (algorithm);
			}

			return algorithms.ToArray ();
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
		/// <paramref name="certificate"/> is <see langword="null"/>.
		/// </exception>
		public static EncryptionAlgorithm[] GetEncryptionAlgorithms (this X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			foreach (var extension in certificate.Extensions) {
				// PkcsObjectIdentifiers.Pkcs9AtSmimeCapabilities
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
		/// <paramref name="certificate"/> is <see langword="null"/>.
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
