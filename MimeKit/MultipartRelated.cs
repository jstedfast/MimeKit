//
// MultipartRelated.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A multipart/related MIME entity.
	/// </summary>
	/// <remarks>
	/// A multipart/related MIME entity contains, as one might expect, inter-related MIME parts which
	/// typically reference each other via URIs based on the Content-Id and/or Content-Location headers.
	/// </remarks>
	public class MultipartRelated : Multipart
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MultipartRelated"/> class.
		/// </summary>
		/// <remarks>This constructor is used by <see cref="MimeKit.MimeParser"/>.</remarks>
		/// <param name="entity">Information used by the constructor.</param>
		public MultipartRelated (MimeEntityConstructorInfo entity) : base (entity)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MultipartRelated"/> class.
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
		/// Initializes a new instance of the <see cref="MimeKit.MultipartRelated"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MultipartRelated"/> part.
		/// </remarks>
		public MultipartRelated () : base ("related")
		{
		}

		int GetRootIndex ()
		{
			string start = ContentType.Parameters["start"];

			if (start == null)
				return -1;

			string contentId;

			if ((contentId = MimeUtils.EnumerateReferences (start).FirstOrDefault ()) == null)
				contentId = start;

			var cid = new Uri (string.Format ("cid:{0}", contentId));

			return IndexOf (cid);
		}

		/// <summary>
		/// Gets or sets the root document of the multipart/related part and the appropriate Content-Type parameters.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the root document that references the other MIME parts within the multipart/related.</para>
		/// <para>When getting the root document, the <c>"start"</c> parameter of the Content-Type header is used to
		/// determine which of the parts is the root. If the <c>"start"</c> parameter does not exist or does not reference
		/// any of the child parts, then the first child is assumed to be the root.</para>
		/// <para>When setting the root document MIME part, the Content-Type header of the multipart/related part is also
		/// updated with a appropriate <c>"start"</c> and <c>"type"</c> parameters.</para>
		/// </remarks>
		/// <value>The root MIME part.</value>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public MimeEntity Root {
			get {
				int index = GetRootIndex ();

				if (index < 0 && Count == 0)
					return null;

				return this[Math.Max (index, 0)];
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

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

				if (string.IsNullOrEmpty (value.ContentId))
					value.ContentId = MimeUtils.GenerateMessageId ();

				ContentType.Parameters["type"] = value.ContentType.MediaType + "/" + value.ContentType.MediaSubtype;

				// Note: we only use a "start" parameter if the index of the root entity is not at index 0 in order
				// to work around the following Thunderbird bug: https://bugzilla.mozilla.org/show_bug.cgi?id=471402
				if (index > 0)
					ContentType.Parameters["start"] = "<" + value.ContentId + ">";
				else
					ContentType.Parameters.Remove ("start");
			}
		}

		/// <summary>
		/// Checks if the <see cref="MultipartRelated"/> contains a part matching the specified URI.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the multipart/related entity contains a part matching the specified URI.
		/// </remarks>
		/// <returns><value>true</value> if the specified part exists; otherwise <value>false</value>.</returns>
		/// <param name="uri">The URI of the MIME part.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="uri"/> is <c>null</c>.
		/// </exception>
		public bool Contains (Uri uri)
		{
			return IndexOf (uri) != -1;
		}

		/// <summary>
		/// Gets the index of the part matching the specified URI.
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
		/// <returns>The index of the part matching the specified URI if found; otherwise <c>-1</c>.</returns>
		/// <param name="uri">The URI of the MIME part.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="uri"/> is <c>null</c>.
		/// </exception>
		public int IndexOf (Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			bool cid = uri.IsAbsoluteUri && uri.Scheme.ToLowerInvariant () == "cid";

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
		/// Opens a stream for reading the decoded content of the MIME part specified by the provided URI.
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
		public Stream Open (Uri uri, out string mimeType, out string charset)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			int index = IndexOf (uri);

			if (index == -1)
				throw new FileNotFoundException ();

			var part = this[index] as MimePart;

			if (part == null || part.ContentObject == null)
				throw new FileNotFoundException ();

			mimeType = part.ContentType.MimeType;
			charset = part.ContentType.Charset;

			return part.ContentObject.Open ();
		}

		/// <summary>
		/// Opens a stream for reading the decoded content of the MIME part specified by the provided URI.
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
		public Stream Open (Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			int index = IndexOf (uri);

			if (index == -1)
				throw new FileNotFoundException ();

			var part = this[index] as MimePart;

			if (part == null || part.ContentObject == null)
				throw new FileNotFoundException ();

			return part.ContentObject.Open ();
		}
	}
}
