//
// CertificateValidationFailure.cs
//
// Author: Joseph Shook <joseph.shook@surescripts.com>
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

using Org.BouncyCastle.X509;

namespace MimeKit.Cryptography {
	/// <summary>
	/// Represents a recipient certificate that failed validation.
	/// </summary>
	/// <remarks>
	/// Contains the certificate that failed validation along with the exception
	/// that describes the reason for the failure.
	/// </remarks>
	public class CertificateValidationFailure
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="CertificateValidationFailure"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="CertificateValidationFailure"/>.
		/// </remarks>
		/// <param name="certificate">The certificate that failed validation.</param>
		/// <param name="exception">The exception that describes the validation failure.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="certificate"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="exception"/> is <see langword="null"/>.</para>
		/// </exception>
		public CertificateValidationFailure (X509Certificate certificate, Exception exception)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			if (exception == null)
				throw new ArgumentNullException (nameof (exception));

			Certificate = certificate;
			ValidationException = exception;
		}

		/// <summary>
		/// Get the certificate that failed validation.
		/// </summary>
		/// <remarks>
		/// Gets the certificate that failed validation. The certificate's Subject Alternative Name
		/// extension can be used to identify the recipient (rfc822Name for email-level certificates,
		/// dNSName for domain-level certificates).
		/// </remarks>
		/// <value>The certificate that failed validation.</value>
		public X509Certificate Certificate {
			get; private set;
		}

		/// <summary>
		/// Get the exception that describes the validation failure.
		/// </summary>
		/// <remarks>
		/// Gets the exception that describes why the certificate chain validation failed.
		/// This is typically a <see cref="Org.BouncyCastle.Pkix.PkixCertPathBuilderException"/>
		/// or <see cref="Org.BouncyCastle.Pkix.PkixCertPathValidatorException"/>.
		/// </remarks>
		/// <value>The validation exception.</value>
		public Exception ValidationException {
			get; private set;
		}
	}
}
