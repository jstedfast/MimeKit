//
// AttachmentCollection.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc.
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
	public class AttachmentCollection : IList<MimeEntity>
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

		#region IList implementation

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

		/// <summary>
		/// Gets or sets the <see cref="MimeKit.MimeEntity"/> at the specified index.
		/// </summary>
		/// <remarks>
		/// Gets or sets the <see cref="MimeKit.MimeEntity"/> at the specified index.
		/// </remarks>
		/// <value>The attachment at the specified index.</value>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public MimeEntity this [int index] {
			get {
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("index");

				return attachments[index];
			}
			set {
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("index");

				if (value == null)
					throw new ArgumentNullException ("value");

				attachments[index] = value;
			}
		}

		static void LoadContent (MimePart attachment, Stream stream)
		{
			var content = new MemoryBlockStream ();
			var filter = new BestEncodingFilter ();
			var buf = new byte[4096];
			int index, length;
			int nread;

			while ((nread = stream.Read (buf, 0, buf.Length)) > 0) {
				filter.Filter (buf, 0, nread, out index, out length);
				content.Write (buf, 0, nread);
			}

			filter.Flush (buf, 0, 0, out index, out length);
			content.Position = 0;

			attachment.ContentTransferEncoding = filter.GetBestEncoding (EncodingConstraint.SevenBit);
			attachment.ContentObject = new ContentObject (content);
		}

		static ContentType GetMimeType (string fileName)
		{
			var mimeType = MimeTypes.GetMimeType (fileName);

			return ContentType.Parse (mimeType);
		}

		MimePart CreateAttachment (ContentType contentType, string fileName, Stream stream)
		{
			MimePart attachment;

			if (contentType.Matches ("text", "*")) {
				attachment = new TextPart (contentType.MediaSubtype);
				foreach (var param in contentType.Parameters)
					attachment.ContentType.Parameters.Add (param);

				// TODO: should we try to auto-detect charsets if no charset parameter is specified?
			} else {
				attachment = new MimePart (contentType);
			}

			attachment.FileName = Path.GetFileName (fileName);
			attachment.IsAttachment = true;

			if (linked)
				attachment.ContentLocation = new Uri (Path.GetFileName (fileName), UriKind.Relative);

			LoadContent (attachment, stream);

			return attachment;
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
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
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
		/// The specified file path is empty.
		/// </exception>
		public MimeEntity Add (string fileName, byte[] data, ContentType contentType)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", "fileName");

			if (data == null)
				throw new ArgumentNullException ("data");

			if (contentType == null)
				throw new ArgumentNullException ("contentType");

			using (var stream = new MemoryStream (data, false)) {
				var attachment = CreateAttachment (contentType, fileName, stream);

				attachments.Add (attachment);

				return attachment;
			}
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
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="stream">The content stream.</param>
		/// <param name="contentType">The mime-type of the file.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="contentType"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para>The specified file path is empty.</para>
		/// <para>-or-</para>
		/// <para>The stream cannot be read.</para>
		/// </exception>
		public MimeEntity Add (string fileName, Stream stream, ContentType contentType)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", "fileName");

			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (!stream.CanRead)
				throw new ArgumentException ("The stream cannot be read.", "stream");

			if (contentType == null)
				throw new ArgumentNullException ("contentType");

			var attachment = CreateAttachment (contentType, fileName, stream);

			attachments.Add (attachment);

			return attachment;
		}

		/// <summary>
		/// Add the specified attachment.
		/// </summary>
		/// <remarks>
		/// <para>Adds the data as an attachment, using the specified file name for deducing
		/// the mime-type by extension and for setting the Content-Location.</para>
		/// </remarks>
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="data">The file data to attach.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="data"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The specified file path is empty.
		/// </exception>
		public MimeEntity Add (string fileName, byte[] data)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", "fileName");

			if (data == null)
				throw new ArgumentNullException ("data");

			using (var stream = new MemoryStream (data, false)) {
				var attachment = CreateAttachment (GetMimeType (fileName), fileName, stream);

				attachments.Add (attachment);

				return attachment;
			}
		}

		/// <summary>
		/// Add the specified attachment.
		/// </summary>
		/// <remarks>
		/// <para>Adds the stream as an attachment, using the specified file name for deducing
		/// the mime-type by extension and for setting the Content-Location.</para>
		/// </remarks>
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
		/// <param name="fileName">The name of the file.</param>
		/// <param name="stream">The content stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="fileName"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para>The specified file path is empty.</para>
		/// <para>-or-</para>
		/// <para>The stream cannot be read</para>
		/// </exception>
		public MimeEntity Add (string fileName, Stream stream)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", "fileName");

			if (stream == null)
				throw new ArgumentNullException ("stream");

			if (!stream.CanRead)
				throw new ArgumentException ("The stream cannot be read.", "stream");

			var attachment = CreateAttachment (GetMimeType (fileName), fileName, stream);

			attachments.Add (attachment);

			return attachment;
		}

#if !PORTABLE
		/// <summary>
		/// Add the specified attachment.
		/// </summary>
		/// <remarks>
		/// <para>Adds the specified file as an attachment using the supplied Content-Type.</para>
		/// <para>For a list of known mime-types and their associated file extensions, see
		/// http://svn.apache.org/repos/asf/httpd/httpd/trunk/docs/conf/mime.types</para>
		/// </remarks>
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
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
		public MimeEntity Add (string fileName, ContentType contentType)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", "fileName");

			if (contentType == null)
				throw new ArgumentNullException ("contentType");

			using (var stream = File.OpenRead (fileName)) {
				var attachment = CreateAttachment (contentType, fileName, stream);

				attachments.Add (attachment);

				return attachment;
			}
		}

		/// <summary>
		/// Add the specified attachment.
		/// </summary>
		/// <remarks>
		/// <para>Adds the specified file as an attachment.</para>
		/// </remarks>
		/// <returns>The newly added attachment <see cref="MimeEntity"/>.</returns>
		/// <param name="fileName">The name of the file.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="fileName"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <paramref name="fileName"/> is a zero-length string, contains only white space, or
		/// contains one or more invalid characters as defined by
		/// <see cref="System.IO.Path.InvalidPathChars"/>.
		/// </exception>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// <paramref name="fileName"/> is an invalid file path.
		/// </exception>
		/// <exception cref="System.IO.FileNotFoundException">
		/// The specified file path could not be found.
		/// </exception>
		/// <exception cref="System.UnauthorizedAccessException">
		/// The user does not have access to read the specified file.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurred.
		/// </exception>
		public MimeEntity Add (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");

			if (fileName.Length == 0)
				throw new ArgumentException ("The specified file path is empty.", "fileName");

			using (var stream = File.OpenRead (fileName)) {
				var attachment = CreateAttachment (GetMimeType (fileName), fileName, stream);

				attachments.Add (attachment);

				return attachment;
			}
		}
#endif

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
		/// Gets the index of the requested attachment, if it exists.
		/// </summary>
		/// <remarks>
		/// Finds the index of the specified attachment, if it exists.
		/// </remarks>
		/// <returns>The index of the requested attachment; otherwise <value>-1</value>.</returns>
		/// <param name="attachment">The attachment.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="attachment"/> is <c>null</c>.
		/// </exception>
		public int IndexOf (MimeEntity attachment)
		{
			if (attachment == null)
				throw new ArgumentNullException ("attachment");

			return attachments.IndexOf (attachment);
		}

		/// <summary>
		/// Inserts the specified attachment at the given index.
		/// </summary>
		/// <remarks>
		/// Inserts the attachment at the specified index.
		/// </remarks>
		/// <param name="index">The index to insert the attachment.</param>
		/// <param name="attachment">The attachment.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="attachment"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void Insert (int index, MimeEntity attachment)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			if (attachment == null)
				throw new ArgumentNullException ("attachment");

			attachments.Insert (index, attachment);
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

		/// <summary>
		/// Removes the attachment at the specified index.
		/// </summary>
		/// <remarks>
		/// Removes the attachment at the specified index.
		/// </remarks>
		/// <param name="index">The index.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is out of range.
		/// </exception>
		public void RemoveAt (int index)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException ("index");

			attachments.RemoveAt (index);
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
