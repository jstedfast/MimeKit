//
// AutocryptTests.cs
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

using System.Text;

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography {
	[TestFixture]
	public class AutocryptTests
	{
		const string Advertisement = "addr=ALICE@example.org; _optional=yes; prefer-encrypt=mutual; keydata=YW\r\n\tJj";

		internal static PgpSecretKeyRing CreateLocalKeys (string address)
		{
			var random = new SecureRandom ();
			var generator = GeneratorUtilities.GetKeyPairGenerator ("RSA");
			generator.Init (new KeyGenerationParameters (random, 2048));
			var date = DateTime.UtcNow;
			var primary = new PgpKeyPair (PublicKeyAlgorithmTag.RsaSign, generator.GenerateKeyPair (), date);
			var subkey = new PgpKeyPair (PublicKeyAlgorithmTag.RsaEncrypt, generator.GenerateKeyPair (), date);
			var ring = new PgpKeyRingGenerator (PgpSignature.PositiveCertification, primary, address,
				SymmetricKeyAlgorithmTag.Aes128, HashAlgorithmTag.Sha256, "no.secret".ToCharArray (), true, null, null, random);
			ring.AddSubKey (subkey, HashAlgorithmTag.Sha256);
			return ring.GenerateSecretKeyRing ();
		}

		internal static readonly Lazy<PgpSecretKeyRing> SenderKeys = new Lazy<PgpSecretKeyRing> (() => CreateLocalKeys ("sender@example.org"));
		static readonly Lazy<PgpSecretKeyRing> VisibleKeys = new Lazy<PgpSecretKeyRing> (() => CreateLocalKeys ("visible@example.org"));
		static readonly Lazy<PgpSecretKeyRing> BlindKeys = new Lazy<PgpSecretKeyRing> (() => CreateLocalKeys ("blind@example.org"));

		internal static AutocryptAccount Bind (PgpSecretKeyRing ring, DateTimeOffset time)
		{
			var address = ring.GetPublicKey ().GetUserIds ().Single ();
			return new AutocryptAccount (address, AutocryptPreference.Mutual, ring, address, ring.GetPublicKeys ().Last ().GetFingerprint (), time);
		}

		sealed class RecipientContext : DummyOpenPgpContext
		{
			readonly string password;

			public RecipientContext (PgpSecretKeyRing secret, AutocryptKey sender, string password = "no.secret") : base (Path.Combine (Path.GetTempPath (), Guid.NewGuid ().ToString ()))
			{
				this.password = password;
				SecretKeyRingBundle = PgpSecretKeyRingBundle.AddSecretKeyRing (SecretKeyRingBundle, secret);
				PublicKeyRingBundle = PgpPublicKeyRingBundle.AddPublicKeyRing (PublicKeyRingBundle, new PgpPublicKeyRing (sender.KeyData.ToArray ()));
			}

			protected override string GetPasswordForKey (PgpSecretKey key) => password;
		}

		// Alice's published Ed25519/Cv25519 example from the Autocrypt Level 1.1 specification.
		const string Alice = "mDMEXEcE6RYJKwYBBAHaRw8BAQdArjWwk3FAqyiFbFBKT4TzXcVBqPTB3gmzlC/Ub7O1u120F2F" +
			"saWNlQGF1dG9jcnlwdC5leGFtcGxliJYEExYIAD4WIQTrhbtfozp14V6UTmPyMVUMT0fjjgUCXE" +
			"cE6QIbAwUJA8JnAAULCQgHAgYVCgkICwIEFgIDAQIeAQIXgAAKCRDyMVUMT0fjjkqLAP9frlijw" +
			"BJvA+HFnqCZcYIVxlyXzS5Gi5gMTpp37K73jgD/VbKYhkwk9iu689OYH4K7q7LbmdeaJ+RX88Y/" +
			"ad9hZwy4OARcRwTpEgorBgEEAZdVAQUBAQdAQv8GIa2rSTzgqbXCpDDYMiKRVitCsy203x3sE9+" +
			"eviIDAQgHiHgEGBYIACAWIQTrhbtfozp14V6UTmPyMVUMT0fjjgUCXEcE6QIbDAAKCRDyMVUMT0" +
			"fjjlnQAQDFHUs6TIcxrNTtEZFjUFm1M0PJ1Dng/cDW4xN80fsn0QEA22Kr7VkCjeAEC08VSTeV+" +
			"QFsmz55/lntWkwYWhmvOgE=";

		// Non-expiring Ed25519/Cv25519 public fixture generated with GnuPG on a fixed clock.
		const string NonExpiring = "mDMEXCqtgBYJKwYBBAHaRw8BAQdAS+PQ5Fw4TALHXWiaOOCfiMzhQR+PN5/BqhaA7/32k8q0FXJl" +
			"Y2lwaWVudEBleGFtcGxlLm9yZ4iQBBMWCgA4FiEEg4fiGo3F2nsGr6cSZ178X/zjnnsFAlwqrYAC" +
			"GwMFCwkIBwIGFQoJCAsCBBYCAwECHgECF4AACgkQZ178X/zjnntGlwD/XkWXEeSxyd38n8Bd5mCy" +
			"8qm7zC82vG3eVdou1JK0+N4BANk6qGH4muj2MBjFNTTZ1rBdquIWPgF5fMx/DRKx+A8CuDgEXCqt" +
			"gBIKKwYBBAGXVQEFAQEHQHuv5gxxalbRBI85tf/1wz+wvGAVj5UFCCEiOiIhvLktAwEIB4h4BBgW" +
			"CgAgFiEEg4fiGo3F2nsGr6cSZ178X/zjnnsFAlwqrYACGwwACgkQZ178X/zjnnv27wD/WdSRCgZJ" +
			"sey3Q1MKHFi+phkifPNg6s+xImCRcUoKnacBAJIrrk2jJDiaZ6sJlnjOqm5uNvmXgiKjDUrBcKfTaM4E";

		[Test]
		public void TestLevelOneKey ()
		{
			var data = Convert.FromBase64String (Alice);
			var direct = new Autocrypt ("different@example.org", data);

			Assert.That (AutocryptKey.TryParse (direct.KeyData, out var key), Is.True);
			Assert.That (key.UserId, Is.EqualTo ("alice@autocrypt.example"));

			var at = new DateTimeOffset (2019, 1, 22, 12, 56, 25, TimeSpan.FromHours (1));

			Assert.That (key.IsUsableAt (at), Is.True);
			Assert.That (key.IsUsableAt (at.AddYears (-1)), Is.False);
			Assert.That (key.IsUsableAt (at.AddYears (10)), Is.False);

			var ring = new PgpPublicKeyRing (data);
			var exported = AutocryptKey.Export (ring, key.UserId, key.EncryptionKey.GetFingerprint (), at);

			Assert.That (AutocryptKey.TryParse (new AutocryptGossip (direct.Address, exported).KeyData, out var reparsed), Is.True);
			Assert.That (reparsed.PrimaryKey.GetFingerprint (), Is.EqualTo (key.PrimaryKey.GetFingerprint ()));
			Assert.That (reparsed.EncryptionKey.GetFingerprint (), Is.EqualTo (key.EncryptionKey.GetFingerprint ()));
			Assert.Throws<PgpException> (() => AutocryptKey.Export (ring, key.UserId, key.EncryptionKey.GetFingerprint (), at.AddYears (10)));
			Assert.That (AutocryptKey.TryParse (data.Concat (data).ToArray (), out _), Is.False);
			Assert.That (AutocryptKey.TryParse (data.AsMemory (0, data.Length - 1), out _), Is.False);

			data[data.Length - 1] ^= 1;

			Assert.That (AutocryptKey.TryParse (data, out _), Is.False, "invalid subkey binding signature");
		}

		[Test]
		public void TestDirectPeerUpdates ()
		{
			var empty = new AutocryptPeerState ("Alice@example.org");
			var date = new DateTimeOffset (2019, 2, 1, 0, 0, 0, TimeSpan.Zero);
			var seen = empty.ApplyDirect (new AutocryptObservation (empty.Address, date.AddDays (2), null));
			var data = Convert.FromBase64String (Alice);
			var ad = new Autocrypt (empty.Address, data, AutocryptPreference.Mutual);
			var observation = new AutocryptObservation (ad.Address, date, ad);
			var state = seen.ApplyDirect (observation);

			Assert.That (empty.LastSeen, Is.Null);
			Assert.That (seen.DirectKey, Is.Null);
			Assert.That (state.LastSeen, Is.EqualTo (date.AddDays (2)));
			Assert.That (state.DirectTimestamp, Is.EqualTo (date));
			Assert.That (state.DirectKey.KeyData.ToArray (), Is.EqualTo (data));
			Assert.That (state.Preference, Is.EqualTo (AutocryptPreference.Mutual));
			Assert.That (state.ApplyDirect (null), Is.SameAs (state));
			Assert.That (state.ApplyDirect (observation), Is.SameAs (state));

			var changed = new Autocrypt (empty.Address, data);
			foreach (int days in new [] { -1, 0 })
				Assert.That (state.ApplyDirect (new AutocryptObservation (ad.Address, date.AddDays (days), changed)), Is.SameAs (state));

			var invalid = new Autocrypt (empty.Address, [1]);
			var later = state.ApplyDirect (new AutocryptObservation (ad.Address, date.AddDays (3), invalid));

			Assert.That (later.LastSeen, Is.EqualTo (date.AddDays (3)));
			Assert.That (later.DirectKey, Is.SameAs (state.DirectKey));
			Assert.That (later.DirectTimestamp, Is.EqualTo (date));
			Assert.That (later.Preference, Is.EqualTo (AutocryptPreference.Mutual));

			var updated = later.ApplyDirect (new AutocryptObservation (ad.Address, date.AddDays (1), changed));

			Assert.That (updated.DirectTimestamp, Is.EqualTo (date.AddDays (1)));
			Assert.That (updated.Preference, Is.EqualTo (AutocryptPreference.NoPreference));
			Assert.That (updated.LastSeen, Is.EqualTo (later.LastSeen));
		}

		[Test]
		public void TestIndependentGossipUpdates ()
		{
			var empty = new AutocryptPeerState ("alice@example.org");
			var data = Convert.FromBase64String (Alice);
			var gossip = new AutocryptGossip (empty.Address, data);
			var date = new DateTimeOffset (2019, 2, 1, 0, 0, 0, TimeSpan.Zero);
			var indirect = empty.ApplyGossip (gossip, date);

			Assert.That (indirect.GossipTimestamp, Is.EqualTo (date));
			Assert.That (indirect.GossipKey.KeyData.ToArray (), Is.EqualTo (data));
			Assert.That (indirect.LastSeen, Is.Null);
			Assert.That (indirect.DirectKey, Is.Null);
			Assert.That (indirect.ApplyGossip (gossip, date), Is.SameAs (indirect));
			Assert.That (indirect.ApplyGossip (gossip, date.AddDays (-1)), Is.SameAs (indirect));
			Assert.That (indirect.ApplyGossip (new AutocryptGossip (empty.Address, [1]), date.AddDays (1)), Is.SameAs (indirect));

			var ad = new Autocrypt (empty.Address, data, AutocryptPreference.Mutual);
			var direct = indirect.ApplyDirect (new AutocryptObservation (ad.Address, date.AddDays (-1), ad));
			Assert.That (direct.GossipKey, Is.SameAs (indirect.GossipKey));
			Assert.That (direct.GossipTimestamp, Is.EqualTo (date));

			var restored = new AutocryptPeerState (direct.Address, direct.LastSeen, direct.DirectKey,
				direct.DirectTimestamp, direct.Preference, direct.GossipKey, direct.GossipTimestamp);
			var updated = restored.ApplyGossip (gossip, date.AddDays (1));
			Assert.That (updated.GossipTimestamp, Is.EqualTo (date.AddDays (1)));
			Assert.That (updated.LastSeen, Is.EqualTo (date.AddDays (-1)));
			Assert.That (updated.DirectTimestamp, Is.EqualTo (direct.DirectTimestamp));
			Assert.That (updated.DirectKey, Is.SameAs (direct.DirectKey));
			Assert.That (updated.Preference, Is.EqualTo (AutocryptPreference.Mutual));
		}

		[TestCase (false, false, false, false, AutocryptRecommendationLevel.Available)]
		[TestCase (false, true, false, false, AutocryptRecommendationLevel.Available)]
		[TestCase (true, false, false, false, AutocryptRecommendationLevel.Available)]
		[TestCase (true, true, false, false, AutocryptRecommendationLevel.Encrypt)]
		[TestCase (true, true, true, false, AutocryptRecommendationLevel.Discourage)]
		[TestCase (false, false, true, true, AutocryptRecommendationLevel.Encrypt)]
		public void TestDirectRecommendation (bool localMutual, bool peerMutual, bool stale, bool reply, AutocryptRecommendationLevel expected)
		{
			Assert.That (AutocryptKey.TryParse (Convert.FromBase64String (Alice), out var key), Is.True);

			var date = new DateTimeOffset (2019, 2, 1, 0, 0, 0, TimeSpan.Zero);
			var lastSeen = date.AddDays (35).AddTicks (stale ? 1 : 0);
			var peer = new AutocryptPeerState ("alice@example.org", lastSeen, key, date,
				peerMutual ? AutocryptPreference.Mutual : AutocryptPreference.NoPreference, key, lastSeen);
			var peers = new Dictionary<string, AutocryptPeerState> { { peer.Address, peer } };
			var result = AutocryptRecommendation.Evaluate (localMutual ? AutocryptPreference.Mutual : AutocryptPreference.NoPreference,
				[MailboxAddress.Parse ("Alice@example.org")], peers, lastSeen.AddMonths (2), reply);
			var target = result.Recipients[peer.Address];
			var reason = stale ? AutocryptRecommendationReason.StaleDirectKey : AutocryptRecommendationReason.DirectKey;

			if (localMutual && peerMutual && !stale)
				reason |= AutocryptRecommendationReason.MutualPreference;

			if (reply)
				reason |= AutocryptRecommendationReason.EncryptedReply;

			Assert.That (result.Level, Is.EqualTo (expected));
			Assert.That (target.Level, Is.EqualTo (expected));
			Assert.That (target.Reason, Is.EqualTo (reason));
			Assert.That (target.Key, Is.SameAs (key));
		}

		[TestCase (true, false)]
		[TestCase (false, false)]
		[TestCase (true, true)]
		public void TestGossipRecommendation (bool expired, bool reply)
		{
			Assert.That (AutocryptKey.TryParse (Convert.FromBase64String (Alice), out var direct), Is.True);
			Assert.That (AutocryptKey.TryParse (Convert.FromBase64String (NonExpiring), out var gossip), Is.True);

			var date = new DateTimeOffset (2019, 2, 1, 0, 0, 0, TimeSpan.Zero);
			var peer = new AutocryptPeerState ("alice@example.org", date, direct, date, AutocryptPreference.Mutual, gossip, date);
			var peers = new Dictionary<string, AutocryptPeerState> { { peer.Address, peer } };
			var result = AutocryptRecommendation.Evaluate (AutocryptPreference.Mutual, [MailboxAddress.Parse (peer.Address)],
				peers, expired ? date.AddYears (10) : date, reply, expired ? null : key => key != direct);
			var target = result.Recipients[peer.Address];

			Assert.That (target.Key, Is.SameAs (gossip));
			Assert.That (target.Reason, Is.EqualTo (reply ? AutocryptRecommendationReason.GossipKey | AutocryptRecommendationReason.EncryptedReply : AutocryptRecommendationReason.GossipKey));
			Assert.That (result.Level, Is.EqualTo (reply ? AutocryptRecommendationLevel.Encrypt : AutocryptRecommendationLevel.Discourage));
		}

		[Test]
		public void TestEnvelopeRecommendation ()
		{
			Assert.That (AutocryptKey.TryParse (Convert.FromBase64String (Alice), out var key), Is.True);

			var date = new DateTimeOffset (2019, 2, 1, 0, 0, 0, TimeSpan.Zero);
			var alice = new AutocryptPeerState ("alice@example.org", date, key, date, AutocryptPreference.Mutual);
			var bob = new AutocryptPeerState ("bob@example.org", date, key, date);
			var carol = new AutocryptPeerState ("carol@example.org", gossipKey: key, gossipTimestamp: date);
			var peers = new [] { alice, bob, carol }.ToDictionary (peer => peer.Address);
			using var message = new MimeMessage ();

			AutocryptRecommendation Evaluate (bool reply) => AutocryptRecommendation.Evaluate (AutocryptPreference.Mutual,
				message.To.Mailboxes.Concat (message.Cc.Mailboxes).Concat (message.Bcc.Mailboxes), peers, date, reply);

			Assert.That (Evaluate (false).Level, Is.EqualTo (AutocryptRecommendationLevel.Disable));

			message.To.Add (MailboxAddress.Parse (alice.Address));
			message.Cc.Add (MailboxAddress.Parse (bob.Address));

			Assert.That (Evaluate (false).Level, Is.EqualTo (AutocryptRecommendationLevel.Available));

			message.Bcc.Add (MailboxAddress.Parse ("Alice@example.org"));
			message.Bcc.Add (MailboxAddress.Parse (carol.Address));

			Assert.That (Evaluate (false).Level, Is.EqualTo (AutocryptRecommendationLevel.Discourage));
			Assert.That (Evaluate (true).Level, Is.EqualTo (AutocryptRecommendationLevel.Encrypt));

			message.Bcc.Insert (0, MailboxAddress.Parse ("unknown@example.org"));
			var disabled = Evaluate (true);

			Assert.That (disabled.Level, Is.EqualTo (AutocryptRecommendationLevel.Disable));
			Assert.That (disabled.Recipients.Count, Is.EqualTo (4));
			Assert.That (disabled.Recipients[carol.Address].Key, Is.SameAs (key));
			Assert.That (disabled.Recipients["unknown@example.org"].Reason, Is.EqualTo (AutocryptRecommendationReason.NoUsableKey));
			Assert.That (disabled.Recipients["unknown@example.org"].Key, Is.Null);
		}

		[Test]
		public void TestPublishedSetupMessage ()
		{
			using var message = MimeMessage.Load (Path.Combine (TestHelper.ProjectDir, "TestData", "openpgp", "autocrypt-setup.eml"));
			const string address = "alice@autocrypt.example";
			const string exampleCode = "1742-0185-6197-1303-7016-8412-3581-4441-0597";
			var password = "new local protection".ToCharArray ();
			var recovered = AutocryptSetupMessage.Recover (message, address, exampleCode, password);
			Assert.That (recovered.Preference, Is.EqualTo (AutocryptPreference.Mutual));
			Assert.That (recovered.Key.KeyData.ToArray (), Is.EqualTo (Convert.FromBase64String (Alice)));
			foreach (var secret in recovered.KeyRing.GetSecretKeys ()) {
				Assert.That (secret.KeyEncryptionAlgorithm, Is.EqualTo (SymmetricKeyAlgorithmTag.Aes256));
				Assert.That (secret.ExtractPrivateKeyUtf8 (password), Is.Not.Null);
			}
			Assert.Throws<FormatException> (() => AutocryptSetupMessage.Recover (message, "other@example.org", exampleCode, password));
			Assert.Throws<FormatException> (() => AutocryptSetupMessage.Recover (message, address, exampleCode, password, 512));
			Assert.Throws<OperationCanceledException> (() => AutocryptSetupMessage.Recover (message, address, exampleCode, password, cancellationToken: new CancellationToken (true)));
			message.Headers[HeaderId.AutocryptSetupMessage] = "v2";
			Assert.Throws<FormatException> (() => AutocryptSetupMessage.Recover (message, address, exampleCode, password));
			message.Headers[HeaderId.AutocryptSetupMessage] = "v1";
			message.Headers.Add (HeaderId.AutocryptSetupMessage, "v1");
			Assert.Throws<FormatException> (() => AutocryptSetupMessage.Recover (message, address, exampleCode, password));
		}

		[Test]
		public async Task TestSetupMessageRecoveryAsync ()
		{
			var ring = SenderKeys.Value;
			var time = DateTimeOffset.UtcNow.AddMinutes (1);
			var account = Bind (ring, time);
			using var original = new TextPart { Text = "before transfer" };
			using var oldMail = new MimeMessage { Body = original };
			oldMail.From.Add (MailboxAddress.Parse (account.Address));
			oldMail.To.Add (MailboxAddress.Parse (account.Address));
			using (var sender = new RecipientContext (ring, account.Key))
				account.SignAndEncrypt (sender, oldMail, oldMail.GetRecipients (), new Dictionary<string, AutocryptPeerState> (), time);

			var local = "no.secret".ToCharArray ();
			using var setup = AutocryptSetupMessage.Create (account, local, out var code);
			Assert.That (code, Does.Match (@"^[0-9]{4}(-[0-9]{4}){8}$"));
			using var wire = new MemoryStream ();
			await setup.WriteToAsync (wire);
			Assert.That (Encoding.ASCII.GetString (wire.ToArray ()), Does.Not.Contain (code));
			wire.Position = 0;
			using var received = await MimeMessage.LoadAsync (wire);
			var recovered = await AutocryptSetupMessage.RecoverAsync (received, account.Address, code, local);
			using var restored = new RecipientContext (recovered.KeyRing, recovered.Key);
			using var decrypted = ((MultipartEncrypted) oldMail.Body).Decrypt (restored, out var signatures);
			Assert.That (((TextPart) decrypted).Text.TrimEnd (), Is.EqualTo ("before transfer"));
			Assert.That (signatures.Single ().Verify (), Is.True);

			Assert.CatchAsync<PgpException> (() => AutocryptSetupMessage.RecoverAsync (received, account.Address, "incorrect", local));
			var part = (MimePart) ((Multipart) received.Body)[1];
			using var input = part.Content.Open ();
			using var armor = new ArmoredInputStream (input);
			using var binary = new MemoryStream ();
			armor.CopyTo (binary);
			binary.GetBuffer ()[(int) binary.Length - 1] ^= 1;
			var damaged = new MemoryStream ();
			using (var output = new ArmoredOutputStream (damaged))
				binary.WriteTo (output);
			damaged.Position = 0;
			part.Content = new MimeContent (damaged);
			Assert.ThrowsAsync<PgpException> (() => AutocryptSetupMessage.RecoverAsync (received, account.Address, code, local));
		}

		[Test]
		public void TestGenerateAccount ()
		{
			const string password = "local-\u00e9-\u96ea";

			var account = AutocryptAccount.Generate ("Sender@example.org", AutocryptPreference.Mutual, password.ToCharArray (), out var ring);

			Assert.That (account.Key.UserId, Is.EqualTo ("sender@example.org"));
			Assert.That (account.Key.PrimaryKey.Algorithm, Is.EqualTo (PublicKeyAlgorithmTag.EdDsa_Legacy));
			Assert.That (account.Key.EncryptionKey.Algorithm, Is.EqualTo (PublicKeyAlgorithmTag.ECDH));

			foreach (var secret in ring.GetSecretKeys ()) {
				Assert.That (secret.KeyEncryptionAlgorithm, Is.EqualTo (SymmetricKeyAlgorithmTag.Aes256));
				Assert.Catch<PgpException> (() => secret.ExtractPrivateKeyUtf8 ("incorrect".ToCharArray ()));
			}

			using var original = new TextPart { Text = "first device" };
			using var message = new MimeMessage { Body = original };

			message.From.Add (MailboxAddress.Parse (account.Address));
			message.To.Add (MailboxAddress.Parse (account.Address));

			var time = DateTimeOffset.UtcNow;

			using (var sender = new RecipientContext (ring, account.Key, password))
				account.SignAndEncrypt (sender, message, message.GetRecipients (), new Dictionary<string, AutocryptPeerState> (), time);

			var ad = message.GetAutocryptObservation (time).Advertisement;

			Assert.That (ad.KeyData.ToArray (), Is.EqualTo (account.Key.KeyData.ToArray ()));

			using var setup = AutocryptSetupMessage.Create (account, password.ToCharArray (), out var code);
			var recovered = AutocryptSetupMessage.Recover (setup, account.Address, code, "new-\u00e9".ToCharArray ());
			using var receiver = new RecipientContext (recovered.KeyRing, recovered.Key, "new-\u00e9");
			using var decrypted = ((MultipartEncrypted) message.Body).Decrypt (receiver, out var signatures);

			Assert.That (signatures.Single ().Verify (), Is.True);
			Assert.That (((TextPart) decrypted).Text.TrimEnd (), Is.EqualTo ("first device"));
		}

		[Test]
		public void TestAdvertise ()
		{
			var time = DateTimeOffset.UtcNow.AddMinutes (1);
			var account = Bind (SenderKeys.Value, time);
			using var message = new MimeMessage ();
			message.From.Add (MailboxAddress.Parse (account.Address));
			message.Body = new TextPart { Text = "cleartext" };
			account.Advertise (message, time);
			account.Advertise (message, time);
			var value = message.GetAutocryptObservation (time).Advertisement;
			Assert.That (value.KeyData.ToArray (), Is.EqualTo (account.Key.KeyData.ToArray ()));
			Assert.That (value.Preference, Is.EqualTo (account.Preference));
			Assert.That (message.TextBody, Is.EqualTo ("cleartext"));
			message.From.Clear ();
			message.From.Add (MailboxAddress.Parse ("different@example.org"));
			Assert.Throws<InvalidOperationException> (() => account.Advertise (message, time));
		}

		[Test]
		public void TestOutgoingEncryption () => ExerciseOutgoingEncryption (false).GetAwaiter ().GetResult ();

		[Test]
		public Task TestOutgoingEncryptionAsync () => ExerciseOutgoingEncryption (true);

		static async Task ExerciseOutgoingEncryption (bool doAsync)
		{
			var senderRing = SenderKeys.Value;
			var blindRing = BlindKeys.Value;
			var visibleRing = VisibleKeys.Value;
			var time = DateTimeOffset.UtcNow.AddMinutes (1);
			var sender = Bind (senderRing, time);
			var hidden = Bind (blindRing, time);
			using var senderContext = new RecipientContext (senderRing, sender.Key);
			using var blindContext = new RecipientContext (blindRing, sender.Key);
			using var visibleContext = new RecipientContext (visibleRing, sender.Key);
			using var message = new MimeMessage ();
			using var original = new TextPart { Text = "private content" };
			message.Body = original;
			message.From.Add (MailboxAddress.Parse (sender.Address));
			message.To.Add (MailboxAddress.Parse ("visible@example.org"));
			message.Bcc.Add (MailboxAddress.Parse (hidden.Address));
			var envelope = message.GetRecipients ();
			var visibleKey = Bind (visibleRing, time).Key;
			var visible = new AutocryptPeerState ("visible@example.org", time, visibleKey, time);
			var blind = new AutocryptPeerState (hidden.Address, time, hidden.Key, time);
			var peers = new [] { visible, blind }.ToDictionary (state => state.Address);
			Assert.That (AutocryptRecommendation.Evaluate (sender.Preference, envelope, peers, time, false).Level, Is.EqualTo (AutocryptRecommendationLevel.Available));

			peers.Remove (hidden.Address);
			if (doAsync)
				Assert.ThrowsAsync<InvalidOperationException> (() => sender.SignAndEncryptAsync (senderContext, message, envelope, peers, time));
			else
				Assert.Throws<InvalidOperationException> (() => sender.SignAndEncrypt (senderContext, message, envelope, peers, time));
			Assert.That (message.Body, Is.SameAs (original));
			Assert.That (message.Headers.Contains (HeaderId.Autocrypt), Is.False);
			Assert.That (message.Bcc.Count, Is.EqualTo (1));
			Assert.That (original.Headers.Contains (HeaderId.AutocryptGossip), Is.False);

			peers.Add (hidden.Address, blind);
			if (doAsync)
				Assert.ThrowsAsync<InvalidOperationException> (() => sender.SignAndEncryptAsync (senderContext, message, envelope, peers, time, key => key != sender.Key));
			else
				Assert.Throws<InvalidOperationException> (() => sender.SignAndEncrypt (senderContext, message, envelope, peers, time, key => key != sender.Key));
			Assert.That (message.Body, Is.SameAs (original));

			if (doAsync)
				await sender.SignAndEncryptAsync (senderContext, message, envelope, peers, time);
			else
				sender.SignAndEncrypt (senderContext, message, envelope, peers, time);

			Assert.That (message.GetAutocryptObservation (time).Advertisement.KeyData.ToArray (), Is.EqualTo (sender.Key.KeyData.ToArray ()));
			Assert.That (message.Headers.Contains (HeaderId.Bcc), Is.False);
			Assert.That (message.Headers.Contains (HeaderId.AutocryptGossip), Is.False);
			var encrypted = (MultipartEncrypted) message.Body;
			foreach (var context in new [] { senderContext, visibleContext, blindContext }) {
				using var decrypted = encrypted.Decrypt (context, out var signatures);
				Assert.That (signatures.Single ().Verify (), Is.True);
				Assert.That (((TextPart) decrypted).Text.TrimEnd (), Is.EqualTo ("private content"));
				var fields = decrypted.Headers.Where (header => header.Id == HeaderId.AutocryptGossip).ToArray ();
				var gossip = AutocryptGossip.Parse (fields.Single ().RawValue);
				Assert.That (gossip.Address, Is.EqualTo (visible.Address));
				Assert.That (gossip.KeyData.ToArray (), Is.EqualTo (visibleKey.KeyData.ToArray ()));
			}
		}

		[Test]
		public void TestWireValues ()
		{
			var bytes = Encoding.ASCII.GetBytes ("x" + Advertisement + "x");
			var direct = Autocrypt.Parse (bytes, 1, bytes.Length - 2);

			Assert.That (direct.Address, Is.EqualTo ("alice@example.org"));
			Assert.That (direct.Preference, Is.EqualTo (AutocryptPreference.Mutual));
			Assert.That (direct.KeyData.ToArray (), Is.EqualTo ("abc"u8.ToArray ()));
			Assert.That (Autocrypt.Parse (direct.ToHeader ().RawValue).Preference, Is.EqualTo (direct.Preference));

			var key = new byte[512];
			var gossip = new AutocryptGossip ("Alice@b\u00fccher.example", key);

			// Mutate the caller's array to verify that gossip kept its own copy of the key.
			key[0] = 123;
			var header = gossip.ToHeader ();
			
			Assert.That (header.Id, Is.EqualTo (HeaderId.AutocryptGossip));
			Assert.That (header.RawValue.Count (b => b == '\n'), Is.GreaterThan (1));
			
			var parsed = AutocryptGossip.Parse (header.RawValue);
			
			Assert.That (parsed.KeyData.ToArray (), Is.EqualTo (new byte[512]));
			Assert.That (parsed.Address, Is.EqualTo ("alice@xn--bcher-kva.example"));
			Assert.That (Autocrypt.Parse (Advertisement.Replace ("mutual", "other")).Preference, Is.EqualTo (AutocryptPreference.NoPreference));
		}

		[TestCase ("addr=alice; keydata=YQ==")]
		[TestCase ("addr=Alice <alice@example.org>; keydata=YQ==")]
		[TestCase ("addr=a@b; ADDR=a@b; keydata=YQ==")]
		[TestCase ("addr=a@b; unknown=x; keydata=YQ==")]
		[TestCase ("addr=a@b; keydata=YW!j")]
		[TestCase ("addr=a@b; keydata=")]
		[TestCase ("addr=a@b;\r\nkeydata=YQ==")]
		[TestCase ("=?us-ascii?q?addr=3Da@b=3B_keydata=3DYQ=3D=3D?=")]
		public void TestInvalidAdvertisement (string text)
		{
			Assert.That (Autocrypt.TryParse (text, out _), Is.False);
			Assert.That (AutocryptGossip.TryParse (Encoding.UTF8.GetBytes (text), out _), Is.False);
		}

		[Test]
		public void TestWireLimit ()
		{
			var exact = Advertisement.PadRight (10 * 1024 - "Autocrypt:".Length);
			
			Assert.That (Autocrypt.TryParse (exact, out _), Is.True);
			Assert.That (Autocrypt.TryParse (exact + " ", out _), Is.False);
			Assert.That (AutocryptGossip.TryParse (exact, out _), Is.False);
			Assert.Throws<FormatException> (() => new Autocrypt ("alice@example.org", new byte[7600]).ToHeader ());
		}

		[Test]
		public void TestSenderObservation ()
		{
			using var message = new MimeMessage ();
			var received = new DateTimeOffset (2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
			
			Assert.That (message.GetAutocryptObservation (received), Is.Null);
			
			message.From.Add (MailboxAddress.Parse ("Alice <Alice@example.org>"));
			message.Headers.RemoveAll (HeaderId.Date);
			
			var observed = message.GetAutocryptObservation (received);
			
			Assert.That (observed.EffectiveDate, Is.EqualTo (received));
			Assert.That (observed.Advertisement, Is.Null);
			
			message.Date = received.AddDays (1);
			message.Headers.Add (new Autocrypt ("other@example.org", [1]).ToHeader ());
			message.Headers.Add (Autocrypt.Parse (Advertisement).ToHeader ());
			observed = message.GetAutocryptObservation (received);
			
			Assert.That (observed.EffectiveDate, Is.EqualTo (received));
			Assert.That (observed.Advertisement.Address, Is.EqualTo (observed.Address));
			
			message.Date = received.AddDays (-1);
			message.Headers.Add (Autocrypt.Parse (Advertisement).ToHeader ());
			observed = message.GetAutocryptObservation (received);
			
			Assert.That (observed.Advertisement, Is.Null);
			Assert.That (observed.EffectiveDate, Is.EqualTo (message.Date));
			
			message.Body = new Multipart ("report");
			
			Assert.That (message.GetAutocryptObservation (received), Is.Null);
		}

		[Test]
		public void TestGossipScope ()
		{
			using var message = new MimeMessage ();
			using var inner = new Multipart ("mixed") { new TextPart () };

			message.To.Add (MailboxAddress.Parse ("to@example.org"));
			message.Cc.Add (MailboxAddress.Parse ("cc@example.org"));
			message.ReplyTo.Add (MailboxAddress.Parse ("reply@example.org"));
			message.Bcc.Add (MailboxAddress.Parse ("hidden@example.org"));

			foreach (var local in new [] { "to", "cc", "CC", "reply", "hidden" })
				inner.Headers.Add (new AutocryptGossip (local + "@example.org", [1]).ToHeader ());

			var duplicate = new AutocryptGossip ("to@example.org", [1]);

			inner[0].Headers.Add (duplicate.ToHeader ());
			message.Headers.Add (duplicate.ToHeader ());

			Assert.That (message.GetAutocryptGossip (inner).Select (g => g.Address), Is.EquivalentTo (new [] { "to@example.org", "reply@example.org" }));
		}
	}
}
