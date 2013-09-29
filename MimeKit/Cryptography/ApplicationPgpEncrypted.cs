//
// ApplicationPgpEncrypted.cs
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
using System.Text;
using System.Collections.Generic;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A MIME part with a Content-Type of application/pgp-encrypted.
	/// </summary>
	public class ApplicationPgpEncrypted : MimePart
	{
		internal ApplicationPgpEncrypted (ParserOptions options, ContentType type, IEnumerable<Header> headers, bool toplevel) : base (options, type, headers, toplevel)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.ApplicationPgpEncrypted"/>
		/// class with a Content-Type of application/pgp-encrypted and content matching "Version: 1\n".
		/// </summary>
		public ApplicationPgpEncrypted () : base ("application", "pgp-encrypted")
		{
			ContentDisposition = new ContentDisposition ("attachment");
			ContentTransferEncoding = ContentEncoding.SevenBit;
			ContentDisposition.FileName = "smime.p7s";
			ContentType.Name = "smime.p7s";

			var content = new MemoryStream (Encoding.ASCII.GetBytes ("Version: 1\n"));

			ContentObject = new ContentObject (content, ContentEncoding.Default);
		}
	}
}

