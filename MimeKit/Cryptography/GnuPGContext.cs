//
// GnuPGContext.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// A <see cref="OpenPgpContext"/> that uses the GnuPG keyrings.
	/// </summary>
	/// <remarks>
	/// A <see cref="OpenPgpContext"/> that uses the GnuPG keyrings.
	/// </remarks>
	public abstract class GnuPGContext : OpenPgpContext
	{
		static readonly string PublicKeyRing;
		static readonly string SecretKeyRing;
		static readonly string Configuration;

		static GnuPGContext ()
		{
			var gnupg = Environment.GetEnvironmentVariable ("GNUPGHOME");

			if (gnupg == null) {
#if !NET_STANDARD
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
					}
				}
			}
		}
	}
}
