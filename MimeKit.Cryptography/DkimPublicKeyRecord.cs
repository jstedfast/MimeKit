//
// DkimPublicKeyRecord.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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
using System.Diagnostics.CodeAnalysis;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// The key type of a DKIM public key record.
	/// </summary>
	enum DkimKeyType
	{
		Rsa,
		Ed25519
	}

	/// <summary>
	/// A parsed DKIM public key record (RFC 6376, Section 3.6.1 and RFC 8463).
	/// </summary>
	sealed class DkimPublicKeyRecord
	{
		DkimPublicKeyRecord (DkimKeyType keyType, AsymmetricKeyParameter? publicKey, List<string>? hashAlgorithms, bool testing, bool strict)
		{
			HashAlgorithms = hashAlgorithms;
			PublicKey = publicKey;
			KeyType = keyType;
			IsTesting = testing;
			IsStrict = strict;
		}

		/// <summary>
		/// Get the key type (the k= tag).
		/// </summary>
		public DkimKeyType KeyType { get; }

		/// <summary>
		/// Get the public key, or <see langword="null"/> if the key has been revoked (an empty p= tag).
		/// </summary>
		public AsymmetricKeyParameter? PublicKey { get; }

		/// <summary>
		/// Get whether the key has been revoked (the p= tag is empty).
		/// </summary>
		public bool IsRevoked => PublicKey is null;

		/// <summary>
		/// Get the list of acceptable hash algorithms (the h= tag), or <see langword="null"/> if all algorithms are acceptable.
		/// </summary>
		public IReadOnlyList<string>? HashAlgorithms { get; }

		/// <summary>
		/// Get whether the domain is testing DKIM (the y flag in the t= tag).
		/// </summary>
		public bool IsTesting { get; }

		/// <summary>
		/// Get whether the i= domain of a signature must exactly match the d= domain (the s flag in the t= tag).
		/// </summary>
		public bool IsStrict { get; }

		static string GetHashAlgorithm (DkimSignatureAlgorithm algorithm)
		{
			return algorithm == DkimSignatureAlgorithm.RsaSha1 ? "sha1" : "sha256";
		}

		static DkimKeyType GetKeyType (DkimSignatureAlgorithm algorithm)
		{
			return algorithm == DkimSignatureAlgorithm.Ed25519Sha256 ? DkimKeyType.Ed25519 : DkimKeyType.Rsa;
		}

		/// <summary>
		/// Check whether this key may be used to verify a signature using the specified algorithm.
		/// </summary>
		public bool IsCompatible (DkimSignatureAlgorithm algorithm)
		{
			if (GetKeyType (algorithm) != KeyType)
				return false;

			if (HashAlgorithms is null)
				return true;

			var hash = GetHashAlgorithm (algorithm);

			for (int i = 0; i < HashAlgorithms.Count; i++) {
				if (HashAlgorithms[i].Equals (hash, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		static AsymmetricKeyParameter? ParseRsaPublicKey (byte[] data)
		{
			Asn1Object asn1;

			try {
				asn1 = Asn1Object.FromByteArray (data);
			} catch {
				return null;
			}

			if (asn1 is not Asn1Sequence sequence)
				return null;

			// RFC 6376 specifies a DER-encoded SubjectPublicKeyInfo, but some domains publish a bare PKCS#1
			// RSAPublicKey structure, so accept both.
			try {
				var info = SubjectPublicKeyInfo.GetInstance (sequence);

				if (PublicKeyFactory.CreateKey (info) is RsaKeyParameters { IsPrivate: false } rsa)
					return rsa;

				return null;
			} catch {
			}

			try {
				var structure = RsaPublicKeyStructure.GetInstance (sequence);

				return new RsaKeyParameters (false, structure.Modulus, structure.PublicExponent);
			} catch {
				return null;
			}
		}

		/// <summary>
		/// Try to parse a DKIM public key record.
		/// </summary>
		/// <remarks>
		/// <para>A record is considered invalid (and should be ignored) if it is not syntactically valid, if it has a
		/// v= tag that is not the first tag or not <c>DKIM1</c>, if it has duplicate tags, if it has an unrecognized
		/// key type, if its s= tag does not include <c>email</c> or <c>*</c>, if it has no p= tag, or if the p= tag
		/// cannot be decoded into a public key of the specified key type.</para>
		/// <para>An empty p= tag is valid and indicates that the key has been revoked.</para>
		/// </remarks>
		public static bool TryParse (string text, [NotNullWhen (true)] out DkimPublicKeyRecord? record)
		{
			List<string>? hashAlgorithms = null;
			bool testing = false, strict = false;
			var keyType = DkimKeyType.Rsa;
			string? p = null;

			record = null;

			if (!TagValueList.TryParse (text, out var tags))
				return false;

			var seen = new HashSet<string> (StringComparer.Ordinal);

			for (int i = 0; i < tags.Count; i++) {
				var name = tags[i].Key;
				var value = tags[i].Value;

				// Tags with duplicate names MUST NOT occur within a single tag-list; if a tag name does occur more
				// than once, the entire tag-list is invalid. (RFC 6376, Section 3.2)
				if (!seen.Add (name))
					return false;

				switch (name) {
				case "v":
					// If specified, this tag MUST be set to "DKIM1" (without the quotes). This tag MUST be the
					// first tag in the record. (RFC 6376, Section 3.6.1)
					if (i != 0 || !value.Equals ("DKIM1", StringComparison.Ordinal))
						return false;
					break;
				case "h":
					hashAlgorithms = TagValueList.SplitColonList (value);
					break;
				case "k":
					if (value.Equals ("rsa", StringComparison.OrdinalIgnoreCase))
						keyType = DkimKeyType.Rsa;
					else if (value.Equals ("ed25519", StringComparison.OrdinalIgnoreCase))
						keyType = DkimKeyType.Ed25519;
					else
						return false;
					break;
				case "p":
					p = TagValueList.RemoveWhiteSpace (value);
					break;
				case "s":
					bool email = false;

					foreach (var service in TagValueList.SplitColonList (value)) {
						if (service == "*" || service.Equals ("email", StringComparison.OrdinalIgnoreCase)) {
							email = true;
							break;
						}
					}

					// Unrecognized service types MUST be ignored, which means a record that does not apply to email
					// is not usable for verifying email signatures.
					if (!email)
						return false;
					break;
				case "t":
					foreach (var flag in TagValueList.SplitColonList (value)) {
						if (flag.Equals ("y", StringComparison.OrdinalIgnoreCase))
							testing = true;
						else if (flag.Equals ("s", StringComparison.OrdinalIgnoreCase))
							strict = true;
					}
					break;
				}
			}

			if (p is null)
				return false;

			if (p.Length == 0) {
				record = new DkimPublicKeyRecord (keyType, null, hashAlgorithms, testing, strict);
				return true;
			}

			byte[] data;

			try {
				data = Convert.FromBase64String (p);
			} catch (FormatException) {
				return false;
			}

			AsymmetricKeyParameter? key;

			if (keyType == DkimKeyType.Ed25519) {
				// The public key is the raw 32-byte Ed25519 public key. (RFC 8463, Section 4.2)
				if (data.Length != Ed25519PublicKeyParameters.KeySize)
					return false;

				key = new Ed25519PublicKeyParameters (data, 0);
			} else {
				key = ParseRsaPublicKey (data);
			}

			if (key is null)
				return false;

			record = new DkimPublicKeyRecord (keyType, key, hashAlgorithms, testing, strict);

			return true;
		}
	}
}
