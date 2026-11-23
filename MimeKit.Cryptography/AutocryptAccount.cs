//
// AutocryptAccount.cs
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

using MimeKit.Utils;

namespace MimeKit.Cryptography {
	/// <summary>
	/// An outgoing Autocrypt identity bound to an explicitly selected local key.
	/// </summary>
	/// <remarks>
	/// The application owns the keyring and account binding. Preparing mail never imports keys,
	/// searches for keys by mailbox, sends a message, or chooses cleartext on encryption failure.
	/// </remarks>
	public sealed class AutocryptAccount
	{
		readonly PgpSecretKey signer;

		/// <summary>
		/// Bind an account to a primary signing key and a particular encryption subkey.
		/// </summary>
		/// <remarks>
		/// Both secret keys must be present so the account can sign and read its own mail.
		/// The selected user ID is included in the public advertisement.
		/// </remarks>
		/// <param name="address">The account's addr-spec.</param>
		/// <param name="preference">The account's advertised encryption preference.</param>
		/// <param name="keyRing">The explicitly selected local secret keyring.</param>
		/// <param name="userId">The certified user ID to advertise.</param>
		/// <param name="encryptionKeyFingerprint">The full fingerprint of the encryption subkey.</param>
		/// <param name="time">The instant at which to validate the selection.</param>
		/// <exception cref="ArgumentNullException">
		/// A required argument is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The address, user ID, or key selection is invalid.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The preference is undefined.
		/// </exception>
		/// <exception cref="PgpException">
		/// The selected certificate cannot produce a usable advertisement.
		/// </exception>
		public AutocryptAccount (string address, AutocryptPreference preference, PgpSecretKeyRing keyRing, string userId, byte[] encryptionKeyFingerprint, DateTimeOffset time)
		{
			var canonical = ValidateIdentity (address, preference);

			if (keyRing is null)
				throw new ArgumentNullException (nameof (keyRing));

			if (userId is null)
				throw new ArgumentNullException (nameof (userId));

			if (encryptionKeyFingerprint is null)
				throw new ArgumentNullException (nameof (encryptionKeyFingerprint));

			signer = keyRing.GetSecretKey ();

			if (!signer.IsMasterKey || !signer.IsSigningKey || signer.IsPrivateKeyEmpty)
				throw new ArgumentException ("The primary signing secret key is required.", nameof (keyRing));

			var encryption = keyRing.GetSecretKey (encryptionKeyFingerprint);

			if (encryption is null || encryption.IsMasterKey || encryption.IsPrivateKeyEmpty)
				throw new ArgumentException ("Expected a local encryption subkey.", nameof (encryptionKeyFingerprint));

			Key = AutocryptKey.FromSelection (signer.PublicKey, encryption.PublicKey, userId, time);
			EncryptionSecretKey = encryption;
			Address = canonical;
			Preference = preference;
		}

		static string ValidateIdentity (string address, AutocryptPreference preference)
		{
			if (address is null)
				throw new ArgumentNullException (nameof (address));

			if (!AutocryptValue.TryCanonicalize (address, out var canonical))
				throw new ArgumentException ("Expected a single addr-spec.", nameof (address));

			if (preference != AutocryptPreference.NoPreference && preference != AutocryptPreference.Mutual)
				throw new ArgumentOutOfRangeException (nameof (preference));

			return canonical;
		}

		/// <summary>
		/// Generate a new Autocrypt Level 1.1 identity and bind it to a local account.
		/// </summary>
		/// <remarks>
		/// Generate a key only when setting up a new identity, not when a lookup or recovery fails.
		/// The version 4 certificate contains a signing and certification primary key and a separate
		/// encryption subkey. No key is imported or persisted.
		/// </remarks>
		/// <param name="address">The account addr-spec. Its canonical form becomes the certified user ID.</param>
		/// <param name="preference">The account's advertised encryption preference.</param>
		/// <param name="passphrase">A nonempty passphrase used to protect both secret keys, encoded as UTF-8.</param>
		/// <param name="keyRing">The generated, passphrase-protected secret keyring for the application to store.</param>
		/// <returns>An account bound to the generated Ed25519 signing key and Cv25519 encryption subkey.</returns>
		/// <exception cref="ArgumentNullException">
		/// The address or passphrase is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The address is invalid or the passphrase is empty or cannot be encoded as UTF-8.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The encryption preference is undefined.
		/// </exception>
		/// <exception cref="PgpException">
		/// The certificate could not be generated or validated.
		/// </exception>
		public static AutocryptAccount Generate (string address, AutocryptPreference preference, char[] passphrase, out PgpSecretKeyRing keyRing)
		{
			var canonical = ValidateIdentity (address, preference);
			if (passphrase is null)
				throw new ArgumentNullException (nameof (passphrase));

			if (passphrase.Length == 0)
				throw new ArgumentException ("A local key passphrase is required.", nameof (passphrase));

			var random = new SecureRandom ();
			var now = DateTime.UtcNow;
			var signing = new Ed25519PrivateKeyParameters (random);
			var encryption = new X25519PrivateKeyParameters (random);
			// Level 1.1 uses the v4 EdDSA and ECDH packet encodings, not the RFC 9580 algorithm tags.
			var primary = new PgpKeyPair (PublicKeyAlgorithmTag.EdDsa_Legacy, signing.GeneratePublicKey (), signing, now);
			var subkey = new PgpKeyPair (PublicKeyAlgorithmTag.ECDH, encryption.GeneratePublicKey (), encryption, now);
			var certification = new PgpSignatureSubpacketGenerator ();

			certification.SetSignatureCreationTime (false, now);
			certification.SetKeyFlags (false, PgpKeyFlags.CanCertify | PgpKeyFlags.CanSign);
			certification.SetFeature (false, Org.BouncyCastle.Bcpg.Sig.Features.FEATURE_MODIFICATION_DETECTION);
			certification.SetPreferredHashAlgorithms (false, [(int) HashAlgorithmTag.Sha256, (int) HashAlgorithmTag.Sha512]);
			certification.SetPreferredSymmetricAlgorithms (false, [(int) SymmetricKeyAlgorithmTag.Aes256, (int) SymmetricKeyAlgorithmTag.Aes128]);

			var binding = new PgpSignatureSubpacketGenerator ();
			binding.SetSignatureCreationTime (false, now);
			binding.SetKeyFlags (false, PgpKeyFlags.CanEncryptCommunications | PgpKeyFlags.CanEncryptStorage);

			var password = CharsetUtils.UTF8.GetBytes (passphrase);

			try {
				var generator = new PgpKeyRingGenerator (PgpSignature.PositiveCertification, primary, canonical, SymmetricKeyAlgorithmTag.Aes256,
					HashAlgorithmTag.Sha256, password, true, certification.Generate (), null, random);

				generator.AddSubKey (subkey, binding.Generate (), null, HashAlgorithmTag.Sha256);

				var ring = generator.GenerateSecretKeyRing ();
				var account = new AutocryptAccount (canonical, preference, ring, canonical, subkey.PublicKey.GetFingerprint (), now);

				keyRing = ring;

				return account;
			} finally {
				Array.Clear (password, 0, password.Length);
			}
		}

		internal PgpSecretKey SigningSecretKey {
			get { return signer; }
		}

		internal PgpSecretKey EncryptionSecretKey { get; }

		/// <summary>
		/// Get the canonical sender address.
		/// </summary>
		/// <remarks>
		/// Outgoing messages must have this single From mailbox.
		/// </remarks>
		/// <value>The canonical addr-spec.</value>
		public string Address { get; }

		/// <summary>
		/// Get the account's encryption preference.
		/// </summary>
		/// <remarks>
		/// This is included in the direct advertisement independently of encryption.
		/// </remarks>
		/// <value>The local encryption preference.</value>
		public AutocryptPreference Preference { get; }

		/// <summary>
		/// Get the selected public certificate.
		/// </summary>
		/// <remarks>
		/// Its encryption subkey is always included when encrypting outgoing mail.
		/// </remarks>
		/// <value>The validated minimal Autocrypt key.</value>
		public AutocryptKey Key { get; }

		Header PrepareAdvertisement (MimeMessage message, DateTimeOffset time, Predicate<AutocryptKey>? canUseKey)
		{
			if (message is null)
				throw new ArgumentNullException (nameof (message));

			if (!Key.IsUsableAt (time) || (canUseKey != null && !canUseKey (Key)))
				throw new InvalidOperationException ("The selected account key is no longer usable.");

			Header? from = null;
			foreach (var header in message.Headers) {
				if (header.Id == HeaderId.From && from is null)
					from = header;
				else if (header.Id == HeaderId.From || header.Field.StartsWith ("Resent-", StringComparison.OrdinalIgnoreCase))
					throw new InvalidOperationException ("Expected an original message with one From field.");
			}

			if (from is null || !MailboxAddress.TryParse (from.RawValue, out var mailbox) ||
			    !AutocryptValue.TryCanonicalize (mailbox.Address, out var address) || address != Address)
				throw new InvalidOperationException ("The From mailbox does not match this account.");

			var headers = message.Body?.Headers;
			if (message.Headers.Contains (HeaderId.AutocryptGossip) || (headers != null &&
			    (headers.Contains (HeaderId.AutocryptGossip) || headers.Contains (HeaderId.Autocrypt) ||
			     headers.Contains (HeaderId.Bcc) || headers.Contains (HeaderId.ResentBcc))))
				throw new InvalidOperationException ("Remove pre-existing gossip or misplaced address and advertisement fields before preparing the message.");

			return new Autocrypt (Address, Key.KeyData.ToArray (), Preference).ToHeader ();
		}

		/// <summary>
		/// Add or replace the sender's direct advertisement without encrypting the message.
		/// </summary>
		/// <remarks>
		/// This changes only the Autocrypt field. The transport remains responsible for omitting Bcc headers.
		/// </remarks>
		/// <param name="message">The original outgoing message from this account.</param>
		/// <param name="time">The time at which the account key must be usable.</param>
		/// <param name="canUseKey">An optional application check for revocations or other exclusions.</param>
		/// <exception cref="ArgumentNullException">
		/// The message is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The account key, sender, or existing headers are unsuitable.
		/// </exception>
		/// <exception cref="FormatException">
		/// The advertisement exceeds the protocol size limit.
		/// </exception>
		public void Advertise (MimeMessage message, DateTimeOffset time, Predicate<AutocryptKey>? canUseKey = null)
		{
			var header = PrepareAdvertisement (message, time, canUseKey);

			message.Headers.Replace (header);
		}

		static string GetAddress (MailboxAddress mailbox)
		{
			if (mailbox is null || !AutocryptValue.TryCanonicalize (mailbox.Address, out var address))
				throw new ArgumentException ("Expected a recipient with a single addr-spec.");

			return address;
		}

		IEnumerable<PgpPublicKey> PrepareEncryption (OpenPgpContext ctx, MimeMessage message, IEnumerable<MailboxAddress> recipients,
			IReadOnlyDictionary<string, AutocryptPeerState> peers, DateTimeOffset time, Predicate<AutocryptKey>? canUseKey, CancellationToken cancellationToken,
			out MimeEntity body, out Header advertisement, out HeaderList gossip)
		{
			if (ctx is null)
				throw new ArgumentNullException (nameof (ctx));

			if (recipients is null)
				throw new ArgumentNullException (nameof (recipients));

			if (peers is null)
				throw new ArgumentNullException (nameof (peers));

			cancellationToken.ThrowIfCancellationRequested ();

			advertisement = PrepareAdvertisement (message, time, canUseKey);
			body = message.Body ?? throw new InvalidOperationException ("The message has no body.");
			var envelope = new HashSet<string> (StringComparer.Ordinal);
			var others = new List<MailboxAddress> ();

			foreach (var mailbox in recipients) {
				var address = GetAddress (mailbox);

				if (envelope.Add (address) && address != Address)
					others.Add (mailbox);
			}

			if (envelope.Count == 0)
				throw new InvalidOperationException ("The delivery envelope is empty.");

			var visible = new HashSet<string> (message.To.Mailboxes.Concat (message.Cc.Mailboxes).Select (GetAddress), StringComparer.Ordinal);
			var blind = new HashSet<string> (message.Bcc.Mailboxes.Select (GetAddress), StringComparer.Ordinal);

			if (!visible.IsSubsetOf (envelope) || !blind.IsSubsetOf (envelope))
				throw new InvalidOperationException ("The delivery envelope omits a message recipient.");

			visible.ExceptWith (blind);

			var selection = AutocryptRecommendation.Evaluate (Preference, others, peers, time, false, canUseKey);
			var targets = new Dictionary<long, PgpPublicKey> { { Key.EncryptionKey.KeyId, Key.EncryptionKey } };
			gossip = new HeaderList ();

			foreach (var pair in selection.Recipients) {
				var key = pair.Value.Key ?? throw new InvalidOperationException ("A delivery recipient has no usable encryption key.");
				var subkey = key.EncryptionKey;

				// OpenPgpContext deduplicates by 64-bit key ID. Never silently omit a distinct subkey.
				if (targets.TryGetValue (subkey.KeyId, out var other) && !other.GetFingerprint ().SequenceEqual (subkey.GetFingerprint ()))
					throw new InvalidOperationException ("Distinct encryption keys have the same key ID.");

				targets[subkey.KeyId] = subkey;

				if (visible.Contains (pair.Key))
					gossip.Add (new AutocryptGossip (pair.Key, key.KeyData.ToArray ()).ToHeader ());
			}

			// The body brings its own headers and separator. These fields go before them, not in the outer message.
			gossip.HasBodySeparator = false;

			return targets.Values;
		}

		static void ApplyEncryption (MimeMessage message, MultipartEncrypted encrypted, Header advertisement)
		{
			message.Body = encrypted;
			message.Headers.Replace (advertisement);
			message.Headers.RemoveAll (HeaderId.Bcc);
		}

		/// <summary>
		/// Advertise, sign and encrypt an outgoing message after the caller has chosen encryption.
		/// </summary>
		/// <remarks>
		/// Rechecks all keys, encrypts to the account and every delivery recipient, and places To/Cc
		/// gossip only inside the encrypted root. On success the body is replaced and Bcc headers are
		/// removed: retain a snapshot of the explicit envelope for delivery. The original body is not disposed.
		/// As with ordinary OpenPGP encryption,
		/// recipient key IDs may be visible in the ciphertext. No cleartext fallback is attempted.
		/// </remarks>
		/// <param name="ctx">The context used for signing and encryption.</param>
		/// <param name="message">The original outgoing message. Do not edit it during preparation.</param>
		/// <param name="recipients">The final delivery envelope, including all blind recipients.</param>
		/// <param name="peers">Current peer snapshots indexed by canonical address.</param>
		/// <param name="time">The send-time instant used for key validation.</param>
		/// <param name="canUseKey">An optional check for currently known revocations or other exclusions.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="ArgumentNullException">
		/// A required argument is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// A recipient address or peer mapping is invalid.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The message, envelope, or current key selection is unsuitable.
		/// </exception>
		/// <exception cref="FormatException">
		/// An advertisement exceeds the protocol size limit.
		/// </exception>
		/// <exception cref="OperationCanceledException">
		/// Preparation was cancelled.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// The message content could not be read.
		/// </exception>
		/// <exception cref="PgpException">
		/// Signing or encryption failed.
		/// </exception>
		public void SignAndEncrypt (OpenPgpContext ctx, MimeMessage message, IEnumerable<MailboxAddress> recipients,
			IReadOnlyDictionary<string, AutocryptPeerState> peers, DateTimeOffset time, Predicate<AutocryptKey>? canUseKey = null, CancellationToken cancellationToken = default)
		{
			var targets = PrepareEncryption (ctx, message, recipients, peers, time, canUseKey, cancellationToken, out var body, out var advertisement, out var gossip);
			var encrypted = MultipartEncrypted.SignAndEncrypt (ctx, signer, DigestAlgorithm.Sha256, EncryptionAlgorithm.Aes128, targets, body, gossip, cancellationToken);

			ApplyEncryption (message, encrypted, advertisement);
		}

		/// <summary>
		/// Asynchronously advertise, sign and encrypt an outgoing message.
		/// </summary>
		/// <inheritdoc cref="SignAndEncrypt"/>
		/// <returns>A task that completes after replacing the body and preparing the outer headers.</returns>
		public async Task SignAndEncryptAsync (OpenPgpContext ctx, MimeMessage message, IEnumerable<MailboxAddress> recipients,
			IReadOnlyDictionary<string, AutocryptPeerState> peers, DateTimeOffset time, Predicate<AutocryptKey>? canUseKey = null, CancellationToken cancellationToken = default)
		{
			var targets = PrepareEncryption (ctx, message, recipients, peers, time, canUseKey, cancellationToken, out var body, out var advertisement, out var gossip);
			var encrypted = await MultipartEncrypted.SignAndEncryptAsync (ctx, signer, DigestAlgorithm.Sha256, EncryptionAlgorithm.Aes128, targets, body, gossip, cancellationToken).ConfigureAwait (false);

			ApplyEncryption (message, encrypted, advertisement);
		}
	}
}
