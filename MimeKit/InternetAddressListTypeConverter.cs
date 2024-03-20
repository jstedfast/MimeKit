//
// InternetAddressListTypeConverter.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2023 .NET Foundation and Contributors
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
	/// Provides a way of converting <see cref="InternetAddressList"/> from and to string.
	/// </summary>
	public class InternetAddressListTypeConverter : TypeConverter
	{
		private static ParserOptions _parserOptions;

		/// <summary>
		/// Register the type converter so that it's available through <c>TypeDescriptor.GetConverter(typeof(InternetAddressList))</c>.
		/// </summary>
		/// <param name="parserOptions">The <see cref="ParserOptions"/> to use when converting from string or <see langword="null"/> to use the default options.</param>
		/// <exception cref="InvalidOperationException">The Register method is called more than once.</exception>
		public static void Register (ParserOptions parserOptions = null)
		{
			if (Interlocked.Exchange (ref _parserOptions, parserOptions ?? ParserOptions.Default) != null) {
				throw new InvalidOperationException ($"The {typeof(InternetAddressListTypeConverter)}.{nameof(Register)} method must be called only once.");
			}

			TypeDescriptor.AddAttributes (typeof(InternetAddressList), new TypeConverterAttribute (typeof(InternetAddressListTypeConverter)));
		}

		/// <inheritdoc/>
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string) || base.CanConvertFrom (context, sourceType);
		}

		/// <inheritdoc/>
		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string text) {
				return InternetAddressList.Parse (_parserOptions ?? ParserOptions.Default, text);
			}

			return base.ConvertFrom (context, culture, value);
		}

		/// <inheritdoc/>
		public override bool IsValid (ITypeDescriptorContext context, object value)
		{
			if (value is string text) {
				return InternetAddressList.TryParse (_parserOptions ?? ParserOptions.Default, text, out _);
			}

			return base.IsValid (context, value);
		}
	}
}
