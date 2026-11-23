//
// AutocryptSetupMessage.cs
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

using MimeKit.IO;

namespace MimeKit.Cryptography {
	/// <summary>
	/// Create an Autocrypt Level 1 setup message or recover its account data.
	/// </summary>
	/// <remarks>
	/// Both operations require explicit user action. No message is sent, no key is imported, and no
	/// account is replaced. Applications handle confirmation, transport and protected key storage.
	/// </remarks>
	public sealed class AutocryptSetupMessage
	{
		static readonly PacketTag[] SecretPackets = [PacketTag.SecretKey, PacketTag.UserId, PacketTag.Signature, PacketTag.SecretSubkey, PacketTag.Signature];
		const string PreferenceHeader = "Autocrypt-Prefer-Encrypt";
		const int DefaultLimit = 64 * 1024;

		AutocryptSetupMessage (string address, AutocryptPreference preference, PgpSecretKeyRing keyRing, AutocryptKey key)
		{
			Address = address;
			Preference = preference;
			KeyRing = keyRing;
			Key = key;
		}

		/// <summary>
		/// Get the canonical address matched against the caller's account.
		/// </summary>
		/// <remarks>
		/// The setup message is addressed from and to this mailbox.
		/// </remarks>
		/// <value>The canonical addr-spec.</value>
		public string Address { get; }

		/// <summary>
		/// Get the recovered encryption preference.
		/// </summary>
		/// <remarks>
		/// This comes from the authenticated inner armor, not from outer message headers.
		/// </remarks>
		/// <value>The account preference, or no preference if absent.</value>
		public AutocryptPreference Preference { get; }

		/// <summary>
		/// Get the recovered secret keyring.
		/// </summary>
		/// <remarks>
		/// Both keys are protected with the new local passphrase supplied to recovery.
		/// </remarks>
		/// <value>The primary signing key and encryption subkey, not imported into any key store.</value>
		public PgpSecretKeyRing KeyRing { get; }

		/// <summary>
		/// Get the verified public certificate corresponding to the recovered keys.
		/// </summary>
		/// <remarks>
		/// An expired key may still be needed to read archived mail.
		/// </remarks>
		/// <value>The validated minimal certificate.</value>
		public AutocryptKey Key { get; }

		// Avoid leaving our intermediate plaintext copies in disposed MemoryStreams.
		sealed class PlaintextBuffer : MemoryStream
		{
			public PlaintextBuffer (int capacity) : base (capacity) { }

			protected override void Dispose (bool disposing)
			{
				if (disposing) {
					var bytes = GetBuffer ();
					Array.Clear (bytes, 0, bytes.Length);
				}

				base.Dispose (disposing);
			}
		}

		static void WriteSecret (Stream output, PgpSecretKey secret, PgpPublicKey minimal, char[] passphrase, SecureRandom random)
		{
			var selected = PgpSecretKey.ReplacePublicKey (secret, minimal);
			var unprotected = PgpSecretKey.CopyWithNewPasswordUtf8 (selected, passphrase, null, SymmetricKeyAlgorithmTag.Null, random);

			unprotected.Encode (output);
		}

		/// <summary>
		/// Create a self-addressed setup message for an explicitly selected account.
		/// </summary>
		/// <remarks>
		/// Display the generated code separately. Do not send it by email, log it, or store it with
		/// the message. Only the two-digit hint is included. No unencrypted secret key is written to disk.
		/// </remarks>
		/// <param name="account">The local account whose selected keys and preference are being transferred.</param>
		/// <param name="keyPassphrase">The current UTF-8 passphrase for the selected secret keys (empty for unprotected keys).</param>
		/// <param name="setupCode">The generated 36-digit code in nine hyphen-separated groups.</param>
		/// <returns>The setup message, ready for the application to deliver to the same account.</returns>
		/// <exception cref="ArgumentNullException">
		/// The account or key passphrase is null.
		/// </exception>
		/// <exception cref="PgpException">
		/// The secret keys could not be unlocked or encrypted.
		/// </exception>
		public static MimeMessage Create (AutocryptAccount account, char[] keyPassphrase, out string setupCode)
		{
			if (account is null)
				throw new ArgumentNullException (nameof (account));

			if (keyPassphrase is null)
				throw new ArgumentNullException (nameof (keyPassphrase));

			var random = new SecureRandom ();
			var code = string.Join ("-", Enumerable.Range (0, 9).Select (_ => random.Next (10000).ToString ("D4", CultureInfo.InvariantCulture)));
			using var plain = new PlaintextBuffer (DefaultLimit);
			var headers = new Dictionary<string, string> { { PreferenceHeader, account.Preference == AutocryptPreference.Mutual ? "mutual" : "nopreference" } };

			using (var armor = new ArmoredOutputStream (plain, headers, false)) {
				WriteSecret (armor, account.SigningSecretKey, account.Key.PrimaryKey, keyPassphrase, random);
				WriteSecret (armor, account.EncryptionSecretKey, account.Key.EncryptionKey, keyPassphrase, random);
			}

			headers.Clear ();
			headers.Add ("Passphrase-Format", "numeric9x4");
			headers.Add ("Passphrase-Begin", code.Substring (0, 2));

			using var ciphertext = new MemoryStream ();

			using (var armor = new ArmoredOutputStream (ciphertext, headers, false)) {
				var cipher = new PgpEncryptedDataGenerator (SymmetricKeyAlgorithmTag.Aes256, true, random);
				var chars = code.ToCharArray ();

				try {
					cipher.AddMethodUtf8 (chars, HashAlgorithmTag.Sha256, 0xe0);

					using var encrypted = cipher.Open (armor, new byte[4096]);
					using var literal = new PgpLiteralDataGenerator ().Open (encrypted, PgpLiteralData.Binary, "", plain.Length, DateTime.UtcNow);
					plain.WriteTo (literal);
				} finally {
					Array.Clear (chars, 0, chars.Length);
				}
			}

			var mailbox = MailboxAddress.Parse (account.Address);
			var attachment = new MimePart ("application", "autocrypt-setup") {
				Content = new MimeContent (new MemoryStream (ciphertext.ToArray (), false)),
				ContentDisposition = new ContentDisposition (ContentDisposition.Attachment),
				FileName = "autocrypt-setup.asc"
			};
			var message = new MimeMessage ();

			message.From.Add (mailbox);
			message.To.Add (mailbox);
			message.Subject = "Autocrypt Setup Message";
			message.Headers.Add (HeaderId.AutocryptSetupMessage, "v1");
			message.Body = new Multipart ("mixed") {
				new TextPart ("plain") { Text = "Use the setup code from your other device to recover your Autocrypt account. Keep the code separate from this message." },
				attachment
			};

			setupCode = code;

			return message;
		}

		static bool MatchesMailbox (Header header, string address)
		{
			return MailboxAddress.TryParse (header.RawValue, out var mailbox) && AutocryptValue.TryCanonicalize (mailbox.Address, out var canonical) && canonical == address;
		}

		static IMimeContent GetContent (MimeMessage message, string address, string setupCode, char[] newPassphrase, int maxPayloadSize, out string canonical)
		{
			if (message is null)
				throw new ArgumentNullException (nameof (message));

			if (address is null)
				throw new ArgumentNullException (nameof (address));

			if (!AutocryptValue.TryCanonicalize (address, out var mailbox))
				throw new ArgumentException ("Expected an account addr-spec.", nameof (address));

			if (setupCode is null)
				throw new ArgumentNullException (nameof (setupCode));

			if (setupCode.Length == 0)
				throw new ArgumentException ("A setup code is required.", nameof (setupCode));

			if (newPassphrase is null)
				throw new ArgumentNullException (nameof (newPassphrase));

			if (newPassphrase.Length == 0)
				throw new ArgumentException ("A new local key passphrase is required.", nameof (newPassphrase));

			if (maxPayloadSize <= 0)
				throw new ArgumentOutOfRangeException (nameof (maxPayloadSize));

			Header? from = null, to = null, version = null;

			foreach (var header in message.Headers) {
				switch (header.Id) {
				case HeaderId.From when from is null: from = header; break;
				case HeaderId.To when to is null: to = header; break;
				case HeaderId.AutocryptSetupMessage when version is null: version = header; break;
				case HeaderId.From:
				case HeaderId.To:
				case HeaderId.AutocryptSetupMessage:
					throw new FormatException ("Invalid Autocrypt setup message headers.");
				}
			}

			if (from is null || !MatchesMailbox (from, mailbox) || to is null || !MatchesMailbox (to, mailbox) ||
			    version is null || System.Text.Encoding.ASCII.GetString (version.RawValue).Trim () != "v1" || message.Body is not Multipart mixed ||
			    !mixed.ContentType.IsMimeType ("multipart", "mixed") || mixed.Count < 2 ||
			    !(mixed[0].ContentType.IsMimeType ("text", "plain") || mixed[0].ContentType.IsMimeType ("text", "html") ||
			      mixed[0].ContentType.IsMimeType ("multipart", "alternative")) ||
			    mixed[1] is not MimePart part || !part.ContentType.IsMimeType ("application", "autocrypt-setup") || part.Content is null)
				throw new FormatException ("Expected a self-addressed v1 setup message with a setup attachment.");

			if (part.Content.Stream is { CanSeek: true } stream && stream.Length > maxPayloadSize)
				throw new FormatException ("Setup attachment exceeds the configured limit.");

			canonical = mailbox;

			return part.Content;
		}

		static void ReadLimited (Stream input, MemoryStream output, int limit, CancellationToken cancellationToken)
		{
			var buffer = new byte[4096];

			try {
				var cancellable = input as ICancellableStream;

				while (true) {
					int n;

					cancellationToken.ThrowIfCancellationRequested ();

					if (cancellable != null)
						n = cancellable.Read (buffer, 0, buffer.Length, cancellationToken);
					else
						n = input.Read (buffer, 0, buffer.Length);

					if (n == 0)
						break;

					if (output.Length + n > limit)
						throw new FormatException ("Setup payload exceeds the configured limit.");

					output.Write (buffer, 0, n);
				}

				output.Position = 0;
			} finally {
				Array.Clear (buffer, 0, buffer.Length);
			}
		}

		static ArmoredInputStream OpenArmor (MemoryStream input, string label, bool allowPreamble)
		{
			var bytes = input.GetBuffer ().AsSpan (0, (int) input.Length);
			var begin = System.Text.Encoding.ASCII.GetBytes ("-----BEGIN PGP " + label + "-----");
			var end = System.Text.Encoding.ASCII.GetBytes ("-----END PGP " + label + "-----");
			int start = -1;

			for (int pos = 0; pos < bytes.Length;) {
				int length = bytes.Slice (pos).IndexOf ((byte) '\n');

				length = length < 0 ? bytes.Length - pos : length + 1;

				int last = pos + length;

				while (last > pos && bytes[last - 1] is (byte) '\r' or (byte) '\n')
					last--;

				var line = bytes.Slice (pos, last - pos);

				if (start == -1) {
					if (line.SequenceEqual (begin))
						start = pos;
					else if (!allowPreamble)
						break;
				} else if (line.SequenceEqual (end)) {
					return new ArmoredInputStream (new MemoryStream (input.GetBuffer (), start, pos + length - start, false));
				}

				pos += length;
			}

			throw new FormatException ("Missing or incomplete OpenPGP armor.");
		}

		static SymmetricKeyEncSessionPacket ReadSessionPacket (MemoryStream binary)
		{
			// Check the small SKESK packet before allowing the OpenPGP reader to select a KDF.
			var bytes = binary.GetBuffer ().AsSpan (0, (int) binary.Length);
			int index = 0;

			if (!AutocryptKey.ReadPacket (bytes, ref index, PacketTag.SymmetricKeyEncryptedSessionKey, out var session) ||
			    session.Length is not (13 or 30 or 46) || session[0] != 4 ||
			    session[1] is not ((byte) SymmetricKeyAlgorithmTag.Aes128 or (byte) SymmetricKeyAlgorithmTag.Aes256) ||
			    session[2] != S2k.SaltedAndIterated || index >= bytes.Length || bytes[index] != (0xc0 | (int) PacketTag.SymmetricEncryptedIntegrityProtected))
				throw new FormatException ("Expected salted, iterated AES encryption with integrity protection.");

			using var view = new MemoryStream (binary.GetBuffer (), 0, (int) binary.Length, false);
			using var reader = new BcpgInputStream (view);
			var skesk = (SymmetricKeyEncSessionPacket) reader.ReadPacket ();

			if (reader.ReadPacket () is not SymmetricEncIntegrityPacket { Version: 1 })
				throw new FormatException ("Expected a version 1 integrity-protected packet.");

			return skesk;
		}

		static void CheckPayloadAlgorithm (SymmetricKeyEncSessionPacket skesk, char[] chars)
		{
			var wrapped = skesk.GetSecKeyData ();

			if (wrapped is null || wrapped.Length == 0)
				return;

			// With a wrapped session key the SKESK algorithm describes only the wrapping cipher.
			// Bouncy Castle does not expose the payload algorithm it used; check the authenticated result.
			var wrappingKey = PgpUtilities.MakeKeyFromPassPhraseUtf8 (skesk.EncAlgorithm, skesk.S2k, chars);
			var cipher = CipherUtilities.GetCipher ("AES/CFB/NoPadding");
			cipher.Init (false, new ParametersWithIV (wrappingKey, new byte[16]));
			var key = cipher.DoFinal (wrapped);

			try {
				if (!(key[0] == (byte) SymmetricKeyAlgorithmTag.Aes128 && key.Length == 17 ||
				      key[0] == (byte) SymmetricKeyAlgorithmTag.Aes256 && key.Length == 33))
					throw new FormatException ("Expected an AES-128 or AES-256 setup payload.");
			} finally {
				Array.Clear (key, 0, key.Length);
			}
		}

		static void Decrypt (MemoryStream attachment, string setupCode, MemoryStream plaintext, int limit, CancellationToken cancellationToken)
		{
			using var armor = OpenArmor (attachment, "MESSAGE", true);
			using var binary = new MemoryStream ();

			ReadLimited (armor, binary, limit, cancellationToken);

			var skesk = ReadSessionPacket (binary);
			using var packets = new BcpgInputStream (binary);
			var list = new PgpEncryptedDataList (packets);

			if (list.Count != 1 || list[0] is not PgpPbeEncryptedData encrypted || !encrypted.IsIntegrityProtected ())
				throw new FormatException ("Expected one passphrase-protected session.");

			var chars = setupCode.ToCharArray ();

			try {
				using var clear = encrypted.GetDataStreamUtf8 (chars);

				ReadLimited (clear, plaintext, limit, cancellationToken);

				if (!encrypted.Verify ())
					throw new PgpException ("Setup message integrity check failed.");

				if ((int) packets.NextPacketTag () >= 0)
					throw new FormatException ("Unexpected packet after encrypted setup data.");

				CheckPayloadAlgorithm (skesk, chars);
			} finally {
				Array.Clear (chars, 0, chars.Length);
			}
		}

		static void ReadLiteral (BcpgInputStream packets, MemoryStream output, int limit, CancellationToken cancellationToken)
		{
			if (packets.NextPacketTag () != PacketTag.LiteralData)
				throw new FormatException ("Expected an OpenPGP literal data packet.");

			var literal = new PgpLiteralData (packets);

			ReadLimited (literal.GetInputStream (), output, limit, cancellationToken);

			if ((int) packets.NextPacketTag () >= 0)
				throw new FormatException ("Unexpected packet after setup literal data.");
		}

		static AutocryptSetupMessage Unpack (string address, string setupCode, char[] newPassphrase, MemoryStream attachment, int limit, CancellationToken cancellationToken)
		{
			using var plaintext = new PlaintextBuffer (Math.Min (limit, DefaultLimit));

			Decrypt (attachment, setupCode, plaintext, limit, cancellationToken);

			// Do not decompress, interpret a preference, or parse a secret key until the MDC has verified.
			using var packets = new BcpgInputStream (plaintext);
			using var text = new PlaintextBuffer (Math.Min (limit, DefaultLimit));

			if (packets.NextPacketTag () == PacketTag.CompressedData) {
				var compressed = new PgpCompressedData (packets);
				using var inflated = compressed.GetDataStream (limit);
				using var inner = new BcpgInputStream (inflated);

				ReadLiteral (inner, text, limit, cancellationToken);

				if ((int) packets.NextPacketTag () >= 0)
					throw new FormatException ("Unexpected packet after compressed setup data.");
			} else {
				ReadLiteral (packets, text, limit, cancellationToken);
			}

			using var armor = OpenArmor (text, "PRIVATE KEY BLOCK", false);
			var preference = AutocryptPreference.NoPreference;
			bool found = false;

			foreach (var header in armor.GetArmorHeaders () ?? Array.Empty<string> ()) {
				if (!header.StartsWith (PreferenceHeader + ":", StringComparison.OrdinalIgnoreCase))
					continue;

				if (found)
					throw new FormatException ("Duplicate setup encryption preference.");

				found = true;

				if (header.Substring (PreferenceHeader.Length + 1).Trim ().Equals ("mutual", StringComparison.OrdinalIgnoreCase))
					preference = AutocryptPreference.Mutual;
			}

			using var binary = new PlaintextBuffer (Math.Min (limit, DefaultLimit));

			ReadLimited (armor, binary, limit, cancellationToken);

			var bytes = binary.GetBuffer ().AsSpan (0, (int) binary.Length);
			int index = 0;

			foreach (var tag in SecretPackets) {
				if (!AutocryptKey.ReadPacket (bytes, ref index, tag, out var body) ||
				    (tag != PacketTag.UserId && (body.IsEmpty || body[0] != 4)))
					throw new FormatException ("Expected a minimal version 4 transferable secret key.");
			}

			if (index != bytes.Length)
				throw new FormatException ("Unexpected data in the transferable secret key.");

			var ring = new PgpSecretKeyRing (binary);
			var primary = ring.GetSecretKey ();
			var subkey = ring.GetSecretKeys ().Last ();

			if (primary.IsPrivateKeyEmpty || subkey.IsPrivateKeyEmpty || primary.KeyEncryptionAlgorithm != SymmetricKeyAlgorithmTag.Null ||
			    subkey.KeyEncryptionAlgorithm != SymmetricKeyAlgorithmTag.Null)
				throw new FormatException ("Expected two secret keys unlocked by the setup code.");

			var key = AutocryptKey.FromSelection (primary.PublicKey, subkey.PublicKey, primary.UserIds.Single (), null);
			var random = new SecureRandom ();
			var pass = MimeKit.Utils.CharsetUtils.UTF8.GetBytes (newPassphrase);

			try {
				foreach (var secret in new [] { primary, subkey }) {
					var protectedKey = PgpSecretKey.CopyWithNewPasswordRaw (secret, Array.Empty<byte> (), pass, SymmetricKeyAlgorithmTag.Aes256, random);
					ring = PgpSecretKeyRing.InsertSecretKey (ring, protectedKey);
				}
			} finally {
				Array.Clear (pass, 0, pass.Length);
			}

			return new AutocryptSetupMessage (address, preference, ring, key);
		}

		/// <summary>
		/// Recover the account data from a setup message after user confirmation.
		/// </summary>
		/// <remarks>
		/// Accepts a single literal payload, optionally compressed. Integrity is checked before returning any keys.
		/// </remarks>
		/// <param name="message">The purported setup message.</param>
		/// <param name="address">The expected local account address, supplied independently of the message.</param>
		/// <param name="setupCode">The out-of-band passphrase, including any hyphens.</param>
		/// <param name="newPassphrase">A nonempty local passphrase used to protect the recovered secret keys.</param>
		/// <param name="maxPayloadSize">Maximum attachment and intermediate payload length in bytes, including decompressed data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The recovered public certificate, protected secret keyring and account preference.</returns>
		/// <exception cref="ArgumentNullException">
		/// A required argument is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The address or a passphrase is invalid.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The size limit is not positive.
		/// </exception>
		/// <exception cref="FormatException">
		/// The message is malformed or too large.
		/// </exception>
		/// <exception cref="PgpException">
		/// The code, integrity check, or key validation failed.
		/// </exception>
		/// <exception cref="IOException">
		/// Reading or decoding the payload failed.
		/// </exception>
		/// <exception cref="OperationCanceledException">
		/// Recovery was cancelled.
		/// </exception>
		public static AutocryptSetupMessage Recover (MimeMessage message, string address, string setupCode, char[] newPassphrase,
			int maxPayloadSize = DefaultLimit, CancellationToken cancellationToken = default)
		{
			var content = GetContent (message, address, setupCode, newPassphrase, maxPayloadSize, out var canonical);

			cancellationToken.ThrowIfCancellationRequested ();

			using var input = content.Open ();
			using var attachment = new MemoryStream ();

			ReadLimited (input, attachment, maxPayloadSize, cancellationToken);

			return Unpack (canonical, setupCode, newPassphrase, attachment, maxPayloadSize, cancellationToken);
		}

		/// <summary>
		/// Asynchronously read a setup message and recover its account data after user confirmation.
		/// </summary>
		/// <remarks>
		/// Accepts a single literal payload, optionally compressed. Integrity is checked before returning any keys.
		/// </remarks>
		/// <param name="message">The purported setup message.</param>
		/// <param name="address">The expected local account address, supplied independently of the message.</param>
		/// <param name="setupCode">The out-of-band passphrase, including any hyphens.</param>
		/// <param name="newPassphrase">A nonempty local passphrase used to protect the recovered secret keys.</param>
		/// <param name="maxPayloadSize">Maximum attachment and intermediate payload length in bytes, including decompressed data.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A task yielding the recovered public certificate, protected secret keyring and account preference without importing any keys.</returns>
		/// <exception cref="ArgumentNullException">
		/// A required argument is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The address or a passphrase is invalid.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The size limit is not positive.
		/// </exception>
		/// <exception cref="FormatException">
		/// The message is malformed or too large.
		/// </exception>
		/// <exception cref="PgpException">
		/// The code, integrity check, or key validation failed.
		/// </exception>
		/// <exception cref="IOException">
		/// Reading or decoding the payload failed.
		/// </exception>
		/// <exception cref="OperationCanceledException">
		/// Recovery was cancelled.
		/// </exception>
		public static async Task<AutocryptSetupMessage> RecoverAsync (MimeMessage message, string address, string setupCode, char[] newPassphrase,
			int maxPayloadSize = DefaultLimit, CancellationToken cancellationToken = default)
		{
			var content = GetContent (message, address, setupCode, newPassphrase, maxPayloadSize, out var canonical);

			cancellationToken.ThrowIfCancellationRequested ();

			using var input = content.Open ();
			using var attachment = new MemoryStream ();
			var buffer = new byte[4096];
			int n;

			while ((n = await input.ReadAsync (buffer, 0, buffer.Length, cancellationToken).ConfigureAwait (false)) > 0) {
				if (attachment.Length + n > maxPayloadSize)
					throw new FormatException ("Setup payload exceeds the configured limit.");

				attachment.Write (buffer, 0, n);
			}

			return Unpack (canonical, setupCode, newPassphrase, attachment, maxPayloadSize, cancellationToken);
		}
	}
}
