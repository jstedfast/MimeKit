//
// ITnefPart.cs
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

using System.Threading;
using System.Threading.Tasks;

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
		/// Load the TNEF message contained within the part.
		/// </summary>
		/// <remarks>
		/// <para>Decodes the content of the part and loads the TNEF message that it contains.</para>
		/// <para>If <paramref name="options"/> is <see langword="null"/> and the Content-Type of the part has a charset
		/// parameter, the codepage of that charset is used as the <see cref="TnefOptions.DefaultCodepage"/>.</para>
		/// <para>The returned message is independent of the part and must be disposed by the caller. Use
		/// <see cref="TnefMessage.ConvertToMime"/> to convert it to a <see cref="MimeMessage"/>.</para>
		/// </remarks>
		/// <returns>The TNEF message.</returns>
		/// <param name="options">The options to use, or <see langword="null"/> to use the default options.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The part has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The part does not have any content.
		/// </exception>
		/// <exception cref="TnefException">
		/// The content does not begin with the TNEF signature.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		TnefMessage LoadTnefMessage (TnefOptions? options = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously load the TNEF message contained within the part.
		/// </summary>
		/// <remarks>
		/// <para>Decodes the content of the part and asynchronously loads the TNEF message that it contains.</para>
		/// <para>If <paramref name="options"/> is <see langword="null"/> and the Content-Type of the part has a charset
		/// parameter, the codepage of that charset is used as the <see cref="TnefOptions.DefaultCodepage"/>.</para>
		/// <para>The returned message is independent of the part and must be disposed by the caller. Use
		/// <see cref="TnefMessage.ConvertToMime"/> to convert it to a <see cref="MimeMessage"/>.</para>
		/// </remarks>
		/// <returns>The TNEF message.</returns>
		/// <param name="options">The options to use, or <see langword="null"/> to use the default options.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The part has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The part does not have any content.
		/// </exception>
		/// <exception cref="TnefException">
		/// The content does not begin with the TNEF signature.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		Task<TnefMessage> LoadTnefMessageAsync (TnefOptions? options = null, CancellationToken cancellationToken = default);
	}
}
