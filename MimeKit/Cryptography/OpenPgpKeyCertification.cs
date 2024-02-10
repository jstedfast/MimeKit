//
// OpenPgpKeyCertification.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// An OpenPGP key certification.
	/// </summary>
	/// <remarks>
	/// An OpenPGP key certification.
	/// </remarks>
	public enum OpenPgpKeyCertification {
		/// <summary>
		/// Generic certification of a User ID and Public-Key packet.
		/// The issuer of this certification does not make any particular
		/// assertion as to how well the certifier has checked that the owner
		/// of the key is in fact the person described by the User ID.
		/// </summary>
		GenericCertification = 0x10,

		/// <summary>
		/// Persona certification of a User ID and Public-Key packet.
		/// The issuer of this certification has not done any verification of
		/// the claim that the owner of this key is the User ID specified.
		/// </summary>
		PersonaCertification = 0x11,

		/// <summary>
		/// Casual certification of a User ID and Public-Key packet.
		/// The issuer of this certification has done some casual
		/// verification of the claim of identity.
		/// </summary>
		CasualCertification = 0x12,

		/// <summary>
		/// Positive certification of a User ID and Public-Key packet.
		/// The issuer of this certification has done substantial
		/// verification of the claim of identity.
		/// </summary>
		PositiveCertification = 0x13
	}
}
