//
// CertificateNotFoundException.cs
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
using System.Runtime.Serialization;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An exception that is thrown when a certificate could not be found for a specified mailbox.
	/// </summary>
	/// <remarks>
	/// An exception that is thrown when a certificate could not be found for a specified mailbox.
	/// </remarks>
#if !PORTABLE
	[Serializable]
#endif
	public class CertificateNotFoundException : Exception
	{
#if !PORTABLE
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CertificateNotFoundException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="CertificateNotFoundException"/>.
		/// </remarks>
		/// <param name="info">The serialization info.</param>
		/// <param name="context">The stream context.</param>
		protected CertificateNotFoundException (SerializationInfo info, StreamingContext context) : base (info, context)
		{
			var text = info.GetString ("Mailbox");
			MailboxAddress mailbox;

			if (MailboxAddress.TryParse (text, out mailbox))
				Mailbox = mailbox;
		}
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.CertificateNotFoundException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="CertificateNotFoundException"/>.
		/// </remarks>
		/// <param name="mailbox">The mailbox that could not be resolved to a valid certificate.</param>
		/// <param name="message">A message explaining the error.</param>
		public CertificateNotFoundException (MailboxAddress mailbox, string message) : base (message)
		{
			Mailbox = mailbox;
		}

#if !PORTABLE
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
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("Mailbox", Mailbox.ToString (true));

			base.GetObjectData (info, context);
		}
#endif

		/// <summary>
		/// Gets the mailbox address that could not be resolved to a certificate.
		/// </summary>
		/// <remarks>
		/// Gets the mailbox address that could not be resolved to a certificate.
		/// </remarks>
		/// <value>The mailbox address.</value>
		public MailboxAddress Mailbox {
			get; private set;
		}
	}
}
