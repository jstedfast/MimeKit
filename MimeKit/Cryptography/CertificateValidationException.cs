//
// CertificateValidationException.cs
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
using System.Collections.Generic;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An exception that is thrown when one or more recipient certificates fail validation during encryption.
	/// </summary>
	/// <remarks>
	/// <para>This exception is thrown by <see cref="BouncyCastleSecureMimeContext"/> when
	/// <see cref="BouncyCastleSecureMimeContext.CheckCertificateRevocation"/> is enabled and one or more
	/// recipient certificates fail certificate chain validation (e.g. due to revocation).</para>
	/// <para>The <see cref="Failures"/> property contains a list of
	/// <see cref="CertificateValidationFailure"/> objects, each identifying a certificate
	/// that failed validation and the reason for the failure.</para>
	/// <para>Callers can use this information to remove invalid recipients and retry
	/// the encryption operation with only the valid recipients.</para>
	/// </remarks>
	public class CertificateValidationException : Exception
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="CertificateValidationException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="CertificateValidationException"/>.
		/// </remarks>
		/// <param name="failures">The list of certificate validation failures.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="failures"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="failures"/> is empty.
		/// </exception>
		public CertificateValidationException (IList<CertificateValidationFailure> failures)
			: base ("One or more recipient certificates failed validation.")
		{
			if (failures == null)
				throw new ArgumentNullException (nameof (failures));

			if (failures.Count == 0)
				throw new ArgumentException ("At least one failure is required.", nameof (failures));

			Failures = new List<CertificateValidationFailure> (failures).AsReadOnly ();
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="CertificateValidationException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="CertificateValidationException"/>.
		/// </remarks>
		/// <param name="message">A message explaining the error.</param>
		/// <param name="failures">The list of certificate validation failures.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="failures"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="failures"/> is empty.
		/// </exception>
		public CertificateValidationException (string message, IList<CertificateValidationFailure> failures)
			: base (message)
		{
			if (failures == null)
				throw new ArgumentNullException (nameof (failures));

			if (failures.Count == 0)
				throw new ArgumentException ("At least one failure is required.", nameof (failures));

			Failures = new List<CertificateValidationFailure> (failures).AsReadOnly ();
		}

		/// <summary>
		/// Get the list of certificate validation failures.
		/// </summary>
		/// <remarks>
		/// <para>Gets the list of certificates that failed validation along with the
		/// exceptions describing why each certificate's chain validation failed.</para>
		/// <para>Each <see cref="CertificateValidationFailure"/> contains the certificate
		/// that failed and the exception that describes the failure reason (typically
		/// a revocation or chain-building error).</para>
		/// </remarks>
		/// <value>The list of certificate validation failures.</value>
		public IReadOnlyList<CertificateValidationFailure> Failures {
			get; private set;
		}
	}
}
