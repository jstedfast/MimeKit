//
// IMultipartReport.cs
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
	/// An interface for a multipart/report MIME entity.
	/// </summary>
	/// <remarks>
	/// A multipart/related MIME entity is a general container part for electronic mail
	/// reports of any kind.
	/// <seealso cref="MimeKit.MessageDeliveryStatus"/>
	/// <seealso cref="MimeKit.MessageDispositionNotification"/>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MessageDeliveryStatusExamples.cs" region="ProcessDeliveryStatusNotification" />
	/// </example>
	public interface IMultipartReport : IMultipart
	{
		/// <summary>
		/// Get or set the type of the report.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the type of the report.</para>
		/// <para>The report type should be the subtype of the second <see cref="MimeEntity"/>
		/// of the multipart/report.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MessageDeliveryStatusExamples.cs" region="ProcessDeliveryStatusNotification" />
		/// </example>
		/// <value>The type of the report.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="IMultipartReport"/> has been disposed.
		/// </exception>
		string ReportType {
			get; set;
		}
	}
}
