//
// AttachmentCollection.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc.
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
using System.Collections;
using System.Collections.Generic;

using MimeKit.IO;
using MimeKit.IO.Filters;

namespace MimeKit {
	/// <summary>
	/// A collection of attachments.
	/// </summary>
	/// <remarks>
	/// The <see cref="AttachmentCollection"/> is only used when building a message body with a <see cref="BodyBuilder"/>.
	/// </remarks>
	public class AttachmentCollection : ICollection<MimeEntity>
	{
		readonly List<MimeEntity> attachments;
		readonly bool linked;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.AttachmentCollection"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="AttachmentCollection"/>.</para>
		/// <para>If <paramref name="linkedResources"/> is <c>true</c>, then the attachments
		/// are treated as if they are linked to another <see cref="MimePart"/>.</para>
		/// </remarks>
		/// <param name="linkedResources">If set to <c>true</c>; the attachments are treated as linked resources.</param>
		public AttachmentCollection (bool linkedResources)
		{
			attachments = new List<MimeEntity> ();
			linked = linkedResources;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.AttachmentCollection"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="AttachmentCollection"/>.
		/// </remarks>
		public AttachmentCollection () : this (false)
		{
		}

		#region ICollection implementation

		/// <summary>
		/// Gets the number of attachments currently in the collection.
		/// </summary>
		/// <remarks>
		/// Indicates the number of attachments in the collection.
		/// </remarks>
		/// <value>The number of attachments.</value>
		public int Count {
			get { return attachments.Count; }
		}

		/// <summary>
		/// Gets whther or not the collection is read-only.
		/// </summary>
		/// <remarks>
		/// A <see cref="AttachmentCollection"/> is never read-only.
		/// </remarks>
		/// <value><c>true</c> if the collection is read only; otherwise, <c>false</c>.</value>
		public bool IsReadOnly {
			get { return false; }
		}

#if !PORTABLE
		void LoadContent (MimePart attachment, string fileName, byte[] data)
		{
			var content = new MemoryBlockStream();
			var filter = new BestEncodingFilter();
			int index, length;

			filter.Flush (data, 0, data.Length, out index, out length);
			content.Write (data, 0, data.Length);

			content.Position = 0;

			if (linked)
				attachment.ContentLocation = new Uri (Path.GetFileName (fileName), UriKind.Relative);

			attachment.ContentTransferEncoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
			attachment.ContentObject = new ContentObject (content);
		}

		void LoadContent (MimePart attachment, string fileName)
		{
			var content = new MemoryBlockStream ();
			var filter = new BestEncodingFilter ();

			using (var stream = File.OpenRead (fileName)) {
				var buf = new byte[4096];
				int index, length;
				int nread;

				while ((nread = stream.Read (buf, 0, buf.Length)) > 0) {
					filter.Filter (buf, 0, nread, out index, out length);
					content.Write (buf, 0, nread);
				}

				filter.Flush (buf, 0, 0, out index, out length);
			}

			content.Position = 0;

			if (linked)
				attachment.ContentLocation = new Uri (Path.GetFileName (fileName), UriKind.Relative);

			attachment.ContentTransferEncoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
			attachment.ContentObject = new ContentObject (content);
		}

		/// <summary>
		/// Add the specified attachment.
		/// </summary>
		/// <remarks>
		/// <para>Adds the specified file as an attachment using the supplied Content-Type.</para>
		/// <para>For a list of known mime-types and their associated file extensions, see
		/// http://svn.apache.org/repos/asf/httpd/httpd/trunk/docs/conf/mime.types</para>
		/// </remarks>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="contentType">The mime-type of the file.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="contentType"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void Add (string fileName, ContentType contentType)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", "fileName");

			if (contentType == null)
				throw new ArgumentNullException ("contentType");

			var attachment = new MimePart (contentType) {
				FileName = Path.GetFileName (fileName),
				IsAttachment = true
			};

			LoadContent (attachment, fileName);

			attachments.Add (attachment);
		}

		/// <summary>
		/// Add the specified attachment.
		/// </summary>
		/// <remarks>
		/// <para>Adds the specified data as an attachment using the supplied Content-Type.</para>
		/// <para>The file name parameter is used to set the Content-Location.</para>
		/// <para>For a list of known mime-types and their associated file extensions, see
		/// http://svn.apache.org/repos/asf/httpd/httpd/trunk/docs/conf/mime.types</para>
		/// </remarks>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="data">The file data.</param>
		/// <param name="contentType">The mime-type of the file.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="data"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="contentType"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para>The specified file path is empty.</para>
		/// <para>-or-</para>
		/// <para>The data is empty.</para>
		/// </exception>
		public void Add (string fileName, byte[] data, ContentType contentType)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", "fileName");

			if (data == null)
				throw new ArgumentNullException ("data");

			if (data.Length == 0)
				throw new ArgumentException ("The data is empty.", "data");

			if (contentType == null)
				throw new ArgumentNullException ("contentType");

			var attachment = new MimePart (contentType) {
				FileName = Path.GetFileName (fileName),
				IsAttachment = true
			};

			LoadContent (attachment, fileName, data);

			attachments.Add (attachment);
		}

		static ContentType GetMimeType (string extension)
		{
			switch (extension) {
			case ".avi":   return new ContentType ("video", "x-msvideo");
			case ".asf":   return new ContentType ("video", "x-ms-asf");
			case ".asx":   return new ContentType ("video", "x-ms-asf");
			case ".gif":   return new ContentType ("image", "gif");
			case ".htm":   return new ContentType ("text", "html");
			case ".html":  return new ContentType ("text", "html");
			case ".ics":   return new ContentType ("text", "calendar");
			case ".jpeg":  return new ContentType ("image", "jpeg");
			case ".jpg":   return new ContentType ("image", "jpeg");
			case ".mov":   return new ContentType ("video", "quicktime");
			case ".mp2":   return new ContentType ("audio", "mpeg");
			case ".mp3":   return new ContentType ("audio", "mpeg");
			case ".mp4":   return new ContentType ("video", "mp4");
			case ".mp4a":  return new ContentType ("audio", "mp4");
			case ".mp4v":  return new ContentType ("video", "mp4");
			case ".mpeg":  return new ContentType ("video", "mpeg");
			case ".mpg":   return new ContentType ("video", "mpeg");
			case ".mpg4":  return new ContentType ("video", "mp4");
			case ".png":   return new ContentType ("image", "png");
			case ".qt":    return new ContentType ("video", "quicktime");
			case ".svg":   return new ContentType ("image", "svg+xml");
			case ".txt":   return new ContentType ("text", "plain");
			case ".vcard": return new ContentType ("text", "vcard");
			case ".webm":  return new ContentType ("video", "webm");
			case ".wmv":   return new ContentType ("video", "x-ms-wmv");
			case ".wmx":   return new ContentType ("video", "x-ms-wmx");
			case ".wvx":   return new ContentType ("video", "x-ms-wvx");
			case ".xml":   return new ContentType ("text", "xml");
			default:       return new ContentType ("application", "octet-stream");
			}
		}

		/// <summary>
		/// Add the specified attachment.
		/// </summary>
		/// <remarks>
		/// <para>Adds the data as an attachment, using the specified file name for deducing
		/// the mime-type by extension and for setting the Content-Location.</para>
		/// </remarks>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="data">The file data to attach.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="data"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para>The specified file path is empty.</para>
		/// <para>-or-</para>
		/// <para>The specified data is empty.</para>
		/// </exception>
		public void Add (string fileName, byte[] data)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", "fileName");

			if (data == null)
				throw new ArgumentNullException ("data");

			if (data.Length == 0)
				throw new ArgumentException ("The specified data is empty.", "data");

			var attachment = new MimePart (GetMimeType (Path.GetExtension (fileName).ToLowerInvariant ())) {
				FileName = Path.GetFileName (fileName),
				IsAttachment = true,
			};

			LoadContent (attachment, fileName, data);

			attachments.Add (attachment);
		}

		/// <summary>
		/// Add the specified attachment.
		/// </summary>
		/// <remarks>
		/// <para>Adds the specified file as an attachment.</para>
		/// </remarks>
		/// <param name="fileName">The name of the file.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public void Add (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", "fileName");

			var attachment = new MimePart (GetMimeType (Path.GetExtension (fileName).ToLowerInvariant ())) {
				FileName = Path.GetFileName (fileName),
				IsAttachment = true
			};

			LoadContent (attachment, fileName);

			attachments.Add (attachment);
		}
#endif // !PORTABLE

		/// <summary>
		/// Add the specified attachment.
		/// </summary>
		/// <remarks>
		/// Adds the specified <see cref="MimePart"/> as an attachment.
		/// </remarks>
		/// <param name="attachment">The attachment.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="attachment"/> is <c>null</c>.
		/// </exception>
		public void Add (MimeEntity attachment)
		{
			if (attachment == null)
				throw new ArgumentNullException ("attachment");

			attachments.Add (attachment);
		}

		/// <summary>
		/// Clears the attachment collection.
		/// </summary>
		/// <remarks>
		/// Removes all attachments from the collection.
		/// </remarks>
		public void Clear ()
		{
			attachments.Clear ();
		}

		/// <summary>
		/// Checks if the collection contains the specified attachment.
		/// </summary>
		/// <remarks>
		/// Determines whether or not the collection contains the specified attachment.
		/// </remarks>
		/// <returns><value>true</value> if the specified attachment exists;
		/// otherwise <value>false</value>.</returns>
		/// <param name="attachment">The attachment.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="attachment"/> is <c>null</c>.
		/// </exception>
		public bool Contains (MimeEntity attachment)
		{
			if (attachment == null)
				throw new ArgumentNullException ("attachment");

			return attachments.Contains (attachment);
		}

		/// <summary>
		/// Copies all of the attachments in the collection to the specified array.
		/// </summary>
		/// <remarks>
		/// Copies all of the attachments within the <see cref="AttachmentCollection"/> into the array,
		/// starting at the specified array index.
		/// </remarks>
		/// <param name="array">The array to copy the attachments to.</param>
		/// <param name="arrayIndex">The index into the array.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="array"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is out of range.
		/// </exception>
		public void CopyTo (MimeEntity[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException ("array");

			if (arrayIndex < 0 || arrayIndex >= array.Length)
				throw new ArgumentOutOfRangeException ("arrayIndex");

			attachments.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Removes the specified attachment.
		/// </summary>
		/// <remarks>
		/// Removes the specified attachment.
		/// </remarks>
		/// <returns><value>true</value> if the attachment was removed; otherwise <value>false</value>.</returns>
		/// <param name="attachment">The attachment.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="attachment"/> is <c>null</c>.
		/// </exception>
		public bool Remove (MimeEntity attachment)
		{
			if (attachment == null)
				throw new ArgumentNullException ("attachment");

			return attachments.Remove (attachment);
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Gets an enumerator for the list of attachments.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of attachments.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		public IEnumerator<MimeEntity> GetEnumerator ()
		{
			return attachments.GetEnumerator ();
		}

		#endregion

		#region IEnumerable implementation

		/// <summary>
		/// Gets an enumerator for the list of attachments.
		/// </summary>
		/// <remarks>
		/// Gets an enumerator for the list of attachments.
		/// </remarks>
		/// <returns>The enumerator.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		#endregion
	}
}
