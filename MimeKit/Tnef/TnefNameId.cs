// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace MimeKit.Tnef {
	/// <summary>
	/// A TNEF name identifier.
	/// </summary>
	/// <remarks>
	/// A TNEF name identifier.
	/// </remarks>
	public readonly struct TnefNameId : IEquatable<TnefNameId>
	{
		readonly TnefNameIdKind kind;
		readonly string name;
		readonly Guid guid;
		readonly int id;

		/// <summary>
		/// Get the property set GUID.
		/// </summary>
		/// <remarks>
		/// Gets the property set GUID.
		/// </remarks>
		/// <value>The property set GUID.</value>
		public Guid PropertySetGuid => guid;

		/// <summary>
		/// Get the kind of TNEF name identifier.
		/// </summary>
		/// <remarks>
		/// Gets the kind of TNEF name identifier.
		/// </remarks>
		/// <value>The kind of identifier.</value>
		public TnefNameIdKind Kind => kind;

		/// <summary>
		/// Get the name, if available.
		/// </summary>
		/// <remarks>
		/// If the <see cref="Kind"/> is <see cref="TnefNameIdKind.Name"/>, then this property will be available.
		/// </remarks>
		/// <value>The name.</value>
		public string Name => name;

		/// <summary>
		/// Get the identifier, if available.
		/// </summary>
		/// <remarks>
		/// If the <see cref="Kind"/> is <see cref="TnefNameIdKind.Id"/>, then this property will be available.
		/// </remarks>
		/// <value>The identifier.</value>
		public int Id => id;

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefNameId"/> struct.
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
		/// Initialize a new instance of the <see cref="TnefNameId"/> struct.
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
		/// Serves as a hash function for a <see cref="TnefNameId"/> object.
		/// </summary>
		/// <remarks>
		/// Serves as a hash function for a <see cref="TnefNameId"/> object.
		/// </remarks>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms
		/// and data structures such as a hash table.</returns>
		public override int GetHashCode ()
		{
			int hash = kind == TnefNameIdKind.Id ? id : name.GetHashCode ();

			return kind.GetHashCode () ^ guid.GetHashCode () ^ hash;
		}

		/// <summary>
		/// Determine whether the specified <see cref="System.Object"/> is equal to the current <see cref="TnefNameId"/>.
		/// </summary>
		/// <remarks>
		/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="TnefNameId"/>.
		/// </remarks>
		/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="TnefNameId"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
		/// <see cref="TnefNameId"/>; otherwise, <c>false</c>.</returns>
		public override bool Equals (object obj)
		{
			return obj is TnefNameId other && Equals (other);
		}

		/// <summary>
		/// Determine whether the specified <see cref="TnefNameId"/> is equal to the current <see cref="TnefNameId"/>.
		/// </summary>
		/// <remarks>
		/// Compares two TNEF name identifiers to determine if they are identical or not.
		/// </remarks>
		/// <param name="other">The <see cref="TnefNameId"/> to compare with the current <see cref="TnefNameId"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="TnefNameId"/> is equal to the current
		/// <see cref="TnefNameId"/>; otherwise, <c>false</c>.</returns>
		public bool Equals (TnefNameId other)
		{
			if (kind != other.kind || guid != other.guid)
				return false;

			return kind is TnefNameIdKind.Id ? other.id == id : other.name == name;
		}
	}
}
