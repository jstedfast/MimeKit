//
// TrustLevel.cs
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
	/// The trust level of a certificate or key.
	/// </summary>
	public enum TrustLevel {
		/// <summary>
		/// No trust level specified.
		/// </summary>
		None        = 0,

		/// <summary>
		/// The trust has expired.
		/// </summary>
		Expired     = 1,

		/// <summary>
		/// An unspecified trust.
		/// </summary>
		Undefined   = 2,

		/// <summary>
		/// Never trust this certificate or key.
		/// </summary>
		Never       = 3,

		/// <summary>
		/// Marginally trust this certificate or key.
		/// </summary>
		Marginal    = 4,

		/// <summary>
		/// Fully trust this certificate or key.
		/// </summary>
		Fully       = 5,

		/// <summary>
		/// The ultimate level of trust for a certificate or key.
		/// </summary>
		Ultimate    = 6
	}
}
