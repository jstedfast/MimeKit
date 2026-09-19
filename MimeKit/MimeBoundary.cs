//
// MimeBoundary.cs
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
using System.Diagnostics;

namespace MimeKit {
	enum MimeBoundaryType
	{
		None,
		Eos,
		ImmediateBoundary,
		ImmediateEndBoundary,
		ParentBoundary,
		ParentEndBoundary,
	}

	[DebuggerDisplay ("{System.Text.Encoding.ASCII.GetString (Marker)}")]
	class MimeBoundary
	{
		public static readonly byte[] MboxFrom = "From "u8.ToArray ();

		public MimeBoundary? Next { get; set; }

		public byte[] Marker { get; private set; }
		public int FinalLength { get { return Marker.Length; } }
		public int Length { get; private set; }
		public int MaxLength { get; private set; }
		public bool IsMboxMarker { get { return Marker == MboxFrom; } }

		public MimeBoundary (string boundary, MimeBoundary? parent)
		{
			Marker = Encoding.UTF8.GetBytes ("--" + boundary + "--");
			Length = Marker.Length - 2;
			Next = parent;

			if (parent != null) {
				MaxLength = Math.Max (parent.MaxLength, Marker.Length);
			} else {
				MaxLength = Marker.Length;
			}
		}

		MimeBoundary (byte[] marker, int maxLength, int length)
		{
			Marker = marker;
			MaxLength = maxLength;
			Length = length;
		}

		public static MimeBoundary CreateMboxBoundary ()
		{
			return new MimeBoundary (MboxFrom, 5, 5);
		}

#if DEBUG_PARSER
		public override string ToString ()
		{
			return Encoding.UTF8.GetString (Marker, 0, Marker.Length);
		}
#endif
	}
}
