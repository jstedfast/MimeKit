//
// RsaEncryptionPadding.cs
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

#if NETCOREAPP3_0
using System.Security.Cryptography;
#endif

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;

namespace MimeKit.Cryptography {
	/// <summary>
	/// The RSA encryption padding schemes and parameters used by S/MIME.
	/// </summary>
	/// <remarks>
	/// The RSA encryption padding schemes and parameters used by S/MIME as described in
	/// <a href="https://tools.ietf.org/html/rfc8017">rfc8017</a>.
	/// </remarks>
	public sealed class RsaEncryptionPadding : IEquatable<RsaEncryptionPadding>
	{
		/// <summary>
		/// The PKCS #1 v1.5 encryption padding.
		/// </summary>
		/// <remarks>
		/// The PKCS #1 v1.5 encryption padding.
		/// </remarks>
		public static readonly RsaEncryptionPadding Pkcs1 = new RsaEncryptionPadding (RsaEncryptionPaddingScheme.Pkcs1, DigestAlgorithm.None);

		/// <summary>
		/// The Optimal Asymmetric Encryption Padding (OAEP) scheme using the default (SHA-1) hash algorithm.
		/// </summary>
		/// <remarks>
		/// The Optimal Asymmetric Encryption Padding (OAEP) scheme using the default (SHA-1) hash algorithm.
		/// </remarks>
		public static readonly RsaEncryptionPadding OaepSha1 = new RsaEncryptionPadding (RsaEncryptionPaddingScheme.Oaep, DigestAlgorithm.Sha1);

		/// <summary>
		/// The Optimal Asymmetric Encryption Padding (OAEP) scheme using the SHA-256 hash algorithm.
		/// </summary>
		/// <remarks>
		/// The Optimal Asymmetric Encryption Padding (OAEP) scheme using the SHA-256 hash algorithm.
		/// </remarks>
		public static readonly RsaEncryptionPadding OaepSha256 = new RsaEncryptionPadding (RsaEncryptionPaddingScheme.Oaep, DigestAlgorithm.Sha256);

		/// <summary>
		/// The Optimal Asymmetric Encryption Padding (OAEP) scheme using the SHA-384 hash algorithm.
		/// </summary>
		/// <remarks>
		/// The Optimal Asymmetric Encryption Padding (OAEP) scheme using the SHA-384 hash algorithm.
		/// </remarks>
		public static readonly RsaEncryptionPadding OaepSha384 = new RsaEncryptionPadding (RsaEncryptionPaddingScheme.Oaep, DigestAlgorithm.Sha384);

		/// <summary>
		/// The Optimal Asymmetric Encryption Padding (OAEP) scheme using the SHA-512 hash algorithm.
		/// </summary>
		/// <remarks>
		/// The Optimal Asymmetric Encryption Padding (OAEP) scheme using the SHA-512 hash algorithm.
		/// </remarks>
		public static readonly RsaEncryptionPadding OaepSha512 = new RsaEncryptionPadding (RsaEncryptionPaddingScheme.Oaep, DigestAlgorithm.Sha512);

		RsaEncryptionPadding (RsaEncryptionPaddingScheme scheme, DigestAlgorithm oaepHashAlgorithm)
		{
			OaepHashAlgorithm = oaepHashAlgorithm;
			Scheme = scheme;
		}

		/// <summary>
		/// Get the RSA encryption padding scheme.
		/// </summary>
		/// <remarks>
		/// Gets the RSA encryption padding scheme.
		/// </remarks>
		/// <value>The RSA encryption padding scheme.</value>
		public RsaEncryptionPaddingScheme Scheme {
			get; private set;
		}

		/// <summary>
		/// Get the hash algorithm used for RSAES-OAEP padding.
		/// </summary>
		/// <remarks>
		/// Gets the hash algorithm used for RSAES-OAEP padding.
		/// </remarks>
		/// <value>The hash algorithm used for RSAES-OAEP padding.</value>
		public DigestAlgorithm OaepHashAlgorithm {
			get; private set;
		}

		/// <summary>
		/// Determines whether the specified <see cref="RsaEncryptionPadding"/> is equal to the current <see cref="RsaEncryptionPadding"/>.
		/// </summary>
		/// <remarks>
		/// Compares two RSA encryption paddings to determine if they are identical or not.
		/// </remarks>
		/// <param name="other">The <see cref="RsaEncryptionPadding"/> to compare with the current <see cref="RsaEncryptionPadding"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="RsaEncryptionPadding"/> is equal to the current
		/// <see cref="RsaEncryptionPadding"/>; otherwise, <c>false</c>.</returns>
		public bool Equals (RsaEncryptionPadding other)
		{
			if (other is null)
				return false;

			return other.Scheme == Scheme && other.OaepHashAlgorithm == OaepHashAlgorithm;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <remarks>
		/// The type of comparison between the current instance and the <paramref name="obj"/> parameter depends on whether
		/// the current instance is a reference type or a value type.
		/// </remarks>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
		public override bool Equals (object obj)
		{
			return Equals (obj as RsaEncryptionPadding);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <remarks>
		/// Returns the hash code for this instance.
		/// </remarks>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode ()
		{
			int hash = Scheme.GetHashCode ();

			return ((hash << 5) + hash) ^ OaepHashAlgorithm.GetHashCode ();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current
		/// <see cref="RsaEncryptionPadding"/>.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="RsaEncryptionPadding"/>.
		/// </remarks>
		/// <returns>A <see cref="System.String"/> that represents the current
		/// <see cref="RsaEncryptionPadding"/>.</returns>
		public override string ToString ()
		{
			return Scheme == RsaEncryptionPaddingScheme.Pkcs1 ? "Pkcs1" : "Oaep" + OaepHashAlgorithm.ToString ();
		}

		/// <summary>
		/// Compare two <see cref="RsaEncryptionPadding"/> objects for equality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="RsaEncryptionPadding"/> objects for equality.
		/// </remarks>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <c>false</c>.</returns>
		public static bool operator == (RsaEncryptionPadding left, RsaEncryptionPadding right)
		{
			if (left is null)
				return right is null;

			return left.Equals (right);
		}

		/// <summary>
		/// Compare two <see cref="RsaEncryptionPadding"/> objects for inequality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="RsaEncryptionPadding"/> objects for inequality.
		/// </remarks>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> are unequal; otherwise, <c>false</c>.</returns>
		public static bool operator != (RsaEncryptionPadding left, RsaEncryptionPadding right)
		{
			return !(left == right);
		}

		/// <summary>
		/// Create a new <see cref="RsaEncryptionPadding"/> using <see cref="RsaEncryptionPaddingScheme.Oaep"/> and the specified hash algorithm.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="RsaEncryptionPadding"/> using <see cref="RsaEncryptionPaddingScheme.Oaep"/> and the specified hash algorithm.
		/// </remarks>
		/// <param name="hashAlgorithm">The hash algorithm.</param>
		/// <returns>An <see cref="RsaEncryptionPadding"/> using <see cref="RsaEncryptionPaddingScheme.Oaep"/> and the specified hash algorithm.</returns>
		/// <exception cref="NotSupportedException">
		/// The <paramref name="hashAlgorithm"/> is not supported.
		/// </exception>
		public static RsaEncryptionPadding CreateOaep (DigestAlgorithm hashAlgorithm)
		{
			switch (hashAlgorithm) {
			case DigestAlgorithm.Sha1: return OaepSha1;
			case DigestAlgorithm.Sha256: return OaepSha256;
			case DigestAlgorithm.Sha384: return OaepSha384;
			case DigestAlgorithm.Sha512: return OaepSha512;
			default: throw new NotSupportedException ($"The {hashAlgorithm} hash algorithm is not supported.");
			}
		}

		internal RsaesOaepParameters GetRsaesOaepParameters ()
		{
			if (OaepHashAlgorithm == DigestAlgorithm.Sha1)
				return new RsaesOaepParameters ();

			var oid = SecureMimeContext.GetDigestOid (OaepHashAlgorithm);
			var hashAlgorithm = new AlgorithmIdentifier (new DerObjectIdentifier (oid), DerNull.Instance);
			var maskGenFunction = new AlgorithmIdentifier (PkcsObjectIdentifiers.IdMgf1, hashAlgorithm);

			return new RsaesOaepParameters (hashAlgorithm, maskGenFunction, RsaesOaepParameters.DefaultPSourceAlgorithm);
		}

		internal AlgorithmIdentifier GetAlgorithmIdentifier ()
		{
			if (Scheme != RsaEncryptionPaddingScheme.Oaep)
				return null;

			return new AlgorithmIdentifier (PkcsObjectIdentifiers.IdRsaesOaep, GetRsaesOaepParameters ());
		}

#if NETCOREAPP3_0
		internal RSAEncryptionPadding AsRSAEncryptionPadding ()
		{
			switch (Scheme) {
			case RsaEncryptionPaddingScheme.Oaep:
				switch (OaepHashAlgorithm) {
				case DigestAlgorithm.Sha1: return RSAEncryptionPadding.OaepSHA1;
				case DigestAlgorithm.Sha256: return RSAEncryptionPadding.OaepSHA256;
				case DigestAlgorithm.Sha384: return RSAEncryptionPadding.OaepSHA384;
				case DigestAlgorithm.Sha512: return RSAEncryptionPadding.OaepSHA512;
				default: return null;
				}
			case RsaEncryptionPaddingScheme.Pkcs1:
				return RSAEncryptionPadding.Pkcs1;
			default:
				return null;
			}
		}
#endif
	}
}
