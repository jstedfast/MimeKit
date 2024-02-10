//
// PublicKeyNotFoundException.cs
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

#if SERIALIZABLE
using System.Security;
using System.Runtime.Serialization;
#endif

namespace MimeKit.Cryptography {
	/// <summary>
	/// An exception that is thrown when a public key could not be found for a specified mailbox.
	/// </summary>
	/// <remarks>
	/// An exception that is thrown when a public key could not be found for a specified mailbox.
	/// </remarks>
#if SERIALIZABLE
	[Serializable]
#endif
	public class PublicKeyNotFoundException : Exception
	{
#if SERIALIZABLE
		/// <summary>
		/// Initialize a new instance of the <see cref="PublicKeyNotFoundException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="PublicKeyNotFoundException"/>.
		/// </remarks>
		/// <param name="info">The serialization info.</param>
		/// <param name="context">The stream context.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="info"/> is <c>null</c>.
		/// </exception>
		[Obsolete ("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		protected PublicKeyNotFoundException (SerializationInfo info, StreamingContext context) : base (info, context)
		{
			var text = info.GetString ("Mailbox");

			if (MailboxAddress.TryParse (text, out var mailbox))
				Mailbox = mailbox;
		}
#endif

		/// <summary>
		/// Initialize a new instance of the <see cref="PublicKeyNotFoundException"/> class.
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

#if SERIALIZABLE
		/// <summary>
		/// When overridden in a derived class, sets the <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// with information about the exception.
		/// </summary>
		/// <remarks>
		/// Sets the <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// with information about the exception.
		/// </remarks>
		/// <param name="info">The serialization info.</param>
		/// <param name="context">The streaming context.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="info"/> is <c>null</c>.
		/// </exception>
		[SecurityCritical]
#if NET8_0_OR_GREATER
		[Obsolete ("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
#endif
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);

			info.AddValue ("Mailbox", Mailbox.ToString (true));
		}
#endif

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
