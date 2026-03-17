//
// CmsRecipientException.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// The exception that is thrown when an error occurs while processing a CMS recipient.
	/// </summary>
	/// <remarks>
	/// <para>The exception that is thrown as a result of an error while processing a CMS recipient.</para>
	/// <para>The <see cref="Exception.InnerException"/> should be used to discover more detailed diagnostic
	/// information while the <see cref="CmsRecipientException.Recipient"/> and
	/// <see cref="CmsRecipientException.Mailbox"/> properties may be used for additional context.</para>
	/// </remarks>
	public class CmsRecipientException : Exception
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="CmsRecipientException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="CmsRecipientException"/>.
		/// </remarks>
		/// <param name="message">A message explaining the error.</param>
		/// <param name="recipient">The recipient that caused the error.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="recipient"/> is <see langword="null"/>.
		/// </exception>
		public CmsRecipientException (string message, CmsRecipient recipient, Exception innerException) : base (message, innerException)
		{
			if (recipient == null)
				throw new ArgumentNullException (nameof (recipient));

			Recipient = recipient;
		}

		/// <summary>
		/// Get the recipient that caused the error.
		/// </summary>
		/// <remarks>
		/// Gets the recipient that caused the error.
		/// </remarks>
		/// <value>The recipient.</value>
		public CmsRecipient Recipient {
			get; private set;
		}

		/// <summary>
		/// Get the mailbox address associated with the recipient.
		/// </summary>
		/// <remarks>
		/// Gets the mailbox address associated with the recipient, if available.
		/// </remarks>
		/// <value>The mailbox address of the recipient.</value>
		public MailboxAddress? Mailbox {
			get { return Recipient.Mailbox; }
		}
	}
}
