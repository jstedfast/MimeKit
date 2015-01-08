//
// TnefException.cs
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
using System.Runtime.Serialization;

namespace MimeKit.Tnef {
	/// <summary>
	/// A TNEF exception.
	/// </summary>
	/// <remarks>
	/// A <see cref="TnefException"/> occurs when when a TNEF stream is found to be
	/// corrupted and cannot be read any futher.
	/// </remarks>
#if !PORTABLE
	[Serializable]
#endif
	public class TnefException : FormatException
	{
#if !PORTABLE
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Tnef.TnefException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefException"/>.
		/// </remarks>
		/// <param name="info">The serialization info.</param>
		/// <param name="context">The stream context.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="info"/> is <c>null</c>.
		/// </exception>
		protected TnefException (SerializationInfo info, StreamingContext context) : base (info, context)
		{
		}
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Tnef.TnefException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefException"/>.
		/// </remarks>
		/// <param name="message">The error message.</param>
		/// <param name="innerException">The inner exception.</param>
		public TnefException (string message, Exception innerException) : base (message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Tnef.TnefException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefException"/>.
		/// </remarks>
		/// <param name="message">The error message.</param>
		public TnefException (string message) : base (message)
		{
		}
	}
}
