//
// OpenPgpDetectionFilter.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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

using MimeKit.Utils;
using MimeKit.IO.Filters;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A filter meant to aid in the detection of OpenPGP blocks.
	/// </summary>
	/// <remarks>
	/// Detects OpenPGP block markers and their byte offsets.
	/// </remarks>
	public class OpenPgpDetectionFilter : MimeFilterBase
	{
		enum OpenPgpState {
			None                    = 0,
			BeginPgpMessage         = (1 << 0),
			EndPgpMessage           = (1 << 1) | (1 << 0),
			BeginPgpSignedMessage   = (1 << 2),
			BeginPgpSignature       = (1 << 3) | (1 << 2),
			EndPgpSignature         = (1 << 4) | (1 << 3) | (1 << 2),
			BeginPgpPublicKeyBlock  = (1 << 5),
			EndPgpPublicKeyBlock    = (1 << 6) | (1 << 5),
			BeginPgpPrivateKeyBlock = (1 << 7),
			EndPgpPrivateKeyBlock   = (1 << 8) | (1 << 7)
		}

		readonly struct OpenPgpMarker
		{
			public readonly byte[] Marker;
			public readonly OpenPgpState InitialState;
			public readonly OpenPgpState DetectedState;
			public readonly bool IsEnd;

			public OpenPgpMarker (string marker, OpenPgpState initial, OpenPgpState detected, bool isEnd)
			{
				Marker = CharsetUtils.UTF8.GetBytes (marker);
				InitialState = initial;
				DetectedState = detected;
				IsEnd = isEnd;
			}
		}

		static readonly OpenPgpMarker[] OpenPgpMarkers = {
			new OpenPgpMarker ("-----BEGIN PGP MESSAGE-----", OpenPgpState.None, OpenPgpState.BeginPgpMessage, false),
			new OpenPgpMarker ("-----END PGP MESSAGE-----", OpenPgpState.BeginPgpMessage, OpenPgpState.EndPgpMessage, true),
			new OpenPgpMarker ("-----BEGIN PGP SIGNED MESSAGE-----", OpenPgpState.None, OpenPgpState.BeginPgpSignedMessage, false),
			new OpenPgpMarker ("-----BEGIN PGP SIGNATURE-----", OpenPgpState.BeginPgpSignedMessage, OpenPgpState.BeginPgpSignature, false),
			new OpenPgpMarker ("-----END PGP SIGNATURE-----", OpenPgpState.BeginPgpSignature, OpenPgpState.EndPgpSignature, true),
			new OpenPgpMarker ("-----BEGIN PGP PUBLIC KEY BLOCK-----", OpenPgpState.None, OpenPgpState.BeginPgpPublicKeyBlock, false),
			new OpenPgpMarker ("-----END PGP PUBLIC KEY BLOCK-----", OpenPgpState.BeginPgpPublicKeyBlock, OpenPgpState.EndPgpPublicKeyBlock, true),
			new OpenPgpMarker ("-----BEGIN PGP PRIVATE KEY BLOCK-----", OpenPgpState.None, OpenPgpState.BeginPgpPrivateKeyBlock, false),
			new OpenPgpMarker ("-----END PGP PRIVATE KEY BLOCK-----", OpenPgpState.BeginPgpPrivateKeyBlock, OpenPgpState.EndPgpPrivateKeyBlock, false)
		};

		OpenPgpState state;
		int position, next;
		bool seenEndMarker;
		bool midline;

		/// <summary>
		/// Initialize a new instance of the <see cref="OpenPgpDetectionFilter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="OpenPgpDetectionFilter"/>.
		/// </remarks>
		public OpenPgpDetectionFilter ()
		{
		}

		/// <summary>
		/// Get the byte offset of the BEGIN marker, if available.
		/// </summary>
		/// <remarks>
		/// Gets the byte offset of the BEGIN marker if available.
		/// </remarks>
		/// <value>The byte offset.</value>
		public int? BeginOffset {
			get; private set;
		}

		/// <summary>
		/// Get the byte offset of the END marker, if available.
		/// </summary>
		/// <remarks>
		/// Gets the byte offset of the END marker if available.
		/// </remarks>
		/// <value>The byte offset.</value>
		public int? EndOffset {
			get; private set;
		}

		/// <summary>
		/// Get the type of OpenPGP data detected.
		/// </summary>
		/// <remarks>
		/// Gets the type of OpenPGP data detected.
		/// </remarks>
		/// <value>The type of OpenPGP data detected.</value>
		public OpenPgpDataType DataType {
			get {
				switch (state) {
				case OpenPgpState.EndPgpPrivateKeyBlock: return OpenPgpDataType.PrivateKey;
				case OpenPgpState.EndPgpPublicKeyBlock: return OpenPgpDataType.PublicKey;
				case OpenPgpState.EndPgpSignature: return OpenPgpDataType.SignedMessage;
				case OpenPgpState.EndPgpMessage: return OpenPgpDataType.EncryptedMessage;
				default: return OpenPgpDataType.None;
				}
			}
		}

		static bool IsMarker (byte[] input, int startIndex, int endIndex, byte[] marker, out bool cr)
		{
			int i = startIndex;
			int j = 0;

			cr = false;

			while (j < marker.Length && i < endIndex) {
				if (input[i++] != marker[j++])
					return false;
			}

			if (j < marker.Length)
				return false;

			if (i < endIndex && input[i] == (byte) '\r') {
				cr = true;
				i++;
			}

			return i < endIndex && input[i] == (byte) '\n';
		}

		static bool IsPartialMatch (byte[] input, int startIndex, int endIndex, byte[] marker)
		{
			int i = startIndex;
			int j = 0;

			while (j < marker.Length && i < endIndex) {
				if (input[i++] != marker[j++])
					return false;
			}

			if (i < endIndex && input[i] == (byte) '\r')
				i++;

			return i == endIndex;
		}

		void SetPosition (int offset, int marker, bool cr)
		{
			int length = OpenPgpMarkers[marker].Marker.Length + (cr ? 2 : 1);

			switch (state) {
			case OpenPgpState.BeginPgpPrivateKeyBlock: BeginOffset = position + offset; break;
			case OpenPgpState.EndPgpPrivateKeyBlock: EndOffset = position + offset + length; break;
			case OpenPgpState.BeginPgpPublicKeyBlock: BeginOffset = position + offset; break;
			case OpenPgpState.EndPgpPublicKeyBlock: EndOffset = position + offset + length; break;
			case OpenPgpState.BeginPgpSignedMessage: BeginOffset = position + offset; break;
			case OpenPgpState.EndPgpSignature: EndOffset = position + offset + length; break;
			case OpenPgpState.BeginPgpMessage: BeginOffset = position + offset; break;
			case OpenPgpState.EndPgpMessage: EndOffset = position + offset + length; break;
			}
		}

		/// <summary>
		/// Filter the specified input.
		/// </summary>
		/// <remarks>
		/// Filters the specified input buffer starting at the given index,
		/// spanning across the specified number of bytes.
		/// </remarks>
		/// <returns>The filtered output.</returns>
		/// <param name="input">The input buffer.</param>
		/// <param name="startIndex">The starting index of the input buffer.</param>
		/// <param name="length">The length of the input buffer, starting at <paramref name="startIndex"/>.</param>
		/// <param name="outputIndex">The output index.</param>
		/// <param name="outputLength">The output length.</param>
		/// <param name="flush">If set to <c>true</c>, all internally buffered data should be flushed to the output buffer.</param>
		protected override byte[] Filter (byte[] input, int startIndex, int length, out int outputIndex, out int outputLength, bool flush)
		{
			int endIndex = startIndex + length;
			int index = startIndex;
			bool cr;

			outputIndex = startIndex;
			outputLength = 0;

			if (seenEndMarker || length == 0)
				return input;

			if (midline) {
				while (index < endIndex && input[index] != (byte) '\n')
					index++;

				if (index == endIndex) {
					if (state != OpenPgpState.None)
						outputLength = index - startIndex;

					position += index - startIndex;

					return input;
				}

				midline = false;
			}

			if (state == OpenPgpState.None) {
				do {
					int lineIndex = index;

					while (index < endIndex && input[index] != (byte) '\n')
						index++;

					if (index == endIndex) {
						bool isPartialMatch = false;

						for (int i = 0; i < OpenPgpMarkers.Length; i++) {
							ref OpenPgpMarker marker = ref OpenPgpMarkers[i];
							if (marker.InitialState == state && IsPartialMatch (input, lineIndex, index, marker.Marker)) {
								isPartialMatch = true;
								break;
							}
						}

						if (isPartialMatch) {
							SaveRemainingInput (input, lineIndex, index - lineIndex);
							position += lineIndex - startIndex;
						} else {
							position += index - lineIndex;
							midline = true;
						}

						return input;
					}

					index++;

					for (int i = 0; i < OpenPgpMarkers.Length; i++) {
						ref OpenPgpMarker marker = ref OpenPgpMarkers[i];
						if (marker.InitialState == state && IsMarker (input, lineIndex, endIndex, marker.Marker, out cr)) {
							state = marker.DetectedState;
							SetPosition (lineIndex - startIndex, i, cr);
							outputLength = index - lineIndex;
							outputIndex = lineIndex;
							next = i + 1;
							break;
						}
					}
				} while (index < endIndex && state == OpenPgpState.None);

				if (index == endIndex) {
					position += index - startIndex;
					return input;
				}
			}

			do {
				int lineIndex = index;

				while (index < endIndex && input[index] != (byte) '\n')
					index++;

				if (index == endIndex) {
					if (!flush) {
						if (IsPartialMatch (input, lineIndex, index, OpenPgpMarkers[next].Marker)) {
							SaveRemainingInput (input, lineIndex, index - lineIndex);
							outputLength = lineIndex - outputIndex;
							position += lineIndex - startIndex;
						} else {
							outputLength = index - outputIndex;
							position += index - startIndex;
							midline = true;
						}

						return input;
					}

					break;
				}

				index++;

				if (IsMarker (input, lineIndex, endIndex, OpenPgpMarkers[next].Marker, out cr)) {
					seenEndMarker = OpenPgpMarkers[next].IsEnd;
					state = OpenPgpMarkers[next].DetectedState;
					SetPosition (lineIndex - startIndex, next, cr);
					next++;

					if (seenEndMarker)
						break;
				}
			} while (index < endIndex);

			outputLength = index - outputIndex;
			position += index - startIndex;

			return input;
		}

		/// <summary>
		/// Resets the filter.
		/// </summary>
		/// <remarks>
		/// Resets the filter.
		/// </remarks>
		public override void Reset ()
		{
			state = OpenPgpState.None;
			seenEndMarker = false;
			BeginOffset = null;
			EndOffset = null;
			midline = false;
			position = 0;
			next = 0;

			base.Reset ();
		}
	}
}
