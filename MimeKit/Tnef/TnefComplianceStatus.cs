//
// TnefComplianceStatus.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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
