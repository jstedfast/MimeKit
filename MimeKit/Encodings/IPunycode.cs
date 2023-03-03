// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace MimeKit.Encodings {
	/// <summary>
	/// An interface for encoding and decoding international domain names.
	/// </summary>
	/// <remarks>
	/// An interface for encoding and decoding international domain names.
	/// </remarks>
	public interface IPunycode
	{
		/// <summary>
		/// Encode a Unicode domain name, converting it to an ASCII-safe representation.
		/// </summary>
		/// <remarks>
		/// Encodes a Unicode domain name, converting it to an ASCII-safe representation
		/// according to the rules defined by the IDNA standard.
		/// </remarks>
		/// <param name="domain">The Unicode domain name.</param>
		/// <returns>The ASCII-encoded domain name.</returns>
		string Encode (string domain);

		/// <summary>
		/// Encode a Unicode domain name, converting it to an ASCII-safe representation.
		/// </summary>
		/// <remarks>
		/// Encodes a Unicode domain name, converting it to an ASCII-safe representation
		/// according to the rules defined by the IDNA standard.
		/// </remarks>
		/// <param name="domain">The Unicode domain name.</param>
		/// <param name="index">A zero-based offset into <paramref name="domain"/> that
		/// specifies the start of the substring to convert. The conversion operation continues
		/// to the end of the string.</param>
		/// <returns>The ASCII-encoded domain name.</returns>
		string Encode (string domain, int index);

		/// <summary>
		/// Encode a Unicode domain name, converting it to an ASCII-safe representation.
		/// </summary>
		/// <remarks>
		/// Encodes a Unicode domain name, converting it to an ASCII-safe representation
		/// according to the rules defined by the IDNA standard.
		/// </remarks>
		/// <param name="domain">The Unicode domain name.</param>
		/// <param name="index">A zero-based offset into <paramref name="domain"/> that
		/// specifies the start of the substring to convert. The conversion operation continues
		/// to the end of the string.</param>
		/// <param name="count">The number of characters to convert in the substring that starts
		/// at the position specified by <paramref name="index"/> in the <paramref name="domain"/>
		/// string.</param>
		/// <returns>The ASCII-encoded domain name.</returns>
		string Encode (string domain, int index, int count);

		/// <summary>
		/// Decode a domain name, converting it to a string of Unicode characters.
		/// </summary>
		/// <remarks>
		/// Decodes a domain name, converting it to Unicode, according to the rules defined by the IDNA standard.
		/// </remarks>
		/// <param name="domain">The ASCII-encoded domain name.</param>
		/// <returns>The Unicode domain name.</returns>
		string Decode (string domain);

		/// <summary>
		/// Decode a domain name, converting it to a string of Unicode characters.
		/// </summary>
		/// <remarks>
		/// Decodes a domain name, converting it to Unicode, according to the rules defined by the IDNA standard.
		/// </remarks>
		/// <param name="domain">The ASCII-encoded domain name.</param>
		/// <param name="index">A zero-based offset into <paramref name="domain"/> that
		/// specifies the start of the substring to convert. The conversion operation continues
		/// to the end of the string.</param>
		/// <returns>The Unicode domain name.</returns>
		string Decode (string domain, int index);

		/// <summary>
		/// Decode a domain name, converting it to a string of Unicode characters.
		/// </summary>
		/// <remarks>
		/// Decodes a domain name, converting it to Unicode, according to the rules defined by the IDNA standard.
		/// </remarks>
		/// <param name="domain">The ASCII-encoded domain name.</param>
		/// <param name="index">A zero-based offset into <paramref name="domain"/> that
		/// specifies the start of the substring to convert. The conversion operation continues
		/// to the end of the string.</param>
		/// <param name="count">The number of characters to convert in the substring that starts
		/// at the position specified by <paramref name="index"/> in the <paramref name="domain"/>
		/// string.</param>
		/// <returns>The Unicode domain name.</returns>
		string Decode (string domain, int index, int count);
	}
}
