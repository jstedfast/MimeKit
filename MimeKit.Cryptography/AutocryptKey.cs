//
// AutocryptKey.cs
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
using System.Diagnostics.CodeAnalysis;

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;

using MimeKit.Utils;

namespace MimeKit.Cryptography {
	/// <summary>
	/// A verified minimal Autocrypt Level 1 public key.
	/// </summary>
	/// <remarks>
	/// Verification establishes that the primary key certified the user ID and bound the encryption
	/// subkey. It does not authenticate a mailbox, import the key, or consult a local revocation store.
	/// </remarks>
	public sealed class AutocryptKey
	{
		static readonly PacketTag[] Packets = [PacketTag.PublicKey, PacketTag.UserId, PacketTag.Signature, PacketTag.PublicSubkey, PacketTag.Signature];
		const int MaxKeyLength = 10 * 1024 * 3 / 4;

		readonly DateTime validFrom, validUntil;

		AutocryptKey (byte[] keyData, PgpPublicKey primary, PgpPublicKey encryption, string userId, PgpSignature certification, PgpSignature binding)
		{
			KeyData = keyData;
			PrimaryKey = primary;
			EncryptionKey = encryption;
			UserId = userId;

			validFrom = new [] {
				primary.CreationTime,
				encryption.CreationTime,
				certification.CreationTime,
				binding.CreationTime
			}.Max ();

			validUntil = new [] {
				Expiration (primary, certification),
				Expiration (encryption, binding)
			}.Min ();
		}

		static DateTime Expiration (PgpPublicKey key, PgpSignature signature)
		{
			var packets = signature.GetHashedSubPackets ();
			long seconds = packets.GetKeyExpirationTime ();
			var end = seconds == 0 ? DateTime.MaxValue : key.CreationTime.AddSeconds (seconds);

			seconds = packets.GetSignatureExpirationTime ();

			if (seconds != 0 && signature.CreationTime.AddSeconds (seconds) < end)
				end = signature.CreationTime.AddSeconds (seconds);

			return end;
		}

		/// <summary>
		/// Get the binary certificate as advertised.
		/// </summary>
		/// <remarks>
		/// Applications can persist these bytes and restore the validated key with <see cref="TryParse"/>.
		/// </remarks>
		/// <value>The binary transferable public key.</value>
		public ReadOnlyMemory<byte> KeyData { get; }

		/// <summary>
		/// Get the signing-capable primary key.
		/// </summary>
		/// <remarks>
		/// Its fingerprint identifies the key independently of the advertised address.
		/// </remarks>
		/// <value>The primary public key.</value>
		public PgpPublicKey PrimaryKey { get; }

		/// <summary>
		/// Get the certified encryption subkey.
		/// </summary>
		/// <remarks>
		/// Use the selected subkey rather than looking up an encryption key by mailbox.
		/// </remarks>
		/// <value>The bound encryption subkey.</value>
		public PgpPublicKey EncryptionKey { get; }

		/// <summary>
		/// Get the certified user ID.
		/// </summary>
		/// <remarks>
		/// The user ID is decorative and need not match the advertised address.
		/// </remarks>
		/// <value>The user ID in the transferable public key.</value>
		public string UserId { get; }

		/// <summary>
		/// Check the validity period of the advertised keys and signatures.
		/// </summary>
		/// <remarks>
		/// An expired key can still be parsed. This check uses only the supplied advertisement;
		/// the caller must also take account of any independently known revocations.
		/// </remarks>
		/// <param name="time">The instant at which to evaluate the keys and signatures.</param>
		/// <returns><see langword="true"/> if both keys and their signatures have been created and none has expired;
		/// otherwise, <see langword="false"/>.</returns>
		public bool IsUsableAt (DateTimeOffset time)
		{
			var utc = time.UtcDateTime;

			return utc >= validFrom && utc < validUntil;
		}

		// Check definite packet lengths against the actual input before asking the OpenPGP
		// reader to interpret them. In particular it must not skip trust or marker packets.
		internal static bool ReadPacket (ReadOnlySpan<byte> data, ref int index, PacketTag expected, out ReadOnlySpan<byte> body)
		{
			body = default;

			if (index >= data.Length)
				return false;

			int tag = data[index++], octets;
			long length = 0;

			if ((tag & 0x80) == 0)
				return false;

			if ((tag & 0x40) == 0) {
				// Old-format header (RFC 4880, section 4.2.1): bits 5..2 hold the tag.
				// The low two bits select a 1-, 2-, or 4-octet length, or 3 for indeterminate.
				if (((tag >> 2) & 15) != (int) expected || (tag & 3) == 3)
					return false;

				octets = 1 << (tag & 3);
			} else {
				// New-format header (section 4.2.2): the low six bits hold the tag.
				// The first length octet encodes the size directly (0..191), starts a
				// two-octet length (192..223), or introduces a four-octet size (255).
				if ((tag & 63) != (int) expected || index == data.Length)
					return false;

				int first = data[index++];

				if (first < 192) {
					length = first;
					octets = 0;
				} else if (first < 224) {
					if (index == data.Length)
						return false;

					length = ((first - 192) << 8) + data[index++] + 192;
					octets = 0;
				} else if (first == 255) {
					octets = 4;
				} else {
					// 224..254 describe partial body chunks, not a definite packet length.
					return false;
				}
			}

			if (octets > data.Length - index)
				return false;

			while (octets-- > 0)
				length = (length << 8) | data[index++];

			if (length > data.Length - index)
				return false;

			body = data.Slice (index, (int) length);
			index += (int) length;

			return true;
		}

		static bool SupportedKey (PgpPublicKey key, bool primary)
		{
			switch (key.Algorithm) {
			case PublicKeyAlgorithmTag.RsaGeneral:
			case PublicKeyAlgorithmTag.RsaSign when primary:
			case PublicKeyAlgorithmTag.RsaEncrypt when !primary:
				return key.BitStrength >= 2048 && key.BitStrength <= 8192;
			case PublicKeyAlgorithmTag.EdDsa_Legacy when primary:
				return key.PublicKeyPacket.Key is EdDsaPublicBcpgKey ed && ed.CurveOid.Id == "1.3.6.1.4.1.11591.15.1";
			case PublicKeyAlgorithmTag.ECDH when !primary:
				return key.PublicKeyPacket.Key is ECDHPublicBcpgKey ec && ec.CurveOid.Id == "1.3.6.1.4.1.3029.1.5.1";
			default:
				return false;
			}
		}

		static bool SupportedSignature (PgpSignature sig, PgpPublicKey primary, int flags)
		{
			if (sig.KeyAlgorithm != primary.Algorithm || sig.HashAlgorithm is not (HashAlgorithmTag.Sha256 or HashAlgorithmTag.Sha384 or HashAlgorithmTag.Sha512))
				return false;

			var packets = sig.GetHashedSubPackets ();

			if (packets == null || !packets.HasSignatureCreationTime () || !packets.IsExportable ())
				return false;

			foreach (var tag in packets.GetCriticalTags ()) {
				if (tag is not (SignatureSubpacketTag.CreationTime or SignatureSubpacketTag.ExpireTime or SignatureSubpacketTag.KeyExpireTime or SignatureSubpacketTag.KeyFlags or SignatureSubpacketTag.Exportable))
					return false;
			}

			return !packets.HasSubpacket (SignatureSubpacketTag.KeyFlags) || (packets.GetKeyFlags () & flags) != 0;
		}

		/// <summary>
		/// Try to validate the decoded key data of a direct or gossip advertisement.
		/// </summary>
		/// <remarks>
		/// Accepts binary version 4 certificates with legacy Ed25519/Cv25519 or RSA keys
		/// (2048 to 8192 bits) and SHA-256, SHA-384, or SHA-512 certifications.
		/// This operation neither reads nor modifies a keyring.
		/// </remarks>
		/// <param name="keyData">The binary OpenPGP certificate from the advertisement, after Base64 decoding.</param>
		/// <param name="key">The validated key on success, or <see langword="null"/> on failure.</param>
		/// <returns><see langword="true"/> if the certificate matches the supported profile and both signatures verify;
		/// otherwise, <see langword="false"/>. Use <see cref="IsUsableAt"/> to check its validity period.</returns>
		public static bool TryParse (ReadOnlyMemory<byte> keyData, [NotNullWhen (true)] out AutocryptKey? key)
		{
			key = null;

			if (keyData.Length == 0 || keyData.Length > MaxKeyLength)
				return false;

			var bytes = keyData.ToArray ();
			int index = 0;

			foreach (var tag in Packets) {
				if (!ReadPacket (bytes, ref index, tag, out var body) ||
				    (tag != PacketTag.UserId && (body.IsEmpty || body[0] != 4)))
					return false;
			}

			if (index != bytes.Length)
				return false;

			try {
				var ring = new PgpPublicKeyRing (bytes);
				var primary = ring.GetPublicKey ();
				var encryption = ring.GetPublicKeys ().Last ();

				if (!SupportedKey (primary, true) || !SupportedKey (encryption, false))
					return false;

				var userId = CharsetUtils.UTF8.GetString (primary.GetRawUserIds ().Single ());
				var certification = primary.GetSignatures ().Single ();
				var binding = encryption.GetSignatures ().Single ();

				if (!certification.IsCertification () || binding.SignatureType != PgpSignature.SubkeyBinding ||
				    !SupportedSignature (certification, primary, PgpKeyFlags.CanSign) ||
				    !SupportedSignature (binding, primary, PgpKeyFlags.CanEncryptCommunications))
					return false;

				encryption.GetKey ();
				certification.InitVerify (primary);
				binding.InitVerify (primary);

				if (!certification.VerifyCertification (userId, primary) || !binding.VerifyCertification (primary, encryption))
					return false;

				key = new AutocryptKey (bytes, primary, encryption, userId, certification, binding);

				return true;
			} catch (Exception ex) when (ex is IOException or PgpException or CryptoException or ArgumentException or
				InvalidOperationException or ArithmeticException or IndexOutOfRangeException) {
				return false;
			}
		}

		/// <summary>
		/// Export a minimal advertisement for an explicitly selected local certificate, user ID, and subkey.
		/// </summary>
		/// <remarks>
		/// The result contains binary public packets only. The selected keys and certifications must
		/// be usable at the supplied time. Revoked keys or identities and certificates with direct-key
		/// signatures are refused rather than silently dropping restrictions.
		/// </remarks>
		/// <param name="keyRing">The public keyring containing the selected primary key and encryption subkey.</param>
		/// <param name="userId">The certified user ID to include in the advertisement.</param>
		/// <param name="encryptionKeyFingerprint">The full binary fingerprint of the encryption subkey, not its key ID.</param>
		/// <param name="time">The instant at which the selected keys and signatures must be usable.</param>
		/// <returns>The minimal binary transferable public key, suitable for use as Autocrypt key data without further decoding.</returns>
		/// <exception cref="ArgumentException">
		/// The selected user ID or subkey does not belong to the ring.
		/// </exception>
		/// <exception cref="PgpException">
		/// The selection cannot produce a usable minimal advertisement.
		/// </exception>
		public static byte[] Export (PgpPublicKeyRing keyRing, string userId, byte[] encryptionKeyFingerprint, DateTimeOffset time)
		{
			if (keyRing is null)
				throw new ArgumentNullException (nameof (keyRing));

			if (userId is null)
				throw new ArgumentNullException (nameof (userId));

			if (encryptionKeyFingerprint is null)
				throw new ArgumentNullException (nameof (encryptionKeyFingerprint));

			var primary = keyRing.GetPublicKey ();
			var encryption = keyRing.GetPublicKey (encryptionKeyFingerprint);

			if (encryption is null || encryption.IsMasterKey)
				throw new ArgumentException ("Expected the fingerprint of a subkey in the selected keyring.", nameof (encryptionKeyFingerprint));

			return FromSelection (primary, encryption, userId, time).KeyData.ToArray ();
		}

		internal static AutocryptKey FromSelection (PgpPublicKey primary, PgpPublicKey encryption, string userId, DateTimeOffset? time)
		{
			var signatures = primary.GetSignaturesForId (userId) ?? throw new ArgumentException ("User ID not found in the selected keyring.", nameof (userId));

			if (primary.HasRevocation () || encryption.HasRevocation () || primary.GetSignaturesOfType (PgpSignature.DirectKey).Any () ||
			    signatures.Any (sig => sig.SignatureType == PgpSignature.CertificationRevocation))
				throw new PgpException ("Cannot strip revocations or direct-key signatures from an Autocrypt advertisement.");

			var certification = signatures.Where (sig => sig.IsCertification () && sig.KeyId == primary.KeyId).OrderByDescending (sig => sig.CreationTime).FirstOrDefault ();
			var binding = encryption.GetSignaturesOfType (PgpSignature.SubkeyBinding).OrderByDescending (sig => sig.CreationTime).FirstOrDefault ();

			if (certification is null || binding is null)
				throw new PgpException ("Missing self-certification or subkey binding.");

			using var stream = new MemoryStream ();
			using var output = new BcpgOutputStream (stream);

			primary.PublicKeyPacket.Encode (output);

			new UserIdPacket (userId).Encode (output);

			certification.Encode (output, true);

			// A freshly generated secret ring can expose the subkey as a PublicKeyPacket until reloaded.
			var packet = encryption.PublicKeyPacket;
			new PublicSubkeyPacket (packet.VersionByte, packet.Algorithm, encryption.CreationTime, packet.Key).Encode (output);
			binding.Encode (output, true);
			output.Flush ();

			var bytes = stream.ToArray ();

			if (!TryParse (bytes, out var key) || (time.HasValue && !key.IsUsableAt (time.Value)))
				throw new PgpException ("The selected keys cannot produce a usable Autocrypt Level 1 advertisement.");

			return key;
		}
	}
}
