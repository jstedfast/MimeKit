//
// TnefComplianceIssue.cs
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

using System;
using System.Text;
using System.Globalization;

namespace MimeKit.Tnef {
	/// <summary>
	/// A TNEF compliance issue detected while parsing.
	/// </summary>
	/// <remarks>
	/// <para>Describes a single deviation from the TNEF specifications along with the location within
	/// the stream where it was detected.</para>
	/// <para>New properties may be added to this structure in future versions of MimeKit in order to
	/// provide richer context about a violation. For that reason, always construct instances using one
	/// of the available constructors rather than relying on the default value.</para>
	/// <para>The default value of this structure has a <see cref="Violation"/> of
	/// <see cref="TnefComplianceViolation.None"/>, which is never reported and cannot be constructed,
	/// so it may be used to detect an uninitialized instance.</para>
	/// </remarks>
	public readonly struct TnefComplianceIssue : IEquatable<TnefComplianceIssue>
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="TnefComplianceIssue"/> struct.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefComplianceIssue"/> for a violation that was detected in the
		/// top-level TNEF stream outside of any attribute.
		/// </remarks>
		/// <param name="violation">The specific TNEF compliance violation that occurred.</param>
		/// <param name="streamOffset">The offset within the stream where the violation was found.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="TnefComplianceViolation"/>.
		/// </exception>
		public TnefComplianceIssue (TnefComplianceViolation violation, long streamOffset) : this (violation, streamOffset, 0, TnefAttributeTag.Null, TnefPropertyTag.Null)
		{
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefComplianceIssue"/> struct.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefComplianceIssue"/>.
		/// </remarks>
		/// <param name="violation">The specific TNEF compliance violation that occurred.</param>
		/// <param name="streamOffset">The offset within the stream where the violation was found.</param>
		/// <param name="depth">The nesting depth of the embedded message that the violation was found in,
		/// or <c>0</c> for the top-level message.</param>
		/// <param name="attributeTag">The attribute that was being read when the violation was found, or
		/// <see cref="TnefAttributeTag.Null"/> if the violation was not found within an attribute.</param>
		/// <param name="propertyTag">The property that was being read when the violation was found, or
		/// <see cref="TnefPropertyTag.Null"/> if the violation was not found within a property.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <para><paramref name="violation"/> is not a valid <see cref="TnefComplianceViolation"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="depth"/> is negative.</para>
		/// </exception>
		public TnefComplianceIssue (TnefComplianceViolation violation, long streamOffset, int depth, TnefAttributeTag attributeTag, TnefPropertyTag propertyTag)
		{
			// Note: TooManyComplianceIssues is not part of the contiguous range of defects, so it is
			// checked separately.
			if ((violation <= TnefComplianceViolation.None || violation > TnefComplianceViolation.DataSizeLimitExceeded) && violation != TnefComplianceViolation.TooManyComplianceIssues)
				throw new ArgumentOutOfRangeException (nameof (violation));

			if (depth < 0)
				throw new ArgumentOutOfRangeException (nameof (depth));

			Violation = violation;
			StreamOffset = streamOffset;
			Depth = depth;
			AttributeTag = attributeTag;
			PropertyTag = propertyTag;
		}

		/// <summary>
		/// Get the specific TNEF compliance violation that occurred.
		/// </summary>
		/// <remarks>
		/// Gets the specific TNEF compliance violation that occurred.
		/// </remarks>
		/// <value>The TNEF compliance violation.</value>
		public TnefComplianceViolation Violation {
			get;
		}

		/// <summary>
		/// Get the offset within the stream where the violation was found.
		/// </summary>
		/// <remarks>
		/// Gets the offset within the stream where the violation was found. The offset is always
		/// relative to the start of the outermost TNEF stream, even when the violation was found within
		/// an embedded message.
		/// </remarks>
		/// <value>The stream offset.</value>
		public long StreamOffset {
			get;
		}

		/// <summary>
		/// Get the nesting depth of the embedded message that the violation was found in.
		/// </summary>
		/// <remarks>
		/// Gets the nesting depth of the embedded message that the violation was found in, or <c>0</c>
		/// if the violation was found in the top-level message.
		/// </remarks>
		/// <value>The nesting depth.</value>
		public int Depth {
			get;
		}

		/// <summary>
		/// Get the attribute that was being read when the violation was found.
		/// </summary>
		/// <remarks>
		/// Gets the attribute that was being read when the violation was found, or
		/// <see cref="TnefAttributeTag.Null"/> if the violation was not found within an attribute.
		/// </remarks>
		/// <value>The attribute tag.</value>
		public TnefAttributeTag AttributeTag {
			get;
		}

		/// <summary>
		/// Get the property that was being read when the violation was found.
		/// </summary>
		/// <remarks>
		/// Gets the property that was being read when the violation was found, or
		/// <see cref="TnefPropertyTag.Null"/> if the violation was not found within a property.
		/// </remarks>
		/// <value>The property tag.</value>
		public TnefPropertyTag PropertyTag {
			get;
		}

		/// <summary>
		/// Get the severity of the TNEF compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets how much practical harm the violation is likely to cause.
		/// </remarks>
		/// <value>The severity.</value>
		public MimeComplianceSeverity Severity {
			get { return GetSeverity (Violation); }
		}

		/// <summary>
		/// Get the categories of harm that the TNEF compliance violation may cause.
		/// </summary>
		/// <remarks>
		/// <para>Gets what kind of harm the violation may cause, as opposed to <see cref="Severity"/>,
		/// which rates how much.</para>
		/// <para>A violation may fall into more than one category, so this is a bit field.</para>
		/// </remarks>
		/// <value>The categories.</value>
		public MimeComplianceCategories Categories {
			get { return GetCategories (Violation); }
		}

		/// <summary>
		/// Get a brief description of the TNEF compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets a brief, single-sentence description of what went wrong. Use <see cref="Remarks"/>
		/// for a longer explanation of why it matters.
		/// </remarks>
		/// <value>The description.</value>
		public string Description {
			get { return GetDescription (Violation); }
		}

		/// <summary>
		/// Get a detailed explanation of the TNEF compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets a detailed explanation of what the relevant specifications require and the problems
		/// that the violation is likely to cause.
		/// </remarks>
		/// <value>The remarks.</value>
		public string Remarks {
			get { return GetRemarks (Violation); }
		}

		/// <summary>
		/// Get a string representation of the TNEF compliance issue.
		/// </summary>
		/// <remarks>
		/// Gets a string representation of the TNEF compliance issue.
		/// </remarks>
		/// <returns>A string representation of the TNEF compliance issue.</returns>
		public override string ToString ()
		{
			var builder = new StringBuilder ();

			builder.Append (Violation.ToString ());
			builder.AppendFormat (CultureInfo.InvariantCulture, " at offset {0}", StreamOffset);

			if (AttributeTag != TnefAttributeTag.Null)
				builder.AppendFormat (CultureInfo.InvariantCulture, " in attribute {0}", AttributeTag);

			if (PropertyTag != TnefPropertyTag.Null)
				builder.AppendFormat (CultureInfo.InvariantCulture, " in property {0}", PropertyTag);

			if (Depth > 0)
				builder.AppendFormat (CultureInfo.InvariantCulture, " (depth {0})", Depth);

			return builder.ToString ();
		}

		/// <summary>
		/// Determine whether the specified <see cref="TnefComplianceIssue"/> is equal to the current <see cref="TnefComplianceIssue"/>.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified <see cref="TnefComplianceIssue"/> is equal to the current <see cref="TnefComplianceIssue"/>.
		/// </remarks>
		/// <param name="other">The <see cref="TnefComplianceIssue"/> to compare with the current <see cref="TnefComplianceIssue"/>.</param>
		/// <returns><see langword="true" /> if the specified <see cref="TnefComplianceIssue"/> is equal to the current
		/// <see cref="TnefComplianceIssue"/>; otherwise, <see langword="false" />.</returns>
		public bool Equals (TnefComplianceIssue other)
		{
			return other.Violation == Violation && other.StreamOffset == StreamOffset && other.Depth == Depth &&
				other.AttributeTag == AttributeTag && other.PropertyTag == PropertyTag;
		}

		/// <summary>
		/// Determine whether the specified <see cref="System.Object"/> is equal to the current <see cref="TnefComplianceIssue"/>.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="TnefComplianceIssue"/>.
		/// </remarks>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="TnefComplianceIssue"/>.</param>
		/// <returns><see langword="true" /> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="TnefComplianceIssue"/>; otherwise, <see langword="false" />.</returns>
		public override bool Equals (object? obj)
		{
			return obj is TnefComplianceIssue other && Equals (other);
		}

		/// <summary>
		/// Serve as a hash function for a <see cref="TnefComplianceIssue"/> object.
		/// </summary>
		/// <remarks>
		/// Serves as a hash function for a <see cref="TnefComplianceIssue"/> object.
		/// </remarks>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms
		/// and data structures such as a hash table.</returns>
		public override int GetHashCode ()
		{
			return Violation.GetHashCode () ^ StreamOffset.GetHashCode () ^ Depth ^ AttributeTag.GetHashCode () ^ PropertyTag.GetHashCode ();
		}

		/// <summary>
		/// Compare two <see cref="TnefComplianceIssue"/> objects for equality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="TnefComplianceIssue"/> objects for equality.
		/// </remarks>
		/// <param name="left">The first issue to compare.</param>
		/// <param name="right">The second issue to compare.</param>
		/// <returns><see langword="true" /> if the two issues are equal; otherwise, <see langword="false" />.</returns>
		public static bool operator == (TnefComplianceIssue left, TnefComplianceIssue right)
		{
			return left.Equals (right);
		}

		/// <summary>
		/// Compare two <see cref="TnefComplianceIssue"/> objects for inequality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="TnefComplianceIssue"/> objects for inequality.
		/// </remarks>
		/// <param name="left">The first issue to compare.</param>
		/// <param name="right">The second issue to compare.</param>
		/// <returns><see langword="true" /> if the two issues are not equal; otherwise, <see langword="false" />.</returns>
		public static bool operator != (TnefComplianceIssue left, TnefComplianceIssue right)
		{
			return !left.Equals (right);
		}

		/// <summary>
		/// Get the severity of a TNEF compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets how much practical harm the violation is likely to cause.
		/// </remarks>
		/// <returns>The severity.</returns>
		/// <param name="violation">The TNEF compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="TnefComplianceViolation"/>.
		/// </exception>
		public static MimeComplianceSeverity GetSeverity (TnefComplianceViolation violation)
		{
			switch (violation) {
			// Note: Implementations are known to disagree about whether a late message attribute
			// overrides the earlier value or is discarded, so a message attribute that follows an
			// attachment can smuggle a value (e.g. a body or message class) past a content scanner
			// that resolves the conflict differently than the recipient's mail client.
			case TnefComplianceViolation.MessageAttributeAfterAttachment:
				return MimeComplianceSeverity.Critical;

			// Note: Both of these are tolerated in practice. A version mismatch does not change how any
			// attribute is framed, and a checksum mismatch does not change which bytes are read, only
			// whether they are the bytes that the writer intended.
			case TnefComplianceViolation.UnsupportedVersion:
			case TnefComplianceViolation.AttributeChecksumMismatch:
			// Note: These lose a single attribute or value, and nothing else is affected.
			case TnefComplianceViolation.UnknownAttribute:
			case TnefComplianceViolation.InvalidDate:
				return MimeComplianceSeverity.Minor;

			// Note: Attributes that may be applied to a different object by different readers. A
			// content scanner and a mail client can be made to disagree about which attachment some
			// content belongs to, or whether it belongs to the message itself.
			case TnefComplianceViolation.InvalidAttributeLevel:
			case TnefComplianceViolation.AttributeLevelMismatch:
			// Note: The message class determines how a client renders the message, so a reader that
			// cannot interpret it may render something other than what a scanner examined.
			case TnefComplianceViolation.InvalidMessageClass:
				return MimeComplianceSeverity.Major;

			// Note: Corruption that loses more than a single value. In each case, the remainder of the
			// attribute (or, for the framing violations, the remainder of the stream) cannot be parsed.
			case TnefComplianceViolation.InvalidSignature:
			case TnefComplianceViolation.InvalidMessageCodepage:
			case TnefComplianceViolation.InvalidAttributeLength:
			case TnefComplianceViolation.InvalidAttributeValue:
			case TnefComplianceViolation.TruncatedStream:
			case TnefComplianceViolation.InvalidPropertyCount:
			case TnefComplianceViolation.InvalidRowCount:
			case TnefComplianceViolation.InvalidValueCount:
			case TnefComplianceViolation.InvalidPropertyLength:
			case TnefComplianceViolation.UnsupportedPropertyType:
			case TnefComplianceViolation.InvalidNamedPropertyKind:
				return MimeComplianceSeverity.Major;

			// Note: These are not defects in the stream, but some of its content was not processed,
			// and a consumer that only looks at Major and above needs to know that.
			case TnefComplianceViolation.NestingTooDeep:
			case TnefComplianceViolation.TooManyAttachments:
			case TnefComplianceViolation.DataSizeLimitExceeded:
			case TnefComplianceViolation.TooManyComplianceIssues:
				return MimeComplianceSeverity.Major;

			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}

		/// <summary>
		/// Get the categories of harm that a TNEF compliance violation may cause.
		/// </summary>
		/// <remarks>
		/// <para>Gets what kind of harm the violation may cause, as opposed to
		/// <see cref="GetSeverity(TnefComplianceViolation)"/>, which rates how much.</para>
		/// <para>A violation may fall into more than one category, so this is a bit field.</para>
		/// </remarks>
		/// <returns>The categories.</returns>
		/// <param name="violation">The TNEF compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="TnefComplianceViolation"/>.
		/// </exception>
		public static MimeComplianceCategories GetCategories (TnefComplianceViolation violation)
		{
			const MimeComplianceCategories Interop = MimeComplianceCategories.Interoperability;
			const MimeComplianceCategories DataLoss = MimeComplianceCategories.DataLoss;
			const MimeComplianceCategories Security = MimeComplianceCategories.Security;

			switch (violation) {
			case TnefComplianceViolation.UnsupportedVersion:
			case TnefComplianceViolation.AttributeChecksumMismatch:
				return Interop;

			// Note: The content of the stream, or of a single attribute or value, is not available.
			case TnefComplianceViolation.InvalidSignature:
			case TnefComplianceViolation.InvalidMessageCodepage:
			case TnefComplianceViolation.UnknownAttribute:
			case TnefComplianceViolation.InvalidAttributeLength:
			case TnefComplianceViolation.InvalidAttributeValue:
			case TnefComplianceViolation.InvalidDate:
			case TnefComplianceViolation.InvalidPropertyCount:
			case TnefComplianceViolation.InvalidRowCount:
			case TnefComplianceViolation.InvalidValueCount:
			case TnefComplianceViolation.InvalidPropertyLength:
			case TnefComplianceViolation.UnsupportedPropertyType:
			case TnefComplianceViolation.InvalidNamedPropertyKind:
				return Interop | DataLoss;

			// Note: Different readers may attach the content to different objects or render it
			// differently, so what a scanner examines may not be what the recipient is shown.
			case TnefComplianceViolation.InvalidAttributeLevel:
			case TnefComplianceViolation.MessageAttributeAfterAttachment:
			case TnefComplianceViolation.AttributeLevelMismatch:
			case TnefComplianceViolation.InvalidMessageClass:
				return Interop | Security;

			// Note: A truncated stream is unlikely to be interpreted differently by different readers,
			// but whatever followed the point of truncation has been lost.
			case TnefComplianceViolation.TruncatedStream:
				return DataLoss;

			// Note: The stream did not cause these, so they do not harm interoperability, but content
			// that went unprocessed is content that went unscanned.
			case TnefComplianceViolation.NestingTooDeep:
			case TnefComplianceViolation.TooManyAttachments:
			case TnefComplianceViolation.DataSizeLimitExceeded:
				return DataLoss | Security;

			// Note: A suppressed report is a blind spot, and hiding a violation from whatever is
			// inspecting the stream is exactly what an attacker would want out of a flood of cheap ones.
			case TnefComplianceViolation.TooManyComplianceIssues:
				return Security;

			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}

		/// <summary>
		/// Get a brief description of a TNEF compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets a brief, single-sentence description of what went wrong. Use
		/// <see cref="GetRemarks(TnefComplianceViolation)"/> for a longer explanation of why it matters.
		/// </remarks>
		/// <returns>The description.</returns>
		/// <param name="violation">The TNEF compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="TnefComplianceViolation"/>.
		/// </exception>
		public static string GetDescription (TnefComplianceViolation violation)
		{
			switch (violation) {
			case TnefComplianceViolation.InvalidSignature:
				return "The stream did not begin with the TNEF signature.";
			case TnefComplianceViolation.UnsupportedVersion:
				return "The attTnefVersion attribute specified an unsupported version.";
			case TnefComplianceViolation.InvalidMessageCodepage:
				return "The attOemCodepage attribute specified a codepage that could not be resolved.";
			case TnefComplianceViolation.InvalidMessageClass:
				return "The attMessageClass or attOriginalMessageClass attribute was not a valid message class.";
			case TnefComplianceViolation.InvalidAttributeLevel:
				return "An attribute had a level that was neither the message level nor the attachment level.";
			case TnefComplianceViolation.MessageAttributeAfterAttachment:
				return "A message-level attribute appeared after the first attachment-level attribute.";
			case TnefComplianceViolation.AttributeLevelMismatch:
				return "An attribute appeared at a level that it is not defined for.";
			case TnefComplianceViolation.UnknownAttribute:
				return "An attribute had an unrecognized attribute identifier.";
			case TnefComplianceViolation.InvalidAttributeLength:
				return "An attribute had a length that cannot be valid.";
			case TnefComplianceViolation.AttributeChecksumMismatch:
				return "The checksum of an attribute did not match its value.";
			case TnefComplianceViolation.InvalidAttributeValue:
				return "An attribute value was malformed for the type of the attribute.";
			case TnefComplianceViolation.InvalidDate:
				return "A date value was invalid.";
			case TnefComplianceViolation.TruncatedStream:
				return "The stream ended before the end of the current attribute.";
			case TnefComplianceViolation.InvalidPropertyCount:
				return "A property list had a property count that cannot be valid.";
			case TnefComplianceViolation.InvalidRowCount:
				return "A property table had a row count that cannot be valid.";
			case TnefComplianceViolation.InvalidValueCount:
				return "A property had a value count that cannot be valid.";
			case TnefComplianceViolation.InvalidPropertyLength:
				return "A property value had a length that cannot be valid.";
			case TnefComplianceViolation.UnsupportedPropertyType:
				return "A property had an unsupported property type.";
			case TnefComplianceViolation.InvalidNamedPropertyKind:
				return "A named property had an invalid kind.";
			case TnefComplianceViolation.NestingTooDeep:
				return "Embedded messages were nested more deeply than the configured limit.";
			case TnefComplianceViolation.TooManyAttachments:
				return "The stream contained more attachments than the configured limit.";
			case TnefComplianceViolation.DataSizeLimitExceeded:
				return "The stream contained more data than the configured limit.";
			case TnefComplianceViolation.TooManyComplianceIssues:
				return "Too many compliance issues were detected and the remainder were suppressed.";
			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}

		/// <summary>
		/// Get a detailed explanation of a TNEF compliance violation.
		/// </summary>
		/// <remarks>
		/// Gets a detailed explanation of what the relevant specifications require and the problems
		/// that the violation is likely to cause.
		/// </remarks>
		/// <returns>The remarks.</returns>
		/// <param name="violation">The TNEF compliance violation.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="violation"/> is not a valid <see cref="TnefComplianceViolation"/>.
		/// </exception>
		public static string GetRemarks (TnefComplianceViolation violation)
		{
			switch (violation) {
			case TnefComplianceViolation.InvalidSignature:
				return "The TNEF specification requires that a TNEF stream begin with the 32-bit little-endian signature 0x223E9F78. A stream that does not is not TNEF at all, so nothing in it can be parsed.";
			case TnefComplianceViolation.UnsupportedVersion:
				return "The TNEF specification defines a single TNEF version, 0x00010000. Parsing continues, but a stream that claims a different version may use encodings that are not understood.";
			case TnefComplianceViolation.InvalidMessageCodepage:
				return "The codepage is used to decode every 8-bit string in the stream. When it cannot be resolved, the default codepage is used instead, which may decode those strings incorrectly.";
			case TnefComplianceViolation.InvalidMessageClass:
				return "A message class is a string of at most 255 printable ASCII characters. The message class determines how a client interprets and renders the rest of the message, so a client that cannot interpret it may render something other than what a content scanner examined.";
			case TnefComplianceViolation.InvalidAttributeLevel:
				return "The TNEF specification defines only two attribute levels: 0x01 for message attributes and 0x02 for attachment attributes. There is no way to know which object an attribute with any other level belongs to, so different implementations may apply it to different objects.";
			case TnefComplianceViolation.MessageAttributeAfterAttachment:
				return "The TNEF specification requires that all message attributes precede all attachment attributes. Implementations disagree about whether a late message attribute applies to the message or is discarded, so the same stream can be presented differently by different implementations.";
			case TnefComplianceViolation.AttributeLevelMismatch:
				return "The TNEF specification defines each attribute as belonging to either the message or an attachment. An attachment attribute at the message level, or a message attribute at the attachment level, may be applied to the wrong object or discarded depending on the implementation.";
			case TnefComplianceViolation.UnknownAttribute:
				return "The attribute identifier is not one that the TNEF specification defines. The attribute is skipped, so whatever information it carried is not available.";
			case TnefComplianceViolation.InvalidAttributeLength:
				return "The length of an attribute is the only way to find where the next attribute begins. A length that is too large to represent cannot be followed, so the remainder of the stream cannot be parsed.";
			case TnefComplianceViolation.AttributeChecksumMismatch:
				return "The TNEF specification defines the checksum of an attribute as the sum of the bytes in its value modulo 65536. A mismatch indicates that the value was altered or corrupted after it was written. The value is still used.";
			case TnefComplianceViolation.InvalidAttributeValue:
				return "The attribute value was too short, or otherwise did not have the layout that the TNEF specification defines for the attribute, so it could not be interpreted.";
			case TnefComplianceViolation.InvalidDate:
				return "A date attribute or a PT_SYSTIME property described a date and time that does not exist or cannot be represented, so the value is not available.";
			case TnefComplianceViolation.TruncatedStream:
				return "The stream was most likely truncated in transit or in storage. Whatever followed the point of truncation is lost.";
			case TnefComplianceViolation.InvalidPropertyCount:
				return "The property count was negative, or larger than the number of properties that could fit within the attribute that contains them. Only the properties that fit are read.";
			case TnefComplianceViolation.InvalidRowCount:
				return "The row count of the attRecipTable attribute was negative, or larger than the number of rows that could fit within the attribute. Only the rows that fit are read.";
			case TnefComplianceViolation.InvalidValueCount:
				return "The value count of a multi-valued or variable-length property was negative, larger than the number of values that could fit within the attribute that contains it, or, for a property that is not multi-valued, other than exactly one.";
			case TnefComplianceViolation.InvalidPropertyLength:
				return "The length of a variable-length property value was negative, or extended beyond the end of the attribute that contains it, so the value cannot be read in full.";
			case TnefComplianceViolation.UnsupportedPropertyType:
				return "The size of a property value is determined by its type, so a value of an unknown type cannot be skipped. The remaining properties in the same attribute cannot be read.";
			case TnefComplianceViolation.InvalidNamedPropertyKind:
				return "The TNEF specification defines the kind of a named property as either 0x00000000 (a numeric identifier) or 0x00000001 (a string name). With any other kind, the size of the name cannot be determined, so the remaining properties in the same attribute cannot be read.";
			case TnefComplianceViolation.NestingTooDeep:
				return "This is not a defect in the stream, but the embedded messages nested beyond the configured limit were not processed. A content scanner that relies on the result has not examined them.";
			case TnefComplianceViolation.TooManyAttachments:
				return "This is not a defect in the stream, but the attachments beyond the configured limit were not processed. A content scanner that relies on the result has not examined them.";
			case TnefComplianceViolation.DataSizeLimitExceeded:
				return "This is not a defect in the stream, but the data beyond the configured limit was not processed. A content scanner that relies on the result has not examined it.";
			case TnefComplianceViolation.TooManyComplianceIssues:
				return "A limit was placed on how many times each violation may be reported and some violation reached it, so the remaining occurrences of that violation were not reported. Other violations continue to be reported until they reach the limit themselves, but the report as a whole is no longer a complete account of what is wrong with the stream.";
			default:
				throw new ArgumentOutOfRangeException (nameof (violation));
			}
		}
	}
}
