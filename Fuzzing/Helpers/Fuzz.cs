//
// Fuzz.cs
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

using System.Text;

namespace MimeKit.Fuzzing.Helpers {
	static class Fuzz
	{
		public static readonly Encoding Latin1 = Encoding.Latin1;
		public static readonly Encoding Utf8 = new UTF8Encoding (false);

		public static void Check (bool condition, string message)
		{
			if (!condition)
				throw new FuzzInvariantException (message);
		}

		public static void Equal (string expected, string actual, string what)
		{
			if (string.Equals (expected, actual, StringComparison.Ordinal))
				return;

			int index = 0;

			while (index < expected.Length && index < actual.Length && expected[index] == actual[index])
				index++;

			throw new FuzzInvariantException ($"{what}: strings differ at index {index} (lengths {expected.Length} vs {actual.Length}).{Environment.NewLine}expected: {Excerpt (expected, index)}{Environment.NewLine}actual:   {Excerpt (actual, index)}");
		}

		public static void Equal (ReadOnlySpan<byte> expected, ReadOnlySpan<byte> actual, string what)
		{
			if (expected.SequenceEqual (actual))
				return;

			int index = 0;

			while (index < expected.Length && index < actual.Length && expected[index] == actual[index])
				index++;

			throw new FuzzInvariantException ($"{what}: buffers differ at index {index} (lengths {expected.Length} vs {actual.Length}).");
		}

		static string Excerpt (string text, int index)
		{
			int start = Math.Max (0, index - 40);
			int length = Math.Min (text.Length - start, 120);

			return Escape (text.Substring (start, length));
		}

		public static string Escape (string text)
		{
			var builder = new StringBuilder (text.Length);

			foreach (var c in text) {
				if (c < 0x20 || c == 0x7f || c == '\\')
					builder.Append ($"\\x{(int) c:X2}");
				else
					builder.Append (c);
			}

			return builder.ToString ();
		}

		// FNV-1a 64-bit.
		public static ulong Hash (ReadOnlySpan<byte> data, ulong hash = 14695981039346656037UL)
		{
			foreach (var b in data) {
				hash ^= b;
				hash *= 1099511628211UL;
			}

			return hash;
		}

		public static ulong Hash (Stream stream, out long length)
		{
			var buffer = new byte[4096];
			ulong hash = 14695981039346656037UL;
			int n;

			length = 0;

			while ((n = stream.Read (buffer, 0, buffer.Length)) > 0) {
				hash = Hash (buffer.AsSpan (0, n), hash);
				length += n;
			}

			return hash;
		}

		public static async Task<(ulong Hash, long Length)> HashAsync (Stream stream)
		{
			var buffer = new byte[4096];
			ulong hash = 14695981039346656037UL;
			long length = 0;
			int n;

			while ((n = await stream.ReadAsync (buffer, 0, buffer.Length).ConfigureAwait (false)) > 0) {
				hash = Hash (buffer.AsSpan (0, n), hash);
				length += n;
			}

			return (hash, length);
		}

		public static string HashString (string? text)
		{
			if (text == null)
				return "null";

			return $"{text.Length}:{Hash (Utf8.GetBytes (text)):x16}";
		}

		// A selector derived from the whole input. Targets whose input format has a fixed prefix (TNEF
		// signatures, RTF headers, MIME headers) use this to vary their configuration without consuming
		// any input bytes, so that the seed corpora remain valid inputs.
		public static uint Selector (ReadOnlySpan<byte> data)
		{
			var hash = Hash (data);

			return (uint) (hash ^ (hash >> 32));
		}

		public static void Wait (Task task)
		{
			task.GetAwaiter ().GetResult ();
		}

		public static T Wait<T> (Task<T> task)
		{
			return task.GetAwaiter ().GetResult ();
		}
	}
}
