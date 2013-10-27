//
// DigitalSignature.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// A digital signature.
	/// </summary>
	public interface IDigitalSignature
	{
		/// <summary>
		/// Gets information about the signer.
		/// </summary>
		/// <value>The signer.</value>
		IDigitalCertificate SignerCertificate { get; }

		/// <summary>
		/// Gets the status of the digital signature.
		/// </summary>
		/// <value>The status.</value>
		DigitalSignatureStatus Status { get; }

		/// <summary>
		/// Gets a bit field of any errors that occurred while verifying the digital signature.
		/// </summary>
		/// <value>The errors.</value>
		DigitalSignatureError Errors { get; }

		/// <summary>
		/// Gets the creation date of the digital signature.
		/// </summary>
		/// <value>The creation date.</value>
		DateTime CreationDate { get; }

		/// <summary>
		/// Gets the expiration date of the digital signature.
		/// </summary>
		/// <value>The expiration date.</value>
		DateTime ExpirationDate { get; }
	}
}
