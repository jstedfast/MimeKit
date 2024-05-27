//
// BouncyCastleCertificateExtensions.cs
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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Smime;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;

using MimeKit.Utils;

using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;

namespace MimeKit.Cryptography {
	/// <summary>
	/// Extension methods for use with BouncyCastle X509Certificates.
	/// </summary>
	/// <remarks>
	/// Extension methods for use with BouncyCastle X509Certificates.
	/// </remarks>
	public static class BouncyCastleCertificateExtensions
	{
		/// <summary>
		/// Convert a BouncyCastle certificate into an X509Certificate2.
		/// </summary>
		/// <remarks>
		/// Converts a BouncyCastle certificate into an X509Certificate2.
		/// </remarks>
		/// <returns>The X509Certificate2.</returns>
		/// <param name="certificate">The BouncyCastle certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public static X509Certificate2 AsX509Certificate2 (this X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			return new X509Certificate2 (certificate.GetEncoded ());
		}

		internal static bool IsSelfSigned (this X509Certificate certificate)
		{
			return certificate.SubjectDN.Equivalent (certificate.IssuerDN);
		}

		/// <summary>
		/// Gets the issuer name info.
		/// </summary>
		/// <remarks>
		/// For a list of available identifiers, see <see cref="Org.BouncyCastle.Asn1.X509.X509Name"/>.
		/// </remarks>
		/// <returns>The issuer name info.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <param name="identifier">The name identifier.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public static string GetIssuerNameInfo (this X509Certificate certificate, DerObjectIdentifier identifier)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			var list = certificate.IssuerDN.GetValueList (identifier);
			if (list.Count == 0)
				return string.Empty;

			return list[0];
		}

		/// <summary>
		/// Gets the issuer name info.
		/// </summary>
		/// <remarks>
		/// For a list of available identifiers, see <see cref="Org.BouncyCastle.Asn1.X509.X509Name"/>.
		/// </remarks>
		/// <returns>The issuer name info.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <param name="identifier">The name identifier.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public static string GetSubjectNameInfo (this X509Certificate certificate, DerObjectIdentifier identifier)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			var list = certificate.SubjectDN.GetValueList (identifier);
			if (list.Count == 0)
				return string.Empty;

			return list[0];
		}

		/// <summary>
		/// Gets the common name of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the common name of the certificate.
		/// </remarks>
		/// <returns>The common name.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public static string GetCommonName (this X509Certificate certificate)
		{
			return certificate.GetSubjectNameInfo (X509Name.CN);
		}

		/// <summary>
		/// Gets the subject name of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the subject name of the certificate.
		/// </remarks>
		/// <returns>The subject name.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public static string GetSubjectName (this X509Certificate certificate)
		{
			return certificate.GetSubjectNameInfo (X509Name.Name);
		}

		/// <summary>
		/// Gets the subject email address of the certificate.
		/// </summary>
		/// <remarks>
		/// The email address component of the certificate's Subject identifier is
		/// sometimes used as a way of looking up certificates for a particular
		/// user if a fingerprint is not available.
		/// </remarks>
		/// <returns>The subject email address.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public static string GetSubjectEmailAddress (this X509Certificate certificate)
		{
			var address = certificate.GetSubjectNameInfo (X509Name.EmailAddress);

			if (!string.IsNullOrEmpty (address))
				return address;

			var alt = certificate.GetExtensionValue (X509Extensions.SubjectAlternativeName);

			if (alt == null)
				return string.Empty;

			var seq = Asn1Sequence.GetInstance (Asn1Object.FromByteArray (alt.GetOctets ()));

			foreach (Asn1Encodable encodable in seq) {
				var name = GeneralName.GetInstance (encodable);

				if (name.TagNo == GeneralName.Rfc822Name)
					return ((IAsn1String) name.Name).GetString ();
			}

			return null;
		}

		internal static string AsHex (this byte[] blob)
		{
			var hex = new ValueStringBuilder (blob.Length * 2);

			for (int i = 0; i < blob.Length; i++)
				hex.AppendInvariant (blob[i], "x2");

			return hex.ToString ();
		}

		/// <summary>
		/// Gets the fingerprint of the certificate.
		/// </summary>
		/// <remarks>
		/// A fingerprint is a SHA-1 hash of the raw certificate data and is often used
		/// as a unique identifier for a particular certificate in a certificate store.
		/// </remarks>
		/// <returns>The fingerprint.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public static string GetFingerprint (this X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			var encoded = certificate.GetEncoded ();
			var sha1 = new Sha1Digest ();

			sha1.BlockUpdate (encoded, 0, encoded.Length);

			var fingerprint = new byte[20];
			sha1.DoFinal (fingerprint, 0);

			return fingerprint.AsHex ();
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
		public static PublicKeyAlgorithm GetPublicKeyAlgorithm (this X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			var pubkey = certificate.GetPublicKey ();

			if (pubkey is DsaKeyParameters)
				return PublicKeyAlgorithm.Dsa;
			if (pubkey is RsaKeyParameters)
				return PublicKeyAlgorithm.RsaGeneral;
			if (pubkey is ElGamalKeyParameters)
				return PublicKeyAlgorithm.ElGamalGeneral;
			if (pubkey is ECKeyParameters)
				return PublicKeyAlgorithm.EllipticCurve;
			if (pubkey is DHKeyParameters)
				return PublicKeyAlgorithm.DiffieHellman;

			return PublicKeyAlgorithm.None;
		}

		internal static X509KeyUsageFlags GetKeyUsageFlags (bool[] usage)
		{
			var flags = X509KeyUsageFlags.None;

			if (usage == null || usage[(int) X509KeyUsageBits.DigitalSignature])
				flags |= X509KeyUsageFlags.DigitalSignature;
			if (usage == null || usage[(int) X509KeyUsageBits.NonRepudiation])
				flags |= X509KeyUsageFlags.NonRepudiation;
			if (usage == null || usage[(int) X509KeyUsageBits.KeyEncipherment])
				flags |= X509KeyUsageFlags.KeyEncipherment;
			if (usage == null || usage[(int) X509KeyUsageBits.DataEncipherment])
				flags |= X509KeyUsageFlags.DataEncipherment;
			if (usage == null || usage[(int) X509KeyUsageBits.KeyAgreement])
				flags |= X509KeyUsageFlags.KeyAgreement;
			if (usage == null || usage[(int) X509KeyUsageBits.KeyCertSign])
				flags |= X509KeyUsageFlags.KeyCertSign;
			if (usage == null || usage[(int) X509KeyUsageBits.CrlSign])
				flags |= X509KeyUsageFlags.CrlSign;
			if (usage == null || usage[(int) X509KeyUsageBits.EncipherOnly])
				flags |= X509KeyUsageFlags.EncipherOnly;
			if (usage == null || usage[(int) X509KeyUsageBits.DecipherOnly])
				flags |= X509KeyUsageFlags.DecipherOnly;

			return flags;
		}

		/// <summary>
		/// Gets the key usage flags.
		/// </summary>
		/// <remarks>
		/// The X.509 Key Usage Flags are used to determine which operations a certificate
		/// may be used for.
		/// </remarks>
		/// <returns>The key usage flags.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public static X509KeyUsageFlags GetKeyUsageFlags (this X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			return GetKeyUsageFlags (certificate.GetKeyUsage ());
		}

		static EncryptionAlgorithm[] DecodeEncryptionAlgorithms (byte[] rawData)
		{
			using (var memory = new MemoryStream (rawData, false)) {
				using (var asn1 = new Asn1InputStream (memory)) {
					var algorithms = new List<EncryptionAlgorithm> ();

					if (asn1.ReadObject () is not Asn1Sequence sequence)
						return null;

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
		public static EncryptionAlgorithm[] GetEncryptionAlgorithms (this X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			var capabilities = certificate.GetExtensionValue (SmimeAttributes.SmimeCapabilities);

			if (capabilities != null)
				return DecodeEncryptionAlgorithms (capabilities.GetOctets ());

			return new EncryptionAlgorithm[] { EncryptionAlgorithm.TripleDes };
		}

		internal static bool IsDelta (this X509Crl crl)
		{
			var critical = crl.GetCriticalExtensionOids ();

			return critical != null && critical.Contains (X509Extensions.DeltaCrlIndicator.Id);
		}
	}
}
