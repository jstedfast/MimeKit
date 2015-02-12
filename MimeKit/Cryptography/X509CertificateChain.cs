//
// X509CertificateChain.cs
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
using System.Collections;
using System.Collections.Generic;

using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An X.509 certificate chain.
	/// </summary>
	/// <remarks>
	/// An X.509 certificate chain.
	/// </remarks>
	public class X509CertificateChain : IList<X509Certificate>, IX509Store
	{
		readonly List<X509Certificate> certificates;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CertificateChain"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new X.509 certificate chain.
		/// </remarks>
		public X509CertificateChain ()
		{
			certificates = new List<X509Certificate> ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.X509CertificateChain"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new X.509 certificate chain based on the specified collection of certificates.
		/// </remarks>
		/// <param name="collection">A collection of certificates.</param>
		public X509CertificateChain (IEnumerable<X509Certificate> collection)
		{
			certificates = new List<X509Certificate> (collection);
		}

		#region IList implementation

		/// <summary>
		/// Gets the index of the specified certificate within the chain.
		/// </summary>
		/// <remarks>
		/// Finds the index of the specified certificate, if it exists.
		/// </remarks>
		/// <returns>The index of the specified certificate if found; otherwise <c>-1</c>.</returns>
		/// <param name="certificate">The certificate to get the index of.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public int IndexOf (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			return certificates.IndexOf (certificate);
		}

		/// <summary>
		/// Inserts the certificate at the specified index.
		/// </summary>
		/// <remarks>
		/// Inserts the certificate at the specified index in the certificates.
		/// </remarks>
		/// <param name="index">The index to insert the certificate.</param>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void Insert (int index, X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			certificates.Insert (index, certificate);
		}

		/// <summary>
		/// Removes the certificate at the specified index.
		/// </summary>
		/// <remarks>
		/// Removes the certificate at the specified index.
		/// </remarks>
		/// <param name="index">The index of the certificate to remove.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void RemoveAt (int index)
		{
			if (index < 0 || index >= certificates.Count)
				throw new ArgumentOutOfRangeException ("index");

			certificates.RemoveAt (index);
		}

		/// <summary>
		/// Gets or sets the certificate at the specified index.
		/// </summary>
		/// <remarks>
		/// Gets or sets the certificate at the specified index.
		/// </remarks>
		/// <value>The internet certificate at the specified index.</value>
		/// <param name="index">The index of the certificate to get or set.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public X509Certificate this [int index] {
			get { return certificates[index]; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				certificates[index] = value;
			}
		}
		#endregion

		#region ICollection implementation

		/// <summary>
		/// Gets the number of certificates in the chain.
		/// </summary>
		/// <remarks>
		/// Indicates the number of certificates in the chain.
		/// </remarks>
		/// <value>The number of certificates.</value>
		public int Count {
			get { return certificates.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <remarks>
		/// A <see cref="X509CertificateChain"/> is never read-only.
		/// </remarks>
		/// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get { return false; }
		}

		/// <summary>
		/// Adds the specified certificate to the chain.
		/// </summary>
		/// <remarks>
		/// Adds the specified certificate to the chain.
		/// </remarks>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public void Add (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			certificates.Add (certificate);
		}

		/// <summary>
		/// Adds the specified range of certificates to the chain.
		/// </summary>
		/// <remarks>
		/// Adds the specified range of certificates to the chain.
		/// </remarks>
		/// <param name="certificates">The certificates.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificates"/> is <c>null</c>.
		/// </exception>
		public void AddRange (IEnumerable<X509Certificate> certificates)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			foreach (var certificate in certificates)
				Add (certificate);
		}

		/// <summary>
		/// Clears the certificate chain.
		/// </summary>
		/// <remarks>
		/// Removes all of the certificates from the chain.
		/// </remarks>
		public void Clear ()
		{
			certificates.Clear ();
		}

		/// <summary>
		/// Checks if the chain contains the specified certificate.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the certificate chain contains the specified certificate.
		/// </remarks>
		/// <returns><value>true</value> if the specified certificate exists;
		/// otherwise <value>false</value>.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public bool Contains (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			return certificates.Contains (certificate);
		}

		/// <summary>
		/// Copies all of the certificates in the chain to the specified array.
		/// </summary>
		/// <remarks>
		/// Copies all of the certificates within the chain into the array,
		/// starting at the specified array index.
		/// </remarks>
		/// <param name="array">The array to copy the certificates to.</param>
		/// <param name="arrayIndex">The index into the array.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="array"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is out of range.
		/// </exception>
		public void CopyTo (X509Certificate[] array, int arrayIndex)
		{
			certificates.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Removes the specified certificate from the chain.
		/// </summary>
		/// <remarks>
		/// Removes the specified certificate from the chain.
		/// </remarks>
		/// <returns><value>true</value> if the certificate was removed; otherwise <value>false</value>.</returns>
		/// <param name="certificate">The certificate.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificate"/> is <c>null</c>.
		/// </exception>
		public bool Remove (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			return certificates.Remove (certificate);
		}

		/// <summary>
		/// Removes the specified range of certificates from the chain.
		/// </summary>
		/// <remarks>
		/// Removes the specified range of certificates from the chain.
		/// </remarks>
		/// <param name="certificates">The certificates.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="certificates"/> is <c>null</c>.
		/// </exception>
		public void RemoveRange (IEnumerable<X509Certificate> certificates)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			foreach (var certificate in certificates)
				Remove (certificate);
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Gets an enumerator for the list of certificates.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of certificates.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		public IEnumerator<X509Certificate> GetEnumerator ()
		{
			return certificates.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Gets an enumerator for the list of certificates.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of certificates.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return certificates.GetEnumerator ();
		}

		#endregion

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
			foreach (var certificate in certificates) {
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
