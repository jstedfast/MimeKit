//
// MultipartReport.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2018 Xamarin Inc. (www.xamarin.com)
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
	/// </remarks>
	public class MultipartReport : Multipart
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MultipartReport"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeKit.MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public MultipartReport (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MultipartReport"/> class.
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
			if (reportType == null)
				throw new ArgumentNullException (nameof (reportType));

			ReportType = reportType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MultipartReport"/> class.
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
			if (reportType == null)
				throw new ArgumentNullException (nameof (reportType));

			ReportType = reportType;
		}

		/// <summary>
		/// Gets or sets the type of the report.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the type of the report.</para>
		/// <para>The report type should be the subtype of the second <see cref="MimeEntity"/>
		/// of the multipart/report.</para>
		/// </remarks>
		/// <value>The type of the report.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public string ReportType {
			get { return ContentType.Parameters["report-type"]; }
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				if (ReportType == value)
					return;

				ContentType.Parameters["report-type"] = value.Trim ();
			}
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="MimeKit.MultipartReport"/> nodes
		/// calls <see cref="MimeKit.MimeVisitor.VisitMultipartReport"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeKit.MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeKit.MimeVisitor.VisitMultipartReport"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor == null)
				throw new ArgumentNullException (nameof (visitor));

			visitor.VisitMultipartReport (this);
		}
	}
}
