//
// TnefNameId.cs
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

using System;

namespace MimeKit.Tnef {
	/// <summary>
	/// A TNEF name identifier.
	/// </summary>
	/// <remarks>
	/// A TNEF name identifier.
	/// </remarks>
	public struct TnefNameId
	{
		readonly TnefNameIdKind kind;
		readonly string name;
		readonly Guid guid;
		readonly int id;

		/// <summary>
		/// Gets the property set GUID.
		/// </summary>
		/// <remarks>
		/// Gets the property set GUID.
		/// </remarks>
		/// <value>The property set GUID.</value>
		public Guid PropertySetGuid {
			get { return guid; }
		}

		/// <summary>
		/// Gets the kind of TNEF name identifier.
		/// </summary>
		/// <remarks>
		/// Gets the kind of TNEF name identifier.
		/// </remarks>
		/// <value>The kind of identifier.</value>
		public TnefNameIdKind Kind {
			get { return kind; }
		}

		/// <summary>
		/// Gets the name, if available.
		/// </summary>
		/// <remarks>
		/// If the <see cref="Kind"/> is <see cref="TnefNameIdKind.Name"/>, then this property will be available.
		/// </remarks>
		/// <value>The name.</value>
		public string Name {
			get { return name; }
		}

		/// <summary>
		/// Gets the identifier, if available.
		/// </summary>
		/// <remarks>
		/// If the <see cref="Kind"/> is <see cref="TnefNameIdKind.Id"/>, then this property will be available.
		/// </remarks>
		/// <value>The identifier.</value>
		public int Id {
			get { return id; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Tnef.TnefNameId"/> struct.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefNameId"/> with the specified integer identifier.
		/// </remarks>
		/// <param name="propertySetGuid">The property set GUID.</param>
		/// <param name="id">The identifier.</param>
		public TnefNameId (Guid propertySetGuid, int id)
		{
			kind = TnefNameIdKind.Id;
			guid = propertySetGuid;
			this.id = id;
			name = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeKit.Tnef.TnefNameId"/> struct.
		/// </summary>
		/// <remarks>
		/// Creates a new <see cref="TnefNameId"/> with the specified string identifier.
		/// </remarks>
		/// <param name="propertySetGuid">The property set GUID.</param>
		/// <param name="name">The name.</param>
		public TnefNameId (Guid propertySetGuid, string name)
		{
			kind = TnefNameIdKind.Name;
			guid = propertySetGuid;
			this.name = name;
			id = 0;
		}

		/// <summary>
		/// Serves as a hash function for a <see cref="MimeKit.Tnef.TnefNameId"/> object.
		/// </summary>
		/// <remarks>
		/// Serves as a hash function for a <see cref="MimeKit.Tnef.TnefNameId"/> object.
		/// </remarks>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms
		/// and data structures such as a hash table.</returns>
		public override int GetHashCode ()
		{
			int hash = kind == TnefNameIdKind.Id ? id : name.GetHashCode ();

			return kind.GetHashCode () ^ guid.GetHashCode () ^ hash;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="MimeKit.Tnef.TnefNameId"/>.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="MimeKit.Tnef.TnefNameId"/>.
		/// </remarks>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="MimeKit.Tnef.TnefNameId"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="MimeKit.Tnef.TnefNameId"/>; otherwise, <c>false</c>.</returns>
		public override bool Equals (object obj)
		{
			if (!(obj is TnefNameId))
				return false;

			var v = (TnefNameId) obj;

			if (v.kind != kind || v.guid != guid)
				return false;

			return kind == TnefNameIdKind.Id ? v.id == id : v.name == name;
		}
	}
}
