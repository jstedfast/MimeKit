//
// PublicKeyNotFoundException.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (www.xamarin.com)
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// An exception that is thrown when a public key could not be found for a specified mailbox.
	/// </summary>
	/// <remarks>
	/// An exception that is thrown when a public key could not be found for a specified mailbox.
	/// </remarks>
	public class PublicKeyNotFoundException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.PublicKeyNotFoundException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="PublicKeyNotFoundException"/>.
		/// </remarks>
		/// <param name="mailbox">The mailbox that could not be resolved to a valid private key.</param>
		/// <param name="message">A message explaining the error.</param>
		public PublicKeyNotFoundException (MailboxAddress mailbox, string message) : base (message)
		{
			Mailbox = mailbox;
		}

		/// <summary>
		/// Gets the key id that could not be found.
		/// </summary>
		/// <remarks>
		/// Gets the key id that could not be found.
		/// </remarks>
		/// <value>The key id.</value>
		public MailboxAddress Mailbox {
			get; private set;
		}
	}
}
