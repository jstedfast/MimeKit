//
// TnefConversionOptions.cs
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

namespace MimeKit.Tnef {
	/// <summary>
	/// Options for converting a TNEF message to MIME.
	/// </summary>
	/// <remarks>
	/// Options for converting a <see cref="TnefMessage"/> to MIME using <see cref="TnefMessage.ConvertToMime"/>.
	/// </remarks>
	public class TnefConversionOptions
	{
		/// <summary>
		/// The default conversion options.
		/// </summary>
		/// <remarks>
		/// The default conversion options. This instance should not be modified.
		/// </remarks>
		public static readonly TnefConversionOptions Default = new TnefConversionOptions ();

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefConversionOptions"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new set of conversion options with default values.
		/// </remarks>
		public TnefConversionOptions ()
		{
		}

		/// <summary>
		/// Get or set whether embedded messages should be converted to MIME.
		/// </summary>
		/// <remarks>
		/// <para>If <see langword="true"/>, each attachment that is an embedded TNEF message is converted to a
		/// <c>message/rfc822</c> part, and the information that was lost when converting it is included in
		/// <see cref="TnefConversionResult.Losses"/>.</para>
		/// <para>If <see langword="false"/>, each attachment that is an embedded TNEF message is added to the
		/// MIME message as an <c>application/ms-tnef</c> <see cref="TnefPart"/> containing the embedded TNEF stream.</para>
		/// <para>When the message is converted using its <see cref="TnefPropertyId.MimeSkeleton"/>, an embedded message
		/// that the skeleton represents as a <c>message/rfc822</c> part is always converted.</para>
		/// </remarks>
		/// <value><see langword="true"/> if embedded messages should be converted; otherwise, <see langword="false"/>.
		/// The default is <see langword="false"/>.</value>
		public bool ConvertEmbeddedMessages {
			get; set;
		}

		/// <summary>
		/// Clone the options.
		/// </summary>
		/// <remarks>
		/// Creates a copy of the options.
		/// </remarks>
		/// <returns>A copy of the options.</returns>
		public TnefConversionOptions Clone ()
		{
			return new TnefConversionOptions {
				ConvertEmbeddedMessages = ConvertEmbeddedMessages
			};
		}
	}
}
