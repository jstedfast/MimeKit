//
// AsyncTnefMessage.cs
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

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MimeKit.Tnef {
	public sealed partial class TnefMessage
	{
		/// <summary>
		/// Asynchronously load a TNEF message from the specified stream.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously loads a TNEF message from the specified stream.</para>
		/// <para>The stream is left open.</para>
		/// </remarks>
		/// <returns>The TNEF message.</returns>
		/// <param name="stream">The TNEF stream.</param>
		/// <param name="options">The options to use, or <see langword="null"/> to use <see cref="TnefOptions.Default"/>.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="stream"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="TnefException">
		/// The stream does not begin with the TNEF signature.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static async Task<TnefMessage> LoadAsync (Stream stream, TnefOptions? options = null, CancellationToken cancellationToken = default)
		{
			using (var reader = CreateReader (stream, options))
				return await new TnefMessageLoader (reader).LoadAsync (cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Asynchronously load a TNEF message using the specified reader.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously loads a TNEF message using the specified reader, which allows the caller to configure
		/// the reader's <see cref="TnefReader.ComplianceLogger"/>.</para>
		/// <para>The reader must not have been advanced. It is not disposed.</para>
		/// </remarks>
		/// <returns>The TNEF message.</returns>
		/// <param name="reader">The TNEF reader.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="reader"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// <paramref name="reader"/> has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// <paramref name="reader"/> has already been advanced.
		/// </exception>
		/// <exception cref="TnefException">
		/// The stream does not begin with the TNEF signature.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public static Task<TnefMessage> LoadAsync (TnefReader reader, CancellationToken cancellationToken = default)
		{
			CheckReader (reader);

			return new TnefMessageLoader (reader).LoadAsync (cancellationToken);
		}
	}
}
