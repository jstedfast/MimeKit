//
// InternetAddressConverter.cs
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
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace MimeKit {
	/// <summary>
	/// A type converter for converting between <see cref="InternetAddress"/> and string.
	/// </summary>
	/// <remarks>
	/// Provides a way of converting between <see cref="InternetAddress"/> and <see langword="string"/>.
	/// </remarks>
	public class InternetAddressConverter : TypeConverter
	{
		static ParserOptions Options;

		/// <summary>
		/// Register the type converter so that it's available through <see cref="TypeDescriptor.GetConverter(object)"/>.
		/// </summary>
		/// <remarks>
		/// Registers the type converter so that it's available through <see cref="TypeDescriptor.GetConverter(object)"/>.
		/// </remarks>
		/// <param name="options">The <see cref="ParserOptions"/> to use when converting from string or <see langword="null"/> to use the default options.</param>
		/// <exception cref="InvalidOperationException">
		/// The Register method was called more than once.
		/// </exception>
		public static void Register (ParserOptions options = null)
		{
			if (Interlocked.Exchange (ref Options, options ?? ParserOptions.Default) != null)
				throw new InvalidOperationException ($"The {typeof (InternetAddressConverter)}.{nameof (Register)} method must be called only once.");

			TypeDescriptor.AddAttributes (typeof (InternetAddress), new TypeConverterAttribute (typeof (InternetAddressConverter)));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InternetAddressConverter"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="InternetAddressConverter"/>.
		/// </remarks>
		public InternetAddressConverter ()
		{
		}

		/// <summary>
		/// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
		/// </summary>
		/// <remarks>
		/// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
		/// </remarks>
		/// <returns><see langword="true"/> if this converter can perform the conversion; otherwise, <see langword="false"/>.</returns>
		/// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="sourceType">A <see cref="Type"/> that represents the type you want to convert from.</param>
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof (string) || base.CanConvertFrom (context, sourceType);
		}

		/// <summary>
		/// Returns whether this converter can convert the object to the specified type, using the specified context.
		/// </summary>
		/// <remarks>
		/// <para>Use the <paramref name="context"/> parameter to extract additional information about the environment
		/// from which this converter is invoked. This parameter can be <see langword="null"/>, so always check it. Also,
		/// properties on the context object can return <see langword="null"/>.</para>
		/// <para>If <paramref name="destinationType"/> is a string, the default implementation of <see cref="CanConvertTo"/>
		/// always returns <see langword="true"/>.</para>
		/// </remarks>
		/// <returns><see langword="true"/> if this converter can perform the conversion; otherwise, <see langword="false"/>.</returns>
		/// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="destinationType">A <see cref="Type"/> that represents the type you want to convert to.</param>
		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof (string) || base.CanConvertTo (context, destinationType);
		}

		/// <summary>
		/// Converts the given object to the type of this converter, using the specified context and culture information.
		/// </summary>
		/// <remarks>
		/// Converts the given object to the type of this converter, using the specified context and culture information.
		/// </remarks>
		/// <returns>An <see cref="Object"/> that represents the converted value.</returns>
		/// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
		/// <param name="value">The <see cref="Object"/> to convert.</param>
		/// <exception cref="NotSupportedException">
		/// The conversion cannot be performed.
		/// </exception>
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string text)
				return InternetAddress.Parse (Options ?? ParserOptions.Default, text);

			return base.ConvertFrom (context, culture, value);
		}

		/// <summary>
		/// Converts the given value object to the specified type, using the specified context and culture information.
		/// </summary>
		/// <remarks>
		/// Converts the given value object to the specified type, using the specified context and culture information.
		/// </remarks>
		/// <returns>An <see cref="Object"/> that represents the converted value.</returns>
		/// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">A <see cref="CultureInfo"/>. If <see langword="null"/>, the current culture is assumed.</param>
		/// <param name="value">The <see cref="Object"/> to convert.</param>
		/// <param name="destinationType">The <see cref="Type"/> to convert the value to.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="destinationType"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The conversion cannot be performed.
		/// </exception>
		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof (string) && value is InternetAddress address)
				return address.ToString ();

			return base.ConvertTo (context, culture, value, destinationType);
		}

		/// <summary>
		/// Returns whether the given value object is valid for this type and for the specified context.
		/// </summary>
		/// <remarks>
		/// <para>Use the <paramref name="context"/> parameter to extract additional information about the environment from which
		/// this converter is invoked. This parameter can be <see langword="null"/>, so always check it. Also, properties on the
		/// context object can return <see langword="null"/>.</para>
		/// <para>Starting in .NET Framework 4, the <see cref="IsValid"/> method catches exceptions from the <see cref="CanConvertFrom"/>
		/// and <see cref="ConvertFrom"/> methods. If the input value type causes <see cref="CanConvertFrom"/> to return <see langword="false"/>,
		/// or if the input value causes <see cref="ConvertFrom"/> to raise an exception, the <see cref="IsValid"/> method returns
		/// <see langword="false"/>.</para>
		/// </remarks>
		/// <returns><see langword="true"/> if the specified value is valid for this object; otherwise, <see langword="false"/>.</returns>
		/// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="value">The <see cref="Object"/> to test for validity.</param>
		public override bool IsValid (ITypeDescriptorContext context, object value)
		{
			if (value is string text)
				return InternetAddress.TryParse (Options ?? ParserOptions.Default, text, out _);

			return base.IsValid (context, value);
		}
	}
}
