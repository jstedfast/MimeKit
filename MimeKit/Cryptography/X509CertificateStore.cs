//
// X509CertificateStore.cs
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
using System.IO;
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
	/// <remarks>
	/// A store for X.509 certificates and keys.
	/// </remarks>
	public class X509CertificateStore : IX509Store
	{
		readonly Dictionary<X509Certificate, AsymmetricKeyParameter> keys;
		readonly HashSet<X509Certificate> unique;
		readonly List<X509Certificate> certs;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CertificateStore"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="X509CertificateStore"/>.
		/// </remarks>
		public X509CertificateStore ()
		{
			keys = new Dictionary<X509Certificate, AsymmetricKeyParameter> ();
			unique = new HashSet<X509Certificate> ();
			certs = new List<X509Certificate> ();
		}

		/// <summary>
		/// Enumerates the certificates currently in the store.
		/// </summary>
		/// <remarks>
		/// Enumerates the certificates currently in the store.
		/// </remarks>
		/// <value>The certificates.</value>
		public IEnumerable<X509Certificate> Certificates {
			get { return certs; }
		}

		/// <summary>
		/// Gets the private key for the specified certificate.
		/// </summary>
		/// <remarks>
		/// Gets the private key for the specified certificate, if it exists.
		/// </remarks>
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
		/// Adds the specified certificate to the store.
		/// </summary>
		/// <remarks>
		/// Adds the specified certificate to the store.
		/// </remarks>
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
		/// Adds the specified range of certificates to the store.
		/// </summary>
		/// <remarks>
		/// Adds the specified range of certificates to the store.
		/// </remarks>
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
		/// Removes the specified certificate from the store.
		/// </summary>
		/// <remarks>
		/// Removes the specified certificate from the store.
		/// </remarks>
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
		/// Removes the specified range of certificates from the store.
		/// </summary>
		/// <remarks>
		/// Removes the specified range of certificates from the store.
		/// </remarks>
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
		/// Imports the certificate(s) from the specified stream.
		/// </summary>
		/// <remarks>
		/// Imports the certificate(s) from the specified stream.
		/// </remarks>
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
		/// Imports the certificate(s) from the specified file.
		/// </summary>
		/// <remarks>
		/// Imports the certificate(s) from the specified file.
		/// </remarks>
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
		/// Imports the certificate(s) from the specified byte array.
		/// </summary>
		/// <remarks>
		/// Imports the certificate(s) from the specified byte array.
		/// </remarks>
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
		/// Imports certificates and private keys from the specified stream.
		/// </summary>
		/// <remarks>
		/// <para>Imports certificates and private keys from the specified pkcs12 stream.</para>
		/// </remarks>
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
		/// Imports certificates and private keys from the specified file.
		/// </summary>
		/// <remarks>
		/// <para>Imports certificates and private keys from the specified pkcs12 stream.</para>
		/// </remarks>
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
		/// Imports certificates and private keys from the specified byte array.
		/// </summary>
		/// <remarks>
		/// <para>Imports certificates and private keys from the specified pkcs12 stream.</para>
		/// </remarks>
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
		/// Exports the certificates to an unencrypted stream.
		/// </summary>
		/// <remarks>
		/// Exports the certificates to an unencrypted stream.
		/// </remarks>
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
		/// Exports the certificates to an unencrypted file.
		/// </summary>
		/// <remarks>
		/// Exports the certificates to an unencrypted file.
		/// </remarks>
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

			using (var file = File.Create (fileName))
				Export (file);
		}

		/// <summary>
		/// Exports the specified stream and password to a pkcs12 encrypted file.
		/// </summary>
		/// <remarks>
		/// Exports the specified stream and password to a pkcs12 encrypted file.
		/// </remarks>
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

				var alias = certificate.GetCommonName ();

				if (alias == null)
					continue;

				var entry = new X509CertificateEntry (certificate);

				store.SetCertificateEntry (alias, entry);
			}

			foreach (var kvp in keys) {
				var alias = kvp.Key.GetCommonName ();

				if (alias == null)
					continue;

				var entry = new AsymmetricKeyEntry (kvp.Value);
				var cert = new X509CertificateEntry (kvp.Key);
				var chain = new List<X509CertificateEntry> ();

				chain.Add (cert);

				store.SetKeyEntry (alias, entry, chain.ToArray ());
			}

			store.Save (stream, password.ToCharArray (), new SecureRandom ());
		}

		/// <summary>
		/// Exports the specified stream and password to a pkcs12 encrypted file.
		/// </summary>
		/// <remarks>
		/// Exports the specified stream and password to a pkcs12 encrypted file.
		/// </remarks>
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

			using (var file = File.Create (fileName))
				Export (file, password);
		}

		/// <summary>
		/// Gets an enumerator of matching X.509 certificates based on the specified selector.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator of matching X.509 certificates based on the specified selector.
		/// </remarks>
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
		/// Gets a collection of matching X.509 certificates based on the specified selector.
		/// </summary>
		/// <remarks>
		/// Gets a collection of matching X.509 certificates based on the specified selector.
		/// </remarks>
		/// <returns>The matching certificates.</returns>
		/// <param name="selector">The match criteria.</param>
		ICollection IX509Store.GetMatches (IX509Selector selector)
		{
			var matches = new List<X509Certificate> ();

			foreach (var certificate in GetMatches (selector))
				matches.Add (certificate);

			return matches;
		}

		#endregion
	}
}
