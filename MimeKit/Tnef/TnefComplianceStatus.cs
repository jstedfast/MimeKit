// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace MimeKit.Tnef {
	/// <summary>
	/// A bitfield of potential TNEF compliance issues.
	/// </summary>
	/// <remarks>
	/// A bitfield of potential TNEF compliance issues.
	/// </remarks>
	[Flags]
	public enum TnefComplianceStatus {
		/// <summary>
		/// The TNEF stream has no errors.
		/// </summary>
		Compliant                = 0,

		/// <summary>
		/// The TNEF stream has too many attributes.
		/// </summary>
		AttributeOverflow        = 1 << 0,

		/// <summary>
		/// The TNEF stream has one or more invalid attributes.
		/// </summary>
		InvalidAttribute         = 1 << 1,

		/// <summary>
		/// The TNEF stream has one or more attributes with invalid checksums.
		/// </summary>
		InvalidAttributeChecksum = 1 << 2,

		/// <summary>
		/// The TNEF stream has one more more attributes with an invalid length.
		/// </summary>
		InvalidAttributeLength   = 1 << 3,

		/// <summary>
		/// The TNEF stream has one or more attributes with an invalid level.
		/// </summary>
		InvalidAttributeLevel    = 1 << 4,

		/// <summary>
		/// The TNEF stream has one or more attributes with an invalid value.
		/// </summary>
		InvalidAttributeValue    = 1 << 5,

		/// <summary>
		/// The TNEF stream has one or more attributes with an invalid date value.
		/// </summary>
		InvalidDate              = 1 << 6,

		/// <summary>
		/// The TNEF stream has one or more invalid MessageClass attributes.
		/// </summary>
		InvalidMessageClass      = 1 << 7,

		/// <summary>
		/// The TNEF stream has one or more invalid MessageCodepage attributes.
		/// </summary>
		InvalidMessageCodepage   = 1 << 8,

		/// <summary>
		/// The TNEF stream has one or more invalid property lengths.
		/// </summary>
		InvalidPropertyLength    = 1 << 9,

		/// <summary>
		/// The TNEF stream has one or more invalid row counts.
		/// </summary>
		InvalidRowCount          = 1 << 10,

		/// <summary>
		/// The TNEF stream has an invalid signature value.
		/// </summary>
		InvalidTnefSignature     = 1 << 11,

		/// <summary>
		/// The TNEF stream has an invalid version value.
		/// </summary>
		InvalidTnefVersion       = 1 << 12,

		/// <summary>
		/// The TNEF stream is nested too deeply.
		/// </summary>
		NestingTooDeep           = 1 << 13,

		/// <summary>
		/// The TNEF stream is truncated.
		/// </summary>
		StreamTruncated          = 1 << 14,

		/// <summary>
		/// The TNEF stream has one or more unsupported property types.
		/// </summary>
		UnsupportedPropertyType  = 1 << 15
	}
}
