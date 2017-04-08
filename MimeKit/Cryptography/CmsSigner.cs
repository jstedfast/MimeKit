//
// CmsSigner.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
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

#if !PORTABLE
using System.Security.Cryptography;
using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;
#endif

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An S/MIME signer.
	/// </summary>
	/// <remarks>
	/// If the X.509 certificate is known for the signer, you may wish to use a
	/// <see cref="CmsSigner"/> as opposed to having the <see cref="CryptographyContext"/>
	/// do its own certificate lookup for the signer's <see cref="MailboxAddress"/>.
	/// </remarks>
	public class CmsSigner
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsSigner"/> class.
		/// </summary>
		/// <remarks>
		/// <para>The initial value of the <see cref="DigestAlgorithm"/> will be set to
		/// <see cref="MimeKit.Cryptography.DigestAlgorithm.Sha1"/> and both the
		/// <see cref="SignedAttributes"/> and <see cref="UnsignedAttributes"/> properties
		/// will be initialized to empty tables.</para>
		/// </remarks>
		CmsSigner ()
		{
			UnsignedAttributes = new AttributeTable (new Dictionary<DerObjectIdentifier, Asn1Encodable> ());
			SignedAttributes = new AttributeTable (new Dictionary<DerObjectIdentifier, Asn1Encodable> ());
			DigestAlgorithm = DigestAlgorithm.Sha1;
		}

		static void CheckCertificateCanBeUsedForSigning (X509Certificate certificate)
		{
			var flags = certificate.GetKeyUsageFlags ();

			if (flags != X509KeyUsageFlags.None && (flags & SecureMimeContext.DigitalSignatureKeyUsageFlags) == 0)
				throw new ArgumentException ("The certificate cannot be used for signing.");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsSigner"/> class.
		/// </summary>
		/// <remarks>
		/// <para>The initial value of the <see cref="DigestAlgorithm"/> will be set to
		/// <see cref="MimeKit.Cryptography.DigestAlgorithm.Sha1"/> and both the
		/// <see cref="SignedAttributes"/> and <see cref="UnsignedAttributes"/> properties
		/// will be initialized to empty tables.</para>
		/// </remarks>
		/// <param name="chain">The chain of certificates starting with the signer's certificate back to the root.</param>
		/// <param name="key">The signer's private key.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="chain"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="key"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="chain"/> did not contain any certificates.</para>
		/// <para>-or-</para>
		/// <para>The certificate cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para><paramref name="key"/> is not a private key.</para>
		/// </exception>
		public CmsSigner (IEnumerable<X509Certificate> chain, AsymmetricKeyParameter key) : this ()
		{
			if (chain == null)
				throw new ArgumentNullException (nameof (chain));

			if (key == null)
				throw new ArgumentNullException (nameof (key));

			CertificateChain = new X509CertificateChain (chain);

			if (CertificateChain.Count == 0)
				throw new ArgumentException ("The certificate chain was empty.", nameof (chain));

			CheckCertificateCanBeUsedForSigning (CertificateChain[0]);

			if (!key.IsPrivate)
				throw new ArgumentException ("The key must be a private key.", nameof (key));

			Certificate = CertificateChain[0];
			PrivateKey = key;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsSigner"/> class.
		/// </summary>
		/// <remarks>
		/// <para>The initial value of the <see cref="MimeKit.Cryptography.DigestAlgorithm"/> will
		/// be set to <see cref="MimeKit.Cryptography.DigestAlgorithm.Sha1"/> and both the
		/// <see cref="SignedAttributes"/> and <see cref="UnsignedAttributes"/> properties will be
		/// initialized to empty tables.</para>
		/// </remarks>
		/// <param name="certificate">The signer's certificate.</param>
		/// <param name="key">The signer's private key.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="certificate"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="key"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="certificate"/> cannot be used for signing.</para>
		/// <para>-or-</para>
		/// <para><paramref name="key"/> is not a private key.</para>
		/// </exception>
		public CmsSigner (X509Certificate certificate, AsymmetricKeyParameter key) : this ()
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			CheckCertificateCanBeUsedForSigning (certificate);

			if (key == null)
				throw new ArgumentNullException (nameof (key));

			if (!key.IsPrivate)
				throw new ArgumentException ("The key must be a private key.", nameof (key));

			CertificateChain = new X509CertificateChain ();
			CertificateChain.Add (certificate);
			Certificate = certificate;
			PrivateKey = key;
		}

		void LoadPkcs12 (Stream stream, string password)
		{
			var pkcs12 = new Pkcs12Store (stream, password.ToCharArray ());

			foreach (string alias in pkcs12.Aliases) {
				if (!pkcs12.IsKeyEntry (alias))
					continue;

				var chain = pkcs12.GetCertificateChain (alias);
				var key = pkcs12.GetKey (alias);

				if (!key.Key.IsPrivate || chain.Length == 0)
					continue;

				var flags = chain[0].Certificate.GetKeyUsageFlags ();

				if (flags != X509KeyUsageFlags.None && (flags & SecureMimeContext.DigitalSignatureKeyUsageFlags) == 0)
					continue;

				CheckCertificateCanBeUsedForSigning (chain[0].Certificate);

				CertificateChain = new X509CertificateChain ();
				Certificate = chain[0].Certificate;
				PrivateKey = key.Key;

				foreach (var entry in chain)
					CertificateChain.Add (entry.Certificate);

				break;
			}

			if (PrivateKey == null)
				throw new ArgumentException ("The stream did not contain a private key.", nameof (stream));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsSigner"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="CmsSigner"/>, loading the X.509 certificate and private key
		/// from the specified stream.</para>
		/// <para>The initial value of the <see cref="MimeKit.Cryptography.DigestAlgorithm"/> will
		/// be set to <see cref="MimeKit.Cryptography.DigestAlgorithm.Sha1"/> and both the
		/// <see cref="SignedAttributes"/> and <see cref="UnsignedAttributes"/> properties will be
		/// initialized to empty tables.</para>
		/// </remarks>
		/// <param name="stream">The raw certificate and key data in pkcs12 format.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="stream"/> does not contain a private key.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public CmsSigner (Stream stream, string password) : this ()
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (password == null)
				throw new ArgumentNullException (nameof (password));

			LoadPkcs12 (stream, password);
		}

#if !PORTABLE
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsSigner"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="CmsSigner"/>, loading the X.509 certificate and private key
		/// from the specified file.</para>
		/// <para>The initial value of the <see cref="MimeKit.Cryptography.DigestAlgorithm"/> will
		/// be set to <see cref="MimeKit.Cryptography.DigestAlgorithm.Sha1"/> and both the
		/// <see cref="SignedAttributes"/> and <see cref="UnsignedAttributes"/> properties will be
		/// initialized to empty tables.</para>
		/// </remarks>
		/// <param name="fileName">The raw certificate and key data in pkcs12 format.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters as defined by
		/// <see cref="System.IO.Path.InvalidPathChars"/>.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public CmsSigner (string fileName, string password) : this ()
		{
			if (fileName == null)
				throw new ArgumentNullException (nameof (fileName));

			if (password == null)
				throw new ArgumentNullException (nameof (password));

			using (var stream = File.OpenRead (fileName))
				LoadPkcs12 (stream, password);
		}
#endif

#if !PORTABLE && !NETSTANDARD
		static X509Certificate GetBouncyCastleCertificate (X509Certificate2 certificate)
		{
			var rawData = certificate.GetRawCertData ();

			return new X509CertificateParser ().ReadCertificate (rawData);
		}

		static AsymmetricCipherKeyPair GetBouncyCastleDsaKeyPair (DSA dsa)
		{
			var dp = dsa.ExportParameters (true);
			var validationParameters = dp.Seed != null ?
				new DsaValidationParameters (dp.Seed, dp.Counter) : null;
			var parameters = new DsaParameters (
				new BigInteger (1, dp.P),
				new BigInteger (1, dp.Q),
				new BigInteger (1, dp.G),
				validationParameters);
			var pubKey = new DsaPublicKeyParameters (new BigInteger (1, dp.Y), parameters);
			var privKey = new DsaPrivateKeyParameters (new BigInteger (1, dp.X), parameters);

			return new AsymmetricCipherKeyPair (pubKey, privKey);
		}

		static AsymmetricCipherKeyPair GetBouncyCastleRsaKeyPair (RSA rsa)
		{
			var rp = rsa.ExportParameters (true);
			var modulus = new BigInteger (1, rp.Modulus);
			var exponent = new BigInteger (1, rp.Exponent);
			var pubKey = new RsaKeyParameters (false, modulus, exponent);
			var privKey = new RsaPrivateCrtKeyParameters (modulus, exponent,
				new BigInteger (1, rp.D),
				new BigInteger (1, rp.P),
				new BigInteger (1, rp.Q),
				new BigInteger (1, rp.DP),
				new BigInteger (1, rp.DQ),
				new BigInteger (1, rp.InverseQ));

			return new AsymmetricCipherKeyPair (pubKey, privKey);
		}

		internal static AsymmetricCipherKeyPair GetBouncyCastleKeyPair (AsymmetricAlgorithm privateKey)
		{
			if (privateKey is DSA)
				return GetBouncyCastleDsaKeyPair ((DSA) privateKey);

			if (privateKey is RSA)
				return GetBouncyCastleRsaKeyPair ((RSA) privateKey);

			throw new ArgumentException ("Unsupported algorithm specified", "privateKey");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CmsSigner"/> class.
		/// </summary>
		/// <remarks>
		/// <para>The initial value of the <see cref="MimeKit.Cryptography.DigestAlgorithm"/> will
		/// be set to <see cref="MimeKit.Cryptography.DigestAlgorithm.Sha1"/> and both the
		/// <see cref="SignedAttributes"/> and <see cref="UnsignedAttributes"/> properties will be
		/// initialized to empty tables.</para>
		/// </remarks>
		/// <param name="certificate">The signer's certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="certificate"/> cannot be used for signing.
		/// </exception>
		public CmsSigner (X509Certificate2 certificate) : this ()
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			if (!certificate.HasPrivateKey)
				throw new ArgumentException ("The certificate does not contain a private key.", nameof (certificate));

			var cert = GetBouncyCastleCertificate (certificate);
			var key = GetBouncyCastleKeyPair (certificate.PrivateKey);

			CheckCertificateCanBeUsedForSigning (cert);

			CertificateChain = new X509CertificateChain ();
			CertificateChain.Add (cert);
			Certificate = cert;
			PrivateKey = key.Private;
		}
#endif

		/// <summary>
		/// Gets the signer's certificate.
		/// </summary>
		/// <remarks>
		/// The signer's certificate that contains a public key that can be used for
		/// verifying the digital signature.
		/// </remarks>
		/// <value>The signer's certificate.</value>
		public X509Certificate Certificate {
			get; private set;
		}

		/// <summary>
		/// Gets the certificate chain.
		/// </summary>
		/// <remarks>
		/// Gets the certificate chain.
		/// </remarks>
		/// <value>The certificate chain.</value>
		public X509CertificateChain CertificateChain {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the digest algorithm.
		/// </summary>
		/// <remarks>
		/// Specifies which digest algorithm to use to generate the
		/// cryptographic hash of the content being signed.
		/// </remarks>
		/// <value>The digest algorithm.</value>
		public DigestAlgorithm DigestAlgorithm {
			get; set;
		}

		/// <summary>
		/// Gets the private key.
		/// </summary>
		/// <remarks>
		/// The private key used for signing.
		/// </remarks>
		/// <value>The private key.</value>
		public AsymmetricKeyParameter PrivateKey {
			get; private set;
		}

		/// <summary>
		/// Gets or sets the signed attributes.
		/// </summary>
		/// <remarks>
		/// A table of attributes that should be included in the signature.
		/// </remarks>
		/// <value>The signed attributes.</value>
		public AttributeTable SignedAttributes {
			get; set;
		}

		/// <summary>
		/// Gets or sets the unsigned attributes.
		/// </summary>
		/// <remarks>
		/// A table of attributes that should not be signed in the signature,
		/// but still included in transport.
		/// </remarks>
		/// <value>The unsigned attributes.</value>
		public AttributeTable UnsignedAttributes {
			get; set;
		}
	}
}
