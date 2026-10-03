//
// TnefPart.cs
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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using MimeKit.Utils;
namespace MimeKit.Tnef {
	/// <summary>
	/// A MIME part containing Microsoft TNEF data.
	/// </summary>
	/// <remarks>
	/// <para>Represents an application/ms-tnef or application/vnd.ms-tnef part.</para>
	/// <para>TNEF (Transport Neutral Encapsulation Format) attachments are most often
	/// sent by Microsoft Outlook clients.</para>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\TnefExamples.cs" region="ConvertToMime"/>
	/// </example>
	public class TnefPart : MimePart, ITnefPart
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="TnefPart"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="IMimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <see langword="null"/>.
		/// </exception>
		public TnefPart (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefPart"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefPart"/> with a Content-Type of application/ms-tnef
		/// and a Content-Disposition value of "attachment" and a filename paremeter with a
		/// value of "winmail.dat".
		/// </remarks>
		public TnefPart () : base ("application", "ms-tnef")
		{
			FileName = "winmail.dat";
		}

		void CheckDisposed ()
		{
			CheckDisposed (nameof (TnefPart));
		}

		Stream OpenContent (ref TnefOptions? options)
		{
			CheckDisposed ();

			if (Content is null)
				throw new InvalidOperationException ("The TNEF part does not have any content.");

			if (options is null) {
				var charset = ContentType.Charset;
				int codepage;

				if (!string.IsNullOrEmpty (charset) && (codepage = CharsetUtils.GetCodePage (charset!)) > 0)
					options = new TnefOptions { DefaultCodepage = codepage };
			}

			return Content.Open ();
		}

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
		/// <example>
		/// <code language="c#" source="Examples\TnefExamples.cs" region="ConvertToMime"/>
		/// </example>
		/// <returns>The TNEF message.</returns>
		/// <param name="options">The options to use, or <see langword="null"/> to use the default options.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefPart"/> has been disposed.
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
		public TnefMessage LoadTnefMessage (TnefOptions? options = null, CancellationToken cancellationToken = default)
		{
			using (var stream = OpenContent (ref options))
				return TnefMessage.Load (stream, options, cancellationToken);
		}

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
		/// <example>
		/// <code language="c#" source="Examples\TnefExamples.cs" region="ConvertToMimeAsync"/>
		/// </example>
		/// <returns>The TNEF message.</returns>
		/// <param name="options">The options to use, or <see langword="null"/> to use the default options.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefPart"/> has been disposed.
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
		public async Task<TnefMessage> LoadTnefMessageAsync (TnefOptions? options = null, CancellationToken cancellationToken = default)
		{
			using (var stream = OpenContent (ref options))
				return await TnefMessage.LoadAsync (stream, options, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="TnefPart"/> nodes
		/// calls <see cref="MimeVisitor.VisitTnefPart"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitTnefPart"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="TnefPart"/> has been disposed.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor is null)
				throw new ArgumentNullException (nameof (visitor));

			CheckDisposed ();

			visitor.VisitTnefPart (this);
		}
	}
}
