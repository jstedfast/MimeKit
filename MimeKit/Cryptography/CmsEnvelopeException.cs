//
// CmsEnvelopeException.cs
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

using Org.BouncyCastle.Cms;

namespace MimeKit.Cryptography {
	/// <summary>
	/// The exception that is thrown if an error occurs while generating a CMS envelope.
	/// </summary>
	/// <remarks>
	/// <para>This exception is thrown by the
	/// <a href="Overload_MimeKit_Cryptography_BouncyCastleSecureMimeContext_Encrypt.htm">BouncyCastleSecureMimeContext.Encrypt</a>
	/// and
	/// <a href="Overload_MimeKit_Cryptography_BouncyCastleSecureMimeContext_EncryptAsync.htm">BouncyCastleSecureMimeContext.EncryptAsync</a>
	/// methods when an error occurs while generating a CMS envelope.</para>
	/// <para>The <see cref="Exception.InnerException"/> will typically be one of the following types:</para>
	/// <list type="number">
	/// <item>
	/// <para><see cref="AggregateException"/>: One or more of the recipient certificates failed validation.</para>
	/// <para>In this scenario, callers may iterate over each of the <see cref="AggregateException.InnerExceptions"/>
	/// for exceptions of type <see cref="CmsRecipientException"/>. These exceptions can be used to remove the failed
	/// recipients before retrying the encryption operation.</para>
	/// </item>
	/// <item>
	/// <para><see cref="CmsException"/>: An error within BouncyCastle occurred.</para>
	/// </item>
	/// </list>
	/// </remarks>
	public class CmsEnvelopeException : CmsException
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="CmsEnvelopeException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="CmsEnvelopeException"/>.
		/// </remarks>
		/// <param name="message">A message explaining the error.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="innerException"/> is <see langword="null"/>.
		/// </exception>
		public CmsEnvelopeException (string message, Exception innerException) : base (message, innerException)
		{
		}
	}
}
