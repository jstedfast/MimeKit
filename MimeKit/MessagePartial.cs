//
// MessagePartial.cs
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
using System.Linq;
using System.Collections.Generic;

using MimeKit.IO;
using MimeKit.IO.Filters;
using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// A MIME part containing a partial message as its content.
	/// </summary>
	/// <remarks>
	/// Represents a MIME part with a Content-Type of message/partial.
	/// </remarks>
	public class MessagePartial : MimePart
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MessagePartial"/> class.
		/// </summary>
		/// <remarks>This constructor is used by <see cref="MimeKit.MimeParser"/>.</remarks>
		/// <param name="entity">Information used by the constructor.</param>
		public MessagePartial (MimeEntityConstructorInfo entity) : base (entity)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.MessagePartial"/> class.
		/// </summary>
		/// <param name="id">The id value shared among the partial message parts.</param>
		/// <param name="number">The part number for this partial message part.</param>
		/// <param name="total">The total number of partial message parts.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="id"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="number"/> is negative.</para>
		/// <para>-or-</para>
		/// <para><paramref name="total"/> is less than <paramref name="number"/>.</para>
		/// </exception>
		public MessagePartial (string id, int number, int total) : base ("message", "partial")
		{
			if (id == null)
				throw new ArgumentNullException ("id");

			if (number < 0)
				throw new ArgumentOutOfRangeException ("number");

			if (total < number)
				throw new ArgumentOutOfRangeException ("total");

			ContentType.Parameters.Add (new Parameter ("id", id));
			ContentType.Parameters.Add (new Parameter ("number", number.ToString ()));
			ContentType.Parameters.Add (new Parameter ("total", total.ToString ()));
		}

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public string Id {
			get { return ContentType.Parameters["id"]; }
		}

		/// <summary>
		/// Gets the part number for this partial message part.
		/// </summary>
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
		/// Gets the total number of parts.
		/// </summary>
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

		static MimeMessage CloneMessage (MimeMessage message)
		{
			var options = message.Headers.Options;
			var clone = new MimeMessage (options);

			foreach (var header in message.Headers)
				clone.Headers.Add (new Header (options, header.Field, header.RawValue));

			return clone;
		}

		/// <summary>
		/// Split the specified message into multiple messages, each with a
		/// message/partial body no larger than the max size specified.
		/// </summary>
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
				throw new ArgumentNullException ("message");

			if (maxSize < 1)
				throw new ArgumentOutOfRangeException ("maxSize");

			using (var memory = new MemoryStream ()) {
				message.WriteTo (memory);
				memory.Seek (0, SeekOrigin.Begin);

				if (memory.Length <= maxSize) {
					yield return message;
					yield break;
				}

				var streams = new List<Stream> ();
				var buf = memory.GetBuffer ();
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

				var id = message.MessageId ?? MimeUtils.GenerateMessageId ();
				int number = 1;

				foreach (var stream in streams) {
					var part = new MessagePartial (id, number++, streams.Count);
					part.ContentObject = new ContentObject (stream, ContentEncoding.Default);

					var submessage = CloneMessage (message);
					submessage.MessageId = MimeUtils.GenerateMessageId ();
					submessage.Body = part;

					yield return submessage;
				}
			}

			yield break;
		}

		static int PartialCompare (MessagePartial partial1, MessagePartial partial2)
		{
			if (!partial1.Number.HasValue || !partial2.Number.HasValue || partial1.Id != partial2.Id)
				throw new ArgumentException ("partial");

			return partial1.Number.Value - partial2.Number.Value;
		}

		/// <summary>
		/// Join the specified message/partial parts into the complete message.
		/// </summary>
		/// <param name="options">The parser options to use.</param>
		/// <param name="partials">The list of partial message parts.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="options"/> is <c>null</c>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="partials"/>is <c>null</c>.</para>
		/// </exception>
		public static MimeMessage Join (ParserOptions options, IEnumerable<MessagePartial> partials)
		{
			if (options == null)
				throw new ArgumentNullException ("options");

			if (partials == null)
				throw new ArgumentNullException ("partials");

			var parts = partials.ToList ();

			if (parts.Count == 0)
				return null;

			parts.Sort (PartialCompare);

			if (!parts[parts.Count - 1].Total.HasValue)
				throw new ArgumentException ("partials");

			int total = parts[parts.Count - 1].Total.Value;
			if (parts.Count != total)
				throw new ArgumentException ("partials");

			string id = parts[0].Id;

			using (var chained = new ChainedStream ()) {
				// chain all of the partial content streams...
				for (int i = 0; i < parts.Count; i++) {
					int number = parts[i].Number.Value;

					if (number != i + 1)
						throw new ArgumentException ("partials");

					var content = parts[i].ContentObject;
					content.Stream.Seek (0, SeekOrigin.Begin);
					var filtered = new FilteredStream (content.Stream);
					filtered.Add (DecoderFilter.Create (content.Encoding));
					chained.Add (filtered);
				}

				var parser = new MimeParser (options, chained);

				return parser.ParseMessage ();
			}
		}

		/// <summary>
		/// Join the specified message/partial parts into the complete message.
		/// </summary>
		/// <param name="partials">The list of partial message parts.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="partials"/>is <c>null</c>.
		/// </exception>
		public static MimeMessage Join (IEnumerable<MessagePartial> partials)
		{
			return Join (ParserOptions.Default, partials);
		}
	}
}
