//
// MultipartRelated.cs
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
using System.IO;
using System.Linq;

using MimeKit.Text;
using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A multipart/related MIME entity.
	/// </summary>
	/// <remarks>
	/// A multipart/related MIME entity contains, as one might expect, inter-related MIME parts which
	/// typically reference each other via URIs based on the Content-Id and/or Content-Location headers.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
	/// </example>
	public class MultipartRelated : Multipart, IMultipartRelated
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="MultipartRelated"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public MultipartRelated (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MultipartRelated"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MultipartRelated"/> part.
		/// </remarks>
		/// <param name="args">An array of initialization parameters: headers and MIME entities.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="args"/> contains one or more arguments of an unknown type.
		/// </exception>
		public MultipartRelated (params object[] args) : base ("related", args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MultipartRelated"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MultipartRelated"/> part.
		/// </remarks>
		public MultipartRelated () : base ("related")
		{
		}

		void CheckDisposed ()
		{
			CheckDisposed (nameof (MultipartRelated));
		}

		int GetRootIndex ()
		{
			var start = ContentType.Parameters["start"];

			if (start != null) {
				string contentId;

				if ((contentId = MimeUtils.EnumerateReferences (start).FirstOrDefault ()) is null)
					contentId = start;

				var cid = new Uri ($"cid:{contentId}");

				return IndexOf (cid);
			}

			var type = ContentType.Parameters["type"];

			if (type is null)
				return -1;

			for (int index = 0; index < Count; index++) {
				var mimeType = this[index].ContentType.MimeType;

				if (mimeType.Equals (type, StringComparison.OrdinalIgnoreCase))
					return index;
			}

			return -1;
		}

		/// <summary>
		/// Get or set the root document of the multipart/related part and the appropriate Content-Type parameters.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the root document that references the other MIME parts within the multipart/related.</para>
		/// <para>When getting the root document, the <c>"start"</c> parameter of the Content-Type header is used to
		/// determine which of the parts is the root. If the <c>"start"</c> parameter does not exist or does not reference
		/// any of the child parts, then the first child is assumed to be the root.</para>
		/// <para>When setting the root document MIME part, the Content-Type header of the multipart/related part is also
		/// updated with a appropriate <c>"start"</c> and <c>"type"</c> parameters.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <value>The root MIME part.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartRelated"/> has been disposed.
		/// </exception>
		public MimeEntity Root {
			get {
				CheckDisposed ();

				int index = GetRootIndex ();

				if (index < 0 && Count == 0)
					return null;

				return this[Math.Max (index, 0)];
			}
			set {
				if (value is null)
					throw new ArgumentNullException (nameof (value));

				CheckDisposed ();

				int index;

				if (Count > 0) {
					if ((index = GetRootIndex ()) != -1) {
						this[index] = value;
					} else {
						Insert (0, value);
						index = 0;
					}
				} else {
					Add (value);
					index = 0;
				}

				ContentType.Parameters["type"] = value.ContentType.MediaType + "/" + value.ContentType.MediaSubtype;

				// Note: we only use a "start" parameter if the index of the root entity is not at index 0 in order
				// to work around the following Thunderbird bug: https://bugzilla.mozilla.org/show_bug.cgi?id=471402
				if (index > 0) {
					if (string.IsNullOrEmpty (value.ContentId))
						value.ContentId = MimeUtils.GenerateMessageId ();

					ContentType.Parameters["start"] = "<" + value.ContentId + ">";
				} else {
					ContentType.Parameters.Remove ("start");
				}
			}
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="MultipartRelated"/> nodes
		/// calls <see cref="MimeVisitor.VisitMultipartRelated"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitMultipartRelated"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartRelated"/> has been disposed.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor is null)
				throw new ArgumentNullException (nameof (visitor));

			CheckDisposed ();

			visitor.VisitMultipartRelated (this);
		}

		/// <summary>
		/// Get the preferred message body if it exists.
		/// </summary>
		/// <remarks>
		/// Gets the preferred message body if it exists.
		/// </remarks>
		/// <param name="format">The preferred text format.</param>
		/// <param name="body">The MIME part containing the message body in the preferred text format.</param>
		/// <returns><c>true</c> if the body part is found; otherwise, <c>false</c>.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="Multipart"/> has been disposed.
		/// </exception>
		public override bool TryGetValue (TextFormat format, out TextPart body)
		{
			CheckDisposed ();

			// Note: If the multipart/related root document is HTML, then this is the droid we are looking for.
			var root = Root;

			if (root is TextPart text) {
				body = text.IsFormat (format) ? text : null;
				return body != null;
			}

			// The root may be a multipart such as a multipart/alternative.
			if (root is Multipart multipart)
				return multipart.TryGetValue (format, out body);

			body = null;

			return false;
		}

		/// <summary>
		/// Check if the <see cref="MultipartRelated"/> contains a part matching the specified URI.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the multipart/related entity contains a part matching the specified URI.
		/// </remarks>
		/// <returns><value>true</value> if the specified part exists; otherwise <value>false</value>.</returns>
		/// <param name="uri">The URI of the MIME part.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="uri"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartRelated"/> has been disposed.
		/// </exception>
		public bool Contains (Uri uri)
		{
			return IndexOf (uri) != -1;
		}

		/// <summary>
		/// Get the index of the part matching the specified URI.
		/// </summary>
		/// <remarks>
		/// <para>Finds the index of the part matching the specified URI, if it exists.</para>
		/// <para>If the URI scheme is <c>"cid"</c>, then matching is performed based on the Content-Id header
		/// values, otherwise the Content-Location headers are used. If the provided URI is absolute and a child
		/// part's Content-Location is relative, then then the child part's Content-Location URI will be combined
		/// with the value of its Content-Base header, if available, otherwise it will be combined with the
		/// multipart/related part's Content-Base header in order to produce an absolute URI that can be
		/// compared with the provided absolute URI.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <returns>The index of the part matching the specified URI if found; otherwise <c>-1</c>.</returns>
		/// <param name="uri">The URI of the MIME part.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="uri"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartRelated"/> has been disposed.
		/// </exception>
		public int IndexOf (Uri uri)
		{
			if (uri is null)
				throw new ArgumentNullException (nameof (uri));

			CheckDisposed ();

			bool cid = uri.IsAbsoluteUri && string.Equals (uri.Scheme, "cid", StringComparison.OrdinalIgnoreCase);

			for (int index = 0; index < Count; index++) {
				var entity = this[index];

				if (uri.IsAbsoluteUri) {
					if (cid) {
						if (entity.ContentId == uri.AbsolutePath)
							return index;
					} else if (entity.ContentLocation != null) {
						Uri absolute;

						if (!entity.ContentLocation.IsAbsoluteUri) {
							if (entity.ContentBase != null) {
								absolute = new Uri (entity.ContentBase, entity.ContentLocation);
							} else if (ContentBase != null) {
								absolute = new Uri (ContentBase, entity.ContentLocation);
							} else {
								continue;
							}
						} else {
							absolute = entity.ContentLocation;
						}

						if (absolute == uri)
							return index;
					}
				} else if (entity.ContentLocation == uri) {
					return index;
				}
			}

			return -1;
		}

		/// <summary>
		/// Open a stream for reading the decoded content of the MIME part specified by the provided URI.
		/// </summary>
		/// <remarks>
		/// Opens a stream for reading the decoded content of the MIME part specified by the provided URI.
		/// </remarks>
		/// <returns>A stream for reading the decoded content of the MIME part specified by the provided URI.</returns>
		/// <param name="uri">The URI.</param>
		/// <param name="mimeType">The mime-type of the content.</param>
		/// <param name="charset">The charset of the content (if the content is text-based)</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="uri"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The MIME part for the specified URI could not be found.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartRelated"/> has been disposed.
		/// </exception>
		public Stream Open (Uri uri, out string mimeType, out string charset)
		{
			if (uri is null)
				throw new ArgumentNullException (nameof (uri));

			int index = IndexOf (uri);

			if (index == -1)
				throw new FileNotFoundException ();

			if (this[index] is not MimePart part || part.Content is null)
				throw new FileNotFoundException ();

			mimeType = part.ContentType.MimeType;
			charset = part.ContentType.Charset;

			return part.Content.Open ();
		}

		/// <summary>
		/// Open a stream for reading the decoded content of the MIME part specified by the provided URI.
		/// </summary>
		/// <remarks>
		/// Opens a stream for reading the decoded content of the MIME part specified by the provided URI.
		/// </remarks>
		/// <returns>A stream for reading the decoded content of the MIME part specified by the provided URI.</returns>
		/// <param name="uri">The URI.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="uri"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The MIME part for the specified URI could not be found.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MultipartRelated"/> has been disposed.
		/// </exception>
		public Stream Open (Uri uri)
		{
			if (uri is null)
				throw new ArgumentNullException (nameof (uri));

			int index = IndexOf (uri);

			if (index == -1)
				throw new FileNotFoundException ();

			if (this[index] is not MimePart part || part.Content is null)
				throw new FileNotFoundException ();

			return part.Content.Open ();
		}
	}
}
