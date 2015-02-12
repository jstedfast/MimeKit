//
// IDigitalSigner.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// An interface for a digital certificate.
	/// </summary>
	/// <remarks>
	/// An interface for a digital certificate.
	/// </remarks>
	public interface IDigitalCertificate
	{
		/// <summary>
		/// Gets the public key algorithm supported by the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the public key algorithm supported by the certificate.
		/// </remarks>
		/// <value>The public key algorithm.</value>
		PublicKeyAlgorithm PublicKeyAlgorithm { get; }

		/// <summary>
		/// Gets the date that the certificate was created.
		/// </summary>
		/// <remarks>
		/// Gets the date that the certificate was created.
		/// </remarks>
		/// <value>The creation date.</value>
		DateTime CreationDate { get; }

		/// <summary>
		/// Gets the expiration date of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the expiration date of the certificate.
		/// </remarks>
		/// <value>The expiration date.</value>
		DateTime ExpirationDate { get; }

		/// <summary>
		/// Gets the fingerprint of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the fingerprint of the certificate.
		/// </remarks>
		/// <value>The fingerprint.</value>
		string Fingerprint { get; }

		/// <summary>
		/// Gets the email address of the owner of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the email address of the owner of the certificate.
		/// </remarks>
		/// <value>The email address.</value>
		string Email { get; }

		/// <summary>
		/// Gets the name of the owner of the certificate.
		/// </summary>
		/// <remarks>
		/// Gets the name of the owner of the certificate.
		/// </remarks>
		/// <value>The name of the owner.</value>
		string Name { get; }
	}
}
