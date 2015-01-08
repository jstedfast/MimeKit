//
// DbExtensions.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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

using System.Data;

namespace MimeKit.Cryptography {
	/// <summary>
	/// Useful extensions for working with System.Data types.
	/// </summary>
	static class DbExtensions
	{
		/// <summary>
		/// Creates a <see cref="System.Data.IDbDataParameter"/> with the specified name and value and then adds it to the command's parameters.
		/// </summary>
		/// <param name="command">The database command.</param>
		/// <param name="name">The parameter name.</param>
		/// <param name="value">The parameter value.</param>
		public static int AddParameterWithValue (this IDbCommand command, string name, object value)
		{
			var parameter = command.CreateParameter ();
			parameter.ParameterName = name;
			parameter.Value = value;

			return command.Parameters.Add (parameter);
		}
	}
}
