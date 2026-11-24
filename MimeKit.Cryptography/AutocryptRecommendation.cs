//
// AutocryptRecommendation.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MimeKit.Cryptography {
	/// <summary>
	/// The Autocrypt Level 1 recommendation for composing a message.
	/// </summary>
	/// <remarks>
	/// This is advice for the application, not permission to send or downgrade a message.
	/// </remarks>
	public enum AutocryptRecommendationLevel
	{
		/// <summary>
		/// Do not offer encryption.
		/// </summary>
		/// <remarks>
		/// Encryption is not possible for the supplied envelope.
		/// </remarks>
		Disable,

		/// <summary>
		/// Offer encryption with a warning.
		/// </summary>
		/// <remarks>
		/// A recipient may not be able to read the encrypted message.
		/// </remarks>
		Discourage,

		/// <summary>
		/// Offer encryption without enabling it by default.
		/// </summary>
		/// <remarks>
		/// The user may choose to encrypt.
		/// </remarks>
		Available,

		/// <summary>
		/// Enable encryption by default.
		/// </summary>
		/// <remarks>
		/// The user retains the final decision.
		/// </remarks>
		Encrypt
	}

	/// <summary>
	/// The factors determining one recipient's recommendation.
	/// </summary>
	/// <remarks>
	/// A key-selection reason is retained even when a reply or mutual preference promotes the recommendation.
	/// </remarks>
	[Flags]
	public enum AutocryptRecommendationReason
	{
		/// <summary>
		/// No usable key was found.
		/// </summary>
		/// <remarks>
		/// No peer state, expired keys, and otherwise unusable keys can all give this result.
		/// </remarks>
		NoUsableKey = 0,

		/// <summary>
		/// A current direct advertisement supplies the key.
		/// </summary>
		/// <remarks>
		/// Its timestamp is within 35 days of the peer's last-seen date.
		/// </remarks>
		DirectKey = 1,

		/// <summary>
		/// A stale direct advertisement supplies the key.
		/// </summary>
		/// <remarks>
		/// Staleness concerns the advertisement, not the key's validity period.
		/// </remarks>
		StaleDirectKey = 2,

		/// <summary>
		/// A gossip advertisement supplies the key.
		/// </summary>
		/// <remarks>
		/// No usable direct key was available.
		/// </remarks>
		GossipKey = 4,

		/// <summary>
		/// Both parties prefer encryption.
		/// </summary>
		/// <remarks>
		/// Mutual preferences promote only a current direct advertisement.
		/// </remarks>
		MutualPreference = 8,

		/// <summary>
		/// The draft is a reply to an encrypted message.
		/// </summary>
		/// <remarks>
		/// An encrypted reply requires a usable key, but it may be stale or gossiped.
		/// </remarks>
		EncryptedReply = 16
	}

	/// <summary>
	/// The recommendation and target key for one envelope recipient.
	/// </summary>
	/// <remarks>
	/// A selected key does not authenticate the recipient's identity.
	/// </remarks>
	public sealed class AutocryptRecipientRecommendation
	{
		internal AutocryptRecipientRecommendation (AutocryptRecommendationLevel level, AutocryptRecommendationReason reason, AutocryptKey? key)
		{
			Level = level;
			Reason = reason;
			Key = key;
		}

		/// <summary>
		/// Get the recommendation for this recipient.
		/// </summary>
		/// <remarks>
		/// Other recipients can prevent encryption of the message.
		/// </remarks>
		/// <value>The individual recommendation.</value>
		public AutocryptRecommendationLevel Level { get; }

		/// <summary>
		/// Get the reasons for the recommendation.
		/// </summary>
		/// <remarks>
		/// Includes the source of the selected key, if any.
		/// </remarks>
		/// <value>The contributing factors.</value>
		public AutocryptRecommendationReason Reason { get; }

		/// <summary>
		/// Get the selected recipient key.
		/// </summary>
		/// <remarks>
		/// A usable direct key takes precedence over gossip, even if its advertisement is stale.
		/// </remarks>
		/// <value>The selected certificate, or <see langword="null"/> when encryption is disabled.</value>
		public AutocryptKey? Key { get; }
	}

	/// <summary>
	/// An Autocrypt encryption recommendation for an entire envelope.
	/// </summary>
	/// <remarks>
	/// The result is a snapshot. Re-evaluate it if the recipients, peer states, or key usability change.
	/// </remarks>
	public sealed class AutocryptRecommendation
	{
		AutocryptRecommendation (AutocryptRecommendationLevel level, Dictionary<string, AutocryptRecipientRecommendation> recipients)
		{
			Recipients = new ReadOnlyDictionary<string, AutocryptRecipientRecommendation> (recipients);
			Level = level;
		}

		/// <summary>
		/// Get the aggregate encryption recommendation.
		/// </summary>
		/// <remarks>
		/// An empty envelope or any recipient without a usable key disables encryption.
		/// </remarks>
		/// <value>The recommendation for all recipients together.</value>
		public AutocryptRecommendationLevel Level { get; }

		/// <summary>
		/// Get the individual recipient recommendations.
		/// </summary>
		/// <remarks>
		/// Includes blind recipients; this mapping must not be published in outgoing headers.
		/// </remarks>
		/// <value>A read-only map keyed by distinct canonical envelope addresses.</value>
		public IReadOnlyDictionary<string, AutocryptRecipientRecommendation> Recipients { get; }

		static bool IsUsable (AutocryptKey? key, DateTimeOffset time, Predicate<AutocryptKey>? canUseKey)
		{
			return key != null && key.IsUsableAt (time) && (canUseKey is null || canUseKey (key));
		}

		static AutocryptRecipientRecommendation EvaluateRecipient (AutocryptPeerState? peer, AutocryptPreference preference,
			DateTimeOffset time, bool encryptedReply, Predicate<AutocryptKey>? canUseKey)
		{
			AutocryptRecommendationReason reason;
			AutocryptRecommendationLevel level;
			AutocryptKey? key;

			if (peer != null && IsUsable (peer.DirectKey, time, canUseKey)) {
				key = peer.DirectKey;

				// Measure silence since the advertisement against the last direct message, not today's clock.
				if (peer.LastSeen - peer.DirectTimestamp > TimeSpan.FromDays (35)) {
					reason = AutocryptRecommendationReason.StaleDirectKey;
					level = AutocryptRecommendationLevel.Discourage;
				} else {
					reason = AutocryptRecommendationReason.DirectKey;
					level = AutocryptRecommendationLevel.Available;

					if (preference == AutocryptPreference.Mutual && peer.Preference == AutocryptPreference.Mutual) {
						reason |= AutocryptRecommendationReason.MutualPreference;
						level = AutocryptRecommendationLevel.Encrypt;
					}
				}
			} else if (peer != null && IsUsable (peer.GossipKey, time, canUseKey)) {
				key = peer.GossipKey;
				reason = AutocryptRecommendationReason.GossipKey;
				level = AutocryptRecommendationLevel.Discourage;
			} else {
				return new AutocryptRecipientRecommendation (AutocryptRecommendationLevel.Disable, AutocryptRecommendationReason.NoUsableKey, null);
			}

			if (encryptedReply) {
				reason |= AutocryptRecommendationReason.EncryptedReply;
				level = AutocryptRecommendationLevel.Encrypt;
			}

			return new AutocryptRecipientRecommendation (level, reason, key);
		}

		/// <summary>
		/// Evaluate the Autocrypt Level 1 recommendation for the actual delivery envelope.
		/// </summary>
		/// <remarks>
		/// Assumes an Autocrypt-enabled local account. No message is modified and no keys are imported.
		/// Missing peers disable encryption; do not omit their addresses from the envelope.
		/// </remarks>
		/// <param name="preference">The local account's encryption preference.</param>
		/// <param name="recipients">All envelope recipients, including Bcc.</param>
		/// <param name="peers">Snapshots for this account indexed by their canonical addresses.</param>
		/// <param name="time">The instant at which keys must be usable.</param>
		/// <param name="encryptedReply">Whether the draft is a reply to an encrypted message.</param>
		/// <param name="canUseKey">An optional application check that can exclude an otherwise time-valid
		/// key, for example because of a known revocation. No external revocation source is consulted by default.</param>
		/// <returns>The aggregate recommendation and one result per distinct recipient.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="recipients"/> or <paramref name="peers"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="preference"/> is not a defined preference.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// A recipient is null or has an invalid address, or a retrieved snapshot does not match its address.
		/// </exception>
		public static AutocryptRecommendation Evaluate (AutocryptPreference preference, IEnumerable<MailboxAddress> recipients,
			IReadOnlyDictionary<string, AutocryptPeerState> peers, DateTimeOffset time, bool encryptedReply, Predicate<AutocryptKey>? canUseKey = null)
		{
			if (recipients is null)
				throw new ArgumentNullException (nameof (recipients));

			if (peers is null)
				throw new ArgumentNullException (nameof (peers));

			if (preference != AutocryptPreference.NoPreference && preference != AutocryptPreference.Mutual)
				throw new ArgumentOutOfRangeException (nameof (preference));

			var results = new Dictionary<string, AutocryptRecipientRecommendation> (StringComparer.Ordinal);
			var level = AutocryptRecommendationLevel.Encrypt;

			foreach (var recipient in recipients) {
				if (recipient is null || !AutocryptValue.TryCanonicalize (recipient.Address, out var address))
					throw new ArgumentException ("Expected a recipient with a single addr-spec.", nameof (recipients));

				if (results.ContainsKey (address))
					continue;

				if (peers.TryGetValue (address, out var peer) && (peer is null || peer.Address != address))
					throw new ArgumentException ("The peer state does not match the recipient.", nameof (peers));

				var result = EvaluateRecipient (peer, preference, time, encryptedReply, canUseKey);
				results.Add (address, result);

				// The levels are ordered from most restrictive to least restrictive.
				if (result.Level < level)
					level = result.Level;
			}

			return new AutocryptRecommendation (results.Count == 0 ? AutocryptRecommendationLevel.Disable : level, results);
		}
	}
}
