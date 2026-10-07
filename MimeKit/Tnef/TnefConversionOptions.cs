//
// TnefConversionOptions.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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

namespace MimeKit.Tnef {
	/// <summary>
	/// Options for converting a TNEF message to MIME.
	/// </summary>
	/// <remarks>
	/// Options for converting a <see cref="TnefMessage"/> to MIME using <see cref="TnefMessage.ConvertToMime"/>.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\TnefExamples.cs" region="ConvertToMime"/>
	/// </example>
	public class TnefConversionOptions
	{
		/// <summary>
		/// The default maximum number of modified occurrences of a recurring appointment to write to the calendar.
		/// </summary>
		/// <remarks>
		/// The default maximum number of modified occurrences (exceptions) of a recurring appointment to write to the
		/// generated <c>text/calendar</c> part.
		/// </remarks>
		public const int DefaultMaxCalendarExceptions = 1024;

		/// <summary>
		/// The default maximum total size of the modified occurrences of a recurring appointment that are written to the calendar.
		/// </summary>
		/// <remarks>
		/// The default maximum total size, in bytes, of the modified occurrences (exceptions) of a recurring appointment
		/// that are written to the generated <c>text/calendar</c> part (16 MB).
		/// </remarks>
		public const long DefaultMaxCalendarExceptionsSize = 16L * 1024 * 1024;

		/// <summary>
		/// The default conversion options.
		/// </summary>
		/// <remarks>
		/// The default conversion options. This instance should not be modified.
		/// </remarks>
		public static readonly TnefConversionOptions Default = new TnefConversionOptions ();

		int maxCalendarExceptions = DefaultMaxCalendarExceptions;
		long maxCalendarExceptionsSize = DefaultMaxCalendarExceptionsSize;

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefConversionOptions"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new set of conversion options with default values.
		/// </remarks>
		public TnefConversionOptions ()
		{
		}

		/// <summary>
		/// Get or set whether embedded messages should be converted to MIME.
		/// </summary>
		/// <remarks>
		/// <para>If <see langword="true"/>, each attachment that is an embedded TNEF message is converted to a
		/// <c>message/rfc822</c> part, and the information that was lost when converting it is included in
		/// <see cref="TnefConversionResult.Losses"/>.</para>
		/// <para>If <see langword="false"/>, each attachment that is an embedded TNEF message is added to the
		/// MIME message as an <c>application/ms-tnef</c> <see cref="TnefPart"/> containing the embedded TNEF stream.</para>
		/// <para>When the message is converted using its <see cref="TnefPropertyId.MimeSkeleton"/>, an embedded message
		/// that the skeleton represents as a <c>message/rfc822</c> part is always converted.</para>
		/// </remarks>
		/// <value><see langword="true"/> if embedded messages should be converted; otherwise, <see langword="false"/>.
		/// The default is <see langword="false"/>.</value>
		public bool ConvertEmbeddedMessages {
			get; set;
		}

		/// <summary>
		/// Get or set whether a <c>text/calendar</c> part should be generated for calendar items and meeting messages.
		/// </summary>
		/// <remarks>
		/// <para>If <see langword="true"/>, a message whose <see cref="TnefPropertyId.MessageClass"/> identifies it
		/// as a calendar item (<c>IPM.Appointment</c>) or a meeting message (<c>IPM.Schedule.Meeting.*</c>) gets an
		/// iCalendar (RFC 5545) <c>text/calendar</c> part, generated from its properties as described by [MS-OXCICAL]
		/// and placed as the last alternative of the message body, as described by [MS-OXCMAIL] section 2.1.3.3.8.
		/// The embedded messages that hold the exceptions to a recurring appointment are written to the calendar
		/// instead of being added as attachments.</para>
		/// <para>The calendar is not generated when the message is converted using its
		/// <see cref="TnefPropertyId.MimeSkeleton"/>, because the skeleton already describes the original MIME
		/// structure.</para>
		/// </remarks>
		/// <value><see langword="true"/> if a calendar should be generated; otherwise, <see langword="false"/>.
		/// The default is <see langword="true"/>.</value>
		public bool GenerateCalendar {
			get; set;
		} = true;

		/// <summary>
		/// Get or set the maximum number of modified occurrences of a recurring appointment to write to the calendar.
		/// </summary>
		/// <remarks>
		/// <para>Each modified occurrence (exception) of a recurring appointment is written to the generated
		/// <c>text/calendar</c> part as its own <c>VEVENT</c>, which repeats most of the appointment's properties,
		/// such as its description and attendees. A recurrence pattern needs only a few bytes per exception, so this
		/// limit protects against a small, maliciously formed TNEF message producing a very large calendar.</para>
		/// <para>Exceptions beyond the limit are omitted and reported as
		/// <see cref="TnefConversionLossKind.CalendarExceptionLimitExceeded"/>. Their embedded messages, if any, are added to
		/// the MIME message as attachments.</para>
		/// </remarks>
		/// <value>The maximum number of exceptions. The default is <see cref="DefaultMaxCalendarExceptions"/>.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than <c>0</c>.
		/// </exception>
		public int MaxCalendarExceptions {
			get { return maxCalendarExceptions; }
			set {
				if (value < 0)
					throw new System.ArgumentOutOfRangeException (nameof (value));

				maxCalendarExceptions = value;
			}
		}

		/// <summary>
		/// Get or set the maximum total size, in bytes, of the modified occurrences of a recurring appointment that are
		/// written to the calendar.
		/// </summary>
		/// <remarks>
		/// <para>Each modified occurrence (exception) of a recurring appointment is written to the generated
		/// <c>text/calendar</c> part as its own <c>VEVENT</c>, which repeats most of the appointment's properties.
		/// Limiting the number of exceptions with <see cref="MaxCalendarExceptions"/> is not enough on its own,
		/// because an appointment with a very large description or attendee list multiplies that size by the number
		/// of exceptions. This limit bounds the total size of the exception <c>VEVENT</c>s; the appointment itself
		/// is always written.</para>
		/// <para>Once an exception would exceed the limit, it and the remaining exceptions are omitted and reported as
		/// <see cref="TnefConversionLossKind.CalendarExceptionLimitExceeded"/>. Their embedded messages, if any, are added to
		/// the MIME message as attachments.</para>
		/// </remarks>
		/// <value>The maximum size of the exceptions, in bytes. The default is <see cref="DefaultMaxCalendarExceptionsSize"/>.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is less than <c>0</c>.
		/// </exception>
		public long MaxCalendarExceptionsSize {
			get { return maxCalendarExceptionsSize; }
			set {
				if (value < 0)
					throw new System.ArgumentOutOfRangeException (nameof (value));

				maxCalendarExceptionsSize = value;
			}
		}

		/// <summary>
		/// Get or set the method used to render OLE object attachments as images.
		/// </summary>
		/// <remarks>
		/// <para>[MS-OXCMAIL] section 2.1.3.4.4 recommends that OLE object attachments (whose content is an OLE compound
		/// file that few applications other than Microsoft Outlook can display) be converted to images. Rendering an
		/// OLE object requires the application that created it, so MimeKit cannot do this by itself. When this
		/// property is set, the converter is used for each OLE object attachment, and the image that it returns
		/// replaces the content of the attachment. See <see cref="TnefOleObjectConverter"/> for details.</para>
		/// <para>The converter is not called when the message is converted using its
		/// <see cref="TnefPropertyId.MimeSkeleton"/>.</para>
		/// </remarks>
		/// <value>The OLE object converter, or <see langword="null"/>. The default is <see langword="null"/>.</value>
		public TnefOleObjectConverter? OleObjectConverter {
			get; set;
		}

		/// <summary>
		/// Get or set the method used to generate the text for attachment placeholders in an RTF message body.
		/// </summary>
		/// <remarks>
		/// <para>Microsoft Outlook and Exchange mark the position at which each attachment is displayed in an RTF message
		/// body with a placeholder, as described by [MS-OXRTFEX] section 2.2.3.4. When the text/plain and text/html
		/// bodies are generated from the RTF body, the attachments that are images that web browsers can display are
		/// rendered as <c>&lt;img&gt;</c> elements in the text/html body. Other placeholders produce nothing unless this
		/// callback is set.</para>
		/// <para>When set, the callback is invoked once for each attachment that has a placeholder, and the text that
		/// it returns (if any) is written at that attachment's placeholder in the text/plain body and, unless the
		/// attachment is rendered as an image, in the text/html body (where it is HTML-encoded). If the number of
		/// placeholders does not match the number of attachments, the text is appended to the end of the body.</para>
		/// <para>The callback is not invoked when the message is converted using its
		/// <see cref="TnefPropertyId.MimeSkeleton"/>, or when the text/html body is extracted from HTML that is
		/// encapsulated within the RTF body (in which case only the text/plain body uses the text).</para>
		/// </remarks>
		/// <value>The attachment placeholder callback, or <see langword="null"/>. The default is <see langword="null"/>.</value>
		public TnefAttachmentPlaceholderCallback? AttachmentPlaceholderCallback {
			get; set;
		}

		/// <summary>
		/// Clone the options.
		/// </summary>
		/// <remarks>
		/// Creates a copy of the options.
		/// </remarks>
		/// <returns>A copy of the options.</returns>
		public TnefConversionOptions Clone ()
		{
			return new TnefConversionOptions {
				ConvertEmbeddedMessages = ConvertEmbeddedMessages,
				GenerateCalendar = GenerateCalendar,
				MaxCalendarExceptions = MaxCalendarExceptions,
				MaxCalendarExceptionsSize = MaxCalendarExceptionsSize,
				OleObjectConverter = OleObjectConverter,
				AttachmentPlaceholderCallback = AttachmentPlaceholderCallback
			};
		}
	}
}
