#nullable enable

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MimeKit.Utils {
	internal ref partial struct ValueStringBuilder
	{
		private char[]? _arrayToReturnToPool;
		private Span<char> _chars;
		private int _pos;

#if UNUSED_VALUESTRINGBUILDER_API
		public ValueStringBuilder (Span<char> initialBuffer)
		{
			_arrayToReturnToPool = null;
			_chars = initialBuffer;
			_pos = 0;
		}
#endif

		public ValueStringBuilder (int initialCapacity)
		{
			_arrayToReturnToPool = ArrayPool<char>.Shared.Rent (initialCapacity);
			_chars = _arrayToReturnToPool;
			_pos = 0;
		}

		public int Length {
			get => _pos;
#if UNUSED_VALUESTRINGBUILDER_API
			set {
				Debug.Assert (value >= 0);
				Debug.Assert (value <= _chars.Length);
				_pos = value;
			}
#endif
		}

#if UNUSED_VALUESTRINGBUILDER_API
		public int Capacity => _chars.Length;

		public void EnsureCapacity (int capacity)
		{
			// This is not expected to be called this with negative capacity
			Debug.Assert (capacity >= 0);

			// If the caller has a bug and calls this with negative capacity, make sure to call Grow to throw an exception.
			if ((uint) capacity > (uint) _chars.Length)
				Grow (capacity - _pos);
		}
#endif

#if UNUSED_VALUESTRINGBUILDER_API
		/// <summary>
		/// Get a pinnable reference to the builder.
		/// Does not ensure there is a null char after <see cref="Length"/>
		/// This overload is pattern matched in the C# 7.3+ compiler so you can omit
		/// the explicit method call, and write eg "fixed (char* c = builder)"
		/// </summary>
		public ref char GetPinnableReference ()
		{
			return ref MemoryMarshal.GetReference (_chars);
		}

		/// <summary>
		/// Get a pinnable reference to the builder.
		/// </summary>
		/// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
		public ref char GetPinnableReference (bool terminate)
		{
			if (terminate) {
				EnsureCapacity (Length + 1);
				_chars[Length] = '\0';
			}
			return ref MemoryMarshal.GetReference (_chars);
		}
#endif

		public ref char this[int index] {
			get {
				Debug.Assert (index < _pos);
				return ref _chars[index];
			}
		}

		public override string ToString ()
		{
			string s = _chars.Slice (0, _pos).ToString ();
			Dispose ();
			return s;
		}

#if UNUSED_VALUESTRINGBUILDER_API
		/// <summary>Returns the underlying storage of the builder.</summary>
		public Span<char> RawChars => _chars;
#endif

#if UNUSED_VALUESTRINGBUILDER_API
		/// <summary>
		/// Returns a span around the contents of the builder.
		/// </summary>
		/// <param name="terminate">Ensures that the builder has a null char after <see cref="Length"/></param>
		public ReadOnlySpan<char> AsSpan (bool terminate)
		{
			if (terminate) {
				EnsureCapacity (Length + 1);
				_chars[Length] = '\0';
			}
			return _chars.Slice (0, _pos);
		}
#endif

		public ReadOnlySpan<char> AsSpan () => _chars.Slice (0, _pos);
#if UNUSED_VALUESTRINGBUILDER_API
		public ReadOnlySpan<char> AsSpan (int start) => _chars.Slice (start, _pos - start);
		public ReadOnlySpan<char> AsSpan (int start, int length) => _chars.Slice (start, length);
#endif

#if UNUSED_VALUESTRINGBUILDER_API
		public bool TryCopyTo (Span<char> destination, out int charsWritten)
		{
			if (_chars.Slice (0, _pos).TryCopyTo (destination)) {
				charsWritten = _pos;
				Dispose ();
				return true;
			} else {
				charsWritten = 0;
				Dispose ();
				return false;
			}
		}
#endif

#if UNUSED_VALUESTRINGBUILDER_API
		public void Insert (int index, char value, int count)
		{
			if (_pos > _chars.Length - count) {
				Grow (count);
			}

			int remaining = _pos - index;
			_chars.Slice (index, remaining).CopyTo (_chars.Slice (index + count));
			_chars.Slice (index, count).Fill (value);
			_pos += count;
		}
#endif

		public void Insert (int index, string? s)
		{
			if (s is null) {
				return;
			}

			int count = s.Length;

			if (_pos > (_chars.Length - count)) {
				Grow (count);
			}

			int remaining = _pos - index;
			_chars.Slice (index, remaining).CopyTo (_chars.Slice (index + count));
			s
#if !NET6_0_OR_GREATER
				.AsSpan ()
#endif
				.CopyTo (_chars.Slice (index));
			_pos += count;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public void Append (char c)
		{
			int pos = _pos;
			if ((uint) pos < (uint) _chars.Length) {
				_chars[pos] = c;
				_pos = pos + 1;
			} else {
				GrowAndAppend (c);
			}
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public void Append (string? s)
		{
			if (s is null) {
				return;
			}

			int pos = _pos;
			if (s.Length == 1 && (uint) pos < (uint) _chars.Length) // very common case, e.g. appending strings from NumberFormatInfo like separators, percent symbols, etc.
			{
				_chars[pos] = s[0];
				_pos = pos + 1;
			} else {
				AppendSlow (s);
			}
		}

		public void AppendJoin (char seperator, IList<string> values)
		{
			for (int i = 0; i < values.Count; i++) {
				if (i > 0) 
					Append (seperator);

				Append (values[i]);
			}
		}

		private void AppendSlow (string s)
		{
			int pos = _pos;
			if (pos > _chars.Length - s.Length) {
				Grow (s.Length);
			}

			s
#if !NET6_0_OR_GREATER
				.AsSpan ()
#endif
				.CopyTo (_chars.Slice (pos));
			_pos += s.Length;
		}

#if UNUSED_VALUESTRINGBUILDER_API
		public void Append (char c, int count)
		{
			if (_pos > _chars.Length - count) {
				Grow (count);
			}

			Span<char> dst = _chars.Slice (_pos, count);
			for (int i = 0; i < dst.Length; i++) {
				dst[i] = c;
			}
			_pos += count;
		}
#endif

#if UNUSED_VALUESTRINGBUILDER_API
		public unsafe void Append (char* value, int length)
		{
			int pos = _pos;
			if (pos > _chars.Length - length) {
				Grow (length);
			}

			Span<char> dst = _chars.Slice (_pos, length);
			for (int i = 0; i < dst.Length; i++) {
				dst[i] = *value++;
			}
			_pos += length;
		}
#endif

		public void Append (ReadOnlySpan<char> value)
		{
			int pos = _pos;
			if (pos > _chars.Length - value.Length) {
				Grow (value.Length);
			}

			value.CopyTo (_chars.Slice (_pos));
			_pos += value.Length;
		}

#if UNUSED_VALUESTRINGBUILDER_API
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public Span<char> AppendSpan (int length)
		{
			int origPos = _pos;
			if (origPos > _chars.Length - length) {
				Grow (length);
			}

			_pos = origPos + length;
			return _chars.Slice (origPos, length);
		}
#endif

#if NET6_0_OR_GREATER
		internal void AppendInvariant<T> (T value, string? format = null) where T : ISpanFormattable
		{
			if (value.TryFormat (_chars.Slice (_pos), out int charsWritten, format, CultureInfo.InvariantCulture)) {
				_pos += charsWritten;
			} else {
				Append (value.ToString (format, CultureInfo.InvariantCulture));
			}
		}

#if UNUSED_VALUESTRINGBUILDER_API
		internal void AppendSpanFormattable<T> (T value, string? format = null, IFormatProvider? provider = null) where T : ISpanFormattable
		{
			if (value.TryFormat (_chars.Slice (_pos), out int charsWritten, format, provider)) {
				_pos += charsWritten;
			} else {
				Append (value.ToString (format, provider));
			}
		}
#endif
#else
		internal void AppendInvariant<T> (T value, string? format = null) where T: IFormattable
		{
			Append (value.ToString (format, CultureInfo.InvariantCulture));	
		}

#if UNUSED_VALUESTRINGBUILDER_API
		internal void AppendSpanFormattable<T> (T value, string? format = null, IFormatProvider? provider = null) where T: IFormattable
		{
			Append (value.ToString (format, provider));	
		}
#endif
#endif


		[MethodImpl (MethodImplOptions.NoInlining)]
		private void GrowAndAppend (char c)
		{
			Grow (1);
			Append (c);
		}

		/// <summary>
		/// Resize the internal buffer either by doubling current buffer size or
		/// by adding <paramref name="additionalCapacityBeyondPos"/> to
		/// <see cref="_pos"/> whichever is greater.
		/// </summary>
		/// <param name="additionalCapacityBeyondPos">
		/// Number of chars requested beyond current position.
		/// </param>
		[MethodImpl (MethodImplOptions.NoInlining)]
		private void Grow (int additionalCapacityBeyondPos)
		{
			Debug.Assert (additionalCapacityBeyondPos > 0);
			Debug.Assert (_pos > _chars.Length - additionalCapacityBeyondPos, "Grow called incorrectly, no resize is needed.");

			// Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative
			char[] poolArray = ArrayPool<char>.Shared.Rent ((int) Math.Max ((uint) (_pos + additionalCapacityBeyondPos), (uint) _chars.Length * 2));

			_chars.Slice (0, _pos).CopyTo (poolArray);

			char[]? toReturn = _arrayToReturnToPool;
			_chars = _arrayToReturnToPool = poolArray;
			if (toReturn != null) {
				ArrayPool<char>.Shared.Return (toReturn);
			}
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public void Dispose ()
		{
			char[]? toReturn = _arrayToReturnToPool;
			this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
			if (toReturn != null) {
				ArrayPool<char>.Shared.Return (toReturn);
			}
		}
	}
}
