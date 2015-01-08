//
// X509KeyUsageFlags.cs
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
	/// X.509 key usage flags.
	/// </summary>
	/// <remarks>
	/// <para>The X.509 Key Usage Flags can be used to determine what operations
	/// a certificate can be used for.</para>
	/// <para>Note: a value of <see cref="X509KeyUsageFlags.None"/> indicates that
	/// there are no restrictions on the use of the
	/// <see cref="Org.BouncyCastle.X509.X509Certificate"/>.</para>
	/// </remarks>
	[Flags]
	public enum X509KeyUsageFlags {
		/// <summary>
		/// No limitations for the key usage are set.
		/// </summary>
		/// <remarks>
		/// The key may be used for anything.
		/// </remarks>
		None             = 0,

		/// <summary>
		/// The key may only be used for enciphering data during key agreement.
		/// </summary>
		/// <remarks>
		/// When both the <see cref="EncipherOnly"/> bit and the 
		/// <see cref="KeyAgreement"/> bit are both set, the key 
		/// may be used only for enciphering data while
		/// performing key agreement.
		/// </remarks>
		EncipherOnly     = 1 << 0,

		/// <summary>
		/// The key may be used for verifying signatures on
		/// certificate revocation lists (CRLs).
		/// </summary>
		CrlSign          = 1 << 1,

		/// <summary>
		/// The key may be used for verifying signatures on certificates.
		/// </summary>
		KeyCertSign      = 1 << 2,

		/// <summary>
		/// The key is meant to be used for key agreement.
		/// </summary>
		KeyAgreement     = 1 << 3,

		/// <summary>
		/// The key may be used for data encipherment.
		/// </summary>
		DataEncipherment = 1 << 4,

		/// <summary>
		/// The key is meant to be used for key encipherment.
		/// </summary>
		KeyEncipherment  = 1 << 5,

		/// <summary>
		/// The key may be used to verify digital signatures used to
		/// provide a non-repudiation service.
		/// </summary>
		NonRepudiation   = 1 << 6,

		/// <summary>
		/// The key may be used for digitally signing data.
		/// </summary>
		DigitalSignature = 1 << 7,

		/// <summary>
		/// The key may only be used for deciphering data during key agreement.
		/// </summary>
		/// <remarks>
		/// When both the <see cref="DecipherOnly"/> bit and the 
		/// <see cref="KeyAgreement"/> bit are both set, the key
		/// may be used only for deciphering data while
		/// performing key agreement.
		/// </remarks>
		DecipherOnly     = 1 << 15
	}

	enum X509KeyUsageBits {
		DigitalSignature,
		NonRepudiation,
		KeyEncipherment,
		DataEncipherment,
		KeyAgreement,
		KeyCertSign,
		CrlSign,
		EncipherOnly,
		DecipherOnly,
	}
}
