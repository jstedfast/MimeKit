//
// X509CertificateExtensions.cs
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
using System.Text;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Digests;

namespace MimeKit.Cryptography {
	/// <summary>
	/// X509Certificate extension methods.
	/// </summary>
	public static class X509CertificateExtensions
	{
		/// <summary>
		/// Gets the issuer name info.
		/// </summary>
		/// <returns>The issuer name info.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <param name="identifier">The name identifier.</param>
		public static string GetIssuerNameInfo (this X509Certificate certificate, DerObjectIdentifier identifier)
		{
			// FIXME: this should be fixed to return IList<string>
			var list = certificate.IssuerDN.GetValueList (identifier);
			if (list.Count == 0)
				return null;

			return (string) list[0];
		}

		/// <summary>
		/// Gets the issuer name info.
		/// </summary>
		/// <returns>The issuer name info.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <param name="identifier">The name identifier.</param>
		public static string GetSubjectNameInfo (this X509Certificate certificate, DerObjectIdentifier identifier)
		{
			// FIXME: this should be fixed to return IList<string>
			var list = certificate.SubjectDN.GetValueList (identifier);
			if (list.Count == 0)
				return null;

			return (string) list[0];
		}

		/// <summary>
		/// Gets the common name of the certificate.
		/// </summary>
		/// <returns>The common name.</returns>
		/// <param name="certificate">The certificate.</param>
		public static string GetCommonName (this X509Certificate certificate)
		{
			return certificate.GetSubjectNameInfo (X509Name.CN);
		}

		/// <summary>
		/// Gets the subject name of the certificate.
		/// </summary>
		/// <returns>The subject name.</returns>
		/// <param name="certificate">The certificate.</param>
		public static string GetSubjectName (this X509Certificate certificate)
		{
			return certificate.GetSubjectNameInfo (X509Name.Name);
		}

		/// <summary>
		/// Gets the subject email address of the certificate.
		/// </summary>
		/// <returns>The subject email address.</returns>
		/// <param name="certificate">The certificate.</param>
		public static string GetSubjectEmailAddress (this X509Certificate certificate)
		{
			return certificate.GetSubjectNameInfo (X509Name.EmailAddress);
		}

		/// <summary>
		/// Gets the fingerprint of the certificate.
		/// </summary>
		/// <returns>The fingerprint.</returns>
		/// <param name="certificate">The certificate.</param>
		public static string GetFingerprint (this X509Certificate certificate)
		{
			var encoded = certificate.GetEncoded ();
			var fingerprint = new StringBuilder ();
			var sha1 = new Sha1Digest ();
			var data = new byte[20];

			sha1.BlockUpdate (encoded, 0, encoded.Length);
			sha1.DoFinal (data, 0);

			for (int i = 0; i < data.Length; i++)
				fingerprint.Append (data[i].ToString ("X2"));

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
		/// <returns>The key usage flags.</returns>
		/// <param name="certificate">The certificate.</param>
		public static X509KeyUsageFlags GetKeyUsageFlags (this X509Certificate certificate)
		{
			return GetKeyUsageFlags (certificate.GetKeyUsage ());
		}

		internal static bool IsDelta (this X509Crl crl)
		{
			var critical = crl.GetCriticalExtensionOids ();

			return critical.Contains (X509Extensions.DeltaCrlIndicator.Id);
		}
	}
}
