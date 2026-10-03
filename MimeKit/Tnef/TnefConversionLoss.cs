//
// TnefConversionLoss.cs
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

namespace MimeKit.Tnef {
	/// <summary>
	/// A piece of information that was lost when converting a TNEF message to MIME.
	/// </summary>
	/// <remarks>
	/// A piece of information that was lost when converting a <see cref="TnefMessage"/> to MIME.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\TnefExamples.cs" region="ConvertToMime"/>
	/// </example>
	public sealed class TnefConversionLoss
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="TnefConversionLoss"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefConversionLoss"/>.
		/// </remarks>
		/// <param name="kind">The kind of information that was lost.</param>
		/// <param name="description">A description of the information that was lost.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="description"/> is <see langword="null"/>.
		/// </exception>
		public TnefConversionLoss (TnefConversionLossKind kind, string description)
		{
			if (description is null)
				throw new System.ArgumentNullException (nameof (description));

			Description = description;
			Kind = kind;
		}

		/// <summary>
		/// Get the kind of information that was lost.
		/// </summary>
		/// <remarks>
		/// Gets the kind of information that was lost.
		/// </remarks>
		/// <value>The kind of information that was lost.</value>
		public TnefConversionLossKind Kind {
			get;
		}

		/// <summary>
		/// Get a description of the information that was lost.
		/// </summary>
		/// <remarks>
		/// Gets a human-readable description of the information that was lost.
		/// </remarks>
		/// <value>The description.</value>
		public string Description {
			get;
		}

		/// <summary>
		/// Get a string representation of the loss.
		/// </summary>
		/// <remarks>
		/// Gets a string representation of the loss.
		/// </remarks>
		/// <returns>A string representation of the loss.</returns>
		public override string ToString ()
		{
			return Kind + ": " + Description;
		}
	}
}
