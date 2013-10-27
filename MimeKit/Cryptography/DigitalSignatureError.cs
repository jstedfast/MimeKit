//
// DigitalSignatureError.cs
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
	/// Possible errors that can occur while verifying a digital signature.
	/// </summary>
	[Flags]
	public enum DigitalSignatureError : byte {
		/// <summary>
		/// No errors occurred.
		/// </summary>
		None = 0,

		/// <summary>
		/// The signature expired.
		/// </summary>
		ExpiredSignature = (1 << 0),

		/// <summary>
		/// No public key.
		/// </summary>
		NoPublicKey = (1 << 1),

		/// <summary>
		/// The key used for the digital signature has expired.
		/// </summary>
		ExpiredKey = (1 << 2),

		/// <summary>
		/// The key used for the digital signature has been revoked.
		/// </summary>
		RevokedKey = (1 << 3),

		/// <summary>
		/// The algorithm used in the digital signature is unsupported.
		/// </summary>
		UnsupportedAlgorithm = (1 << 4),
	}
}
