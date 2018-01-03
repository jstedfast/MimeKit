//
// GnuPGContext.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2018 Xamarin Inc. (www.xamarin.com)
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// A <see cref="OpenPgpContext"/> that uses the GnuPG keyrings.
	/// </summary>
	/// <remarks>
	/// A <see cref="OpenPgpContext"/> that uses the GnuPG keyrings.
	/// </remarks>
	public abstract class GnuPGContext : OpenPgpContext
	{
		static readonly Dictionary<string, EncryptionAlgorithm> EncryptionAlgorithms;
		//static readonly Dictionary<string, PublicKeyAlgorithm> PublicKeyAlgorithms;
		static readonly Dictionary<string, DigestAlgorithm> DigestAlgorithms;
		static readonly string PublicKeyRing;
		static readonly string SecretKeyRing;
		static readonly string Configuration;

		static GnuPGContext ()
		{
			var gnupg = Environment.GetEnvironmentVariable ("GNUPGHOME");

			if (gnupg == null) {
#if !NETSTANDARD
				if (Path.DirectorySeparatorChar == '\\') {
					var appData = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
					gnupg = Path.Combine (appData, "gnupg");
				} else {
					var home = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
					gnupg = Path.Combine (home, ".gnupg");
				}
#else
				gnupg = ".gnupg";
#endif
			}

			PublicKeyRing = Path.Combine (gnupg, "pubring.gpg");
			SecretKeyRing = Path.Combine (gnupg, "secring.gpg");
			Configuration = Path.Combine (gnupg, "gpg.conf");

			EncryptionAlgorithms = new Dictionary<string, EncryptionAlgorithm> {
				{ "AES", EncryptionAlgorithm.Aes128 },
				{ "AES128", EncryptionAlgorithm.Aes128 },
				{ "AES192", EncryptionAlgorithm.Aes192 },
				{ "AES256", EncryptionAlgorithm.Aes256 },
				{ "BLOWFISH", EncryptionAlgorithm.Blowfish },
				{ "CAMELLIA128", EncryptionAlgorithm.Camellia128 },
				{ "CAMELLIA192", EncryptionAlgorithm.Camellia192 },
				{ "CAMELLIA256", EncryptionAlgorithm.Camellia256 },
				{ "CAST5", EncryptionAlgorithm.Cast5 },
				{ "IDEA", EncryptionAlgorithm.Idea },
				{ "3DES", EncryptionAlgorithm.TripleDes },
				{ "TWOFISH", EncryptionAlgorithm.Twofish }
			};

			//PublicKeyAlgorithms = new Dictionary<string, PublicKeyAlgorithm> {
			//	{ "DSA", PublicKeyAlgorithm.Dsa },
			//	{ "ECDH", PublicKeyAlgorithm.EllipticCurve },
			//	{ "ECDSA", PublicKeyAlgorithm.EllipticCurveDsa },
			//	{ "EDDSA", PublicKeyAlgorithm.EdwardsCurveDsa },
			//	{ "ELG", PublicKeyAlgorithm.ElGamalGeneral },
			//	{ "RSA", PublicKeyAlgorithm.RsaGeneral }
			//};

			DigestAlgorithms = new Dictionary<string, DigestAlgorithm> {
				{ "RIPEMD160", DigestAlgorithm.RipeMD160 },
				{ "SHA1", DigestAlgorithm.Sha1 },
				{ "SHA224", DigestAlgorithm.Sha224 },
				{ "SHA256", DigestAlgorithm.Sha256 },
				{ "SHA384", DigestAlgorithm.Sha384 },
				{ "SHA512", DigestAlgorithm.Sha512 }
			};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.GnuPGContext"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="GnuPGContext"/>.
		/// </remarks>
		protected GnuPGContext () : base (PublicKeyRing, SecretKeyRing)
		{
			LoadConfiguration ();

			foreach (var algorithm in EncryptionAlgorithmRank)
				Enable (algorithm);

			foreach (var algorithm in DigestAlgorithmRank)
				Enable (algorithm);
		}

		void UpdateKeyServer (string value)
		{
			if (string.IsNullOrEmpty (value)) {
				KeyServer = null;
				return;
			}

			if (!Uri.IsWellFormedUriString (value, UriKind.Absolute))
				return;

			KeyServer = new Uri (value, UriKind.Absolute);
		}

		void UpdateKeyServerOptions (string value)
		{
			if (string.IsNullOrEmpty (value))
				return;

			var options = value.Split (new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < options.Length; i++) {
				switch (options[i]) {
				case "auto-key-retrieve":
					AutoKeyRetrieve = true;
					break;
				}
			}
		}

		static EncryptionAlgorithm[] ParseEncryptionAlgorithms (string value)
		{
			var names = value.Split (new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			var algorithms = new List<EncryptionAlgorithm> ();
			var seen = new HashSet<EncryptionAlgorithm> ();

			for (int i = 0; i < names.Length; i++) {
				var name = names[i].ToUpperInvariant ();
				EncryptionAlgorithm algorithm;

				if (EncryptionAlgorithms.TryGetValue (name, out algorithm) && seen.Add (algorithm))
					algorithms.Add (algorithm);
			}

			if (!seen.Contains (EncryptionAlgorithm.TripleDes))
				algorithms.Add (EncryptionAlgorithm.TripleDes);

			return algorithms.ToArray ();
		}

		//static PublicKeyAlgorithm[] ParsePublicKeyAlgorithms (string value)
		//{
		//	var names = value.Split (new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		//	var algorithms = new List<PublicKeyAlgorithm> ();
		//	var seen = new HashSet<PublicKeyAlgorithm> ();

		//	for (int i = 0; i < names.Length; i++) {
		//		var name = names[i].ToUpperInvariant ();
		//		PublicKeyAlgorithm algorithm;

		//		if (PublicKeyAlgorithms.TryGetValue (name, out algorithm) && seen.Add (algorithm))
		//			algorithms.Add (algorithm);
		//	}

		//	if (!seen.Contains (PublicKeyAlgorithm.Dsa))
		//		seen.Add (PublicKeyAlgorithm.Dsa);

		//	return algorithms.ToArray ();
		//}

		static DigestAlgorithm[] ParseDigestAlgorithms (string value)
		{
			var names = value.Split (new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			var algorithms = new List<DigestAlgorithm> ();
			var seen = new HashSet<DigestAlgorithm> ();

			for (int i = 0; i < names.Length; i++) {
				var name = names[i].ToUpperInvariant ();
				DigestAlgorithm algorithm;

				if (DigestAlgorithms.TryGetValue (name, out algorithm) && seen.Add (algorithm))
					algorithms.Add (algorithm);
			}

			if (!seen.Contains (DigestAlgorithm.Sha1))
				algorithms.Add (DigestAlgorithm.Sha1);

			return algorithms.ToArray ();
		}

		void UpdatePersonalCipherPreferences (string value)
		{
			EncryptionAlgorithmRank = ParseEncryptionAlgorithms (value);
		}

		void UpdatePersonalDigestPreferences (string value)
		{
			DigestAlgorithmRank = ParseDigestAlgorithms (value);
		}

		void LoadConfiguration ()
		{
			if (!File.Exists (Configuration))
				return;

			using (var reader = File.OpenText (Configuration)) {
				string line;

				while ((line = reader.ReadLine ()) != null) {
					int startIndex = 0;

					while (startIndex < line.Length && char.IsWhiteSpace (line[startIndex]))
						startIndex++;

					if (startIndex == line.Length || line[startIndex] == '#')
						continue;

					int endIndex = startIndex;
					while (endIndex < line.Length && !char.IsWhiteSpace (line[endIndex]))
						endIndex++;

					var option = line.Substring (startIndex, endIndex - startIndex);
					string value;

					if (endIndex < line.Length)
						value = line.Substring (endIndex + 1).Trim ();
					else
						value = null;

					switch (option) {
					case "keyserver":
						UpdateKeyServer (value);
						break;
					case "keyserver-options":
						UpdateKeyServerOptions (value);
						break;
					case "personal-cipher-preferences":
						UpdatePersonalCipherPreferences (value);
						break;
					case "personal-digest-preferences":
						UpdatePersonalDigestPreferences (value);
						break;
					//case "personal-compress-preferences":
					//	break;
					}
				}
			}
		}
	}
}
