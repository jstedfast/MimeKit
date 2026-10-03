//
// TnefConversionResult.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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
using System.Collections.Generic;

namespace MimeKit.Tnef {
	/// <summary>
	/// The result of converting a TNEF message to MIME.
	/// </summary>
	/// <remarks>
	/// The result of converting a <see cref="TnefMessage"/> to MIME using <see cref="TnefMessage.ConvertToMime"/>.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\TnefExamples.cs" region="ConvertToMime"/>
	/// </example>
	public sealed class TnefConversionResult : IDisposable
	{
		internal TnefConversionResult (MimeMessage message, IReadOnlyList<TnefConversionLoss> losses)
		{
			Message = message;
			Losses = losses;
		}

		/// <summary>
		/// Get the converted message.
		/// </summary>
		/// <remarks>
		/// <para>Gets the converted MIME message.</para>
		/// <para>The message is independent of the <see cref="TnefMessage"/> that it was converted from. It is disposed
		/// when the <see cref="TnefConversionResult"/> is disposed.</para>
		/// </remarks>
		/// <value>The converted message.</value>
		public MimeMessage Message {
			get;
		}

		/// <summary>
		/// Get the information that was lost during the conversion.
		/// </summary>
		/// <remarks>
		/// Gets the information that was lost during the conversion, in the order in which it was encountered.
		/// </remarks>
		/// <value>The information that was lost.</value>
		public IReadOnlyList<TnefConversionLoss> Losses {
			get;
		}

		/// <summary>
		/// Release all resources used by the <see cref="TnefConversionResult"/> object.
		/// </summary>
		/// <remarks>
		/// Disposes the converted <see cref="Message"/>.
		/// </remarks>
		public void Dispose ()
		{
			Message.Dispose ();
		}
	}
}
