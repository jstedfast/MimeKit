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
		public static readonly Guid PublicStrings = new Guid (0x00020329, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Address property set.
		/// </summary>
		public static readonly Guid Address = new Guid (0x00062004, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Appointment property set.
		/// </summary>
		public static readonly Guid Appointment = new Guid (0x00062002, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Common property set.
		/// </summary>
		public static readonly Guid Common = new Guid (0x00062008, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Log property set.
		/// </summary>
		public static readonly Guid Log = new Guid (0x0006200a, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Meeting property set.
		/// </summary>
		public static readonly Guid Meeting = new Guid (0x6ed8da90, 0x450b, 0x101b, 0x98, 0xda, 0x00, 0xaa, 0x00, 0x3f, 0x13, 0x05);

		/// <summary>
		/// The MAPI PSETID_Note property set.
		/// </summary>
		public static readonly Guid Note = new Guid (0x0006200e, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_PostRss property set.
		/// </summary>
		public static readonly Guid PostRss = new Guid (0x00062041, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Remote property set.
		/// </summary>
		public static readonly Guid Remote = new Guid (0x00062014, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Sharing property set.
		/// </summary>
		public static readonly Guid Sharing = new Guid (0x00062040, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Task property set.
		/// </summary>
		public static readonly Guid Task = new Guid (0x00062003, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSETID_Tracking property set.
		/// </summary>
		public static readonly Guid Tracking = new Guid (0x0006200b, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

		/// <summary>
		/// The MAPI PSGUID_STORAGE property set.
		/// </summary>
		public static readonly Guid Storage = new Guid (0xb725f130, 0x47ef, 0x101a, 0xa5, 0xf1, 0x02, 0x60, 0x8c, 0x9e, 0xeb, 0xac);
	}
}
