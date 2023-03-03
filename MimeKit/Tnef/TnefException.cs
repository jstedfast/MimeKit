// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
