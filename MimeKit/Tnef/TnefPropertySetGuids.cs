//
// TnefPropertySetGuids.cs
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
	/// A static class containing known TNEF/MAPI property set GUIDs.
	/// </summary>
	/// <remarks>
	/// This class contains the GUIDs for the various TNEF/MAPI named property sets.
	/// </remarks>
	public static class TnefPropertySetGuids
	{
		/// <summary>
		/// The MAPI PS_PUBLIC_STRINGS property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PS_PUBLIC_STRINGS property set.
		/// </remarks>
		public static readonly Guid PublicStrings = new Guid (0x00020329, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Address property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_Address property set.
		/// </remarks>
		public static readonly Guid Address = new Guid (0x00062004, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_AirSync property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_AirSync property set.
		/// </remarks>
		public static readonly Guid AirSync = new Guid (0x71035549, 0x0739, 0x4dcb, 0x91, 0x63, 0x00, 0xf0, 0x58, 0x0d, 0xbb, 0xdf);

		/// <summary>
		/// The MAPI PSETID_Appointment property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_Appointment property set.
		/// </remarks>
		public static readonly Guid Appointment = new Guid (0x00062002, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Attachment property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_Attachment property set.
		/// </remarks>
		public static readonly Guid Attachment = new Guid (0x96357f7f, 0x59e1, 0x47d0, 0x99, 0xa7, 0x46, 0x51, 0x5c, 0x18, 0x3b, 0x54);

		/// <summary>
		/// The MAPI PSETID_CalendarAssistant property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_CalendarAssistant property set.
		/// </remarks>
		public static readonly Guid CalendarAssistant = new Guid (0x11000e07, 0xb51b, 0x40d6, 0xaf, 0x21, 0xca, 0xa8, 0x5e, 0xda, 0xb1, 0xd0);

		/// <summary>
		/// The MAPI PSETID_Common property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_Common property set.
		/// </remarks>
		public static readonly Guid Common = new Guid (0x00062008, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PS_INTERNET_HEADERS property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PS_INTERNET_HEADERS property set.
		/// </remarks>
		public static readonly Guid InternetHeaders = new Guid (0x00020386, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Location property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_Location property set.
		/// </remarks>
		public static readonly Guid Location = new Guid (0xa719e259, 0x2a9a, 0x4fb8, 0xba, 0xb3, 0x3a, 0x9f, 0x02, 0x97, 0x0e, 0x4b);

		/// <summary>
		/// The MAPI PSETID_Log property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_Log property set.
		/// </remarks>
		public static readonly Guid Log = new Guid (0x0006200a, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PS_MAPI property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PS_MAPI property set.
		/// </remarks>
		public static readonly Guid MAPI = new Guid (0x00020328, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Meeting property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_Meeting property set.
		/// </remarks>
		public static readonly Guid Meeting = new Guid (0x6ed8da90, 0x450b, 0x101b, 0x98, 0xda, 0x00, 0xaa, 0x00, 0x3f, 0x13, 0x05);

		/// <summary>
		/// The MAPI PSETID_Messaging property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_Messaging property set.
		/// </remarks>
		public static readonly Guid Messaging = new Guid (0x41f28f13, 0x83f4, 0x4114, 0xa5, 0x84, 0xee, 0xdb, 0x5a, 0x6b, 0x0b, 0xff);

		/// <summary>
		/// The MAPI PSETID_Note property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_Note property set.
		/// </remarks>
		public static readonly Guid Note = new Guid (0x0006200e, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_PostRss property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_PostRss property set.
		/// </remarks>
		public static readonly Guid PostRss = new Guid (0x00062041, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Remote property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_Remote property set.
		/// </remarks>
		public static readonly Guid Remote = new Guid (0x00062014, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Report property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_Report property set.
		/// </remarks>
		public static readonly Guid Report = new Guid (0x00062013, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Sharing property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_Sharing property set.
		/// </remarks>
		public static readonly Guid Sharing = new Guid (0x00062040, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Task property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_Task property set.
		/// </remarks>
		public static readonly Guid Task = new Guid (0x00062003, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Tracking property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSETID_Tracking property set.
		/// </remarks>
		public static readonly Guid Tracking = new Guid (0x0006200b, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSGUID_UnifiedMessaging property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSGUID_UnifiedMessaging property set.
		/// </remarks>
		public static readonly Guid UnifiedMessaging = new Guid (0x4442858e, 0xa9e3, 0x4e80, 0xb9, 0x00, 0x31, 0x7a, 0x21, 0x0c, 0xc1, 0x5b);

		/// <summary>
		/// The MAPI PSGUID_XmlExtractedEntities property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSGUID_XmlExtractedEntities property set.
		/// </remarks>
		public static readonly Guid XmlExtractedEntities = new Guid (0x23239608, 0x685d, 0x4732, 0x9c, 0x55, 0x4c, 0x95, 0xcb, 0x4e, 0x8e, 0x33);

		/// <summary>
		/// The MAPI PSGUID_STORAGE property set.
		/// </summary>
		/// <remarks>
		/// The MAPI PSGUID_STORAGE property set.
		/// </remarks>
		public static readonly Guid Storage = new Guid (0xb725f130, 0x47ef, 0x101a, 0xa5, 0xf1, 0x02, 0x60, 0x8c, 0x9e, 0xeb, 0xac);
	}
}
