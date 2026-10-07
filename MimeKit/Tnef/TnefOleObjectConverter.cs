//
// TnefOleObjectConverter.cs
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
	/// <summary>
	/// Renders OLE object attachments as images.
	/// </summary>
	/// <remarks>
	/// <para>A <see cref="TnefOleObjectConverter"/> is used by <see cref="TnefMessage.ConvertToMime"/> (see
	/// <see cref="TnefConversionOptions.OleObjectConverter"/>) for each attachment whose
	/// <see cref="TnefAttachment.Method"/> is <see cref="TnefAttachMethod.Ole"/>: an OLE object, such as a picture or
	/// a document that was embedded into the body of a Rich Text Format (RTF) message. The content of such an
	/// attachment is an OLE compound file (see <see cref="TnefAttachment.OpenRead"/>), which few applications other
	/// than Microsoft Outlook can display. [MS-OXCMAIL] section 2.1.3.4.4 recommends that the attachment be converted
	/// to an image instead.</para>
	/// <para>The converter returns a stream containing a PNG, JPEG, GIF, BMP or WebP image that depicts the object,
	/// or <see langword="null"/> if it cannot render the object. The stream is read from its current position to its
	/// end, so it does not need to be seekable, and it is disposed once it has been read. The image replaces the
	/// content of the attachment, and its <c>Content-Type</c> is determined from the image data rather than from
	/// the attachment's properties. If the stream does not contain an image in one of those formats, the OLE
	/// compound file is used as the content instead.</para>
	/// <para>When the message body is generated from RTF, an image that replaces an attachment's content is also
	/// displayed in the HTML body at the position of the attachment.</para>
	/// </remarks>
	public abstract class TnefOleObjectConverter
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="TnefOleObjectConverter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new OLE object converter.
		/// </remarks>
		protected TnefOleObjectConverter ()
		{
		}

		/// <summary>
		/// Render an OLE object attachment as an image.
		/// </summary>
		/// <remarks>
		/// Renders an OLE object attachment as a PNG, JPEG, GIF, BMP or WebP image.
		/// </remarks>
		/// <param name="attachment">The OLE object attachment.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A stream containing the image, or <see langword="null"/> if the object cannot be rendered.</returns>
		public abstract Stream? Convert (TnefAttachment attachment, CancellationToken cancellationToken = default);

		/// <summary>
		/// Asynchronously render an OLE object attachment as an image.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously renders an OLE object attachment as a PNG, JPEG, GIF, BMP or WebP image.</para>
		/// <para>The default implementation calls <see cref="Convert"/>.</para>
		/// </remarks>
		/// <param name="attachment">The OLE object attachment.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A stream containing the image, or <see langword="null"/> if the object cannot be rendered.</returns>
		public virtual Task<Stream?> ConvertAsync (TnefAttachment attachment, CancellationToken cancellationToken = default)
		{
			return Task.FromResult (Convert (attachment, cancellationToken));
		}
	}
}