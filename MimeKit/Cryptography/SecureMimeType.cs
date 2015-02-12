//
// SecureMimeType.cs
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// The type of S/MIME data that an application/pkcs7-mime part contains.
	/// </summary>
	/// <remarks>
	/// The type of S/MIME data that an application/pkcs7-mime part contains.
	/// </remarks>
	public enum SecureMimeType {
		/// <summary>
		/// S/MIME compressed data.
		/// </summary>
		CompressedData,

		/// <summary>
		/// S/MIME enveloped data.
		/// </summary>
		EnvelopedData,

		/// <summary>
		/// S/MIME signed data.
		/// </summary>
		SignedData,

		/// <summary>
		/// S/MIME certificate data.
		/// </summary>
		CertsOnly,

		/// <summary>
		/// The S/MIME data type is unknown.
		/// </summary>
		Unknown
	}
}
