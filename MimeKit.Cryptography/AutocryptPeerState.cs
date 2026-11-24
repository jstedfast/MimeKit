//
// AutocryptPeerState.cs
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

using System;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An immutable snapshot of the Autocrypt Level 1 state for one peer.
	/// </summary>
	/// <remarks>
	/// Applications isolate peers by local account and persist updates atomically, retrying against
	/// fresh state if needed. Neither a direct advertisement nor gossip authenticates the peer.
	/// </remarks>
	public sealed class AutocryptPeerState
	{
		/// <summary>
		/// Create an empty peer state or restore a previously saved snapshot.
		/// </summary>
		/// <remarks>
		/// Restore saved key bytes using <see cref="AutocryptKey.TryParse"/>. An absent timestamp
		/// means that no corresponding message or advertisement has been observed.
		/// </remarks>
		/// <param name="address">The peer's mailbox address without a display name.</param>
		/// <param name="lastSeen">The latest effective date observed directly from the peer.</param>
		/// <param name="directKey">The last directly advertised key.</param>
		/// <param name="directTimestamp">The effective date of the direct advertisement.</param>
		/// <param name="preference">The preference from the same direct advertisement.</param>
		/// <param name="gossipKey">The last key advertised by gossip.</param>
		/// <param name="gossipTimestamp">The effective date of the gossip advertisement.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="address"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The address is invalid, a key and its timestamp are not supplied together, a direct
		/// advertisement has not been accounted for in <paramref name="lastSeen"/>, or a preference
		/// other than no preference was supplied without a direct key.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="preference"/> is not a defined preference.
		/// </exception>
		public AutocryptPeerState (string address, DateTimeOffset? lastSeen = null, AutocryptKey? directKey = null,
			DateTimeOffset? directTimestamp = null, AutocryptPreference preference = AutocryptPreference.NoPreference,
			AutocryptKey? gossipKey = null, DateTimeOffset? gossipTimestamp = null)
		{
			if (address is null)
				throw new ArgumentNullException (nameof (address));

			if (!AutocryptValue.TryCanonicalize (address, out var canonical))
				throw new ArgumentException ("Expected a single addr-spec.", nameof (address));

			if ((directKey != null) != directTimestamp.HasValue)
				throw new ArgumentException ("Expected a direct key and timestamp together.", nameof (directTimestamp));

			if ((gossipKey != null) != gossipTimestamp.HasValue)
				throw new ArgumentException ("Expected a gossip key and timestamp together.", nameof (gossipTimestamp));

			if (directTimestamp.HasValue && (!lastSeen.HasValue || lastSeen < directTimestamp))
				throw new ArgumentException ("Last seen cannot precede the direct advertisement.", nameof (lastSeen));

			if (preference != AutocryptPreference.NoPreference && preference != AutocryptPreference.Mutual)
				throw new ArgumentOutOfRangeException (nameof (preference));

			if (directKey is null && preference != AutocryptPreference.NoPreference)
				throw new ArgumentException ("A preference requires a direct advertisement.", nameof (preference));

			Address = canonical;
			LastSeen = lastSeen;
			DirectKey = directKey;
			DirectTimestamp = directTimestamp;
			Preference = preference;
			GossipKey = gossipKey;
			GossipTimestamp = gossipTimestamp;
		}

		/// <summary>
		/// Get the peer's canonical address.
		/// </summary>
		/// <remarks>
		/// Updates must belong to this address.
		/// </remarks>
		/// <value>The canonical addr-spec.</value>
		public string Address { get; }

		/// <summary>
		/// Get the latest effective date of an eligible message from the peer.
		/// </summary>
		/// <remarks>
		/// A message need not advertise a key to advance this date.
		/// </remarks>
		/// <value>The last observed date, or <see langword="null"/> for a peer never seen directly.</value>
		public DateTimeOffset? LastSeen { get; private set; }

		/// <summary>
		/// Get the most recently accepted directly advertised key.
		/// </summary>
		/// <remarks>
		/// Check usability at the time of encryption, including any independently known revocations.
		/// </remarks>
		/// <value>The verified certificate, or <see langword="null"/> if none was accepted.</value>
		public AutocryptKey? DirectKey { get; private set; }

		/// <summary>
		/// Get the effective date of the accepted direct advertisement.
		/// </summary>
		/// <remarks>
		/// This can precede <see cref="LastSeen"/>.
		/// </remarks>
		/// <value>The effective date, or <see langword="null"/> if there is no direct key.</value>
		public DateTimeOffset? DirectTimestamp { get; private set; }

		/// <summary>
		/// Get the peer's directly advertised encryption preference.
		/// </summary>
		/// <remarks>
		/// Gossip cannot establish or change this preference.
		/// </remarks>
		/// <value>The advertised preference, or no preference in the absence of a direct advertisement.</value>
		public AutocryptPreference Preference { get; private set; }

		/// <summary>
		/// Get the most recently accepted gossiped key.
		/// </summary>
		/// <remarks>
		/// This key was advertised by a third party, not by the peer.
		/// </remarks>
		/// <value>The verified certificate, or <see langword="null"/> if none was accepted.</value>
		public AutocryptKey? GossipKey { get; private set; }

		/// <summary>
		/// Get the effective date of the accepted gossip advertisement.
		/// </summary>
		/// <remarks>
		/// Gossip has its own timeline independent of direct observations.
		/// </remarks>
		/// <value>The effective date, or <see langword="null"/> if there is no gossip key.</value>
		public DateTimeOffset? GossipTimestamp { get; private set; }

		/// <summary>
		/// Apply a direct sender observation without modifying this snapshot.
		/// </summary>
		/// <remarks>
		/// Only a strictly newer, verified advertisement replaces the direct key and preference.
		/// An eligible message can still advance <see cref="LastSeen"/> without replacing either.
		/// Key expiration is evaluated when choosing an encryption key, not when recording its advertisement.
		/// </remarks>
		/// <param name="observation">The sender observation, or <see langword="null"/> for an ignored message.</param>
		/// <returns>The updated snapshot, or this instance if nothing changed.</returns>
		/// <exception cref="ArgumentException">
		/// The observation is for a different peer.
		/// </exception>
		public AutocryptPeerState ApplyDirect (AutocryptObservation? observation)
		{
			if (observation is null)
				return this;

			if (observation.Address != Address)
				throw new ArgumentException ("The observation is for a different peer.", nameof (observation));

			var date = observation.EffectiveDate;
			if (date <= DirectTimestamp)
				return this;

			var advertisement = observation.Advertisement;
			AutocryptKey? key = null;

			if (advertisement != null)
				AutocryptKey.TryParse (advertisement.KeyData, out key);

			if (key is null && date <= LastSeen)
				return this;

			var updated = (AutocryptPeerState) MemberwiseClone ();

			if (!LastSeen.HasValue || date > LastSeen)
				updated.LastSeen = date;

			if (key != null) {
				updated.DirectKey = key;
				updated.DirectTimestamp = date;
				updated.Preference = advertisement!.Preference;
			}

			return updated;
		}

		/// <summary>
		/// Apply a gossip advertisement without modifying this snapshot.
		/// </summary>
		/// <remarks>
		/// Only a strictly newer, verified gossip key is accepted. Gossip does not change the direct
		/// key, encryption preference, or last-seen date.
		/// </remarks>
		/// <param name="gossip">A recipient-matched advertisement from the integrity-checked decrypted root.</param>
		/// <param name="effectiveDate">The enclosing message's date capped at its recorded receipt time,
		/// or the recorded receipt time if the message date is unavailable.</param>
		/// <returns>The updated snapshot, or this instance if nothing changed.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="gossip"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The advertisement is for a different peer.
		/// </exception>
		public AutocryptPeerState ApplyGossip (AutocryptGossip gossip, DateTimeOffset effectiveDate)
		{
			if (gossip is null)
				throw new ArgumentNullException (nameof (gossip));

			if (gossip.Address != Address)
				throw new ArgumentException ("The advertisement is for a different peer.", nameof (gossip));

			if (effectiveDate <= GossipTimestamp || !AutocryptKey.TryParse (gossip.KeyData, out var key))
				return this;

			var updated = (AutocryptPeerState) MemberwiseClone ();
			updated.GossipTimestamp = effectiveDate;
			updated.GossipKey = key;

			return updated;
		}
	}
}
