//
// IMessageDeliveryStatus.cs
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

namespace MimeKit {
	/// <summary>
	/// An interface for a message delivery status MIME part.
	/// </summary>
	/// <remarks>
	/// <para>A message delivery status MIME part is a machine readable notification denoting the
	/// delivery status of a message and has a MIME-type of message/delivery-status.</para>
	/// <para>For more information, see <a href="https://tools.ietf.org/html/rfc3464">rfc3464</a>.</para>
	/// <seealso cref="MultipartReport"/>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MessageDeliveryStatusExamples.cs" region="ProcessDeliveryStatusNotification" />
	/// </example>
	public interface IMessageDeliveryStatus : IMimePart
	{
		/// <summary>
		/// Get the groups of delivery status fields.
		/// </summary>
		/// <remarks>
		/// <para>Gets the groups of delivery status fields. The first <see cref="HeaderList"/>
		/// contains the per-message fields while each remaining <see cref="HeaderList"/> contains
		/// fields that pertain to particular recipients of the message.</para>
		/// <para>For more information about these fields and their values, check out
		/// <a href="https://tools.ietf.org/html/rfc3464">rfc3464</a>.</para>
		/// <para><a href="https://tools.ietf.org/html/rfc3464#section-2.2">Section 2.2</a> defines
		/// the per-message fields while
		/// <a href="https://tools.ietf.org/html/rfc3464#section-2.3">Section 2.3</a> defines
		/// the per-recipient fields.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MessageDeliveryStatusExamples.cs" region="ProcessDeliveryStatusNotification" />
		/// </example>
		/// <value>The fields.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMessageDeliveryStatus"/> has been disposed.
		/// </exception>
		HeaderListCollection StatusGroups {
			get;
		}
	}
}
