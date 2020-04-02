﻿//
// MessagePartial.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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
using System.Collections.Generic;

using MimeKit.IO;
using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A MIME part containing a partial message as its content.
	/// </summary>
	/// <remarks>
	/// <para>The "message/partial" MIME-type is used to split large messages into
	/// multiple parts, typically to work around transport systems that have size
	/// limitations (for example, some SMTP servers limit have a maximum message
	/// size that they will accept).</para>
	/// </remarks>
	public class MessagePartial : MimePart
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MessagePartial"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by <see cref="MimeKit.MimeParser"/>.
		/// </remarks>
		/// <param name="args">Information used by the constructor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="args"/> is <c>null</c>.
		/// </exception>
		public MessagePartial (MimeEntityConstructorArgs args) : base (args)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MessagePartial"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new message/partial entity.</para>
		/// <para>Three (3) parameters must be specified in the Content-Type header
		/// of a message/partial: id, number, and total.</para>
		/// <para>The "id" parameter is a unique identifier used to match the parts together.</para>
		/// <para>The "number" parameter is the sequential (1-based) index of the partial message fragment.</para>
		/// <para>The "total" parameter is the total number of pieces that make up the complete message.</para>
		/// </remarks>
		/// <param name="id">The id value shared among the partial message parts.</param>
		/// <param name="number">The (1-based) part number for this partial message part.</param>
		/// <param name="total">The total number of partial message parts.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="id"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="number"/> is less than <c>1</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="total"/> is less than <paramref name="number"/>.</para>
		/// </exception>
		public MessagePartial (string id, int number, int total) : base ("message", "partial")
		{
			if (id == null)
				throw new ArgumentNullException (nameof (id));

			if (number < 1)
				throw new ArgumentOutOfRangeException (nameof (number));

			if (total < number)
				throw new ArgumentOutOfRangeException (nameof (total));

			ContentType.Parameters.Add (new Parameter ("id", id));
			ContentType.Parameters.Add (new Parameter ("number", number.ToString ()));
			ContentType.Parameters.Add (new Parameter ("total", total.ToString ()));
		}

		/// <summary>
		/// Gets the "id" parameter of the Content-Type header.
		/// </summary>
		/// <remarks>
		/// The "id" parameter is a unique identifier used to match the parts together.
		/// </remarks>
		/// <value>The identifier.</value>
		public string Id {
			get { return ContentType.Parameters["id"]; }
		}

		/// <summary>
		/// Gets the "number" parameter of the Content-Type header.
		/// </summary>
		/// <remarks>
		/// The "number" parameter is the sequential (1-based) index of the partial message fragment.
		/// </remarks>
		/// <value>The part number.</value>
		public int? Number {
			get {
				var text = ContentType.Parameters["number"];
				int number;

				if (text == null || !int.TryParse (text, out number))
					return null;

				return number;
			}
		}

		/// <summary>
		/// Gets the "total" parameter of the Content-Type header.
		/// </summary>
		/// <remarks>
		/// The "total" parameter is the total number of pieces that make up the complete message.
		/// </remarks>
		/// <value>The total number of parts.</value>
		public int? Total {
			get {
				var text = ContentType.Parameters["total"];
				int total;

				if (text == null || !int.TryParse (text, out total))
					return null;

				return total;
			}
		}

		/// <summary>
		/// Dispatches to the specific visit method for this MIME entity.
		/// </summary>
		/// <remarks>
		/// This default implementation for <see cref="MimeKit.MessagePartial"/> nodes
		/// calls <see cref="MimeKit.MimeVisitor.VisitMessagePartial"/>. Override this
		/// method to call into a more specific method on a derived visitor class
		/// of the <see cref="MimeKit.MimeVisitor"/> class. However, it should still
		/// support unknown visitors by calling
		/// <see cref="MimeKit.MimeVisitor.VisitMessagePartial"/>.
		/// </remarks>
		/// <param name="visitor">The visitor.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="visitor"/> is <c>null</c>.
		/// </exception>
		public override void Accept (MimeVisitor visitor)
		{
			if (visitor == null)
				throw new ArgumentNullException (nameof (visitor));

			visitor.VisitMessagePartial (this);
		}

		static MimeMessage CloneMessage (MimeMessage message)
		{
			var options = message.Headers.Options;
			var clone = new MimeMessage (options);

			foreach (var header in message.Headers)
				clone.Headers.Add (header.Clone ());

			return clone;
		}

		/// <summary>
		/// Splits the specified message into multiple messages.
		/// </summary>
		/// <remarks>
		/// Splits the specified message into multiple messages, each with a
		/// message/partial body no larger than the max size specified.
		/// </remarks>
		/// <returns>An enumeration of partial messages.</returns>
		/// <param name="message">The message.</param>
		/// <param name="maxSize">The maximum size for each message body.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="message"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="maxSize"/> is less than <c>1</c>.
		/// </exception>
		public static IEnumerable<MimeMessage> Split (MimeMessage message, int maxSize)
		{
			if (message == null)
				throw new ArgumentNullException (nameof (message));

			if (maxSize < 1)
				throw new ArgumentOutOfRangeException (nameof (maxSize));

			var options = FormatOptions.Default.Clone ();
			foreach (HeaderId id in Enum.GetValues (typeof (HeaderId))) {
				switch (id) {
				case HeaderId.Subject:
				case HeaderId.MessageId:
				case HeaderId.Encrypted:
				case HeaderId.MimeVersion:
				case HeaderId.ContentAlternative:
				case HeaderId.ContentBase:
				case HeaderId.ContentClass:
				case HeaderId.ContentDescription:
				case HeaderId.ContentDisposition:
				case HeaderId.ContentDuration:
				case HeaderId.ContentFeatures:
				case HeaderId.ContentId:
				case HeaderId.ContentIdentifier:
				case HeaderId.ContentLanguage:
				case HeaderId.ContentLength:
				case HeaderId.ContentLocation:
				case HeaderId.ContentMd5:
				case HeaderId.ContentReturn:
				case HeaderId.ContentTransferEncoding:
				case HeaderId.ContentTranslationType:
				case HeaderId.ContentType:
					break;
				default:
					options.HiddenHeaders.Add (id);
					break;
				}
			}

			var memory = new MemoryStream ();

			message.WriteTo (options, memory);
			memory.Seek (0, SeekOrigin.Begin);

			if (memory.Length <= maxSize) {
				memory.Dispose ();

				yield return message;
				yield break;
			}

			var streams = new List<Stream> ();
#if !NETSTANDARD1_3 && !NETSTANDARD1_6
			var buf = memory.GetBuffer ();
#else
			var buf = memory.ToArray ();
#endif
			long startIndex = 0;

			while (startIndex < memory.Length) {
				// Preferably, we'd split on whole-lines if we can,
				// but if that's not possible, split on max size
				long endIndex = Math.Min (memory.Length, startIndex + maxSize);

				if (endIndex < memory.Length) {
					long ebx = endIndex;

					while (ebx > (startIndex + 1) && buf[ebx] != (byte) '\n')
						ebx--;

					if (buf[ebx] == (byte) '\n')
						endIndex = ebx + 1;
				}

				streams.Add (new BoundStream (memory, startIndex, endIndex, true));
				startIndex = endIndex;
			}

			var msgid = message.MessageId ?? MimeUtils.GenerateMessageId ();
			int number = 1;

			foreach (var stream in streams) {
				var part = new MessagePartial (msgid, number++, streams.Count) {
					Content = new MimeContent (stream)
				};

				var submessage = CloneMessage (message);
				submessage.MessageId = MimeUtils.GenerateMessageId ();
				submessage.Body = part;

				yield return submessage;
			}

			yield break;
		}

		static int PartialCompare (MessagePartial partial1, MessagePartial partial2)
		{
			if (!partial1.Number.HasValue || !partial2.Number.HasValue || partial1.Id != partial2.Id)
				throw new ArgumentException ("Partial messages have mismatching identifiers.", "partials");

			return partial1.Number.Value - partial2.Number.Value;
		}

		static void CombineHeaders (MimeMessage message, MimeMessage joined)
		{
			var headers = new List<Header> ();
			int i = 0;

			// RFC2046: Any header fields in the enclosed message which do not start with "Content-"
			// (except for the "Subject", "Message-ID", "Encrypted", and "MIME-Version" fields) will
			// be ignored and dropped.
			while (i < joined.Headers.Count) {
				var header = joined.Headers[i];

				switch (header.Id) {
				case HeaderId.Subject:
				case HeaderId.MessageId:
				case HeaderId.Encrypted:
				case HeaderId.MimeVersion:
					headers.Add (header);
					header.Offset = null;
					i++;
					break;
				default:
					joined.Headers.RemoveAt (i);
					break;
				}
			}

			// RFC2046: All of the header fields from the initial enclosing message, except
			// those that start with "Content-" and the specific header fields "Subject",
			// "Message-ID", "Encrypted", and "MIME-Version", must be copied, in order,
			// to the new message.
			i = 0;
			foreach (var header in message.Headers) {
				switch (header.Id) {
				case HeaderId.Subject:
				case HeaderId.MessageId:
				case HeaderId.Encrypted:
				case HeaderId.MimeVersion:
					for (int j = 0; j < headers.Count; j++) {
						if (headers[j].Id == header.Id) {
							var original = headers[j];

							joined.Headers.Remove (original);
							joined.Headers.Insert (i++, original);
							headers.RemoveAt (j);
							break;
						}
					}
					break;
				default:
					var clone = header.Clone ();
					clone.Offset = null;

					joined.Headers.Insert (i++, clone);
					break;
				}
			}

			if (joined.Body != null) {
				foreach (var header in joined.Body.Headers)
					header.Offset = null;
			}
		}

		static MimeMessage Join (ParserOptions options, MimeMessage message, IEnumerable<MessagePartial> partials, bool allowNullMessage)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));

			if (!allowNullMessage && message == null)
				throw new ArgumentNullException (nameof (message));

			if (partials == null)
				throw new ArgumentNullException (nameof (partials));

			var parts = partials.ToList ();

			if (parts.Count == 0)
				return null;

			parts.Sort (PartialCompare);

			if (!parts[parts.Count - 1].Total.HasValue)
				throw new ArgumentException ("The last partial does not have a Total.", nameof (partials));

			int total = parts[parts.Count - 1].Total.Value;
			if (parts.Count != total)
				throw new ArgumentException ("The number of partials provided does not match the expected count.", nameof (partials));

			string id = parts[0].Id;

			using (var chained = new ChainedStream ()) {
				// chain all of the partial content streams...
				for (int i = 0; i < parts.Count; i++) {
					int number = parts[i].Number.Value;

					if (number != i + 1)
						throw new ArgumentException ("One or more partials is missing.", nameof (partials));

					var content = parts[i].Content;

					chained.Add (content.Open ());
				}

				var parser = new MimeParser (options, chained);
				var joined = parser.ParseMessage ();

				if (message != null)
					CombineHeaders (message, joined);

				return joined;
			}
		}

		/// <summary>
		/// Joins the specified message/partial parts into the complete message.
		/// </summary>
		/// <remarks>
		/// Combines all of the message/partial fragments into its original,
		/// complete, message.
		/// </remarks>
		/// <returns>The re-combined message.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="message">The message that contains the first `message/partial` part.</param>
		/// <param name="partials">The list of partial message parts.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="message"/>is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="partials"/>is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para>The last partial does not have a Total.</para>
		/// <para>-or-</para>
		/// <para>The number of partials provided does not match the expected count.</para>
		/// <para>-or-</para>
		/// <para>One or more partials is missing.</para>
		/// </exception>
		public static MimeMessage Join (ParserOptions options, MimeMessage message, IEnumerable<MessagePartial> partials)
		{
			return Join (options, message, partials, false);
		}

		/// <summary>
		/// Joins the specified message/partial parts into the complete message.
		/// </summary>
		/// <remarks>
		/// Combines all of the message/partial fragments into its original,
		/// complete, message.
		/// </remarks>
		/// <returns>The re-combined message.</returns>
		/// <param name="message">The message that contains the first `message/partial` part.</param>
		/// <param name="partials">The list of partial message parts.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="message"/>is <c>null</c></para>.
		/// <para>-or-</para>
		/// <para><paramref name="partials"/>is <c>null</c>.</para>
		/// </exception>
		public static MimeMessage Join (MimeMessage message, IEnumerable<MessagePartial> partials)
		{
			return Join (ParserOptions.Default, message, partials, false);
		}

		/// <summary>
		/// Joins the specified message/partial parts into the complete message.
		/// </summary>
		/// <remarks>
		/// Combines all of the message/partial fragments into its original,
		/// complete, message.
		/// </remarks>
		/// <returns>The re-combined message.</returns>
		/// <param name="options">The parser options to use.</param>
		/// <param name="partials">The list of partial message parts.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="partials"/>is <c>null</c>.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// <para>The last partial does not have a Total.</para>
		/// <para>-or-</para>
		/// <para>The number of partials provided does not match the expected count.</para>
		/// <para>-or-</para>
		/// <para>One or more partials is missing.</para>
		/// </exception>
		[Obsolete ("Use MessagePartial.Join (ParserOptions, MimeMessage, IEnumerable<MessagePartial>) instead.")]
		public static MimeMessage Join (ParserOptions options, IEnumerable<MessagePartial> partials)
		{
			return Join (options, null, partials, true);
		}

		/// <summary>
		/// Joins the specified message/partial parts into the complete message.
		/// </summary>
		/// <remarks>
		/// Combines all of the message/partial fragments into its original,
		/// complete, message.
		/// </remarks>
		/// <returns>The re-combined message.</returns>
		/// <param name="partials">The list of partial message parts.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="partials"/>is <c>null</c>.
		/// </exception>
		[Obsolete ("Use MessagePartial.Join (MimeMessage, IEnumerable<MessagePartial>) instead.")]
		public static MimeMessage Join (IEnumerable<MessagePartial> partials)
		{
			return Join (ParserOptions.Default, null, partials, true);
		}
	}
}
