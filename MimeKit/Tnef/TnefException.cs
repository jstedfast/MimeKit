//
// TnefException.cs
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

using System;

#if SERIALIZABLE
using System.Security;
using System.Runtime.Serialization;
#endif

namespace MimeKit.Tnef {
	/// <summary>
	/// A TNEF exception.
	/// </summary>
	/// <remarks>
	/// A <see cref="TnefException"/> occurs when when a TNEF stream is found to be
	/// corrupted and cannot be read any futher.
	/// </remarks>
#if SERIALIZABLE
	[Serializable]
#endif
	public class TnefException : FormatException
	{
#if SERIALIZABLE
		/// <summary>
		/// Initialize a new instance of the <see cref="TnefException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefException"/>.
		/// </remarks>
		/// <param name="info">The serialization info.</param>
		/// <param name="context">The stream context.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="info"/> is <c>null</c>.
		/// </exception>
		[Obsolete ("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		protected TnefException (SerializationInfo info, StreamingContext context) : base (info, context)
		{
			Error = (TnefComplianceStatus) info.GetValue ("Error", typeof (TnefComplianceStatus));
		}
#endif

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefException"/>.
		/// </remarks>
		/// <param name="error">The compliance status error.</param>
		/// <param name="message">The error message.</param>
		/// <param name="innerException">The inner exception.</param>
		public TnefException (TnefComplianceStatus error, string message, Exception innerException) : base (message, innerException)
		{
			Error = error;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefException"/>.
		/// </remarks>
		/// <param name="error">The compliance status error.</param>
		/// <param name="message">The error message.</param>
		public TnefException (TnefComplianceStatus error, string message) : base (message)
		{
			Error = error;
		}

#if SERIALIZABLE
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
		[SecurityCritical]
#if NET8_0_OR_GREATER
		[Obsolete ("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
#endif
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);

			info.AddValue ("Error", Error);
		}
#endif

		/// <summary>
		/// Get the error.
		/// </summary>
		/// <remarks>
		/// Gets the error.
		/// </remarks>
		/// <value>The error.</value>
		public TnefComplianceStatus Error {
			get; private set;
		}
	}
}
