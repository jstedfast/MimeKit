// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MimeKit.Tnef {
	/// <summary>
	/// The type of value that a TNEF property contains.
	/// </summary>
	/// <remarks>
	/// The type of value that a TNEF property contains.
	/// </remarks>
	public enum TnefPropertyType : short {
		/// <summary>
		/// The type of the property is unspecified.
		/// </summary>
		Unspecified = 0,

		/// <summary>
		/// The property has a null value.
		/// </summary>
		Null        = 1,

		/// <summary>
		/// The property has a signed 16-bit value.
		/// </summary>
		I2          = 2,

		/// <summary>
		/// The property has a signed 32-bit value.
		/// </summary>
		Long        = 3,

		/// <summary>
		/// The property has a 32-bit floating point value.
		/// </summary>
		R4          = 4,

		/// <summary>
		/// The property has a 64-bit floating point value.
		/// </summary>
		Double      = 5,

		/// <summary>
		/// The property has a 64-bit integer value representing 1/10000th of a monetary unit (i.e., 1/100th of a cent).
		/// </summary>
		Currency    = 6,

		/// <summary>
		/// The property has a 64-bit integer value specifying the number of 100ns periods since Jan 1, 1601.
		/// </summary>
		AppTime     = 7,

		/// <summary>
		/// The property has a 32-bit error value.
		/// </summary>
		Error       = 10,

		/// <summary>
		/// The property has a boolean value.
		/// </summary>
		Boolean     = 11,

		/// <summary>
		/// The property has an embedded object value.
		/// </summary>
		Object      = 13,

		/// <summary>
		/// The property has a signed 64-bit value.
		/// </summary>
		I8          = 20,

		/// <summary>
		/// The property has a null-terminated 8-bit character string value.
		/// </summary>
		String8     = 30,

		/// <summary>
		/// The property has a null-terminated unicode character string value.
		/// </summary>
		Unicode     = 31,

		/// <summary>
		/// The property has a 64-bit integer value specifying the number of 100ns periods since Jan 1, 1601.
		/// </summary>
		SysTime     = 64,

		/// <summary>
		/// The property has an OLE GUID value.
		/// </summary>
		ClassId     = 72,

		/// <summary>
		/// The property has a binary blob value.
		/// </summary>
		Binary      = 258,

		/// <summary>
		/// A flag indicating that the property contains multiple values.
		/// </summary>
		MultiValued = 4096,
	}
}
