//
// TnefReEmitter.cs
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

using MimeKit.Tnef;

namespace MimeKit.Fuzzing.Helpers {
	// Re-emits a TNEF stream through TnefWriter (modelled on UnitTests/Tnef/TnefWriterTests.cs ReEmit).
	static class TnefReEmitter
	{
		sealed class WriterRejectedException : Exception
		{
		}

		static void Write (Action action)
		{
			try {
				action ();
			} catch (ArgumentException) {
				// Documented by TnefWriter: the value read from a (fuzzed) stream is not something that can be
				// written, e.g. an out-of-range codepage or a property type the writer does not accept.
				throw new WriterRejectedException ();
			} catch (InvalidOperationException) {
				// Documented by TnefWriter: the attribute sequence is not valid for the writer's state.
				throw new WriterRejectedException ();
			}
		}

		static T Write<T> (Func<T> func)
		{
			T result = default!;

			Write (() => { result = func (); });

			return result;
		}

		static void CopyProperties (TnefPropertyReader source, TnefPropertyWriter target, bool table)
		{
			if (table) {
				while (source.ReadNextRow ()) {
					Write (target.BeginRow);

					while (source.ReadNextProperty ()) {
						var property = source.ReadProperty ();
						Write (() => target.WriteProperty (property));
					}
				}
			} else {
				while (source.ReadNextProperty ()) {
					var property = source.ReadProperty ();
					Write (() => target.WriteProperty (property));
				}
			}
		}

		// Returns null when the writer rejected something that the reader produced. Throws
		// TnefException (InvalidSignature) when the input does not start with the TNEF signature.
		public static byte[]? ReEmit (byte[] tnef, TnefOptions options)
		{
			using var reader = new TnefReader (new MemoryStream (tnef, false), options, false);
			using var output = new MemoryStream ();
			TnefWriter? writer = null;

			try {
				while (reader.Read ()) {
					switch (reader.Tag) {
					case TnefAttributeTag.TnefVersion:
					case TnefAttributeTag.OemCodepage:
					case TnefAttributeTag.Null:
						continue;
					}

					// Note: Unknown attributes cannot be written.
					if (!Enum.IsDefined (reader.Tag))
						continue;

					// Note: The codepage is known once the OemCodepage attribute (if any) has been read.
					writer ??= Write (() => new TnefWriter (output, reader.Codepage, reader.LegacyKey, true));

					if (TnefWalker.ContainsProperties (reader.Tag)) {
						var properties = Write (() => writer.OpenPropertyWriter (reader.Tag));

						CopyProperties (reader.GetPropertyReader (), properties, reader.Tag == TnefAttributeTag.RecipientTable);
						Write (properties.Dispose);
					} else {
						var value = reader.ReadValueAsBytes ();

						Write (() => writer.WriteAttribute (reader.Tag, value));
					}
				}

				writer ??= Write (() => new TnefWriter (output, reader.Codepage, reader.LegacyKey, true));
				Write (writer.Flush);
				Write (writer.Dispose);
				writer = null;
			} catch (WriterRejectedException) {
				return null;
			} finally {
				try {
					writer?.Dispose ();
				} catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException) {
					// Disposing a writer that was abandoned part-way through may fail; the round trip is already aborted.
				}
			}

			return output.ToArray ();
		}
	}
}
