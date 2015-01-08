//
// X509CertificateExtensions.cs
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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Digests;

namespace MimeKit.Cryptography {
	/// <summary>
	/// X509Certificate extension methods.
	/// </summary>
	/// <remarks>
	/// A collection of useful extension methods for an <see cref="Org.BouncyCastle.X509.X509Certificate"/>.
	/// </remarks>
	public static class X509CertificateExtensions
	{
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
				throw new ArgumentNullException ("certificate");

			// FIXME: GetValueList() should be fixed to return IList<string>
			var list = certificate.IssuerDN.GetValueList (identifier);
			if (list.Count == 0)
				return null;

			return (string) list[0];
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
				throw new ArgumentNullException ("certificate");

			// FIXME: GetValueList() should be fixed to return IList<string>
			var list = certificate.SubjectDN.GetValueList (identifier);
			if (list.Count == 0)
				return null;

			return (string) list[0];
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
			return certificate.GetSubjectNameInfo (X509Name.EmailAddress);
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
				throw new ArgumentNullException ("certificate");

			var encoded = certificate.GetEncoded ();
			var fingerprint = new StringBuilder ();
			var sha1 = new Sha1Digest ();
			var data = new byte[20];

			sha1.BlockUpdate (encoded, 0, encoded.Length);
			sha1.DoFinal (data, 0);

			for (int i = 0; i < data.Length; i++)
				fingerprint.Append (data[i].ToString ("x2"));

			return fingerprint.ToString ();
		}

		internal static X509KeyUsageFlags GetKeyUsageFlags (bool[] usage)
		{
			var flags = X509KeyUsageFlags.None;

			if (usage != null) {
				if (usage[(int) X509KeyUsageBits.DigitalSignature])
					flags |= X509KeyUsageFlags.DigitalSignature;
				if (usage[(int) X509KeyUsageBits.NonRepudiation])
					flags |= X509KeyUsageFlags.NonRepudiation;
				if (usage[(int) X509KeyUsageBits.KeyEncipherment])
					flags |= X509KeyUsageFlags.KeyEncipherment;
				if (usage[(int) X509KeyUsageBits.DataEncipherment])
					flags |= X509KeyUsageFlags.DataEncipherment;
				if (usage[(int) X509KeyUsageBits.KeyAgreement])
					flags |= X509KeyUsageFlags.KeyAgreement;
				if (usage[(int) X509KeyUsageBits.KeyCertSign])
					flags |= X509KeyUsageFlags.KeyCertSign;
				if (usage[(int) X509KeyUsageBits.CrlSign])
					flags |= X509KeyUsageFlags.CrlSign;
				if (usage[(int) X509KeyUsageBits.EncipherOnly])
					flags |= X509KeyUsageFlags.EncipherOnly;
				if (usage[(int) X509KeyUsageBits.DecipherOnly])
					flags |= X509KeyUsageFlags.DecipherOnly;
			}

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
				throw new ArgumentNullException ("certificate");

			return GetKeyUsageFlags (certificate.GetKeyUsage ());
		}

		internal static bool IsDelta (this X509Crl crl)
		{
			var critical = crl.GetCriticalExtensionOids ();

			return critical.Contains (X509Extensions.DeltaCrlIndicator.Id);
		}
	}
}
