//
// ParseException.cs
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

namespace MimeKit {
	/// <summary>
	/// A Parse exception as thrown by the various Parse methods in MimeKit.
	/// </summary>
	/// <remarks>
	/// A <see cref="ParseException"/> can be thrown by any of the Parse() methods
	/// in MimeKit. Each exception instance will have a <see cref="TokenIndex"/>
	/// which marks the byte offset of the token that failed to parse as well
	/// as a <see cref="ErrorIndex"/> which marks the byte offset where the error
	/// occurred.
	/// </remarks>
#if !PORTABLE
	[Serializable]
#endif
	public class ParseException : FormatException
	{
#if !PORTABLE
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.ParseException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ParseException"/>.
		/// </remarks>
		/// <param name="info">The serialization info.</param>
		/// <param name="context">The stream context.</param>
		protected ParseException (SerializationInfo info, StreamingContext context) : base (info, context)
		{
			TokenIndex = info.GetInt32 ("TokenIndex");
			ErrorIndex = info.GetInt32 ("ErrorIndex");
		}
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.ParseException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ParseException"/>.
		/// </remarks>
		/// <param name="message">The error message.</param>
		/// <param name="tokenIndex">The byte offset of the token.</param>
		/// <param name="errorIndex">The byte offset of the error.</param>
		/// <param name="innerException">The inner exception.</param>
		public ParseException (string message, int tokenIndex, int errorIndex, Exception innerException) : base (message, innerException)
		{
			TokenIndex = tokenIndex;
			ErrorIndex = errorIndex;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.ParseException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="ParseException"/>.
		/// </remarks>
		/// <param name="message">The error message.</param>
		/// <param name="tokenIndex">The byte offset of the token.</param>
		/// <param name="errorIndex">The byte offset of the error.</param>
		public ParseException (string message, int tokenIndex, int errorIndex) : base (message)
		{
			TokenIndex = tokenIndex;
			ErrorIndex = errorIndex;
		}

#if !PORTABLE
		/// <summary>
		/// When overridden in a derived class, sets the <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// with information about the exception.
		/// </summary>
		/// <remarks>
		/// Sets the <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// with information about the exception.
		/// </remarks>
		/// <param name="info">The serialization info.</param>
		/// <param name="context">The streaming context.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="info"/> is <c>null</c>.
		/// </exception>
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("TokenIndex", TokenIndex);
			info.AddValue ("ErrorIndex", ErrorIndex);

			base.GetObjectData (info, context);
		}
#endif

		/// <summary>
		/// Gets the byte index of the token that was malformed.
		/// </summary>
		/// <remarks>
		/// The token index is the byte offset at which the token started.
		/// </remarks>
		/// <value>The byte index of the token.</value>
		public int TokenIndex {
			get; private set;
		}

		/// <summary>
		/// Gets the index of the byte that caused the error.
		/// </summary>
		/// <remarks>
		/// The error index is the byte offset at which the parser encountered an error.
		/// </remarks>
		/// <value>The index of the byte that caused error.</value>
		public int ErrorIndex {
			get; private set;
		}
	}
}
