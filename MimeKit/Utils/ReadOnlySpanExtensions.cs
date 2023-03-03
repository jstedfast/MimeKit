// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.CompilerServices;

namespace MimeKit.Utils {
	static class ReadOnlySpanExtensions
	{
		/// <summary>
		/// Tokenize the values in the input <see cref="ReadOnlySpan{T}"/> instance using a specified separator.
		/// </summary>
		/// <remarks>
		/// <para>Tokenizes the values in the input <see cref="ReadOnlySpan{T}"/> instance using a specified separator.</para>
		/// <para>This extension should be used directly within a <see langword="foreach"/> loop:</para>
		/// <code>
		/// ReadOnlySpan&lt;char&gt; text = "Hello, world!";
		///
		/// foreach (var token in text.Tokenize(','))
		/// {
		///     // Access the tokens here...
		/// }
		/// </code>
		/// <para>The compiler will take care of properly setting up the <see langword="foreach"/> loop with the type returned from this method.</para>
		/// <note type="note">The returned <see cref="ReadOnlySpanTokenizer{T}"/> value shouldn't be used directly: use this extension in a <see langword="foreach"/> loop.</note>
		/// </remarks>
		/// <typeparam name="T">The type of items in the <see cref="ReadOnlySpan{T}"/> to tokenize.</typeparam>
		/// <returns>A tokenizer for <paramref name="span"/>.</returns>
		/// <param name="span">The source <see cref="ReadOnlySpan{T}"/> to tokenize.</param>
		/// <param name="separator">The separator <typeparamref name="T"/> item to use.</param>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static ReadOnlySpanTokenizer<T> Tokenize<T> (this ReadOnlySpan<T> span, T separator)
			where T : IEquatable<T>
		{
			return new ReadOnlySpanTokenizer<T> (span, separator);
		}

		/// <summary>
		/// Tokenize the values in the input <see cref="string"/> instance using a specified separator.
		/// </summary>
		/// <remarks>
		/// <para>Tokenizes the values in the input <see cref="string"/> instance using a specified separator.</para>
		/// <para>This extension should be used directly within a <see langword="foreach"/> loop:</para>
		/// <code>
		/// string text = "Hello, world!";
		///
		/// foreach (var token in text.Tokenize(','))
		/// {
		///     // Access the tokens here...
		/// }
		/// </code>
		/// <para>The compiler will take care of properly setting up the <see langword="foreach"/> loop with the type returned from this method.</para>
		/// <note type="note">The returned <see cref="ReadOnlySpanTokenizer{T}"/> value shouldn't be used directly: use this extension in a <see langword="foreach"/> loop.</note>
		/// </remarks>
		/// <returns>A tokenizer for <paramref name="text"/>.</returns>
		/// <param name="text">The source <see cref="string"/> to tokenize.</param>
		/// <param name="separator">The separator character to use.</param>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static ReadOnlySpanTokenizer<char> Tokenize (this string text, char separator)
		{
			return text.AsSpan ().Tokenize (separator);
		}
	}
}
