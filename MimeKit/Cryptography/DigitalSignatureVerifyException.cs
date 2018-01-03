//
// DigitalSignatureVerifyException.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2018 Xamarin Inc. (www.xamarin.com)
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// An exception that is thrown when an error occurrs in <see cref="IDigitalSignature.Verify"/>.
	/// </summary>
	/// <remarks>
	/// For more information about the error condition, check the <see cref="System.Exception.InnerException"/> property.
	/// </remarks>
#if SERIALIZABLE
	[Serializable]
#endif
	public class DigitalSignatureVerifyException : Exception
	{
#if SERIALIZABLE
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Cryptography.DigitalSignatureVerifyException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DigitalSignatureVerifyException"/>.
		/// </remarks>
		/// <param name="info">The serialization info.</param>
		/// <param name="context">The stream context.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="info"/> is <c>null</c>.
		/// </exception>
		protected DigitalSignatureVerifyException (SerializationInfo info, StreamingContext context) : base (info, context)
		{
			KeyId = (long?) info.GetValue ("KeyId", typeof (long?));
		}
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="T:MimeKit.Cryptography.DigitalSignatureVerifyException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DigitalSignatureVerifyException"/>.
		/// </remarks>
		/// <param name="keyId">The key identifier.</param>
		/// <param name="message">The error message.</param>
		/// <param name="innerException">The inner exception.</param>
		public DigitalSignatureVerifyException (long keyId, string message, Exception innerException) : base (message, innerException)
		{
			KeyId = keyId;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:MimeKit.Cryptography.DigitalSignatureVerifyException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DigitalSignatureVerifyException"/>.
		/// </remarks>
		/// <param name="keyId">The key identifier.</param>
		/// <param name="message">The error message.</param>
		public DigitalSignatureVerifyException (long keyId, string message) : base (message)
		{
			KeyId = keyId;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:MimeKit.Cryptography.DigitalSignatureVerifyException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DigitalSignatureVerifyException"/>.
		/// </remarks>
		/// <param name="message">The error message.</param>
		/// <param name="innerException">The inner exception.</param>
		public DigitalSignatureVerifyException (string message, Exception innerException) : base (message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:MimeKit.Cryptography.DigitalSignatureVerifyException"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="DigitalSignatureVerifyException"/>.
		/// </remarks>
		/// <param name="message">The error message.</param>
		public DigitalSignatureVerifyException (string message) : base (message)
		{
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
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException (nameof (info));

			info.AddValue ("KeyId", KeyId, typeof (long?));

			base.GetObjectData (info, context);
		}
#endif

		/// <summary>
		/// Get the key identifier, if available.
		/// </summary>
		/// <remarks>
		/// Gets the key identifier, if available.
		/// </remarks>
		/// <value>The key identifier.</value>
		public long? KeyId {
			get; private set;
		}
	}
}
