//
// AutocryptObservation.cs
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

using MimeKit.Utils;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An eligible Autocrypt sender observation, with or without a matching advertisement.
	/// </summary>
	/// <remarks>
	/// An observation can update the time a sender was last seen even when the message contains no usable
	/// advertisement.
	/// </remarks>
	public sealed class AutocryptObservation
	{
		internal AutocryptObservation (string address, DateTimeOffset effectiveDate, Autocrypt? advertisement)
		{
			Address = address;
			EffectiveDate = effectiveDate;
			Advertisement = advertisement;
		}

		/// <summary>
		/// The canonical address of the observed sender.
		/// </summary>
		/// <remarks>
		/// Use the canonical form for Autocrypt address comparisons, not as evidence of identity.
		/// </remarks>
		/// <value>The lower-case addr-spec with internationalized domain names encoded as ASCII.</value>
		public string Address { get; }

		/// <summary>
		/// The message date, capped at receipt time, or receipt time if no usable date was supplied.
		/// </summary>
		/// <remarks>
		/// Using the recorded receipt time keeps the effective date stable when rescanning the same message.
		/// </remarks>
		/// <value>The earlier of the usable message date and the recorded receipt time, or the receipt time if the message date is unavailable.</value>
		public DateTimeOffset EffectiveDate { get; }

		/// <summary>
		/// The unique matching wire advertisement, or null. No certificate validation has been performed.
		/// </summary>
		/// <remarks>
		/// A missing advertisement does not revoke a previously advertised key.
		/// </remarks>
		/// <value>The single well-formed advertisement matching the sender, or <see langword="null"/> if there is none or the match is ambiguous.</value>
		public Autocrypt? Advertisement { get; }
	}

	/// <summary>
	/// Extract Autocrypt advertisements without importing keys or updating peer state.
	/// </summary>
	/// <remarks>
	/// The caller supplies receipt times and decrypted content and decides whether to persist an
	/// observation.
	/// </remarks>
	public static class AutocryptExtensions
	{
		/// <summary>
		/// Inspect a non-spam message for a direct sender advertisement.
		/// </summary>
		/// <remarks>
		/// A multipart/report or a message without a single mailbox in From is ignored. Multiple matching
		/// advertisements leave the sender observation intact but without a key.
		/// </remarks>
		/// <param name="message">The message after application spam filtering.</param>
		/// <param name="receivedAt">Recorded receipt time; use the same value when rescanning the message.</param>
		/// <returns>Null for an ineligible message; otherwise a sender observation, even if no advertisement was usable.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="message"/> is <see langword="null"/>.
		/// </exception>
		public static AutocryptObservation? GetAutocryptObservation (this MimeMessage message, DateTimeOffset receivedAt)
		{
			if (message is null)
				throw new ArgumentNullException (nameof (message));

			if (message.Body?.ContentType.IsMimeType ("multipart", "report") == true)
				return null;

			Header? from = null, date = null;

			foreach (var header in message.Headers) {
				if (header.Id == HeaderId.From) {
					if (from != null)
						return null;

					from = header;
				} else if (header.Id == HeaderId.Date && date is null) {
					date = header;
				}
			}

			if (from is null || !MailboxAddress.TryParse (from.RawValue, out var mailbox) ||
			    !AutocryptValue.TryCanonicalize (mailbox.Address, out var address))
				return null;

			Autocrypt? advertisement = null;

			foreach (var header in message.Headers) {
				if (header.Id != HeaderId.Autocrypt || !Autocrypt.TryParse (header.RawValue, out var candidate) || candidate.Address != address)
					continue;

				if (advertisement != null) {
					advertisement = null;
					break;
				}

				advertisement = candidate;
			}

			var effective = date != null && DateUtils.TryParse (date.RawValue, out var sent) && sent < receivedAt ? sent : receivedAt;

			return new AutocryptObservation (address, effective, advertisement);
		}

		/// <summary>
		/// Extract unique gossip advertisements for To, Cc, and Reply-To recipients.
		/// </summary>
		/// <remarks>
		/// Outer headers and nested MIME parts are not searched. Multiple matching advertisements for the same
		/// address are discarded.
		/// </remarks>
		/// <param name="message">The outer message, used only for recipient matching.</param>
		/// <param name="decryptedBody">The root entity obtained after successful integrity-checked decryption.</param>
		/// <returns>Matching advertisements from the decrypted root, or an empty list if none qualify.</returns>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="message"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="decryptedBody"/> is <see langword="null"/>.</para>
		/// </exception>
		public static IReadOnlyList<AutocryptGossip> GetAutocryptGossip (this MimeMessage message, MimeEntity decryptedBody)
		{
			if (message is null)
				throw new ArgumentNullException (nameof (message));

			if (decryptedBody is null)
				throw new ArgumentNullException (nameof (decryptedBody));

			if (!decryptedBody.Headers.Contains (HeaderId.AutocryptGossip))
				return Array.Empty<AutocryptGossip> ();

			var allowed = new HashSet<string> (StringComparer.Ordinal);

			AddAllowedAddresses (allowed, message.To);
			AddAllowedAddresses (allowed, message.Cc);
			AddAllowedAddresses (allowed, message.ReplyTo);

			var candidates = new List<AutocryptGossip> ();
			var seen = new HashSet<string> (StringComparer.Ordinal);

			foreach (var header in decryptedBody.Headers) {
				if (header.Id != HeaderId.AutocryptGossip || !AutocryptGossip.TryParse (header.RawValue, out var gossip) || !allowed.Contains (gossip.Address))
					continue;

				if (seen.Add (gossip.Address))
					candidates.Add (gossip);
				else
					allowed.Remove (gossip.Address);
			}

			candidates.RemoveAll (candidate => !allowed.Contains (candidate.Address));

			return candidates;
		}

		static void AddAllowedAddresses (HashSet<string> allowed, InternetAddressList list)
		{
			foreach (var mailbox in list.Mailboxes) {
				if (AutocryptValue.TryCanonicalize (mailbox.Address, out var address))
					allowed.Add (address);
			}
		}
	}
}
