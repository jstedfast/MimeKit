//
// TnefPropertySetGuid.cs
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

namespace MimeKit.Tnef {
	/// <summary>
	/// The GUIDs of the commonly used named property sets.
	/// </summary>
	/// <remarks>
	/// <para>The GUIDs of the commonly used named property sets as defined by [MS-OXPROPS] section 1.3.2.</para>
	/// <para>A named property is identified by the combination of a property set GUID and either an integer
	/// identifier or a string name (see <see cref="TnefNameId"/>).</para>
	/// </remarks>
	public static class TnefPropertySetGuid
	{
		/// <summary>
		/// The PS_PUBLIC_STRINGS property set.
		/// </summary>
		/// <remarks>
		/// The PS_PUBLIC_STRINGS property set: <c>{00020329-0000-0000-C000-000000000046}</c>.
		/// </remarks>
		public static readonly Guid PublicStrings = new Guid (0x00020329u, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The PSETID_Common property set.
		/// </summary>
		/// <remarks>
		/// The PSETID_Common property set: <c>{00062008-0000-0000-C000-000000000046}</c>.
		/// </remarks>
		public static readonly Guid Common = new Guid (0x00062008u, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The PSETID_Address property set.
		/// </summary>
		/// <remarks>
		/// The PSETID_Address property set: <c>{00062004-0000-0000-C000-000000000046}</c>.
		/// </remarks>
		public static readonly Guid Address = new Guid (0x00062004u, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The PS_INTERNET_HEADERS property set.
		/// </summary>
		/// <remarks>
		/// The PS_INTERNET_HEADERS property set: <c>{00020386-0000-0000-C000-000000000046}</c>.
		/// </remarks>
		public static readonly Guid InternetHeaders = new Guid (0x00020386u, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The PSETID_Appointment property set.
		/// </summary>
		/// <remarks>
		/// The PSETID_Appointment property set: <c>{00062002-0000-0000-C000-000000000046}</c>.
		/// </remarks>
		public static readonly Guid Appointment = new Guid (0x00062002u, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The PSETID_Meeting property set.
		/// </summary>
		/// <remarks>
		/// The PSETID_Meeting property set: <c>{6ED8DA90-450B-101B-98DA-00AA003F1305}</c>.
		/// </remarks>
		public static readonly Guid Meeting = new Guid (0x6ED8DA90u, 0x450B, 0x101B, 0x98, 0xDA, 0x00, 0xAA, 0x00, 0x3F, 0x13, 0x05);

		/// <summary>
		/// The PSETID_Log property set.
		/// </summary>
		/// <remarks>
		/// The PSETID_Log property set: <c>{0006200A-0000-0000-C000-000000000046}</c>.
		/// </remarks>
		public static readonly Guid Log = new Guid (0x0006200Au, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The PSETID_Messaging property set.
		/// </summary>
		/// <remarks>
		/// The PSETID_Messaging property set: <c>{41F28F13-83F4-4114-A584-EEDB5A6B0BFF}</c>.
		/// </remarks>
		public static readonly Guid Messaging = new Guid (0x41F28F13u, 0x83F4, 0x4114, 0xA5, 0x84, 0xEE, 0xDB, 0x5A, 0x6B, 0x0B, 0xFF);

		/// <summary>
		/// The PSETID_Note property set.
		/// </summary>
		/// <remarks>
		/// The PSETID_Note property set: <c>{0006200E-0000-0000-C000-000000000046}</c>.
		/// </remarks>
		public static readonly Guid Note = new Guid (0x0006200Eu, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The PSETID_PostRss property set.
		/// </summary>
		/// <remarks>
		/// The PSETID_PostRss property set: <c>{00062041-0000-0000-C000-000000000046}</c>.
		/// </remarks>
		public static readonly Guid PostRss = new Guid (0x00062041u, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The PSETID_Task property set.
		/// </summary>
		/// <remarks>
		/// The PSETID_Task property set: <c>{00062003-0000-0000-C000-000000000046}</c>.
		/// </remarks>
		public static readonly Guid Task = new Guid (0x00062003u, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The PSETID_UnifiedMessaging property set.
		/// </summary>
		/// <remarks>
		/// The PSETID_UnifiedMessaging property set: <c>{4442858E-A9E3-4E80-B900-317A210CC15B}</c>.
		/// </remarks>
		public static readonly Guid UnifiedMessaging = new Guid (0x4442858Eu, 0xA9E3, 0x4E80, 0xB9, 0x00, 0x31, 0x7A, 0x21, 0x0C, 0xC1, 0x5B);

		/// <summary>
		/// The PS_MAPI property set.
		/// </summary>
		/// <remarks>
		/// The PS_MAPI property set: <c>{00020328-0000-0000-C000-000000000046}</c>.
		/// </remarks>
		public static readonly Guid Mapi = new Guid (0x00020328u, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The PSETID_AirSync property set.
		/// </summary>
		/// <remarks>
		/// The PSETID_AirSync property set: <c>{71035549-0739-4DCB-9163-00F0580DBBDF}</c>.
		/// </remarks>
		public static readonly Guid AirSync = new Guid (0x71035549u, 0x0739, 0x4DCB, 0x91, 0x63, 0x00, 0xF0, 0x58, 0x0D, 0xBB, 0xDF);

		/// <summary>
		/// The PSETID_Sharing property set.
		/// </summary>
		/// <remarks>
		/// The PSETID_Sharing property set: <c>{00062040-0000-0000-C000-000000000046}</c>.
		/// </remarks>
		public static readonly Guid Sharing = new Guid (0x00062040u, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The PSETID_XmlExtractedEntities property set.
		/// </summary>
		/// <remarks>
		/// The PSETID_XmlExtractedEntities property set: <c>{23239608-685D-4732-9C55-4C95CB4E8E33}</c>.
		/// </remarks>
		public static readonly Guid XmlExtractedEntities = new Guid (0x23239608u, 0x685D, 0x4732, 0x9C, 0x55, 0x4C, 0x95, 0xCB, 0x4E, 0x8E, 0x33);

		/// <summary>
		/// The PSETID_Attachment property set.
		/// </summary>
		/// <remarks>
		/// The PSETID_Attachment property set: <c>{96357F7F-59E1-47D0-99A7-46515C183B54}</c>.
		/// </remarks>
		public static readonly Guid Attachment = new Guid (0x96357F7Fu, 0x59E1, 0x47D0, 0x99, 0xA7, 0x46, 0x51, 0x5C, 0x18, 0x3B, 0x54);
	}
}
