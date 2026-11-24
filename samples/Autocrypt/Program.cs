//
// Program.cs
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
using System.Collections.Generic;
using System.Security.Cryptography;

using MimeKit;
using MimeKit.Cryptography;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace AutocryptSample {
	sealed class SampleContext : GnuPGContext
	{
		readonly string password = Convert.ToBase64String (RandomNumberGenerator.GetBytes (32));

		// Start with an unused path instead of reading the user's keyrings. Never save the bundles.
		public SampleContext (string address) : base (Path.Combine (Path.GetTempPath (), Guid.NewGuid ().ToString ()))
		{
			Account = AutocryptAccount.Generate (address, AutocryptPreference.Mutual, password.ToCharArray (), out var ring);
			SecretKeyRingBundle = PgpSecretKeyRingBundle.AddSecretKeyRing (SecretKeyRingBundle, ring);
		}

		public AutocryptAccount Account { get; }
		public Dictionary<string, AutocryptPeerState> Peers { get; } = new (StringComparer.Ordinal);

		public void Observe (MimeMessage message, DateTimeOffset receivedAt)
		{
			var observation = message.GetAutocryptObservation (receivedAt);

			if (observation is null)
				return;

			if (!Peers.TryGetValue (observation.Address, out var peer))
				peer = new AutocryptPeerState (observation.Address);

			Peers[observation.Address] = peer.ApplyDirect (observation);
		}

		public void AddVerificationKey (AutocryptKey key)
		{
			var ring = new PgpPublicKeyRing (key.KeyData.ToArray ());

			PublicKeyRingBundle = PgpPublicKeyRingBundle.AddPublicKeyRing (PublicKeyRingBundle, ring);
		}

		protected override string GetPasswordForKey (PgpSecretKey key) => password;
	}

	static class Program
	{
		static MimeMessage Deliver (MimeMessage outgoing)
		{
			using var wire = new MemoryStream ();

			outgoing.WriteTo (wire);
			wire.Position = 0;

			return MimeMessage.Load (wire);
		}

		static void Main ()
		{
			using var alice = new SampleContext ("alice@example.org");
			using var bob = new SampleContext ("bob@example.org");
			var aliceAddress = new MailboxAddress ("Alice", alice.Account.Address);
			var bobAddress = new MailboxAddress ("Bob", bob.Account.Address);
			using var hello = new MimeMessage ([aliceAddress], [bobAddress], "Hello", new TextPart ("plain") { Text = "Hello, Bob!" });

			alice.Account.Advertise (hello, DateTimeOffset.UtcNow);

			using var inbox = Deliver (hello);
			bob.Observe (inbox, DateTimeOffset.UtcNow);

			var peer = bob.Peers[alice.Account.Address];
			var aliceKey = peer.DirectKey ?? throw new InvalidDataException ("No usable advertisement from Alice.");
			var fingerprint = Convert.ToHexString (aliceKey.PrimaryKey.GetFingerprint ());

			Console.WriteLine ($"Bob received an Autocrypt advertisement from {peer.Address}: preference={peer.Preference}, key fingerprint={fingerprint}");

			MailboxAddress[] envelope = [aliceAddress];
			var now = DateTimeOffset.UtcNow;
			var recommendation = AutocryptRecommendation.Evaluate (bob.Account.Preference, envelope, bob.Peers, now, encryptedReply: false);

			Console.WriteLine ($"Bob's recommendation for his reply to Alice: {recommendation.Level}");

			// Bob's policy for this reply: encrypt when recommended, otherwise do not send.
			if (recommendation.Level != AutocryptRecommendationLevel.Encrypt)
				throw new InvalidOperationException ("Bob did not send the reply.");

			using var cleartext = new TextPart ("plain") { Text = "Hello, Alice!" };
			using var reply = new MimeMessage ([bobAddress], envelope, "Re: " + inbox.Subject, cleartext) { InReplyTo = inbox.MessageId };

			bob.Account.SignAndEncrypt (bob, reply, envelope, bob.Peers, now);

			using var received = Deliver (reply);
			alice.Observe (received, DateTimeOffset.UtcNow);

			var bobKey = alice.Peers[bob.Account.Address].DirectKey ?? throw new InvalidDataException ("No usable advertisement from Bob.");
			alice.AddVerificationKey (bobKey);

			using var decrypted = ((MultipartEncrypted) received.Body!).Decrypt (alice, out var signatures);

			if (signatures?.Count != 1 || !signatures[0].Verify ())
				throw new InvalidDataException ("The reply did not verify against the advertised key.");

			Console.WriteLine ($"Alice decrypted: {((TextPart) decrypted).Text}");
			Console.WriteLine ("The signature matches the advertised key; this does not verify Bob's identity.");
		}
	}
}
