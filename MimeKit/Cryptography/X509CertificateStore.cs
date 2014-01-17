//
// X509CertificateStore.cs
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
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A store for X.509 certificates and keys.
	/// </summary>
	public class X509CertificateStore : IX509Store
	{
		readonly Dictionary<X509Certificate, AsymmetricKeyParameter> keys;
		readonly HashSet<X509Certificate> unique;
		readonly List<X509Certificate> certs;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CertificateStore"/> class.
		/// </summary>
		public X509CertificateStore ()
		{
			keys = new Dictionary<X509Certificate, AsymmetricKeyParameter> ();
			unique = new HashSet<X509Certificate> ();
			certs = new List<X509Certificate> ();
		}

		/// <summary>
		/// Gets a read-only list of certificates currently in the store.
		/// </summary>
		/// <value>The certificates.</value>
		public IEnumerable<X509Certificate> Certificates {
			get { return certs; }
		}

		/// <summary>
		/// Gets the private key for the specified certificate.
		/// </summary>
		/// <returns>The private key on success; otherwise <c>null</c>.</returns>
		/// <param name="certificate">The certificate.</param>
		public AsymmetricKeyParameter GetPrivateKey (X509Certificate certificate)
		{
			AsymmetricKeyParameter key;

			if (!keys.TryGetValue (certificate, out key))
				return null;

			return key;
		}

		/// <summary>
		/// Add the specified certificate.
		/// </summary>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public void Add (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (unique.Add (certificate))
				certs.Add (certificate);
		}

		/// <summary>
		/// Adds the specified range of certificates.
		/// </summary>
		/// <param name="certificates">The certificates.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificates"/> is <c>null</c>.
		/// </exception>
		public void AddRange (IEnumerable<X509Certificate> certificates)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			foreach (var certificate in certificates) {
				if (unique.Add (certificate))
					certs.Add (certificate);
			}
		}

		/// <summary>
		/// Remove the specified certificate.
		/// </summary>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public void Remove (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (unique.Remove (certificate))
				certs.Remove (certificate);
		}

		/// <summary>
		/// Removes the specified range of certificates.
		/// </summary>
		/// <param name="certificates">The certificates.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificates"/> is <c>null</c>.
		/// </exception>
		public void RemoveRange (IEnumerable<X509Certificate> certificates)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			foreach (var certificate in certificates) {
				if (unique.Remove (certificate))
					certs.Remove (certificate);
			}
		}

		/// <summary>
		/// Import the certificate(s) from the specified stream.
		/// </summary>
		/// <param name="stream">The stream to import.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred reading the stream.
		/// </exception>
		public void Import (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			var parser = new X509CertificateParser ();

			foreach (X509Certificate certificate in parser.ReadCertificates (stream)) {
				if (unique.Add (certificate))
					certs.Add (certificate);
			}
		}

		/// <summary>
		/// Import the certificate(s) from the specified file.
		/// </summary>
		/// <param name="fileName">The name of the file to import.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file could not be found.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred reading the file.
		/// </exception>
		public void Import (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			using (var stream = File.OpenRead (fileName))
				Import (stream);
		}

		/// <summary>
		/// Import the certificate(s) from the specified byte array.
		/// </summary>
		/// <param name="rawData">The raw certificate data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="rawData"/> is <c>null</c>.
		/// </exception>
		public void Import (byte[] rawData)
		{
			if (rawData == null)
				throw new ArgumentNullException ("rawData");

			using (var stream = new MemoryStream (rawData, false))
				Import (stream);
		}

		/// <summary>
		/// Import certificates and private keys from the specified stream.
		/// </summary>
		/// <param name="stream">The stream to import.</param>
		/// <param name="password">The password to unlock the stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred reading the stream.
		/// </exception>
		public void Import (Stream stream, string password)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (password == null)
				throw new ArgumentNullException ("password");

			var pkcs12 = new Pkcs12Store (stream, password.ToCharArray ());

			foreach (string alias in pkcs12.Aliases) {
				if (pkcs12.IsKeyEntry (alias)) {
					var chain = pkcs12.GetCertificateChain (alias);
					var entry = pkcs12.GetKey (alias);

					for (int i = 0; i < chain.Length; i++) {
						if (unique.Add (chain[i].Certificate))
							certs.Add (chain[i].Certificate);
					}

					if (entry.Key.IsPrivate)
						keys.Add (chain[0].Certificate, entry.Key);
				} else if (pkcs12.IsCertificateEntry (alias)) {
					var entry = pkcs12.GetCertificate (alias);

					if (unique.Add (entry.Certificate))
						certs.Add (entry.Certificate);
				}
			}
		}

		/// <summary>
		/// Import certificates and private keys from the specified file.
		/// </summary>
		/// <param name="fileName">The name of the file to import.</param>
		/// <param name="password">The password to unlock the file.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred reading the file.
		/// </exception>
		public void Import (string fileName, string password)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (string.IsNullOrEmpty (fileName))
				throw new ArgumentException ("The specified path is empty.", "fileName");

			using (var stream = File.OpenRead (fileName))
				Import (stream, password);
		}

		/// <summary>
		/// Import certificates and private keys from the specified byte array.
		/// </summary>
		/// <param name="rawData">The raw certificate data.</param>
		/// <param name="password">The password to unlock the raw data.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="rawData"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		public void Import (byte[] rawData, string password)
		{
			if (rawData == null)
				throw new ArgumentNullException ("rawData");

			using (var stream = new MemoryStream (rawData, false))
				Import (stream, password);
		}

		/// <summary>
		/// Export the certificates to an unencrypted stream.
		/// </summary>
		/// <param name="stream">The output stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred while writing to the stream.
		/// </exception>
		public void Export (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			foreach (var certificate in certs) {
				var encoded = certificate.GetEncoded ();
				stream.Write (encoded, 0, encoded.Length);
			}
		}

		/// <summary>
		/// Export the certificates to an unencrypted file.
		/// </summary>
		/// <param name="fileName">The file path to write to.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.PathTooLongException">
		/// The specified path exceeds the maximum allowed path length of the system.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// A directory in the specified path does not exist.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to create the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred while writing to the stream.
		/// </exception>
		public void Export (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (string.IsNullOrEmpty (fileName))
				throw new ArgumentException ("The specified path is empty.", "fileName");

			using (var file = File.Create (fileName)) {
				Export (file);
			}
		}

		/// <summary>
		/// Export the specified stream and password to a pkcs12-formatted file.
		/// </summary>
		/// <param name="stream">The output stream.</param>
		/// <param name="password">The password to use to lock the private keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred while writing to the stream.
		/// </exception>
		public void Export (Stream stream, string password)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (password == null)
				throw new ArgumentNullException ("password");

			var store = new Pkcs12Store ();
			foreach (var certificate in certs) {
				if (keys.ContainsKey (certificate))
					continue;

				var entry = new X509CertificateEntry (certificate);
				var alias = certificate.GetCommonName ();
				store.SetCertificateEntry (alias, entry);
			}

			foreach (var kvp in keys) {
				var entry = new AsymmetricKeyEntry (kvp.Value);
				var cert = new X509CertificateEntry (kvp.Key);
				var chain = new List<X509CertificateEntry> ();
				var alias = kvp.Key.GetCommonName ();

				chain.Add (cert);

				store.SetKeyEntry (alias, entry, chain.ToArray ());
			}

			store.Save (stream, password.ToCharArray (), new SecureRandom ());
		}

		/// <summary>
		/// Export the specified stream and password to a pkcs12-formatted file.
		/// </summary>
		/// <param name="fileName">The file path to write to.</param>
		/// <param name="password">The password to use to lock the private keys.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="password"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.PathTooLongException">
		/// The specified path exceeds the maximum allowed path length of the system.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// A directory in the specified path does not exist.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to create the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An error occurred while writing to the stream.
		/// </exception>
		public void Export (string fileName, string password)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (string.IsNullOrEmpty (fileName))
				throw new ArgumentException ("The specified path is empty.", "fileName");

			if (password == null)
				throw new ArgumentNullException ("password");

			using (var file = File.Create (fileName)) {
				Export (file, password);
			}
		}

		/// <summary>
		/// Gets an enumerator of matching <see cref="Org.BouncyCastle.X509.X509Certificate"/>s
		/// based on the specified selector.
		/// </summary>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match criteria.</param>
		public IEnumerable<X509Certificate> GetMatches (IX509Selector selector)
		{
			foreach (var certificate in certs) {
				if (selector == null || selector.Match (certificate))
					yield return certificate;
			}

			yield break;
		}

		#region IX509Store implementation

		/// <summary>
		/// Gets a collection of matching <see cref="Org.BouncyCastle.X509.X509Certificate"/>s
		/// based on the specified selector.
		/// </summary>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match criteria.</param>
		ICollection IX509Store.GetMatches (IX509Selector selector)
		{
			var matches = new List<X509Certificate> ();

			foreach (var certificate in certs) {
				if (selector == null || selector.Match (certificate))
					matches.Add (certificate);
			}

			return matches;
		}

		#endregion
	}
}
