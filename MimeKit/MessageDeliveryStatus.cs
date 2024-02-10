//
// MessageDeliveryStatus.cs
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

using MimeKit.IO;
using MimeKit.IO.Filters;
using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A message delivery status MIME part.
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
	public class MessageDeliveryStatus : MimePart, IMessageDeliveryStatus
	{
		HeaderListCollection groups;

		/// <summary>
		/// Initialize a new instance of the <see cref="MessageDeliveryStatus"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public MessageDeliveryStatus (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="MessageDeliveryStatus"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="MessageDeliveryStatus"/>.
		/// </remarks>
		public MessageDeliveryStatus () : base ("message", "delivery-status")
		{
		}

		void CheckDisposed ()
		{
			CheckDisposed (nameof (MessageDeliveryStatus));
		}

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
		/// The <see cref="MessageDeliveryStatus"/> has been disposed.
		/// </exception>
		public HeaderListCollection StatusGroups {
			get {
				CheckDisposed ();

				if (groups is null) {
					if (Content is null) {
						Content = new MimeContent (new MemoryBlockStream ());
						groups = new HeaderListCollection ();
					} else {
						ParseStatusGroups ();
					}

					groups.Changed += OnGroupsChanged;
				}

				return groups;
			}
		}

		void ParseStatusGroups ()
		{
			groups = new HeaderListCollection ();

			try {
				using (var stream = Content.Open ()) {
					var parser = new MimeParser (stream, MimeFormat.Entity);
					var encoding = ContentEncoding.Default;

					// According to rfc3464, there are 1 or more Status Groups consisting of a block of field/value
					// pairs (aka headers) separated by a blank line.
					while (!parser.IsEndOfStream) {
						var fields = parser.ParseStatusGroup ();
						groups.Add (fields);

						// Note: Office365 seems to sometimes base64 encode everything after the first Status Group of headers.
						//
						// In the sample case that @alex-jitbit provided in issue #250, Office365 added a Content-Transfer-Encoding
						// header to the first Status Group and then base64 encoded the remainder of the content. Therefore, if we
						// encounter a Content-Transfer-Encoding header (that needs decoding), break out of this loop so that we can
						// decode the rest of the content and parse the result for the remainder of the Status Groups.
						if (fields.TryGetHeader (HeaderId.ContentTransferEncoding, out var header)) {
							MimeUtils.TryParse (header.Value, out encoding);

							// Note: Base64, QuotedPrintable and UUEncode are all > Binary
							if (encoding > ContentEncoding.Binary)
								break;

							// Note: If the Content-Transfer-Encoding is 7bit, 8bit, or even binary, then the content doesn't need to
							// be decoded in order to continue parsing the remaining Status Groups.
							encoding = ContentEncoding.Default;
						}
					}

					if (encoding != ContentEncoding.Default) {
						// This means that the remainder of the Status Groups have been encoded, so we'll need to decode
						// the rest of the content stream in order to parse them.
						using (var content = parser.ReadToEos ()) {
							using (var filtered = new FilteredStream (content)) {
								filtered.Add (DecoderFilter.Create (encoding));
								parser.SetStream (filtered, MimeFormat.Entity);

								while (!parser.IsEndOfStream) {
									var fields = parser.ParseHeaders ();
									groups.Add (fields);
								}
							}
						}
					}
				}
			} catch (FormatException) {
			}
		}

		void OnGroupsChanged (object sender, EventArgs e)
		{
			var stream = new MemoryBlockStream ();
			var options = FormatOptions.Default;

			try {
				for (int i = 0; i < groups.Count; i++)
					groups[i].WriteTo (options, stream);

				stream.Position = 0;
			} catch {
				stream.Dispose ();
				throw;
			}

			Content?.Dispose ();
			Content = new MimeContent (stream);
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="MessageDeliveryStatus"/> nodes
		/// calls <see cref="MimeVisitor.VisitMessageDeliveryStatus"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeVisitor.VisitMessageDeliveryStatus"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ObjectDisposedException">
		/// The <see cref="MessageDeliveryStatus"/> has been disposed.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor is null)
				throw new ArgumentNullException (nameof (visitor));

			CheckDisposed ();

			visitor.VisitMessageDeliveryStatus (this);
		}
	}
}
