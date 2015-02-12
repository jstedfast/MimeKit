//
// DigitalSignatureCollection.cs
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

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A collection of digital signatures.
	/// </summary>
	/// <remarks>
	/// <para>When verifying a digitally signed MIME part such as a <see cref="MultipartSigned"/>
	/// or a <see cref="ApplicationPkcs7Mime"/>, you will get back a collection of
	/// digital signatures. Typically, a signed message will only have a single signature
	/// (created by the sender of the message), but it is possible for there to be
	/// multiple signatures.</para>
	/// </remarks>
	public class DigitalSignatureCollection : ReadOnlyCollection<IDigitalSignature>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DigitalSignatureCollection"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DigitalSignatureCollection"/>.
		/// </remarks>
		/// <param name="signatures">The signatures.</param>
		public DigitalSignatureCollection (IList<IDigitalSignature> signatures) : base (signatures)
		{
		}
	}
}
