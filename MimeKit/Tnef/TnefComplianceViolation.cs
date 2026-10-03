//
// TnefComplianceViolation.cs
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
	/// An enumeration of potential TNEF compliance violations.
	/// </summary>
	/// <remarks>
	/// <para>An enumeration of the ways in which a TNEF stream may deviate from the
	/// [MS-OXTNEF] and [MS-OXCDATA] specifications, as reported to an
	/// <see cref="ITnefComplianceLogger"/>.</para>
	/// <para>A handful of members do not describe a defect in the stream at all, but rather a
	/// limit that the stream exceeded. They are reported the same way so that a consumer can
	/// tell that some of the content was not processed.</para>
	/// </remarks>
	public enum TnefComplianceViolation
	{
		// Note: The members below are grouped by subject matter so that the enumeration reads well,
		// but the numeric values are append-only. C# inlines enum constants into consuming
		// assemblies at compile time, so renumbering an existing member silently changes the meaning
		// of code that was compiled against an earlier version of MimeKit, as well as any value that
		// has been persisted or logged. A new violation therefore belongs in whichever #region
		// covers its subject matter, but it must take the next unused value rather than the value
		// that its position would suggest. TnefComplianceViolationTests enforces both halves of this
		// rule.

		/// <summary>
		/// No violation.
		/// </summary>
		/// <remarks>
		/// This value is never reported. It exists so that the default value of a
		/// <see cref="TnefComplianceIssue"/> is distinguishable from a genuine violation.
		/// </remarks>
		None                                        = 0,

		#region Stream Header

		/// <summary>
		/// The stream did not begin with the TNEF signature.
		/// </summary>
		/// <remarks>
		/// [MS-OXTNEF] requires that a TNEF stream begin with the 32-bit little-endian signature
		/// <c>0x223E9F78</c>. A stream that does not is not TNEF at all, so nothing in it can be parsed.
		/// </remarks>
		InvalidSignature                            = 1,

		/// <summary>
		/// The <c>attTnefVersion</c> attribute specified an unsupported version.
		/// </summary>
		/// <remarks>
		/// [MS-OXTNEF] defines a single TNEF version, <c>0x00010000</c>. Parsing continues, but a
		/// stream that claims a different version may use encodings that are not understood.
		/// </remarks>
		UnsupportedVersion                          = 2,

		/// <summary>
		/// The <c>attOemCodepage</c> attribute specified a codepage that could not be resolved.
		/// </summary>
		/// <remarks>
		/// The codepage is used to decode every 8-bit string in the stream. When it cannot be resolved,
		/// the default codepage is used instead, which may decode those strings incorrectly.
		/// </remarks>
		InvalidMessageCodepage                      = 3,

		/// <summary>
		/// The <c>attMessageClass</c> or <c>attOriginalMessageClass</c> attribute was not a valid
		/// message class.
		/// </summary>
		/// <remarks>
		/// [MS-OXCMSG] defines a message class as a string of at most 255 printable ASCII characters.
		/// The message class determines how a client interprets and renders the rest of the message.
		/// </remarks>
		InvalidMessageClass                         = 4,

		#endregion

		#region Attribute Framing

		/// <summary>
		/// An attribute had a level that was neither the message level nor the attachment level.
		/// </summary>
		/// <remarks>
		/// [MS-OXTNEF] defines only two attribute levels: <c>0x01</c> for message attributes and
		/// <c>0x02</c> for attachment attributes. There is no way to know which object an attribute
		/// with any other level belongs to.
		/// </remarks>
		InvalidAttributeLevel                       = 5,

		/// <summary>
		/// A message-level attribute appeared after the first attachment-level attribute.
		/// </summary>
		/// <remarks>
		/// [MS-OXTNEF] requires that all message attributes precede all attachment attributes.
		/// Implementations disagree about whether a late message attribute applies to the message or is
		/// discarded, so the same stream can be presented differently by different readers.
		/// </remarks>
		MessageAttributeAfterAttachment             = 6,

		/// <summary>
		/// An attribute appeared at a level that it is not defined for.
		/// </summary>
		/// <remarks>
		/// [MS-OXTNEF] defines each attribute as belonging to either the message or an attachment. An
		/// attachment attribute at the message level, or a message attribute at the attachment level,
		/// may be applied to the wrong object or discarded depending on the reader.
		/// </remarks>
		AttributeLevelMismatch                      = 7,

		/// <summary>
		/// An attribute had an unrecognized attribute identifier.
		/// </summary>
		/// <remarks>
		/// The attribute is skipped, so whatever information it carried is not available.
		/// </remarks>
		UnknownAttribute                            = 8,

		/// <summary>
		/// An attribute had a length that cannot be valid.
		/// </summary>
		/// <remarks>
		/// The length of an attribute is the only way to find where the next attribute begins. A length
		/// that is too large to represent cannot be followed, so the remainder of the stream cannot be
		/// parsed.
		/// </remarks>
		InvalidAttributeLength                      = 9,

		/// <summary>
		/// The checksum of an attribute did not match its value.
		/// </summary>
		/// <remarks>
		/// [MS-OXTNEF] defines the checksum of an attribute as the sum of the bytes in its value modulo
		/// 65536. A mismatch indicates that the value was altered or corrupted after it was written.
		/// The value is still used.
		/// </remarks>
		AttributeChecksumMismatch                   = 10,

		/// <summary>
		/// An attribute value was malformed for the type of the attribute.
		/// </summary>
		/// <remarks>
		/// The attribute value was too short, or otherwise did not have the layout that [MS-OXTNEF]
		/// defines for the attribute, so it could not be interpreted.
		/// </remarks>
		InvalidAttributeValue                       = 11,

		/// <summary>
		/// A date value was invalid.
		/// </summary>
		/// <remarks>
		/// A date attribute or a <c>PT_SYSTIME</c> property described a date and time that does not exist
		/// or cannot be represented, so the value is not available.
		/// </remarks>
		InvalidDate                                 = 12,

		/// <summary>
		/// The stream ended before the end of the current attribute.
		/// </summary>
		/// <remarks>
		/// The stream was most likely truncated in transit or in storage. Whatever followed the point of
		/// truncation is lost.
		/// </remarks>
		TruncatedStream                             = 13,

		#endregion

		#region MAPI Properties

		/// <summary>
		/// A property list had a property count that cannot be valid.
		/// </summary>
		/// <remarks>
		/// The property count was negative, or larger than the number of properties that could fit
		/// within the attribute that contains them.
		/// </remarks>
		InvalidPropertyCount                        = 14,

		/// <summary>
		/// A property table had a row count that cannot be valid.
		/// </summary>
		/// <remarks>
		/// The row count of the <c>attRecipTable</c> attribute was negative, or larger than the number of
		/// rows that could fit within the attribute.
		/// </remarks>
		InvalidRowCount                             = 15,

		/// <summary>
		/// A property had a value count that cannot be valid.
		/// </summary>
		/// <remarks>
		/// The value count of a multi-valued or variable-length property was negative, larger than the
		/// number of values that could fit within the attribute that contains it, or, for a property
		/// that is not multi-valued, other than exactly one.
		/// </remarks>
		InvalidValueCount                           = 16,

		/// <summary>
		/// A property value had a length that cannot be valid.
		/// </summary>
		/// <remarks>
		/// The length of a variable-length property value was negative, or extended beyond the end of the
		/// attribute that contains it.
		/// </remarks>
		InvalidPropertyLength                       = 17,

		/// <summary>
		/// A property had an unsupported property type.
		/// </summary>
		/// <remarks>
		/// The size of a property value is determined by its type, so a value of an unknown type cannot
		/// be skipped. The remaining properties in the same attribute cannot be read.
		/// </remarks>
		UnsupportedPropertyType                     = 18,

		/// <summary>
		/// A named property had an invalid kind.
		/// </summary>
		/// <remarks>
		/// [MS-OXTNEF] defines the kind of a named property as either <c>0x00000000</c> (a numeric
		/// identifier) or <c>0x00000001</c> (a string name). With any other kind, the size of the name
		/// cannot be determined, so the remaining properties in the same attribute cannot be read.
		/// </remarks>
		InvalidNamedPropertyKind                    = 19,

		#endregion

		#region Limits

		/// <summary>
		/// Embedded messages were nested more deeply than the configured limit.
		/// </summary>
		/// <remarks>
		/// This is not a defect in the stream, but the embedded messages beyond the limit were not
		/// processed.
		/// </remarks>
		NestingTooDeep                              = 20,

		/// <summary>
		/// The stream contained more attachments than the configured limit.
		/// </summary>
		/// <remarks>
		/// This is not a defect in the stream, but the attachments beyond the limit were not processed.
		/// </remarks>
		TooManyAttachments                          = 21,

		/// <summary>
		/// The stream contained more data than the configured limit.
		/// </summary>
		/// <remarks>
		/// This is not a defect in the stream, but the data beyond the limit was not processed.
		/// </remarks>
		DataSizeLimitExceeded                       = 22,

		#endregion

		#region Reporting

		/// <summary>
		/// Too many compliance issues were detected and the remainder were suppressed.
		/// </summary>
		/// <remarks>
		/// <para>This is reported once, at the point where some violation first exceeds the configured
		/// per-violation limit, so that a report that has been truncated is never mistaken for a
		/// complete one.</para>
		/// <para>It is not a defect and does not belong in the catalogue of them, so it is deliberately
		/// parked at the end of the range, leaving the defect values contiguous and free to grow.</para>
		/// </remarks>
		TooManyComplianceIssues                     = int.MaxValue,

		#endregion
	}
}
