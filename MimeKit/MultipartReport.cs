//
// MultipartReport.cs
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

using System;

namespace MimeKit {
	/// <summary>
	/// A multipart/report MIME entity.
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
	public class MultipartReport : Multipart, IMultipartReport
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="MultipartReport"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public MultipartReport (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MultipartReport"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MultipartReport"/> part.
		/// </remarks>
		/// <param name="reportType">The type of the report.</param>
		/// <param name="args">An array of initialization parameters: headers and MIME entities.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="reportType"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="args"/> contains one or more arguments of an unknown type.
		/// </exception>
		public MultipartReport (string reportType, params object[] args) : base ("report", args)
		{
			if (reportType is null)
				throw new ArgumentNullException (nameof (reportType));

			ReportType = reportType;
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MultipartReport"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MultipartReport"/> part.
		/// </remarks>
		/// <param name="reportType">The type of the report.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="reportType"/> is <c>null</c>.
		/// </exception>
		public MultipartReport (string reportType) : base ("report")
		{
			if (reportType is null)
				throw new ArgumentNullException (nameof (reportType));

			ReportType = reportType;
		}

		void CheckDisposed ()
		{
			CheckDisposed (nameof (MultipartReport));
		}

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
		/// The <see cref="MultipartReport"/> has been disposed.
		/// </exception>
		public string ReportType {
			get {
				CheckDisposed ();

				return ContentType.Parameters["report-type"];
			}
			set {
				if (value is null)
					throw new ArgumentNullException (nameof (value));

				CheckDisposed ();

				if (ReportType == value)
					return;

				ContentType.Parameters["report-type"] = value.Trim ();
			}
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="MultipartReport"/> nodes
		/// calls <see cref="MimeVisitor.VisitMultipartReport"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitMultipartReport"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartReport"/> has been disposed.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor is null)
				throw new ArgumentNullException (nameof (visitor));

			CheckDisposed ();

			visitor.VisitMultipartReport (this);
		}
	}
}
