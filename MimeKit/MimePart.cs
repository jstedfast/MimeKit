//
// MimePart.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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
using System.Text;
using System.Security.Cryptography;

using MimeKit.IO;
using MimeKit.IO.Filters;
using MimeKit.Encodings;

namespace MimeKit {
	/// <summary>
	/// A basic leaf-node MIME part that contains content such as the message body or an attachment.
	/// </summary>
	public class MimePart : MimeEntity
	{
		static readonly string[] ContentTransferEncodings = new string[] {
			null, "7bit", "8bit", "binary", "base64", "quoted-printable", "x-uuencode"
		};
		ContentEncoding encoding;
		string md5sum;
		int? duration;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimePart"/> class
		/// based on the <see cref="MimeEntityConstructorInfo"/>.
		/// </summary>
		/// <remarks>This constructor is used by <see cref="MimeKit.MimeParser"/>.</remarks>
		/// <param name="entity">Information used by the constructor.</param>
		public MimePart (MimeEntityConstructorInfo entity) : base (entity)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimePart"/> class
		/// with the specified media type and subtype.
		/// </summary>
		/// <param name="mediaType">The media type.</param>
		/// <param name="mediaSubtype">The media subtype.</param>
		/// <param name="args">An array of initialization parameters: headers and part content.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="mediaType"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="mediaSubtype"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para><paramref name="args"/> contains more than one <see cref="MimeKit.IContentObject"/> or
		/// <see cref="System.IO.Stream"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="args"/> contains one or more arguments of an unknown type.</para>
		/// </exception>
		public MimePart (string mediaType, string mediaSubtype, params object[] args) : this (mediaType, mediaSubtype)
		{
			if (args == null)
				throw new ArgumentNullException ("args");

			IContentObject content = null;

			foreach (object obj in args) {
				if (obj == null || TryInit (obj))
					continue;

				IContentObject co = obj as IContentObject;
				if (co != null) {
					if (content != null)
						throw new ArgumentException ("ContentObject should not be specified more than once.");

					content = co;
					continue;
				}

				Stream s = obj as Stream;
				if (s != null) {
					if (content != null)
						throw new ArgumentException ("Stream (used as content) should not be specified more than once.");

					// Use default as specified by ContentObject ctor when building a new MimePart.
					content = new ContentObject (s, ContentEncoding.Default);
					continue;
				}

				throw new ArgumentException ("Unknown initialization parameter: " + obj.GetType ());
			}

			if (content != null)
				ContentObject = content;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimePart"/> class
		/// with the specified media type and subtype.
		/// </summary>
		/// <param name="mediaType">The media type.</param>
		/// <param name="mediaSubtype">The media subtype.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="mediaType"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="mediaSubtype"/> is <c>null</c>.</para>
		/// </exception>
		public MimePart (string mediaType, string mediaSubtype) : base (mediaType, mediaSubtype)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MimePart"/> class
		/// with the default Content-Type of application/octet-stream.
		/// </summary>
		public MimePart () : base ("application", "octet-stream")
		{
		}

		/// <summary>
		/// Gets or sets the duration of the content if available.
		/// </summary>
		/// <value>The duration of the content.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative.
		/// </exception>
		public int? ContentDuration {
			get { return duration; }
			set {
				if (duration == value)
					return;

				if (value.HasValue && value.Value < 0)
					throw new ArgumentOutOfRangeException ("value");

				if (value.HasValue)
					SetHeader ("Content-Duration", value.Value.ToString ());
				else
					RemoveHeader ("Content-Duration");
			}
		}

		/// <summary>
		/// Gets or sets the md5sum of the content.
		/// </summary>
		/// <value>The md5sum of the content.</value>
		public string ContentMd5 {
			get { return md5sum; }
			set {
				if (md5sum == value)
					return;

				if (value != null)
					md5sum = value.Trim ();
				else
					md5sum = null;

				if (value != null)
					SetHeader ("Content-Md5", md5sum);
				else
					RemoveHeader ("Content-Md5");
			}
		}

		/// <summary>
		/// Gets or sets the content transfer encoding.
		/// </summary>
		/// <value>The content transfer encoding.</value>
		public ContentEncoding ContentTransferEncoding {
			get { return encoding; }
			set {
				if (encoding == value)
					return;

				var text = ContentTransferEncodings[(int) value];

				encoding = value;

				if (text != null)
					SetHeader ("Content-Transfer-Encoding", text);
				else
					RemoveHeader ("Content-Transfer-Encoding");
			}
		}

		/// <summary>
		/// Gets or sets the name of the file.
		/// </summary>
		/// <remarks>
		/// <para>First checks for the "filename" parameter on the Content-Disposition header. If
		/// that does not exist, then the "name" parameter on the Content-Type header is used.</para>
		/// <para>When setting the filename, both the "filename" parameter on the Content-Disposition
		/// header and the "name" parameter on the Content-Type header are set.</para>
		/// </remarks>
		/// <value>The name of the file.</value>
		public string FileName {
			get {
				string filename = null;

				if (ContentDisposition != null)
					filename = ContentDisposition.FileName;

				if (filename == null)
					filename = ContentType.Name;

				if (filename == null)
					return null;

				return filename.Trim ();
			}
			set {
				if (value != null) {
					if (ContentDisposition == null)
						ContentDisposition = new ContentDisposition ();
					ContentDisposition.FileName = value;
				} else if (ContentDisposition != null) {
					ContentDisposition.FileName = value;
				}

				ContentType.Name = value;
			}
		}

		/// <summary>
		/// Gets or sets the content of the mime part.
		/// </summary>
		/// <value>The content of the mime part.</value>
		public IContentObject ContentObject {
			get; set;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="MimePart"/> is an attachment.
		/// </summary>
		/// <value><c>true</c> if this <see cref="MimePart"/> is an attachment; otherwise, <c>false</c>.</value>
		public bool IsAttachment {
			get { return ContentDisposition != null && ContentDisposition.IsAttachment; }
			set {
				if (value) {
					if (ContentDisposition == null)
						ContentDisposition = new ContentDisposition (ContentDisposition.Attachment);
					else if (!ContentDisposition.IsAttachment)
						ContentDisposition.Disposition = ContentDisposition.Attachment;
				} else if (ContentDisposition != null && ContentDisposition.IsAttachment) {
					ContentDisposition.Disposition = ContentDisposition.Inline;
				}
			}
		}

		/// <summary>
		/// Calculates the most efficient content encoding given the specified constraint.
		/// </summary>
		/// <remarks>
		/// If no <see cref="ContentObject"/> is set, <see cref="ContentEncoding.SevenBit"/> will be returned.
		/// </remarks>
		/// <returns>The most efficient content encoding.</returns>
		public ContentEncoding GetBestEncoding (EncodingConstraint constraint)
		{
			if (ContentObject == null)
				return ContentEncoding.SevenBit;

			using (var measure = new MeasuringStream ()) {
				using (var filtered = new FilteredStream (measure)) {
					var filter = new BestEncodingFilter ();

					filtered.Add (filter);
					ContentObject.DecodeTo (filtered);
					filtered.Flush ();

					return filter.GetBestEncoding (constraint);
				}
			}
		}

		/// <summary>
		/// Computes the md5sum of the content.
		/// </summary>
		/// <returns>The md5sum of the content.</returns>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="ContentObject"/> is <c>null</c>.
		/// </exception>
		public string ComputeContentMd5 ()
		{
			if (ContentObject == null)
				throw new InvalidOperationException ("Cannot compute Md5 checksum without a ContentObject.");

			var stream = ContentObject.Stream;
			stream.Seek (0, SeekOrigin.Begin);

			byte[] checksum;

			using (var filtered = new FilteredStream (stream)) {
				filtered.Add (DecoderFilter.Create (ContentObject.Encoding));

				if (ContentType.Matches ("text", "*"))
					filtered.Add (new Unix2DosFilter ());

				using (var md5 = HashAlgorithm.Create ("MD5"))
					checksum = md5.ComputeHash (filtered);
			}

			var base64 = new Base64Encoder (true);
			var digest = new byte[base64.EstimateOutputLength (checksum.Length)];
			int n = base64.Flush (checksum, 0, checksum.Length, digest);

			return Encoding.ASCII.GetString (digest, 0, n);
		}

		/// <summary>
		/// Verifies the Content-Md5 value against an independently computed md5sum.
		/// </summary>
		/// <returns><c>true</c>, if content md5sum was verified, <c>false</c> otherwise.</returns>
		public bool VerifyContentMd5 ()
		{
			if (string.IsNullOrWhiteSpace (md5sum) || ContentObject == null)
				return false;

			return md5sum == ComputeContentMd5 ();
		}

		static bool NeedsEncoding (ContentEncoding encoding)
		{
			switch (encoding) {
			case ContentEncoding.EightBit:
			case ContentEncoding.Binary:
			case ContentEncoding.Default:
				return true;
			default:
				return false;
			}
		}

		/// <summary>
		/// Writes the <see cref="MimeKit.MimePart"/> to the specified stream.
		/// </summary>
		/// <param name="options">The formatting options.</param>
		/// <param name="stream">The stream.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="stream"/> is <c>null</c>.</para>
		/// </exception>
		public override void WriteTo (FormatOptions options, Stream stream)
		{
			var saved = encoding;

			if (NeedsEncoding (encoding)) {
				var best = GetBestEncoding (options.EncodingConstraint);

				if (best != ContentEncoding.SevenBit)
					ContentTransferEncoding = best;
			}

			try {
				base.WriteTo (options, stream);

				if (ContentObject == null)
					return;

				if (ContentObject.Encoding != encoding) {
					if (encoding == ContentEncoding.UUEncode) {
						var begin = string.Format ("begin 0644 {0}", FileName ?? "unknown");
						var buffer = Encoding.UTF8.GetBytes (begin);
						stream.Write (buffer, 0, buffer.Length);
						stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
					}

					// transcode the content into the desired Content-Transfer-Encoding
					using (var filtered = new FilteredStream (stream)) {
						filtered.Add (EncoderFilter.Create (encoding));

						if (encoding != ContentEncoding.Binary)
							filtered.Add (options.CreateNewLineFilter ());

						ContentObject.DecodeTo (filtered);
						filtered.Flush ();
					}

					if (encoding == ContentEncoding.UUEncode) {
						var buffer = Encoding.ASCII.GetBytes ("end");
						stream.Write (buffer, 0, buffer.Length);
						stream.Write (options.NewLineBytes, 0, options.NewLineBytes.Length);
					}
				} else if (encoding != ContentEncoding.Binary) {
					using (var filtered = new FilteredStream (stream)) {
						filtered.Add (options.CreateNewLineFilter ());
						ContentObject.WriteTo (filtered);
						filtered.Flush ();
					}
				} else {
					ContentObject.WriteTo (stream);
				}
			} finally {
				if (saved != ContentEncoding.Default)
					ContentTransferEncoding = saved;
			}
		}

		/// <summary>
		/// Called when the headers change in some way.
		/// </summary>
		/// <param name="action">The type of change.</param>
		/// <param name="header">The header being added, changed or removed.</param>
		protected override void OnHeadersChanged (HeaderListChangedAction action, Header header)
		{
			string text;
			int value;

			base.OnHeadersChanged (action, header);

			switch (action) {
			case HeaderListChangedAction.Added:
			case HeaderListChangedAction.Changed:
				switch (header.Id) {
				case HeaderId.ContentTransferEncoding:
					text = header.Value.Trim ().ToLowerInvariant ();
					encoding = ContentEncoding.Default;
					for (int i = 1; i < ContentTransferEncodings.Length; i++) {
						if (ContentTransferEncodings[i] == text) {
							encoding = (ContentEncoding) i;
							break;
						}
					}
					break;
				case HeaderId.ContentDuration:
					if (int.TryParse (header.Value, out value))
						duration = value;
					else
						duration = null;
					break;
				case HeaderId.ContentMd5:
					md5sum = header.Value.Trim ();
					break;
				}
				break;
			case HeaderListChangedAction.Removed:
				switch (header.Id) {
				case HeaderId.ContentTransferEncoding:
					encoding = ContentEncoding.Default;
					break;
				case HeaderId.ContentDuration:
					duration = null;
					break;
				case HeaderId.ContentMd5:
					md5sum = null;
					break;
				}
				break;
			case HeaderListChangedAction.Cleared:
				encoding = ContentEncoding.Default;
				duration = null;
				md5sum = null;
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}
	}
}
