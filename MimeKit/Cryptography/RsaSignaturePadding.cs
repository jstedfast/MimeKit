//
// RsaSignaturePadding.cs
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

using System;

namespace MimeKit.Cryptography {
	/// <summary>
	/// The RSA signature padding schemes and parameters used by S/MIME.
	/// </summary>
	/// <remarks>
	/// The RSA signature padding schemes and parameters used by S/MIME as described in
	/// <a href="https://tools.ietf.org/html/rfc8017">rfc8017</a>.
	/// </remarks>
	public sealed class RsaSignaturePadding : IEquatable<RsaSignaturePadding>
	{
		/// <summary>
		/// The PKCS #1 v1.5 signature padding.
		/// </summary>
		/// <remarks>
		/// The PKCS #1 v1.5 signature padding.
		/// </remarks>
		public static readonly RsaSignaturePadding Pkcs1 = new RsaSignaturePadding (RsaSignaturePaddingScheme.Pkcs1);

		/// <summary>
		/// The Probibilistic Signature Scheme (PSS) padding.
		/// </summary>
		/// <remarks>
		/// The Probibilistic Signature Scheme (PSS) padding.
		/// </remarks>
		public static readonly RsaSignaturePadding Pss = new RsaSignaturePadding (RsaSignaturePaddingScheme.Pss);

		RsaSignaturePadding (RsaSignaturePaddingScheme scheme)
		{
			Scheme = scheme;
		}

		/// <summary>
		/// Get the RSA signature padding scheme.
		/// </summary>
		/// <remarks>
		/// Gets the RSA signature padding scheme.
		/// </remarks>
		/// <value>The RSA signature padding scheme.</value>
		public RsaSignaturePaddingScheme Scheme {
			get; private set;
		}

		/// <summary>
		/// Determines whether the specified <see cref="RsaSignaturePadding"/> is equal to the current <see cref="RsaSignaturePadding"/>.
		/// </summary>
		/// <remarks>
		/// Compares two RSA Signature paddings to determine if they are identical or not.
		/// </remarks>
		/// <param name="other">The <see cref="RsaSignaturePadding"/> to compare with the current <see cref="RsaSignaturePadding"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="RsaSignaturePadding"/> is equal to the current
		/// <see cref="RsaSignaturePadding"/>; otherwise, <c>false</c>.</returns>
		public bool Equals (RsaSignaturePadding other)
		{
			if (other is null)
				return false;

			return other.Scheme == Scheme;
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <remarks>
		/// The type of comparison between the current instance and the <paramref name="obj"/> parameter depends on whether
		/// the current instance is a reference type or a value type.
		/// </remarks>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
		public override bool Equals (object obj)
		{
			return Equals (obj as RsaSignaturePadding);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <remarks>
		/// Returns the hash code for this instance.
		/// </remarks>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode ()
		{
			return Scheme.GetHashCode ();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current
		/// <see cref="RsaSignaturePadding"/>.
		/// </summary>
		/// <remarks>
		/// Creates a string-representation of the <see cref="RsaSignaturePadding"/>.
		/// </remarks>
		/// <returns>A <see cref="System.String"/> that represents the current
		/// <see cref="RsaSignaturePadding"/>.</returns>
		public override string ToString ()
		{
			return Scheme == RsaSignaturePaddingScheme.Pkcs1 ? "Pkcs1" : "Pss";
		}

		/// <summary>
		/// Compare two <see cref="RsaSignaturePadding"/> objects for equality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="RsaSignaturePadding"/> objects for equality.
		/// </remarks>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <c>false</c>.</returns>
		public static bool operator == (RsaSignaturePadding left, RsaSignaturePadding right)
		{
			if (left is null)
				return right is null;

			return left.Equals (right);
		}

		/// <summary>
		/// Compare two <see cref="RsaSignaturePadding"/> objects for inequality.
		/// </summary>
		/// <remarks>
		/// Compares two <see cref="RsaSignaturePadding"/> objects for inequality.
		/// </remarks>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns><c>true</c> if <paramref name="left"/> and <paramref name="right"/> are unequal; otherwise, <c>false</c>.</returns>
		public static bool operator != (RsaSignaturePadding left, RsaSignaturePadding right)
		{
			return !(left == right);
		}
	}
}
