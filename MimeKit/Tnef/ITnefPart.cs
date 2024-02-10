//
// ITnefPart.cs
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

using System.Collections.Generic;

namespace MimeKit.Tnef {
	/// <summary>
	/// An interface for a MIME part containing Microsoft TNEF data.
	/// </summary>
	/// <remarks>
	/// <para>Represents an application/ms-tnef or application/vnd.ms-tnef part.</para>
	/// <para>TNEF (Transport Neutral Encapsulation Format) attachments are most often
	/// sent by Microsoft Outlook clients.</para>
	/// </remarks>
	public interface ITnefPart : IMimePart
	{
		/// <summary>
		/// Convert the TNEF content into a <see cref="MimeMessage"/>.
		/// </summary>
		/// <remarks>
		/// TNEF data often contains properties that map to <see cref="MimeMessage"/>
		/// headers. TNEF data also often contains file attachments which will be
		/// mapped to MIME parts.
		/// </remarks>
		/// <returns>A message representing the TNEF data in MIME format.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="IMimePart.Content"/> property is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ITnefPart"/> has been disposed.
		/// </exception>
		MimeMessage ConvertToMessage ();

		/// <summary>
		/// Extract the embedded attachments from the TNEF data.
		/// </summary>
		/// <remarks>
		/// Parses the TNEF data and extracts all of the embedded file attachments.
		/// </remarks>
		/// <returns>The attachments.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="IMimePart.Content"/> property is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="ITnefPart"/> has been disposed.
		/// </exception>
		IEnumerable<MimeEntity> ExtractAttachments ();
	}
}
