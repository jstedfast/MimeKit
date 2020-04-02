﻿//
// ArcVerifierTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 Xamarin Inc. (www.xamarin.com)
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
using System.Text;

using NUnit.Framework;

using MimeKit;
using MimeKit.Cryptography;

namespace UnitTests.Cryptography
{
	[TestFixture]
	public class ArcVerifierTests
	{
		[Test]
		public void TestArgumentExceptions ()
		{
			var locator = new DkimPublicKeyLocator ();
			var verifier = new ArcVerifier (locator);
			var message = new MimeMessage ();

			Assert.Throws<ArgumentNullException> (() => new ArcVerifier (null));
			Assert.Throws<ArgumentNullException> (() => new ArcHeaderValidationResult (null));
			Assert.Throws<ArgumentNullException> (() => new ArcHeaderValidationResult (null, ArcSignatureValidationResult.Fail));

			Assert.Throws<ArgumentNullException> (() => verifier.Verify (null));
			Assert.Throws<ArgumentNullException> (async () => await verifier.VerifyAsync (null));

			Assert.Throws<ArgumentNullException> (() => verifier.Verify (null, message));
			Assert.Throws<ArgumentNullException> (() => verifier.Verify (FormatOptions.Default, null));
			Assert.Throws<ArgumentNullException> (async () => await verifier.VerifyAsync (null, message));
			Assert.Throws<ArgumentNullException> (async () => await verifier.VerifyAsync (FormatOptions.Default, null));
		}

		[Test]
		public void TestArcHeaderValidationResult ()
		{
			var header = new Header (HeaderId.ArcMessageSignature, "i=1; a=rsa-sha256; ...");
			var result = new ArcHeaderValidationResult (header, ArcSignatureValidationResult.Fail);

			Assert.AreEqual (header, result.Header, "Header");
			Assert.AreEqual (ArcSignatureValidationResult.Fail, result.Signature);
		}

		[Test]
		public void TestArcValidationResult ()
		{
			var header = new Header (HeaderId.ArcMessageSignature, "i=1; a=rsa-sha256; ...");
			var ams = new ArcHeaderValidationResult (header, ArcSignatureValidationResult.Pass);

			header = new Header (HeaderId.ArcSeal, "i=1; a=rsa-sha256; ...");
			var seal = new ArcHeaderValidationResult (header, ArcSignatureValidationResult.Pass);

			var result = new ArcValidationResult (ArcSignatureValidationResult.Pass, ams, new[] { seal });

			Assert.AreEqual (ArcSignatureValidationResult.Pass, result.Chain, "Chain");
			Assert.IsNotNull (result.MessageSignature, "MessageSignature != null");
			Assert.AreEqual (HeaderId.ArcMessageSignature, result.MessageSignature.Header.Id, "MessageSignature.Header.Id");
			Assert.AreEqual (ArcSignatureValidationResult.Pass, result.MessageSignature.Signature, "MessageSignature.Signature");
			Assert.IsNotNull (result.Seals, "Seals != null");
			Assert.AreEqual (1, result.Seals.Length, "Seals.Length");
			Assert.AreEqual (HeaderId.ArcSeal, result.Seals[0].Header.Id, "Seals[0].Header.Id");
			Assert.AreEqual (ArcSignatureValidationResult.Pass, result.Seals[0].Signature, "Seals[0].Signature");
		}

		[Test]
		public void TestGetArcHeaderSetsBrokenAAR ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			MimeMessage message;

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (input), false)) {
				message = MimeMessage.Load (stream);
				ArcHeaderSet[] sets;
				Header broken, aar;
				int index, count;

				// first, get a copy of the original ARC-Authentication-Results header
				index = message.Headers.IndexOf (HeaderId.ArcAuthenticationResults);
				aar = message.Headers[index];

				// create and set completely broken AAR:
				broken = new Header (HeaderId.ArcAuthenticationResults, "this should be unparsable...");
				message.Headers[index] = broken;

				Assert.AreEqual (ArcSignatureValidationResult.Fail, ArcVerifier.GetArcHeaderSets (message, false, out sets, out count), "Broken AAR");

				try {
					ArcVerifier.GetArcHeaderSets (message, true, out sets, out count);
					Assert.Fail ("Broken AAR should throwOnError");
				} catch (FormatException) {
				} catch {
					Assert.Fail ("Broken AAR throwOnError unexpected exception");
				}

				// set an AAR that is missing the instance value
				broken.Value = aar.Value.Replace ("i=1; ", "");

				Assert.AreEqual (ArcSignatureValidationResult.Fail, ArcVerifier.GetArcHeaderSets (message, false, out sets, out count), "AAR missing i=1");

				try {
					ArcVerifier.GetArcHeaderSets (message, true, out sets, out count);
					Assert.Fail ("AAR missing i=1 should throwOnError");
				} catch (FormatException) {
				} catch {
					Assert.Fail ("AAR missing i=1 throwOnError unexpected exception");
				}

				// set an AAR that has an invalid instance value
				broken.Value = aar.Value.Replace ("i=1; ", "i=0; ");

				Assert.AreEqual (ArcSignatureValidationResult.Fail, ArcVerifier.GetArcHeaderSets (message, false, out sets, out count), "AAR i=0");

				try {
					ArcVerifier.GetArcHeaderSets (message, true, out sets, out count);
					Assert.Fail ("AAR i=0 should throwOnError");
				} catch (FormatException) {
				} catch {
					Assert.Fail ("AAR i=0 throwOnError unexpected exception");
				}

				// remove the AAR completely
				message.Headers.RemoveAt (index);

				Assert.AreEqual (ArcSignatureValidationResult.Fail, ArcVerifier.GetArcHeaderSets (message, false, out sets, out count), "Missing AAR");

				try {
					ArcVerifier.GetArcHeaderSets (message, true, out sets, out count);
					Assert.Fail ("Missing AAR should throwOnError");
				} catch (FormatException) {
				} catch {
					Assert.Fail ("Missing AAR throwOnError unexpected exception");
				}
			}
		}

		[Test]
		public void TestGetArcHeaderSetsBrokenAMS ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			MimeMessage message;

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (input), false)) {
				message = MimeMessage.Load (stream);
				ArcHeaderSet[] sets;
				Header broken, ams;
				int index, count;

				// first, get a copy of the original ARC-Message-Signature header
				index = message.Headers.IndexOf (HeaderId.ArcMessageSignature);
				ams = message.Headers[index];

				// create and set completely broken AMS:
				broken = new Header (HeaderId.ArcMessageSignature, "this should be unparsable...");
				message.Headers[index] = broken;

				Assert.AreEqual (ArcSignatureValidationResult.Fail, ArcVerifier.GetArcHeaderSets (message, false, out sets, out count), "Broken AMS");

				try {
					ArcVerifier.GetArcHeaderSets (message, true, out sets, out count);
					Assert.Fail ("Broken AMS should throwOnError");
				} catch (FormatException) {
				} catch {
					Assert.Fail ("Broken AMS throwOnError unexpected exception");
				}

				// set an AMS that is missing the instance value
				broken.Value = ams.Value.Replace ("i=1; ", "");

				Assert.AreEqual (ArcSignatureValidationResult.Fail, ArcVerifier.GetArcHeaderSets (message, false, out sets, out count), "AMS missing i=1");

				try {
					ArcVerifier.GetArcHeaderSets (message, true, out sets, out count);
					Assert.Fail ("AMS missing i=1 should throwOnError");
				} catch (FormatException) {
				} catch {
					Assert.Fail ("AMS missing i=1 throwOnError unexpected exception");
				}

				// set an AMS that has an invalid instance value
				broken.Value = ams.Value.Replace ("i=1; ", "i=0; ");

				Assert.AreEqual (ArcSignatureValidationResult.Fail, ArcVerifier.GetArcHeaderSets (message, false, out sets, out count), "AMS i=0");

				try {
					ArcVerifier.GetArcHeaderSets (message, true, out sets, out count);
					Assert.Fail ("AMS i=0 should throwOnError");
				} catch (FormatException) {
				} catch {
					Assert.Fail ("AMS i=0 throwOnError unexpected exception");
				}

				// remove the AMS completely
				message.Headers.RemoveAt (index);

				Assert.AreEqual (ArcSignatureValidationResult.Fail, ArcVerifier.GetArcHeaderSets (message, false, out sets, out count), "Missing AMS");

				try {
					ArcVerifier.GetArcHeaderSets (message, true, out sets, out count);
					Assert.Fail ("Missing AMS should throwOnError");
				} catch (FormatException) {
				} catch {
					Assert.Fail ("Missing AMS throwOnError unexpected exception");
				}
			}
		}

		[Test]
		public void TestGetArcHeaderSetsBrokenAS ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			MimeMessage message;

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (input), false)) {
				message = MimeMessage.Load (stream);
				ArcHeaderSet[] sets;
				Header broken, seal;
				int index, count;

				// first, get a copy of the original ARC-Seal header
				index = message.Headers.IndexOf (HeaderId.ArcSeal);
				seal = message.Headers[index];

				// create and set completely broken AS:
				broken = new Header (HeaderId.ArcSeal, "this should be unparsable...");
				message.Headers[index] = broken;

				Assert.AreEqual (ArcSignatureValidationResult.Fail, ArcVerifier.GetArcHeaderSets (message, false, out sets, out count), "Broken AS");

				try {
					ArcVerifier.GetArcHeaderSets (message, true, out sets, out count);
					Assert.Fail ("Broken AS should throwOnError");
				} catch (FormatException) {
				} catch {
					Assert.Fail ("Broken AS throwOnError unexpected exception");
				}

				// set an AS that is missing the instance value
				broken.Value = seal.Value.Replace ("i=1; ", "");

				Assert.AreEqual (ArcSignatureValidationResult.Fail, ArcVerifier.GetArcHeaderSets (message, false, out sets, out count), "AS missing i=1");

				try {
					ArcVerifier.GetArcHeaderSets (message, true, out sets, out count);
					Assert.Fail ("AS missing i=1 should throwOnError");
				} catch (FormatException) {
				} catch {
					Assert.Fail ("AS missing i=1 throwOnError unexpected exception");
				}

				// set an AS that has an invalid instance value
				broken.Value = seal.Value.Replace ("i=1; ", "i=0; ");

				Assert.AreEqual (ArcSignatureValidationResult.Fail, ArcVerifier.GetArcHeaderSets (message, false, out sets, out count), "AS i=0");

				try {
					ArcVerifier.GetArcHeaderSets (message, true, out sets, out count);
					Assert.Fail ("AS i=0 should throwOnError");
				} catch (FormatException) {
				} catch {
					Assert.Fail ("AS i=0 throwOnError unexpected exception");
				}

				// remove the AS completely
				message.Headers.RemoveAt (index);

				Assert.AreEqual (ArcSignatureValidationResult.Fail, ArcVerifier.GetArcHeaderSets (message, false, out sets, out count), "Missing AS");

				try {
					ArcVerifier.GetArcHeaderSets (message, true, out sets, out count);
					Assert.Fail ("Missing AS should throwOnError");
				} catch (FormatException) {
				} catch {
					Assert.Fail ("Missing AS throwOnError unexpected exception");
				}
			}
		}

		[Test]
		public void TestGetArcHeaderSetsMissingSet ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=IAqZJ5HwfNxxsrn9R4ayQgiu9RibPKEUVevbt7XFTkSh1baJ533D2Z6IZ2NaBreUhDBb2e
    K9Gtcv+eyUhWkD8VTmE6fq/F8CDIK3ScIiJykF8hNL1wpa/mGwWWwBnkozIJGAbTAAX7AgnH
    knAehnSW99TeU0lmib0XmOt4TN3sY=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=2cDGNznUmp4YSSThCe9nrQIH2Gpd5qPFw3OU8sWFzZgEQ5UZtaVQifVUXUrsSyEzjro3Ul
    YPPDx+C1K+LbKRlOZ06il4ws2zlPafsrx1piKsKSCUq0KjFs01hYCDBa3tfdyITSfoWu2HHY
    pCjrhPMPH1jruIdBV/5Gk2Fvy+mW8=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			MimeMessage message;

			using (var stream = new MemoryStream (Encoding.ASCII.GetBytes (input), false)) {
				message = MimeMessage.Load (stream);
				ArcHeaderSet[] sets;
				int index, count;

				// Remove the oldest ARC set
				index = message.Headers.LastIndexOf (HeaderId.ArcSeal);
				message.Headers.RemoveAt (index);

				index = message.Headers.LastIndexOf (HeaderId.ArcMessageSignature);
				message.Headers.RemoveAt (index);

				index = message.Headers.LastIndexOf (HeaderId.ArcAuthenticationResults);
				message.Headers.RemoveAt (index);

				Assert.AreEqual (ArcSignatureValidationResult.Fail, ArcVerifier.GetArcHeaderSets (message, false, out sets, out count), "Missing set");

				try {
					ArcVerifier.GetArcHeaderSets (message, true, out sets, out count);
					Assert.Fail ("Missing set should throwOnError");
				} catch (FormatException) {
				} catch {
					Assert.Fail ("Missing set throwOnError unexpected exception");
				}
			}
		}

		static void Validate (string description, string input, DkimPublicKeyLocator locator, ArcSignatureValidationResult expected)
		{
			if (string.IsNullOrEmpty (input)) {
				Assert.AreEqual (expected, ArcSignatureValidationResult.None, description);
				return;
			}

			var buffer = Encoding.UTF8.GetBytes (input);

			using (var stream = new MemoryStream (buffer, false)) {
				var verifier = new ArcVerifier (locator);
				var message = MimeMessage.Load (stream);
				ArcValidationResult result;

				// Test Verify
				result = verifier.Verify (message);
				Assert.AreEqual (expected, result.Chain, description);

				// Test VerifyAsync
				result = verifier.VerifyAsync (message).GetAwaiter ().GetResult ();
				Assert.AreEqual (expected, result.Chain, description);
			}
		}

		#region Chain Validation

		[Test]
		public void cv_empty ()
		{
			const string input = @"";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("empty message", input, locator, ArcSignatureValidationResult.None);
		}

		[Test]
		public void cv_no_headers ()
		{
			const string input = @"
Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("message with no headers", input, locator, ArcSignatureValidationResult.None);
		}

		[Test]
		public void cv_no_body ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("message with no body", input, locator, ArcSignatureValidationResult.None);
		}

		[Test]
		public void cv_base1 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("base test message1", input, locator, ArcSignatureValidationResult.None);
		}

		[Test]
		public void cv_base2 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Tue, 3 Jan 2017 12:31:41 -080
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("base test message2", input, locator, ArcSignatureValidationResult.None);
		}

		[Test]
		public void cv_pass_i1_1 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("passing message i=1 base1", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void cv_pass_i1_2 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("passing message i=1 base2", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void cv_pass_i2_1 ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=IAqZJ5HwfNxxsrn9R4ayQgiu9RibPKEUVevbt7XFTkSh1baJ533D2Z6IZ2NaBreUhDBb2e
    K9Gtcv+eyUhWkD8VTmE6fq/F8CDIK3ScIiJykF8hNL1wpa/mGwWWwBnkozIJGAbTAAX7AgnH
    knAehnSW99TeU0lmib0XmOt4TN3sY=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=2cDGNznUmp4YSSThCe9nrQIH2Gpd5qPFw3OU8sWFzZgEQ5UZtaVQifVUXUrsSyEzjro3Ul
    YPPDx+C1K+LbKRlOZ06il4ws2zlPafsrx1piKsKSCUq0KjFs01hYCDBa3tfdyITSfoWu2HHY
    pCjrhPMPH1jruIdBV/5Gk2Fvy+mW8=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("passing message i=2 base1", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void cv_pass_i2_2 ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=CiZp+ZloBeWiIyjY+Eq0lKt20KQDF3QIJNw7+/jdjtQ1XTSMhHsli7H/ocIXsiU/kLF5pn
    pABQiZPvAWfCaEcCA9lyb/7i3q2i72GLdK1vdrdD2nIM5e7L3u/5Z56SJdKTu46SyoFQve9b
    Cp7qoQB9/TUTxxvkDoapsSjDCDqZ0=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=A2OCip1Cf9z6X7ML9/bRajnToeCD3H7IkP7YqmSKqDtn8Yu8oaJdwP0lZfCTjX++Qas9nj
    tGWMojFpj8Wd2rzdyMXwUWF3xlcFBD2gApO9xbehIASIF4lFQMyP6D80LjsjdtpstgwGZl9P
    y6WTyD1Kw/bNPZadxvNeDg3LVcQpo=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("passing message i=2 base2", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void cv_pass_i2_1_ams1_invalid ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=TKrRvbWMQbGHGQSIMlStVE/2vKjY5E8kVSSXJmEyOL1OjexNoNSfnYpjklVVaG9O4Hsbc5
    ZEbLSkpDIOKlnb+XlLNL5xvYntBNamjtH0e9et3DpyPQUIZ2gyZsuFwPzN/m96BU5iv+blU0
    XLjgABkBLyfaFlEPsQ0SUs8gZjM7Y=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=ZFJ9p6LT/KerwWPXp+WzznYAbt+cF4R/3l5nfSeNZSi38hhtpLkoJi/2R1FXdnnznKa3mQ
    gk3WCEaxLNmHEl90TDHGL5vhViJ57OSS0X7ZgyKzZrNbVSDYj416pFR356MMXdaV8WVq9mD4
    yATaaWBkVi8eOh287Qbqj0k93H/+U=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: Gene Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("i=2 base1 modified from header, ams(1) no longer valid", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void cv_pass_i3_1 ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=EcI6PD1XFx7uTngsG2JZQzTaAyhIGafcKJO+aTb4+PV1QKFHrLLrSv++W872urw2WnEsWJ
    Hs+YPSVbRGJXbHp4rSM0VasdFb6lf2UUJf8Lxy17f3CzqQQz5CGMO++75t+cManzaOmnjq/Z
    gGaqK7euJwWo6hzF3pNZYdTJ6JZOo=; cv=pass; d=example.org; i=3; s=dummy;
    t=12347
ARC-Message-Signature: a=rsa-sha256;
    b=FEp53xrAEL1qQfytTEmR+Lp/ZpX4bXQvtj/peHauDtix/tlBN2v841lm72vOjK6WfqGB4E
    X/9vRfV7ZiSRMFvXAWlnDKw5wzoZFyQ3xebnvqraYnq9OA1CrDIFQVLqqGIaZrcZ+fTXt7Kp
    TBMU/BzIBERwfWXqBG1DqZGYXrHFw=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=3; s=dummy; t=12347
ARC-Authentication-Results: i=3; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 15:38:12 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=IAqZJ5HwfNxxsrn9R4ayQgiu9RibPKEUVevbt7XFTkSh1baJ533D2Z6IZ2NaBreUhDBb2e
    K9Gtcv+eyUhWkD8VTmE6fq/F8CDIK3ScIiJykF8hNL1wpa/mGwWWwBnkozIJGAbTAAX7AgnH
    knAehnSW99TeU0lmib0XmOt4TN3sY=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=2cDGNznUmp4YSSThCe9nrQIH2Gpd5qPFw3OU8sWFzZgEQ5UZtaVQifVUXUrsSyEzjro3Ul
    YPPDx+C1K+LbKRlOZ06il4ws2zlPafsrx1piKsKSCUq0KjFs01hYCDBa3tfdyITSfoWu2HHY
    pCjrhPMPH1jruIdBV/5Gk2Fvy+mW8=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("passing message i=3 base1", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void cv_pass_i4_1 ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=lf+5z/QtA3SZRY8Bz60La2HmprfbE1Q2vUmiP/4Db3Ma3KqpZmnS9/d/wDr3dXgC0TpT4X
    +bUAQ0iK2hWXtvr9bfs0x7s2skzdyeX/Zzvin2NE/a0uhxIOMfO6Fqcr8YNT9hKQa4qHJxE/
    Qpr0aO4ypt+tGkNHf+4gCLoDWss0M=; cv=pass; d=example.org; i=4; s=dummy;
    t=12348
ARC-Message-Signature: a=rsa-sha256;
    b=aqlCYqV7+A1U0pg3Fc3WayaB8cQOH2QBEbwqzJ82ghIERQnLAPMXKR/LfUo27lNbLi+Hfs
    wo3ZOCJOoaC6kvHpMTmgOdq1SWBgl4WjwDhVXSarxZS40HxzF25Gi2O1jn0ke7vj1IyKceiF
    9W6deMSsxrlDqD+1Bas4XUfFeC03M=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=4; s=dummy; t=12348
ARC-Authentication-Results: i=4; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
ARC-Seal: a=rsa-sha256;
    b=EcI6PD1XFx7uTngsG2JZQzTaAyhIGafcKJO+aTb4+PV1QKFHrLLrSv++W872urw2WnEsWJ
    Hs+YPSVbRGJXbHp4rSM0VasdFb6lf2UUJf8Lxy17f3CzqQQz5CGMO++75t+cManzaOmnjq/Z
    gGaqK7euJwWo6hzF3pNZYdTJ6JZOo=; cv=pass; d=example.org; i=3; s=dummy;
    t=12347
ARC-Message-Signature: a=rsa-sha256;
    b=FEp53xrAEL1qQfytTEmR+Lp/ZpX4bXQvtj/peHauDtix/tlBN2v841lm72vOjK6WfqGB4E
    X/9vRfV7ZiSRMFvXAWlnDKw5wzoZFyQ3xebnvqraYnq9OA1CrDIFQVLqqGIaZrcZ+fTXt7Kp
    TBMU/BzIBERwfWXqBG1DqZGYXrHFw=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=3; s=dummy; t=12347
ARC-Authentication-Results: i=3; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 15:38:12 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=IAqZJ5HwfNxxsrn9R4ayQgiu9RibPKEUVevbt7XFTkSh1baJ533D2Z6IZ2NaBreUhDBb2e
    K9Gtcv+eyUhWkD8VTmE6fq/F8CDIK3ScIiJykF8hNL1wpa/mGwWWwBnkozIJGAbTAAX7AgnH
    knAehnSW99TeU0lmib0XmOt4TN3sY=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=2cDGNznUmp4YSSThCe9nrQIH2Gpd5qPFw3OU8sWFzZgEQ5UZtaVQifVUXUrsSyEzjro3Ul
    YPPDx+C1K+LbKRlOZ06il4ws2zlPafsrx1piKsKSCUq0KjFs01hYCDBa3tfdyITSfoWu2HHY
    pCjrhPMPH1jruIdBV/5Gk2Fvy+mW8=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("passing message i=4 base1", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void cv_pass_i5_1 ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=0Kw2RaoquhI2id5WxefhIq+DMaZGXa0iEtjT7oRpCpLhLxH0sofldiwSpVJMh1qZo5k7pk
    JW/uah4CWdln95BAm3AikTH7Bu0gM6To4qzCgFKulTbnvRK3Q7jT4xflPf8M4PAkw3OAN2+k
    d4dsvyOoo3ait+oNeXyFAEuZ4RoD8=; cv=pass; d=example.org; i=5; s=dummy;
    t=12349
ARC-Message-Signature: a=rsa-sha256;
    b=j50SIOsFwO/hXR//iEpwzqDIVtC4qwIdReAesDFZaTvfxzYB6TshuR7u7LqE8PjsUNz6CX
    urhvUkCOMGi2q9vQn3lqh67m3roWzIPivbUDoO0KAd9FghBI3QKbQAJe85gV7jbaTsURM9WZ
    ygbRURxwTz42PatPu9LNGo2QUwaNU=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=5; s=dummy; t=12349      
ARC-Authentication-Results: i=5; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
ARC-Seal: a=rsa-sha256;
    b=lf+5z/QtA3SZRY8Bz60La2HmprfbE1Q2vUmiP/4Db3Ma3KqpZmnS9/d/wDr3dXgC0TpT4X
    +bUAQ0iK2hWXtvr9bfs0x7s2skzdyeX/Zzvin2NE/a0uhxIOMfO6Fqcr8YNT9hKQa4qHJxE/
    Qpr0aO4ypt+tGkNHf+4gCLoDWss0M=; cv=pass; d=example.org; i=4; s=dummy;
    t=12348
ARC-Message-Signature: a=rsa-sha256;
    b=aqlCYqV7+A1U0pg3Fc3WayaB8cQOH2QBEbwqzJ82ghIERQnLAPMXKR/LfUo27lNbLi+Hfs
    wo3ZOCJOoaC6kvHpMTmgOdq1SWBgl4WjwDhVXSarxZS40HxzF25Gi2O1jn0ke7vj1IyKceiF
    9W6deMSsxrlDqD+1Bas4XUfFeC03M=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=4; s=dummy; t=12348
ARC-Authentication-Results: i=4; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
ARC-Seal: a=rsa-sha256;
    b=EcI6PD1XFx7uTngsG2JZQzTaAyhIGafcKJO+aTb4+PV1QKFHrLLrSv++W872urw2WnEsWJ
    Hs+YPSVbRGJXbHp4rSM0VasdFb6lf2UUJf8Lxy17f3CzqQQz5CGMO++75t+cManzaOmnjq/Z
    gGaqK7euJwWo6hzF3pNZYdTJ6JZOo=; cv=pass; d=example.org; i=3; s=dummy;
    t=12347
ARC-Message-Signature: a=rsa-sha256;
    b=FEp53xrAEL1qQfytTEmR+Lp/ZpX4bXQvtj/peHauDtix/tlBN2v841lm72vOjK6WfqGB4E
    X/9vRfV7ZiSRMFvXAWlnDKw5wzoZFyQ3xebnvqraYnq9OA1CrDIFQVLqqGIaZrcZ+fTXt7Kp
    TBMU/BzIBERwfWXqBG1DqZGYXrHFw=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=3; s=dummy; t=12347
ARC-Authentication-Results: i=3; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 15:38:12 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=IAqZJ5HwfNxxsrn9R4ayQgiu9RibPKEUVevbt7XFTkSh1baJ533D2Z6IZ2NaBreUhDBb2e
    K9Gtcv+eyUhWkD8VTmE6fq/F8CDIK3ScIiJykF8hNL1wpa/mGwWWwBnkozIJGAbTAAX7AgnH
    knAehnSW99TeU0lmib0XmOt4TN3sY=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=2cDGNznUmp4YSSThCe9nrQIH2Gpd5qPFw3OU8sWFzZgEQ5UZtaVQifVUXUrsSyEzjro3Ul
    YPPDx+C1K+LbKRlOZ06il4ws2zlPafsrx1piKsKSCUq0KjFs01hYCDBa3tfdyITSfoWu2HHY
    pCjrhPMPH1jruIdBV/5Gk2Fvy+mW8=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("passing message i=5 base1", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void cv_fail_i1_ams_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("failing message i=i no ams", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void cv_fail_i1_ams_invalid ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is an invalid test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("failing message i=i invalid ams", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void cv_fail_i1_as_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("failing message i=i no as", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void cv_fail_i1_as_pass ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=PhxOTCxMOzOkfccg/YXFn+e5FdMyjQK+QXNt9lYytimVUpntsBbAAtBQT5XgYQDRsM3YR+
    vBsf1oJ+kL221cv9qQWYUC3DP3xaE0nZ3vjNR1+//uZpMcTT3k6NYZnlzexAzYMoXByQkrS6
    0Om4kNir1fUo5SOlGXpXf8NRSmU70=; cv=pass; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("failing message i=i as cv=Pass", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void cv_fail_i1_as_cv_fail ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=wddf4DzBcl11ICrYWjYC78s246KGCG4D3XBmouE2PVdLr4LWqyTWTQDvZ7TWrtEDkRsmz+
    wbaMVAWdj2XgewkwQu5qxQ82D5dGiLcNQXfQDRbd8dO1+PZVWlw0wmeM7nRhNb/5tT0BvNQO
    xrrb4oEs4LIFDNYtKgTvCMyCVLzuw=; cv=fail; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("failing message i=i as cv=fail", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void cv_fail_i1_as_invalid ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=OdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("failing message i=i invalid as b=", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void cv_fail_i2_ams_na ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=IAqZJ5HwfNxxsrn9R4ayQgiu9RibPKEUVevbt7XFTkSh1baJ533D2Z6IZ2NaBreUhDBb2e
    K9Gtcv+eyUhWkD8VTmE6fq/F8CDIK3ScIiJykF8hNL1wpa/mGwWWwBnkozIJGAbTAAX7AgnH
    knAehnSW99TeU0lmib0XmOt4TN3sY=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("i=2 base1 missing AMS(2)", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void cv_fail_i2_ams_invalid ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=jsR4La5CWj4665VQZEjoLgxdNhdaE1mZFpkL8jsfEm938sd9TWr/keRkfZQaRFLuFjTxI4
    vg8/D4bUx3UW0G6CngHmcx0kBi375aRfxmD5ad+esDyc5Dw/s6GapOpb4JFrss1n6x4MGOtY
    GQAQi7b0FPUdlXVbKQYIQovi7ZjGU=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=CnX/07HnYNoqdjrn4mE9if486SWqYAytX0weObYC+UCp+ht1qId6MPsQa3QWSWZt3buX+E
    kCwFMMfnBeo1gQ9rPfPEcQtUI5/3D/RYqtBmaZTP1Vpcgj5qw3mQxNJJh0kl57z5holdQ5I0
    g0S02+/k61c6cJzmoDKYsQP/VjebI=;
    bh=invalid; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("i=2 base1 AMS(2) invalid", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void cv_fail_i2_as2_na ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Message-Signature: a=rsa-sha256;
    b=2cDGNznUmp4YSSThCe9nrQIH2Gpd5qPFw3OU8sWFzZgEQ5UZtaVQifVUXUrsSyEzjro3Ul
    YPPDx+C1K+LbKRlOZ06il4ws2zlPafsrx1piKsKSCUq0KjFs01hYCDBa3tfdyITSfoWu2HHY
    pCjrhPMPH1jruIdBV/5Gk2Fvy+mW8=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("i=2 base1 AS(1) NA", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void cv_fail_i2_as2_invalid ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=JAqZJ5HwfNxxsrn9R4ayQgiu9RibPKEUVevbt7XFTkSh1baJ533D2Z6IZ2NaBreUhDBb2e
    K9Gtcv+eyUhWkD8VTmE6fq/F8CDIK3ScIiJykF8hNL1wpa/mGwWWwBnkozIJGAbTAAX7AgnH
    knAehnSW99TeU0lmib0XmOt4TN3sY=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=2cDGNznUmp4YSSThCe9nrQIH2Gpd5qPFw3OU8sWFzZgEQ5UZtaVQifVUXUrsSyEzjro3Ul
    YPPDx+C1K+LbKRlOZ06il4ws2zlPafsrx1piKsKSCUq0KjFs01hYCDBa3tfdyITSfoWu2HHY
    pCjrhPMPH1jruIdBV/5Gk2Fvy+mW8=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("i=2 base1 AS(2) invalid", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void cv_fail_i2_as2_none ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=o0fxNS9D87SVRYy2tkq7rXntZWYLuInRCzW2Jx9U8Px0XEGyD4SdwRIpS+RJ4qK6ufvuuc
    qYLmF9M9aV0tvbe8mp78+qhN8RImVPehz6AFPY7NGy563MQDPDAynBWQyp4EXodlmmzQoEGB
    iMar9e9AuWSwyAok1BDkUFsajLRIA=; cv=none; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=2cDGNznUmp4YSSThCe9nrQIH2Gpd5qPFw3OU8sWFzZgEQ5UZtaVQifVUXUrsSyEzjro3Ul
    YPPDx+C1K+LbKRlOZ06il4ws2zlPafsrx1piKsKSCUq0KjFs01hYCDBa3tfdyITSfoWu2HHY
    pCjrhPMPH1jruIdBV/5Gk2Fvy+mW8=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("i=2 base1 cv2=none", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void cv_fail_i2_as2_fail ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=1mXrddJKGqZTDDgnoDP1IYTu5g4ij0kxFZ8dSsSjo13+vDuoBEa4aKbYWlG4Ij2IAwjaLR
    CDYddDXDBZ5Cpnzrq7fDSVmmUhwQanAAd9aah4TpZeervt3/tOqFnpckUtOus1hq9yr5lvLA
    1umDZf50sOb6AygAm/k8xCco9rDp0=; cv=fail; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=2cDGNznUmp4YSSThCe9nrQIH2Gpd5qPFw3OU8sWFzZgEQ5UZtaVQifVUXUrsSyEzjro3Ul
    YPPDx+C1K+LbKRlOZ06il4ws2zlPafsrx1piKsKSCUq0KjFs01hYCDBa3tfdyITSfoWu2HHY
    pCjrhPMPH1jruIdBV/5Gk2Fvy+mW8=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("i=2 base1 cv2=fail", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void cv_fail_i2_as1_na ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=1JtiDdajC4yqlIqokR/uaPI/KdST9EsS2oPhDdQAe4E96IXwQwkgRZLJF9OODSux9JCWXh
    Z/sCh3yLmcTOKPuBQAwtAfll+PUePsuHh0gRYECVIkY3bGfAr+3hVdrmNpr7B6/Zcq8mjQEt
    u/5q+XkkocYaUxT+ODnGGHwMI/8Q8=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=2cDGNznUmp4YSSThCe9nrQIH2Gpd5qPFw3OU8sWFzZgEQ5UZtaVQifVUXUrsSyEzjro3Ul
    YPPDx+C1K+LbKRlOZ06il4ws2zlPafsrx1piKsKSCUq0KjFs01hYCDBa3tfdyITSfoWu2HHY
    pCjrhPMPH1jruIdBV/5Gk2Fvy+mW8=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("i=2 base1 as(1) not available", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void cv_fail_i2_as1_invalid ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=gifscOcADiR9JpJLFaCULS2DPnnk89AxF3tIfanEQV5PQWJvRSWrDs8hMwLDdDDZRKBWNq
    I1+lBro3Nd9RmUt6YsMNdGYK7XIG5ME9FwamoqqFxq++1jST6wg1gS1YrFExuHreNlICZ9yT
    xSmufAj9mJS2CLuOxYh6YIo6bHj1Q=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=2cDGNznUmp4YSSThCe9nrQIH2Gpd5qPFw3OU8sWFzZgEQ5UZtaVQifVUXUrsSyEzjro3Ul
    YPPDx+C1K+LbKRlOZ06il4ws2zlPafsrx1piKsKSCUq0KjFs01hYCDBa3tfdyITSfoWu2HHY
    pCjrhPMPH1jruIdBV/5Gk2Fvy+mW8=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=OdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("i=2 base1 as(1) invalid", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void cv_fail_i2_as1_pass ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=1p687XiKxG2/cjtpO3A+Qkt/B7Q49iMgcq1CutOBxLs2TXcO5CUozwxFbY9YvEaOyXxf6Q
    EnSvZ4UpYIkKGNrm0PLSbgI0y3cY4Waa/fFlT+/7oJUQmsnN8MreOfcHZRpGrSRU6bu5uOyp
    5mSlxgTwnti4Ua4vAjl+ayFOn0hC0=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=2cDGNznUmp4YSSThCe9nrQIH2Gpd5qPFw3OU8sWFzZgEQ5UZtaVQifVUXUrsSyEzjro3Ul
    YPPDx+C1K+LbKRlOZ06il4ws2zlPafsrx1piKsKSCUq0KjFs01hYCDBa3tfdyITSfoWu2HHY
    pCjrhPMPH1jruIdBV/5Gk2Fvy+mW8=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=PhxOTCxMOzOkfccg/YXFn+e5FdMyjQK+QXNt9lYytimVUpntsBbAAtBQT5XgYQDRsM3YR+
    vBsf1oJ+kL221cv9qQWYUC3DP3xaE0nZ3vjNR1+//uZpMcTT3k6NYZnlzexAzYMoXByQkrS6
    0Om4kNir1fUo5SOlGXpXf8NRSmU70=; cv=pass; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("i=2 base1 as(1) cv=pass", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void cv_fail_i2_as1_fail ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=uMD7AJfyGb+OHxrGSOB3Vbt7nBnEZn0RMBoq8GgyRbz4Xar+BmAIR766rEvlwgLkkKU21u
    GY8S0HK2GgR5lhpcrezkwD9/L+bfe7uyuFDrr4b50mt4oI9FTfwR0MuHpW91gAvR4ZYnwTRy
    PoMy3AaapPFnlY38n+HjseH0JACTo=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=2cDGNznUmp4YSSThCe9nrQIH2Gpd5qPFw3OU8sWFzZgEQ5UZtaVQifVUXUrsSyEzjro3Ul
    YPPDx+C1K+LbKRlOZ06il4ws2zlPafsrx1piKsKSCUq0KjFs01hYCDBa3tfdyITSfoWu2HHY
    pCjrhPMPH1jruIdBV/5Gk2Fvy+mW8=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=wddf4DzBcl11ICrYWjYC78s246KGCG4D3XBmouE2PVdLr4LWqyTWTQDvZ7TWrtEDkRsmz+
    wbaMVAWdj2XgewkwQu5qxQ82D5dGiLcNQXfQDRbd8dO1+PZVWlw0wmeM7nRhNb/5tT0BvNQO
    xrrb4oEs4LIFDNYtKgTvCMyCVLzuw=; cv=fail; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("i=2 base1 as(1) cv=fail", input, locator, ArcSignatureValidationResult.Fail);
		}

		#endregion
		#region AMS Set Structure

		[Test]
		public void ams_struct_i_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=UeCu1SBJupH/8Xp4vCLTNWJAvhmj7xgawvWq/GsnIiqXrrYVg/OvwcqGqGD1kNZHWvZuXH
    W0AIl1z/3vycIGYQdrT22+oy/s0bJjHhcHQWo2iZt/4mP094fecbT/soJv6mERLw74pRwkei
    /skva76UKGLq1xHXzvQkew5RwhgMo=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=mmLqaSDJ2CFu1lXO7s26aXT3MrFC1pfi9ZjuVysKjleXUX3N1+pX+GchfuzHniUGpuQQRQ
    1J7CL7EG6Rd8SzWIM3ghBfhN+G6jjXzv+uVtm89kbNodrVZ1mVtEtUoEo/8BvOCfeNotyGyj
    NyHGzPyAc+kv/zimFml7MKn2By4KI=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    s=dummy; t=12345     
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass                    
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("AMS i= NA", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_struct_i_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=Je7FiPu5s4SChzn1rIOwQd9kAjZodFrZwqsoomdw0TGZsQbL2djL2E160MND7eGKIRe1IP
    hu0WqhhZ3OD9LGFa/JUOoSeTgTA+kGx0Acan9wp+ksw9UeCLtpmRs00FOYiHe4mpl8i1QLVG
    aLcewyizL1HdM1U3N+S+eeOzBtdwk=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=l3O/xWb0Bu0DKaKiwlQrD85VsESQyWPV6fS5r9fou680YgQGIk8ycfBXt0kJLKMf6I7gtC
    AZZ3eypnAjIqiP1o3QNaajeicN5str9K+miibqwANe0/SIPR4/fOqs5oS8/9309wzwqPflUt
    nL5/nuDJt/5KHqXc2jMw8GZfdAf7M=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=; s=dummy; t=12345     
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass                    
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("AMS i= empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_struct_i_zero ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=1BHrrrHUNLNWyocFdPKW6qGOt7Y3TaD8n0pI4pVOKi9hfRljZKQ3xENWzYCPNXiwp9p0ww
    gOt2mC/YtjrGCi+Yco4DoStThYcEkoXKUZVPDSa8AIOE/sH32wTYfhaspKr9A2zvScG91zfI
    EGC1qF4qHAoxnJBPWfHhuzqH2ELk4=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=hbcwQf/Mg+hXbDyGiBN828o/d7O4WzqVD3r4Zym8u02Flc53mO0N3ElghteOiIw4YG2Sa/
    WbfxPpbWAVsdtYprjevGNOvl1hpF4d+xqX7h+GVxaEwEN65GFK3trxJF2BaHQ8F4hc5a6Fmf
    Is8OV3B+2ZW7iegJdH2CVlvusajys=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=0; s=dummy; t=12345     
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass                    
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("AMS i= zero", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_struct_i_invalid ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=gzFG4vgfOas2UvRlmaw3UKLF+bfZLP2RoFnIysRRMQHFDz62uhEUoM/U8MAm4N8DLl3Bw9
    3TcPxmd6zV5sbciSc7zXuMxIAmj3/vyfv4vlTDNZ4n/pHXGfxNhXkDndJcoh+2U2Ia14ARkP
    /v2n677wgmOPcbzq27YL5+DJFEG4o=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=eXN9vsCoo/HcGaJ4yAdXfbaW3zKsIFETZAJcHuD7xRi8BttnxkZTCiFn7zp+AOUIuga+/Q
    w1mGFP5tDK2U+mcKoEXvXIxGBS4CSKueUHDxTyCxdWas4LFYkGd4fKM7XK604RqByh+301RA
    eNU/3uCcRitftaLemX/R089S2HETU=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=a; s=dummy; t=12345      
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass                    
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("AMS i= invalid value", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_struct_dup ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345          
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("Duplicate AMS i=1", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_struct_missing ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("Missing AMS i=1", input, locator, ArcSignatureValidationResult.Fail);
		}

		#endregion
		#region Arc Message Signature Format

		[Test]
		public void ams_format_sc_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=OeNJ7p2NdW3mKv4hyenx+QbRuqqq8iwGAyY1WVX/EJiPHS2vNB5lEI/YmVB3diTkKPHWe8
    ZOq18DTVtOVuahLqM7s/4K/gvx3zal0vcedPL/mtRW4A1Ct0/wyLuFADX2HZ815cELx81SuX
    3fEbbym1br+0JArsz6n8798lidnWY=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=NOLE9bNh30qiTx35h5yKbHlDPahxvhXUWjv8Yiy5L7Ks3NNznK54dmUPZ4D/80tkRYiil0
    8sCqFTh7OH5ZTXXEfArxBMQQl3DAqTjOJQ1c3jPYwaDliWqCLLueSsH+ovaFGRGNPm2O41o0
    J8xUmyji1bXXLKMinB+Adv9ALXsw8=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ= ; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345      
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams format tag whitespace around semicolon ignored", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_format_eq_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=CcoQW04QZ7n7OTPACcP26R0vJtjEwVmcFpj4+PJnvT1kVeOMfcqwt7FEGlCjeJ0QIYMeNW
    TY6kND0fe0WJDVnWvhCyeOb5JjwllbJJ/ThP74I5UPgQ0Cwp1h/O9HIrUJkrje6HQ3nD6Dok
    la2keL/t4R7YtMyAmn9sPWuAOwSrE=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=KLZ8Io9rZzsWt0Q/Mrx8sYO7HPLptFwGoCdabHuyrQsek+1c5yo5tOQidcTc8ksw5PoAZH
    PNOIoyGVte9jMk0LdA1IYjjvvUmEANMZCJf0wm66exDWJ30xMrgbosLN2XvsRk3BDkoCg2AY
    HkR11isMdIhrefd7AHw9YEDTnohQw=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c = relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345      
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams format tag whitespace around equals ignored", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_format_tags_trail_sc ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=Q3iCsG7zmlydzz8zFIm4X+Nyr2636znsyGh+lRhCFtcWbw3m3v8fFrtK3uNvqSM+WW3Cmf
    TbteHFaG9YL34KUMi/ThuPoG8sOwJ18BPjXrdBS5EiXYBBFalkVRV0ktqyiNi57LBVS+VGWV
    FwOD85C/V/Fju2wETdy0ly1VjfLBg=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=H+XsRP2HBJwygQonE/YquKr2y1KqjjlhBQ/hEkIGFjjNhOIvMfuuO054H4+kxMmvHFdwk8
    a8Uwy1MxQBC3a4b0jAQ77rOn5VFhO1tAmCkfZP1bJSxewRfC2Eo7j/07+r8ZLuyuAzlQIW+n
    DPJtOhnIIEOGhLgPNlcTwc9R/XKiE=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345;
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams trailing semi colon no effect", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_format_tags_unknown ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=hnT0gH/efgds1eGi2XgAPK5ts+d6QZHc910SN6Xpr1TmDPXpNoRlq8F4eeMTj8VMgLWOza
    HeMe4quPmcWlZm0vRkiUxK3Q9HJlclElB9ehd+qPKzE92zWdSnkQN3kpyoA9mSAn6eTUmX9d
    ZFCA8DnXTQSl3T7V4GWwO9byXtHmg=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=jHH5JVvEmO5RMAl5NlbOHuIgc72768gwRv2MjCvgh83McNt2Ogx7yFZTPfcyO9F0jT5EIz
    bOzMeH+vIBJjJZz1/FVpBUxXJ7Ir5jQ6rjDGCvztrqeSMkhyF2pdiGIQPn6HuA0qDjMY6IfM
    wGoYUNqNE0+2s5p+DuxXMbT0tZBv4=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345; w=flarp      
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("unknown tags still valid", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_format_inv_tag_key ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ= ; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; ==; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams format invalid tag key", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_format_tags_dup ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("tags are not de duplicated", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_format_tags_key_case ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; H=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("tag keys are case sensitive", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_format_tags_val_case ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=From:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("tag values are case sensitive", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_format_tags_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=1 2345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams tag values whitespace sensitive", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_format_tags_sc ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy;; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("additional semicolons are invalid", input, locator, ArcSignatureValidationResult.Fail);
		}

		#endregion
		#region Arc Message Signature Fields

		[Test]
		public void ams_fields_i_dup1 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("duplicate ams i=1 order 1", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_i_dup2 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("duplicate ams i=1 order 2", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_a_sha1 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=wO65QnQIWl6SnCuXSzaqpFWd1Iz5y/VneN5oorqP3XDWkXh6SYZ/CCIgdzBYqDX6zzKXAW
    /qC29Xl3klg9mg6Epteb3Ie/nmUDCNiGoBF2ZGWW4w3CgYYBJU2nGitvR9ytKZ9VaNJkXqr/
    iWEc+fuCSNgwbAXMe4WeyT1LjU0QU=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha1;
    b=JxQvoennMmz4+A7Strpsa5XUkbwuHHwNZW30eUytxb+M28y/01pkSXPKpsbLuZItfjJw4h
    AIiLraxKMj+Ene95MTUa1Xqk1fzlKTo+mhfpPOwn4pBmZaJilCx25pRNgSrs4uGX79vVcf51
    xiN7GF0ns2hGx7Jg/YTeBsVL9ckd4=;
    bh=bIxxaeIQvmOBdTAitYfSNFgzPP4=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("a= unknown algorithm", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_a_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=xOHf2jEcVrFDvr1gDUYPzfVj90l2aTOJmU3pQXz7FT9tp86svuVyCZqh5laLPdunneTVgg
    wgejwf9PP8A8wCO+HHjMwZF16ZUE4Yqg4ZMjfzUCmK9fv2iT5qx/k4Hl4F6aBstcNhODcTpg
    bN/qhpMAtJqP1+Nk+SxDgLEZPqwNU=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature:
    b=iu9yW2jX7187Y2/yKW8Z3a/8lIsaKvek4h5lscx18jk45kfztcCuR6adEtBx3/szzN5bVW
    iU6qZgimS7l2VjfRhNFEOTzhpxXO20Pd9lJ9O07Jk0CzdYd6PqB+anrPz9+g7+3Bqn+MzhEi
    j5+z3fy6/xMhdmISrSWwpGbT6Jp98=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("a= not avaliable", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_a_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=ErVcYmPHoKhcv4cUMAAlK9Z5tZ2G66c1PD6uaqj09ab7YOmLJ/4SJNH16m51n6oRv6uBtO
    Qp0ikKw9DAG/ZFm550lnr0xwDxgo7C8b26FC7187QlNoW1/we0SFutFo6XqJGgmQsly1glDi
    uyECgQHiR2C+S1DHpYqSMGhl/yehg=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=;
    b=zVPKQ3zrtroGYfUFOyR2TJFk/T+2BorYCDkmE6KSBjbhNFtvx0W+5z+Rnz5gvnQJA7K1Ob
    CdP9IlCJGU9w5vYfsw0hL2nksu+1b1sgS0FZv5N2AqT07Xc+B4xxjUnddUojOdTp33k5rpES
    /F6U0YUFq29+Oec8F/+849FdJIFo4=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("a= empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_a_unknown ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=ZIVn4O02gVZZM69shdqjhGG21S6N8/WPCWKj+NQgoDtMuUYmgIaKkWJ8MyzdiCH+09lKlC
    OeftQqHOztn7eqsqG1+JkY03J0WR1IKcyEJ01mKrKD1FEm5FlCAYfk0zl+S58GDXNC70d7EU
    Ht3EPiXZ237m5+ZELA9ER1wDP8NDY=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-poptart;
    b=kHGBG8KmUtKvaOluO64HVWcSrN7/F02NkxyoDqkfZaoF3brrCj8PWNf9njKc03EKNScVgH
    /77ZoYr06Fsv+cT352CerOJn48/Rg2k/OZFv2j7Cg4b4BOAH0XYfNIw0c3wh3h8P6ML7qk0P
    zE086JAU7m/JHXCmeWdGMOWxAgtDA=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("a= unknown algorithm", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_b_ignores_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=L8GsQ6v/7miEWKMGu16QVCPF6IT8j9+DV/ZHzgm86gi5m2JYAq+BlkmiIDofRPW+QzAq85
    2UlxwI2NZrhyAKgtM4FKO7+84P1eYwJKh57DZfCyUpqRx1Je2+vzT8ZggXQWYjFEu36MTDFX
    fRKVqPV3omyP+CFBzjJFFDLehJaPk=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR /UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams b= ignores whitespace", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_b_head_case ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
FROM: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams b= canonicalization header key case insensitive", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_b_head_unfold ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe
  <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams b= canonicalization headers unfolded", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_b_eol_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>    
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams b= canonicalization eol whitespace stripped", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_b_inl_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John   Q    Doe     <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams b= canonicalization inline whitespace reduced", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_b_col_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date:  Thu, 14 Jan 2015 15:00:01 -0800
From:  John Q Doe <jqd@d1.example.org>
To:   arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams b= canonicalization colon whitespace removed", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_b_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=OzRrkSWsUTrQ14yM/vnwnE3FBgXOC8u2KaiR+kqtJ3fZRudMM3gZm815dG/TaYTIw9Ia22
    voygKoSBc/48fUYFcmfKgwHHW/mlqHqP/eLSQ2/tQR/R+eG0ldsqj9nWhMfqRs6eNU32LMOd
    fpk9IcVRPAx8Uf306RgvfMmwKrwzk=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams b= not available", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_b_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=4Crgst3Y5S/zOuKhpV14MKa5UBleFxBwmEeGMfTBptTZ+Pr9UcLFTmbWBCP2XkD0l4++kz
    LISbCEDLFFzPBXxi3p1TxUa/i7Ib1/F4oDSIwu4r4dGYoC2aR9ah4f1zjj2JM++V7PuFcn+Q
    +wETBwEKIM/uP4VabDjLAwhLfuvR4=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams b= empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_b_base64 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=DMVLxkygsAo8oIVh698NtvdqZ0RspPu/YPQqcSNFWFjjsfcF4hZXE3eGkrze4nTfz5XB6p
    s/210+6vqYSJIxMVxRJxO8wd7xQn0MyG/1NeNoW9qQdBuajHIPhgjCvJe2jnnvHn3MvaAjXR
    oh+Jco1pmRfeoLBPqPJmEvDW1/9F8=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=not_base_64;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams b=", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_b_mod_sig ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=ukwKeUuHm+O5bA374QNtQMipl6EYfgWlGGfgTGznPEaIFiTmV3A5lhOnCEGP8tr1CDYDl4
    yo32S/rA3I0Z+GHWpo6pzcnh3+tvCBuwoxHZ5srbrTwOFNQWmPC2Bfwu4fetM8d73/wN8uKm
    ZziB/Gd1VTLLCRa4xx2C9QM+nYEQU=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=7sRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams b= w/ modified value", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_b_mod_headers1 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams b= w/ modified headers 1", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_b_mod_headers2 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1 (Mod)

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams b= w/ modified headers 2", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_bh_ignores_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=E8x7AZqCzrIuoNF9dWTyteDmtDLHk3J6CSXj1DfRHjk2cd0oeHUIXvtrNtMhYs2sFHoZRR
    NuVvgDUIwPcbtr2Bz9eYvUTuOToBRn9FZFqnpR/rHl5VbPAIhSwE98WT6PJt8pqNCyKyZU3I
    szoWq5cB3OWUv6QJ8ctb6rCZLbk3g=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=xWBIUyGnx5WlX005xU8TYkieptAqvslDc7lkuqyFpACyOklw0t4cAONgr6qUavTnRJyZoJ
    mXXIvvPk/7xgH9eT9lCFYk49vpo+fqZACxJwpRk6WbB3fwbfeZe8C2aL6X/G40ROlh4EVcy2
    +NjgNS2X9ZEmxKuGEehFLqaJnx8yM=;
    bh=K WSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams bh= ignores wsp", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_bh_sim_base ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=d6sLFV7dCrZT/WzJil6ZyWcA/W5tJGLkP+yx1Fln+uZdjkswYMjvPkO2V2kvMrh2GBgjee
    j9QiqfGHsJvGqAKrFVzxHEsgVA0IYN6tI5wTKMLgu09b8BeHUr49/XnBEemjbgO8W9n9SCyX
    hKjsZK5b5ZIYBqjCSDZUwWRWfJywk=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=c+pRG+RBumfEVWDAjHVupy4hZHN2F/AMLHoj6Vha9px35oo6eoyMxxOFUvBgVIUVphuSwV
    198baYTV6Of9DHw44VS5rf6MDZNtVc8lwm8ei8aSAgzSnuhnr0jW2j134QTsEL1TK1bWfs+l
    QGXDBN5AUDsbk4jN5akoDqmH7gNlc=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/simple;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams bh= simple canonicalization", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_bh_sim_end_lines ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=d6sLFV7dCrZT/WzJil6ZyWcA/W5tJGLkP+yx1Fln+uZdjkswYMjvPkO2V2kvMrh2GBgjee
    j9QiqfGHsJvGqAKrFVzxHEsgVA0IYN6tI5wTKMLgu09b8BeHUr49/XnBEemjbgO8W9n9SCyX
    hKjsZK5b5ZIYBqjCSDZUwWRWfJywk=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=c+pRG+RBumfEVWDAjHVupy4hZHN2F/AMLHoj6Vha9px35oo6eoyMxxOFUvBgVIUVphuSwV
    198baYTV6Of9DHw44VS5rf6MDZNtVc8lwm8ei8aSAgzSnuhnr0jW2j134QTsEL1TK1bWfs+l
    QGXDBN5AUDsbk4jN5akoDqmH7gNlc=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/simple;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams bh= simple canonicalization ignores ending empty lines", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_bh_sim_inl_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=d6sLFV7dCrZT/WzJil6ZyWcA/W5tJGLkP+yx1Fln+uZdjkswYMjvPkO2V2kvMrh2GBgjee
    j9QiqfGHsJvGqAKrFVzxHEsgVA0IYN6tI5wTKMLgu09b8BeHUr49/XnBEemjbgO8W9n9SCyX
    hKjsZK5b5ZIYBqjCSDZUwWRWfJywk=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=c+pRG+RBumfEVWDAjHVupy4hZHN2F/AMLHoj6Vha9px35oo6eoyMxxOFUvBgVIUVphuSwV
    198baYTV6Of9DHw44VS5rf6MDZNtVc8lwm8ei8aSAgzSnuhnr0jW2j134QTsEL1TK1bWfs+l
    QGXDBN5AUDsbk4jN5akoDqmH7gNlc=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/simple;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a   test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams bh= simple canonicalization doesnt reduce wsp", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_bh_rel_eol_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,  
This is a test message.  
--J.  
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams bh= relaxed canonicalization deletes trailing whitespace", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_bh_rel_inl_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey       gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams bh= relaxed canonicalization reduces inline whitespace", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_bh_rel_end_lines ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams bh= relaxed canonicalization ignores end of body empty lines", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_bh_rel_trail_crlf ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams bh= relaxed canonicalization adds crlf at end of body if non existant", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_bh_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=YoXbDMNRVADrsGTtqAuMLWVnRIj62jQOSDFCX875c5ksVoWcKstnor+cGw/PJnz0cPuFGH
    +vjw3y+tcgBDDbK1qBVyMUpHrahTLL/0IY2jMzoLgPYz7Yawv/gpn7GlyXL72Vdr58s/nEfk
    le/2NmfPZjlUezbwsw+UHbuqT5V38=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=m5y+bcsy0duHt1KxJ2EakY2mOpwIrFaHD60tlw1PmqNdy4M7XLGTnA10R7k1OsFAQNQdZM
    n1aKsKDpYuRX21avSuDxximXFwkcWYevOqUmaklFXiWyJVXd9fHId0sEtNt0L28HInLwHeCf
    IPYbUuddJ8wRWei04RZjqdybh4f2o=;
    c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams bh= not available", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_bh_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=YoXbDMNRVADrsGTtqAuMLWVnRIj62jQOSDFCX875c5ksVoWcKstnor+cGw/PJnz0cPuFGH
    +vjw3y+tcgBDDbK1qBVyMUpHrahTLL/0IY2jMzoLgPYz7Yawv/gpn7GlyXL72Vdr58s/nEfk
    le/2NmfPZjlUezbwsw+UHbuqT5V38=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=m5y+bcsy0duHt1KxJ2EakY2mOpwIrFaHD60tlw1PmqNdy4M7XLGTnA10R7k1OsFAQNQdZM
    n1aKsKDpYuRX21avSuDxximXFwkcWYevOqUmaklFXiWyJVXd9fHId0sEtNt0L28HInLwHeCf
    IPYbUuddJ8wRWei04RZjqdybh4f2o=;
    bh=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams bh= empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_bh_base64 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=YoXbDMNRVADrsGTtqAuMLWVnRIj62jQOSDFCX875c5ksVoWcKstnor+cGw/PJnz0cPuFGH
    +vjw3y+tcgBDDbK1qBVyMUpHrahTLL/0IY2jMzoLgPYz7Yawv/gpn7GlyXL72Vdr58s/nEfk
    le/2NmfPZjlUezbwsw+UHbuqT5V38=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=m5y+bcsy0duHt1KxJ2EakY2mOpwIrFaHD60tlw1PmqNdy4M7XLGTnA10R7k1OsFAQNQdZM
    n1aKsKDpYuRX21avSuDxximXFwkcWYevOqUmaklFXiWyJVXd9fHId0sEtNt0L28HInLwHeCf
    IPYbUuddJ8wRWei04RZjqdybh4f2o=;
    bh=not_base_64; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams bh= not base64", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_bh_mod_sig ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=YoXbDMNRVADrsGTtqAuMLWVnRIj62jQOSDFCX875c5ksVoWcKstnor+cGw/PJnz0cPuFGH
    +vjw3y+tcgBDDbK1qBVyMUpHrahTLL/0IY2jMzoLgPYz7Yawv/gpn7GlyXL72Vdr58s/nEfk
    le/2NmfPZjlUezbwsw+UHbuqT5V38=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=m5y+bcsy0duHt1KxJ2EakY2mOpwIrFaHD60tlw1PmqNdy4M7XLGTnA10R7k1OsFAQNQdZM
    n1aKsKDpYuRX21avSuDxximXFwkcWYevOqUmaklFXiWyJVXd9fHId0sEtNt0L28HInLwHeCf
    IPYbUuddJ8wRWei04RZjqdybh4f2o=;
    bh=Z3JlbWxpbnM=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams bh= modified sig", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_bh_mod_body ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a modified test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams bh= modified body", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		[Ignore] // Note: apparently if c is missing, assume c=relaxed/relaxed? MimeKit defaults to simple/simple like https://www.ietf.org/rfc/rfc6376.txt says
		public void ams_fields_c_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=ygcIhWO/8u3FP5h+7kQH7X9Yqxs0MIHuMUA6PapmNf+8CP5Fb/mY/mZ5aUcLxJNozQ2oUU
    ukkGEysRaqm5uTJMhiy4YjZgJqMRVka3xMGeIaSw1PiugVu015l8wKR1ollDSN7POJaajQBC
    /4mUnAUFfND8OqfE/VimB6flYiUJ8=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=1+WHHTxU+XLWVsbRsvjlW2kMRRhmGE+OE9jxnmLt4ryEa/AezAflCMmVzM7r1dKwxJA1oc
    YmkN0ga0CO/nxSvB9XR0dsg/TH7TTSQKIllCRxsmGLt+jG/9Mw5yTRxtBOOuFK4xbHbFbCLU
    vRCry9p9YZpoAemnEb24tm9vjlrsQ=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams c= not available", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_c_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=eTLQqvFomQqHaOc36izhl5UMp6wVe8vGsLLuPCraumms100F7tOUhRpAII90YkwX0AK+RT
    5ij+3Ngk2sQRpMupfFTgeF1olGU+jt943VkFbmSYXYp0AwBe4TGsLugWmfkUy2sGBSC1Rv7n
    ZaC9m6Y2bNMJcwix1EAuFFV6ck1Wg=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QdAvD1bnatYxK/JQCvI1uSuKxOYC+oR7wqg/twCt+zAFm8Tvu+fZpO79+TSx+cLAETXKNT
    6mgQLaLROfq3sNf8tP0f/4oqzMUb6Ybz2syHL7hkmC6Za5Ii8RDKwMSc8lmvJk6HXUKgsndZ
    vWsQCfv+jyLmfDfCI8v9WP7xa2UEU=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams c= empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_c_rr ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,  
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams c= relaxed/relaxed", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_c_rs ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=d6sLFV7dCrZT/WzJil6ZyWcA/W5tJGLkP+yx1Fln+uZdjkswYMjvPkO2V2kvMrh2GBgjee
    j9QiqfGHsJvGqAKrFVzxHEsgVA0IYN6tI5wTKMLgu09b8BeHUr49/XnBEemjbgO8W9n9SCyX
    hKjsZK5b5ZIYBqjCSDZUwWRWfJywk=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=c+pRG+RBumfEVWDAjHVupy4hZHN2F/AMLHoj6Vha9px35oo6eoyMxxOFUvBgVIUVphuSwV
    198baYTV6Of9DHw44VS5rf6MDZNtVc8lwm8ei8aSAgzSnuhnr0jW2j134QTsEL1TK1bWfs+l
    QGXDBN5AUDsbk4jN5akoDqmH7gNlc=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/simple;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams c= relaxed/simple", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_c_sr ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=rhXdX7jNW4wMS/SjYKBYC9eW6q5KnnQ7UGICE45CsYhwEoi38c3nM+91lvM3zhUILxo51X
    htsrMDLw5TJeZdiCqgXhQZmSEzR+KEdnu2oidezrK/hUzYPlKdO59EQgGIiDAmIRoKZ6+rGV
    fUCltnyjA07a9KpIpeXRKT3WDCE6A=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256; b=RWHWmB6euT01CXN0PJKCrmmoPPGc+pxxurfyJBjnNzkTizZKD7XwHLqTuNPaRG7PULU6ffq8FQ7IivdffwqXNj4L3ttpKNIjfsndMFvn5lpKZGfvJZfjTmbTJMhF4CCJZZm7l1xy7LbYMaMb12WY47vXOe9RNjW7jQyw8iqctcA=; bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=simple/relaxed; d=example.org; h=From:To:Date:Subject:MIME-Version:ARC-Authentication-Results; i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams c= simple/relaxed", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_c_ss ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=X9qtjasr0URzC564MZz0bwckcIVnBW9yUZP+xt4rStU7MIuuo266KZ1V/e5tbg/MOCZJ2m
    3hvKRsVy1fMeIus2RVBg88zwfjyRMsJBC+zKV8oONpIcxriN8imZcaeWdcfsghbAFBM3viCE
    MdvebSvInMfz0vZsD1DJBYTjPel8w=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256; b=fv7KIaPfZRTQynzpQ7Gkg3thdZn78iGc5L1hTQoWrY1nSaE3pqQTHsGDW7+FRquewwFoakGLSERxBnC67Sdvw9Exv+/CEs/spqRrDjNygkCf/BIZcURb2nXXFHqPy31X6r2bufWKj6Lbo+5MCyaS2tWkV+KoZhUpolYSo0CoGfk=; bh=hhFbTjokraRYc/Af+8v4zyKm/9ApHGkBSLO129NtPbo=; c=simple/simple; d=example.org; h=From:To:Date:Subject:MIME-Version:ARC-Authentication-Results; i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,  
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams c= simple/simple", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_c_invalid ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=YYGtMgeVAGSLLMZ0k9D0yRRzsfKpbHCoqfLAKz+Du2++GE82Dvz2OT60ebG9m6vmT6nT1t
    D+rMJnTXIZDUPZ6BLH8rLo8jMb33cBV5NzBD3SDYqWA7OOkYrMGRGmoMfxpcGV8m77YykscT
    +cpxxA2Ytld+YTd0mTtxdOCN3T1M4=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=DJZENNFBf+SwDthFmU1ztUBIsKRAAaUdY9CjuGXejv8T29jf3q3EDUz6OnMevRWiSLj4ED
    gymMDJNGSTUaz3N85KmzWrTJ7QOLNke1H9L9kkfEFowatF8fW5cV/7Y6Ubzh0e1626TELeE+
    kvczpXT7prdjJZZjQAbDuHsWXkOys=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=pancake/waffle;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams c= invalid", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_d_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=xPtYeQQruf8zzJ9kUrMESmH9ooORAIArDB3MhPcaL+0fgmuc99fprb+aMaSqY6OdZvAEoO
    EBczyfdtlGKcqLqa5qpXYlukRfG3q8mlOd+8UU1u1bikCzfT/JI8PNerzaoxlksJfmt8zJT0
    f40IWBJnoRpPNqJSBFb8acvLVZFcQ=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=iDLI16Dzhtt9CmHLpkUXy7d5legcVvxkPMStdfrYQfNfpwVia165ca2lGI7Sx79pCoMmy3
    sSWBrLHsTQkKylsGswc0br0ycquKhxHgQh0WChxQd6ITVGQvFO/wZJd2jtE5E/KDbPKDjEio
    qLfCWpVe2KT1UZ89V+E9tg0T5TgwY=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams d= not available", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_d_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=Nn38++8Vf80guievTz8fSFN9VjbPdeRVR5LmvzRt0IMRzZ75FThtzO1VM0grGeUj+D39ri
    0ZwIgNyVtZXfG17FEO5BGQq4ZddLQoWHLKTeOWXL59FPhGRJkxiKNefS2c5YqZQ0NI8VkKY0
    HQlX6AeD/CHHE/bpcg7fFB5/WWnLE=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=yKCB5xEcyzGr2+mbXWsVDHDZB1PYe9MqqTWySS7Y32uFObEA/MNJmt5yPnZLScwQUhzeTc
    WL701aDMyPmlYlGnqxl2/QkvEw5hZNfOmD5gltxTlIabWyRrC1Qq/1RS2zDqvF2Qf8SJL1U7
    gL6jf82iBTT61ckhPraYGIdgI9hlo=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams d= empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_d_invalid ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=G8wYXXsNzfrmW5ob/HLkPkg0hz37d0O01HmLr8E8IQUPAa4lywxmOekn0bmKfOvK5p77Dz
    JEue+awK3gHG7/obHdRLamg8cYxmj4qfR6Ay0baikigUF4Wyt77JsVUqCC1qedRNcRN3IGPx
    7rrNSyzVlIWYPal3pQZc3E1ClpG2I=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=rllEQ7rbed0w+ixVEkL/jiUZrjyDdTQ1d+qnNGEvpzzjh2xFla14BKDcXo7q/aX25lxl0e
    yzw6yf5PFJC6JWqj5h3sFtLO6hS+E0DXyPZx0ok9tNiv7QV4YqY9fWeA64OZD183DKISDZnD
    mx/r0Svb5thGZvzvyfuAQapHke/Rk=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example...; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams d= not valid domain name", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_h_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=V/iPFUptKaruDTBpwKcf5i6nu54GxrG3ss2bfPqqT3I5MGMyRmtE+J0kOVtU9qtHIhXUng
    Iezv5+gCOIf2jP1eYGvhN2Wmkf2zsShG6+Rfpnp9fih71C1f6fh6Qp4tTUhB6ww4ZOTKtVdv
    H0C2s/5in4RLMxS0FUWge8CvlTnGs=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=ex6hirqdOz1yO1SZE3ALisw3dj1La5L4qHcv8/ttCs1qGajzw0zEtUyMnskTPQnt9cxxF3
    T74KRXlPVN/4Aqn+K/Q4NHtOW9vyuLt9ek9Vm6/xvZ10KTMrxv24u0eLnsigC6NfablL4wAM
    epZDlyjf/HPBd0yVLQL8yFDtQ5fE0=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=;
    i=1; s=dummy; t=12345      
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams h= empty", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_h_cws1 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=0+WA3Dpt9Y1lJ5wkoOZsh6KXEQFv0YE+ykvXAdS5t1toEui1UWzLyKWxSD/H/Xc6eCaQZM
    ji4IxybZ4OrIdV0yRe1fGqYN/bJ3KnkuzrHpaikXRWxXdX8tiIu5+I+HmERxuGzGqHdNv2zj
    5L8PNAsGs4LDg3xQXEe3FQAvis9OA=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=Lq5Sy1R3C0RaTxKWfggKBJ2MOdgAHeFy1nELK1c+CFnxdvSL+OxuvSxk8HYv7YMJDTR4Na
    1D5GaFedB1uYVQsz1T5e3p9B+54W4bObByD14WvTGKV3ys8FlOf4MdRIlD4o6N3INfHrNbYX
    zwPKjkoYbteAEQ/kTpjESOpm131io=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from : to : date : subject : mime-version : arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams h= colon folding whitspace ignored", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_h_cws2 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=0+WA3Dpt9Y1lJ5wkoOZsh6KXEQFv0YE+ykvXAdS5t1toEui1UWzLyKWxSD/H/Xc6eCaQZM
    ji4IxybZ4OrIdV0yRe1fGqYN/bJ3KnkuzrHpaikXRWxXdX8tiIu5+I+HmERxuGzGqHdNv2zj
    5L8PNAsGs4LDg3xQXEe3FQAvis9OA=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=Lq5Sy1R3C0RaTxKWfggKBJ2MOdgAHeFy1nELK1c+CFnxdvSL+OxuvSxk8HYv7YMJDTR4Na
    1D5GaFedB1uYVQsz1T5e3p9B+54W4bObByD14WvTGKV3ys8FlOf4MdRIlD4o6N3INfHrNbYX
    zwPKjkoYbteAEQ/kTpjESOpm131io=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from : to : date : subject : mime-version :
    arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams h= colon folding whitspace ignored", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_h_case ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=me1uYrnpt5Cdjkfj+bqK8X6abs8TET4r5Wp6e6ZuZ2FAtSzfx8WdnHCnBLUj7t/PR+EGne
    h4auyljzkm2gz09I0MbaYkd+xDmkRoN2WrFotceq+iROoDLf2NgZJb3SfDcVFp8emRMyyaGL
    WAtshPjJWnjoNfm+3clEpXzPw4WM4=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=OCzwOGeJy6YL07Rh1A970C9pAK2YJeXr0rDVVbsd/aOxTeKbrIxOfQsJ5hYaze0aeE5U0p
    y/45cz4Jg07Ch61xZ0G3R3ne4eXxPauAU6QKPwr45HxO2gDywmNruiJP0JPTzcC9SVV/YjyL
    OGobZNIwUWR1hEkd5/UuAXHk23Q4g=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=From:To:Date:Subject:Mime-version:Arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams h= case insensitive", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_h_dup1 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=tv8fgth8OQw5DylJlW253wBM12VcMvjFLj+TwonVXPiSPJ1hV7F24q0rgmYeVhSBK/+4Ou
    kPW3e9oqILXx95sXrE4fiiz46//FtZK7z0YVzy/B3QpR7fGxzzA5uVoUh4WNd0oQEejwDKss
    ILrzkyu6fDUZ1kLeKyk3clE7b/NJo=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=rGZpmx8nA8Fe0yQ319Ns+DPmwx9ToC7Z5Ba5NNGYtmXF87xboR0Cy7yxlJ2ek6j8WqCRXI
    jKV32tgZBXu5upoveTLBGzSe+NPTL2SkU2nFnktJjjPwTiPAYyXVBY1Uy7uSv9dT+wfB4Hvg
    Hm/nSrzqTBOxPsND1F1b2rzE1elQo=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: morty@dmarc.org
To: evil_morty@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams h= with duplicated header correct order(bottom up)", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_h_non_existant ()
		{
			const string input = @"Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=cEfCkdG3zAUpq2XMYEvcI8e+nD53NUuUr/NQ74UBTzSVJBOsNQKADtUWqYirSlB9AFeEIq
    VGstwfXqh5TiMv1Uk9O04vM7WxrmMsqZI+GiRQvtaanfZQMcaYME1pCURdkDbMK/MOUGV+W2
    j9anSPB91SOQruKUDtqgwq8z87Ajc=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QHma3KzZiiP6Yq5jWp+mLznldNAMpK9ffvI87mbvEFFd1YSfoJu9JrxtBgv3/MEBFHLPm9
    qTii8g+94xOLgp/LEC/dM2E/u7yPAKKMz5fMzJfwqSGAGyBg2f12Mkyaqs3dzv97nZTZFkj8
    mHCV6SHNfnC+lkIs5XpJNRtddvolQ=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams h= signing non-existant header field", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_h_mis_hdr ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=FCq5UA4xGNozfvMgZkQ0Wpu4Q0dkGbrNvMKc0SNQnbObHCA84DNwUUp+I41h5ZvwQBAGxf
    hvUfjmsMFHBtsYj/aQ5kkehVPkOZ/6hengnO0IJs78Ab/5eivdD7MRLuShcTWd9qx32dVFJD
    yx8qIaRZplvJYl30ry7sOJQu4qSZk=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=4TbROXpBlHvYUMMvecTyaEqk0DtgISmfrz9L7QEizbAaI6vgDPu1xD8LSj4CfHpak6GMde
    zpqtfiITgVTBKbkZi2kuFQwmu5xWsReExZEiNq7Tr6L5iObGjL0A27RIBj4znEmO6mk2Umnl
    +c6LR5XzyE65FGLZ+9nSH2U12klzI=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to::date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams h= blank sig-header", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_h_non_existant_dup ()
		{
			const string input = @"Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=akTog4W3hR16mF9pNZIhHzcceyST1LHWaIsDPobRX6iy5jBRbpb+lyKlcyZmS02T2kFYG9
    iOWQ6UZruiQXQu/u/GSkn0RSCwHWTfb25YqrQBLwH7pki4bDGHrTSrGbuYnFEHadYl2B8Gxo
    UXYn2/XBBil6Dkku2SswdN6RZhhoM=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=CvqFe5bB3kFEFvITOTVx7VcrJQBT5aAtUJjX0h1L1Gh0MtUQofgKfOakgKr5kUIxv2foZY
    KJzwNSuUNnDyY87HJeT02j4JlpYnj0+PzB8xjW2Kj4/4TrLMkcJsfC2wujZClzXW65uCsFEb
    0ht8EEQis3581f6/S2V+2pHxvqRiM=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results:mime-version;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams h= duplicated non-existant header field", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_h_includes_ams ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=XSOc6bESO7Ek4iCPyVXVE7aR8HUBBOXdOKmFpJO/3DI8rLRHHfRT9XAML3OsBE2RYj+0yd
    ypsBg8UQEewpY6Z5KEUhxfzwaBGObKr1pgwjkYiOBpPTV1Xfv1lGT+1qlJtBR2AGJauCEs7G
    fNzwa3MI+iO9E8g6aO/m9Mk1BlLHY=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=vpypMlcZNGmeVETFS/+v/Uk9npQE1LhY8tha0XTaeeNMgK1fzWaxvUHY0cuumuzK2pU25O
    uWTt08QEXczUR/BLmiZaYUWQV8qGOAv5umtEshqjB+0KPg5W09N20vQp8OXMQrenjZz0YPsy
    VweEidqd3HAcWSbZgW3jAFKXHGSXc=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:arc-message-signature:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345          
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams fields h includes AMS1", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_h_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=O9vrOnKLOdZXxa46F8RDPTzqW14JYE7idGn0AfedcpWh58mPFE9jXHeaMda5L59thiQrJN
    T7Smno713R6DU9CfvnOvq8rQXCJ6D7GzWFhhOn6wEbjTaFQQ3jHn67XVDVnb4yjLElVhixob
    pG5ouN8U1TPqPWf+41wrIrCd5Mocw=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=RidA92CmsCgK81At2aPnlGuFlbvNT5IdWz7Z/6j765oabi0LEDkpB+2q+C5TJfc28Gj0Ok
    gghf2ykPbb7WniSvCue66fvUYaABU5m84urSzGd3MG3F47vTzCQ5qLah7E0UssP2QbP2b1Rt
    Hry/RlkOzlWeSlxpCcPvArmmcADTc=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams h= not available", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_h_dup2 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=tv8fgth8OQw5DylJlW253wBM12VcMvjFLj+TwonVXPiSPJ1hV7F24q0rgmYeVhSBK/+4Ou
    kPW3e9oqILXx95sXrE4fiiz46//FtZK7z0YVzy/B3QpR7fGxzzA5uVoUh4WNd0oQEejwDKss
    ILrzkyu6fDUZ1kLeKyk3clE7b/NJo=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=rGZpmx8nA8Fe0yQ319Ns+DPmwx9ToC7Z5Ba5NNGYtmXF87xboR0Cy7yxlJ2ek6j8WqCRXI
    jKV32tgZBXu5upoveTLBGzSe+NPTL2SkU2nFnktJjjPwTiPAYyXVBY1Uy7uSv9dT+wfB4Hvg
    Hm/nSrzqTBOxPsND1F1b2rzE1elQo=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: evil_morty@dmarc.org      
To: morty@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams h= with duplicated header not correct order(bottom up)", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_h_order ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=vTCiDmh8p+YFqH8WSxCrLVT3IS1Xmt35hs9y2Fb4EriRTTEmD7lWa0UrCe9j/a3yftcMAb
    8W01KgTrdIhmUMF7YrElyT1cGc0ChGHmdkuA2MpVBnLJMCgtXEQkWcVRne38KB9P+GLvr5uD
    nBOjOJNoBt4Nt+Y8zCKG/tN2RetKk=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=2o+Wl1gzbDmg4Hv5q52M7V+E6KBhMISVmqTDrk1HfOgMJwJ+0v8Nl18EjbL+iOTu6Vxz9+
    1m64cPsNr1Tgm79jjqugOKDI/yaU7h4DaFMmN54tGX8j1ElMXSl8ghcfaknApLU060vKVUoo
    F2GfD1qo+SSox3wkZNkPQdGKjNmQM=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams h= mis ordered", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_h_empty_added ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=cEfCkdG3zAUpq2XMYEvcI8e+nD53NUuUr/NQ74UBTzSVJBOsNQKADtUWqYirSlB9AFeEIq
    VGstwfXqh5TiMv1Uk9O04vM7WxrmMsqZI+GiRQvtaanfZQMcaYME1pCURdkDbMK/MOUGV+W2
    j9anSPB91SOQruKUDtqgwq8z87Ajc=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QHma3KzZiiP6Yq5jWp+mLznldNAMpK9ffvI87mbvEFFd1YSfoJu9JrxtBgv3/MEBFHLPm9
    qTii8g+94xOLgp/LEC/dM2E/u7yPAKKMz5fMzJfwqSGAGyBg2f12Mkyaqs3dzv97nZTZFkj8
    mHCV6SHNfnC+lkIs5XpJNRtddvolQ=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams h= signing non-existant header field is then added(MIME-Version)", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		[Ignore] // Note: I think this is expected to fail because AMS2's h= includes arc-seal, but MimeKit passes because the signature is valid
		public void ams_fields_h_includes_as ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=OuFcuRk6CdaxxeBmCdvzoFxM6G0xmA3XNh1F243uPQsstHJ+T0csqD6PADig/UPV/Aj6fQ
    kAOsyZOzIK1X9ZCZLB2idFymnyWtYc2spNgCiSfwQiQuS3SFVUtr+Y7v58PtyAy2HCb2pA5I
    OIY1WjbK1Pd4SrJbZ4/M0d0wgFt7g=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=T5uPa/aCBkG1PK5dsSgO5US5yVvKnf/DAsyxMDCLVgw3auULB52XaLkZbc5KAcbGwz4KQZ
    H8TTB1qbdHGyUpA/1Tq4QveM4z1x/s/2gK/thnoW0wWEHu5frgmd3tVg8kEjrmU6HOJ1SNYq
    Qgjxvsd/OwpjYsfOjODwgyGDR/doE=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:arc-seal:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass          
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345          
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams fields h includes AS(1)", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_s_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=zlVnN6R6lixbru5oAlqBAalgQAcbqVJi0fZe8u57TJTTLHNl+LRLeQRsLQ4OcZ2n5XLTSZ
    ZAEsfzFQWeFruAnDpA7yT7/YTUYvQM7KdVzx4vl4FSTllt1wb0UJ0SNjlNGiudA94D43LOsx
    CsESqhYaVWRz4gLkD2P6FfqZLGCZg=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=1yhACoFkMMv54Xwy9PCxFazQ8BtUb99MhAUEk4Xwq7gVqDoyND9X+pa8CGMYSNUOn2I4tx
    4PyDzLhPNf+a4AciBNvFhHwK4lljIQAS514NuaNfv3PR0KDkkoXYv8J1EkI9yAyvOzl5Ka2B
    2yNTkGi6GucEwUlu2Qrk0RYhOYOVM=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams s= not available", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_s_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=iUyd0NGqGiWwg11FiLSmb+053tfp1baKV04kpufd+RESTCeMHlAHj/N2ZyLCHnCZSfgDTb
    hJy5KSpxO1nsSOlG/FsI6zwfEWCEP91aNjzEQxrX9iCg/zihZ9uv3wgmSOasjjt2kVGCcJUM
    iLpzGuccZW6C0S8QyOA8ClL0cHnrs=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=DMnmzfNSgbRhHJmeSr5Ahc9FzG0ZFQxd7FVPrmmbpB78dtA4tjLUywkekiqhABliJzs0ut
    zzkNYHyP0hlxGTaYOQ6OgV+1loymJCJDin9FhPV62CGOBXznuaRxFI+aWKHjW6SFFrZplQHG
    UQcAeHg8Dd8tdKV4dgUnuW+aphtiQ=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams s= empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_t_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=rx+UjBcicBZ6s5/J7S5oMw3YVWAWg+q4Sb4XqR0tMmhOyhjLq7702sEFlEDHJjdTuTVMg+
    c2qwv/XucEGW8/i4AMzNgkzpwk1Icsr0GHGbR7Jm8V+k6Z08tvQ4x1UaYgrTKmSQeyKq8rQQ
    rRdzsqqX73OFp/cKLa42T3JVTrQpc=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=iRbmo9I0Qn8ZELD2xJ754eoEATUfoRxli5qMUi3AQTwGLHU6oaLFsAP7JDYjRm6al3XGp8
    73NpnbncM6dnqlBvKK5OmekgztBKiyo7w0Uj6NZbq2KJXYiVW2vAbVkNwy4vPNhMHVTbD/xB
    PWROiovFOL0q2mHDT1KKLiSzEfrWA=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy      
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams t= not available", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void ams_fields_t_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=J1fBm2GXu8CCXApvRsyBIITcTcJ4MdgwPIUK2e+vU57BId7RYv2i7/ORWrImxasfuFD17v
    oU0TUpKqBmD/o6ZdLcgxg72iaYN7CoN9uK9Vr1llrVHuhJa4WUW0XG+a3XqKB2PXJh0LckJu
    215qpJ4wqx+/6aGVuxQp5LXwktEDM=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=GO1zQzqzWlsUbs6Rag7bYFPB2LgxCLkex8PRM+4/IbysgHm1TVtsPCVAAYp8+MK8UDyuuR
    s3wgba6Zgh08O4F3MGn5ouJmplCkS/mF1MTAuWF1BiBkzYTdNmwhESK3GBTDNgTzBwa0upsw
    aYiT87hDd1aqIKekvR3ZyEtZAN0Bc=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams t= empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_fields_t_invalid ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=g1Xr4aSSeSDH0CUBae/NLjI30AgmGDwAdG5BC2c/OuTKGROcimWkt3ikql9YlvBv/3O8AQ
    fe1XJqEq8EwFpKgk2YvMiWV4YKWPGb4DVNn/N2nk79o2KH/DlXNU4fLGvae9leiu1E+KJERC
    /sYt7EA0rffMCWMjfHivWEx1swomo=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=B9XbvvEBkWcBoOY6hBRGeJLsADsuzM0ZRvpeBWgF/nx8itykfMZmdeVPzVY5SI7MRCi8jp
    +RtfP938tY75D6wfNd4+mrDkHyEQFAiE+UlYWjZOGx69go2UQyN5+wocPHHps4n9j279es08
    zmmxQXWG8wuoq53Y1CfrwNyniO824=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=icecream
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("ams t= invalid", input, locator, ArcSignatureValidationResult.Fail);
		}

		#endregion
		#region Arc Seal Set Structure

		[Test]
		public void as_struct_i_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=2ScmNq/nw+PcTFaRUZr6ynujs8zh0C1dJiZhO7XwXZ1Wgqjgql0NJzCEPlZ8JLT1EF9vx0
    iCa9BPPYBmopN0d2UcZVRS3rkrioxlCXfCA9bFi287v/mZCAYY0vkEJqpb60oAuOTL4CImqd
    FRzkc52yZZEYC4U/gPyluWJQ6P29M=; cv=none; d=example.org; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass                    
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("AS i= NA", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_struct_i_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=HDF/3ZvQV+3g1pPoOxLwCgVUgalgi68PMmydT4JHPFEq36jEkMwATstQulCu4Qexedp55d
    3SX7YupWVg0nkl13bghp7ax+EvGREVTqPCLjawLFp6rLkM24ryiJb4xwF9WtXHWZlZCJUfTl
    kPUpNHxi52l0u75XeSe69lB4rCWOA=; cv=none; d=example.org; i=; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass                    
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("AS i= empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_struct_i_zero ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=ATiDsxkI1nXV+hpjshT3uFaKndUOSdfMdrGgmZvHEDmTWR2oWB6bNbG6K83D/C/JKKDs1G
    4XLhmWn+5wGAMMkFCdxkuqIxjco4UHJkBj+6KwlvJv5/1yxAyZXdBR+aF9eKrz9YXyrNgMsi
    CADCxXR/5RV8W6sB+Wxrgr1CPkitU=; cv=none; d=example.org; i=0; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass                    
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("AS i= 0", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_struct_i_invalid ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=EcQ16Bmi1GglUN2XnzTmixnR74rMt6K2VhtnHiH3o5+nLA7IPD/e9EbMjnhWK+IKw6WTdY
    MrNju5/13Hy9aUnUNDKRZrFbZx8bQzHk232QjFs1KLaUOwFUarbBEdHGmLRxLvSGzzYDXxFR
    ISrT+Q+55ZN0f5zjJbkeNNr6TraBU=; cv=none; d=example.org; i=blorp; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass                    
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("AS i= invalid value", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_struct_dup ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345          
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.      
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("Duplicate AS for i=1", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_struct_missing ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.      
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("Missing AS for i=1", input, locator, ArcSignatureValidationResult.Fail);
		}

		#endregion
		#region Arc Seal Format

		[Test]
		public void as_format_sc_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=sQHCWC9A8lAbvcPG+3jfih4lRJY/A0OI/GBGE4AYHf8u9cgsxOvyCqDWF3mr91HE5PhNh4
    RZW95NC6qhxEhnXLaXswqco2JXMVR6/rM5Q49bDE2RtlNen7wubw56NoJD2A7IGUSOzHaAiJ
    QhRTSoyG5OwNBC8+GlugUJi5mmZNU=; cv=none; d=example.org; i=1 ; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("as format tag whitespace around semicolon ignored", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_format_eq_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=u4XUza5aJKdMCwCMffAieua1x4N9tZpKlx7UwMcdgV+BuIZc48C3rF8xu6BnoRQCaulZmW
    4EYspmshC6cGg+kmYaWR/sbW712Ag8W33enEcoh35XLTg9QHg7zWvftk746RrVFb5Ch8iRsU
    PJ0gkAieomzXwlqCIBZQD5Yz2LB38=; cv=none; d=example.org; i = 1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("as format tag whitespace around equals ignored", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_format_tags_trail_sc ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=AcBD4PAxYztV5R8jYyYXKuMBWBRja89F6yBTQVtQ1FFUxQVYGOrFlnh3/r8/YtFt13NELg
    FpYeY3gnzudk30PoZZvM2MG9h07ByTgl0lSEsRLhN+ZtqoHRq1QGdW8oqOXntI51FbKwBdoe
    cHtLh18GzKAvazRWzv8//vQInYp/Y=; cv=none; d=example.org; i=1; s=dummy;
    t=12345;
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("as trailing ; no effect", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_format_tags_unknown ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=FriX6cOxgBHhZwNYHn0KXSWVqwHPNV6sRAKUy9iN1OqwvAK9USwMsg/P08yXrUH8LRaijm
    msJjp0KUFYiffoQrhsxHwv1hJIGceJZB7lOFeZn7Z5aym4eBp7q7idwNyIaGKL7E0WzVkeAT
    RQ5LhtOInN23gugfmW6z8MUUvow5Y=; cv=none; d=example.org; i=1; s=dummy;
    t=12345; w=catparty
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("as unknown tags no effect", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_format_inv_tag_key ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=f0DaIeWMnbbdvwQoBVCi3pw8hmSBdK4xfQvJH2BM0qG22MkQwBAsDkD/dnAML6bvVeMFjq
    aaLsCccFC3IZGvOzTsxbJTmbV0gHdPdYcsfhctXtrHfc/KdG1sgnqp+oGjrkveFTYUBO6UdX
    ncJFPHoSnLp6foW3V6zUO9mcuDmeM=; cv=none; d=example.org; i=1; s=dummy; _=;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("as format invalid tag key", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_format_tags_dup ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=IO43AZIKhbGLyWUCD6LAC3GeO+S9ET5T2SFkq/QCjOT5aChUgUziIlm2REH9SDMP6EfWwL
    ex6l4ndFMruyh+ReaORg3wOaXyf9nM21VO/9GyWpNkfMnVIzxspuNhkPsEJz8QglmQdp1Yww
    OItIuEZpAwkFDMzWFuenMY0RnncmE=; cv=none; d=example.org; i=1; s=dummy;
    s=dummy; t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("as dup tag error", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_format_tags_key_case ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=3RAGO9q/6XejhXohu4h3a1at+M3SQzh3NvUB6/9n2fWdnCAF/Y8fvEgul01qPYOVm75+sV
    DzX8LwZ9M8xvbpW02HPpiwJdSfMaSHfLFl7Eyz/X+iV/JhOovv9YoDfpkToqbisARZ6Zo4p+
    Ctok9hM0WxtmOjXqyOfFXfpMZRqI8=; cv=none; d=example.org; i=1; S=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("as tag key case", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_format_tags_val_case ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=Example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("as tag value case", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_format_tags_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=C7Te2RjPpFj3iKc4sPOTP80VHV2B/IeAl3AmyBOETgmdWrOe+q2lQ631QpI2ur/d5i9C6x
    gJvRBbqlGya23VwHyaJPrP6IfWnXokjrcvdnyWX9kvhPCVTMrco+1ouNkKrn/5Rf8OTAYCzZ
    daX8nbXMUANlFgBEQ+tvhb4PEMANc=; cv=none; d=example.org; i=1; s=dummy;
    t=12 345
ARC-Message-Signature: a=rsa-sha256;
    b=Wy3KTYHj5wd/cRfKjr5Or0eOK0YXjU4HH27PRGwY8prB01CRav1Zh4Q+tOZRrTLbYDrPUH
    QKwwxuKQ3IuS5+R9ugvuONhvNHLncIDvvjmK0dQV/9c+/ewkHBU9jZRfDxNMcot+eKUpZduO
    xzUR/tDACt1ZkT1SOwRDAxaMs6+js=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("as whitespace sensitive", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_format_tags_sc ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=qfPkkUJh95JfdvuR67QiOVg0n+krrwlShqIFXu03EvgP+1wJHVJy6M497OPlK1QC3FGXBL
    k2Af5aTM9pyRO4bDX7N21jvGLoF2soMk9r6Er78OFalImdz7rRdFu33PR3dMCFe2cjGkPmAO
    94UKj789r5lfy2+QQwQskXQX/r3pw=; cv=none; d=example.org; i=1; s=dummy;;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("as random semi colon error", input, locator, ArcSignatureValidationResult.Fail);
		}

		#endregion
		#region Arc Seal Fields

		[Test]
		public void as_fields_i_dup ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("duplicate as", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_i_dup2 ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=IAqZJ5HwfNxxsrn9R4ayQgiu9RibPKEUVevbt7XFTkSh1baJ533D2Z6IZ2NaBreUhDBb2e
    K9Gtcv+eyUhWkD8VTmE6fq/F8CDIK3ScIiJykF8hNL1wpa/mGwWWwBnkozIJGAbTAAX7AgnH
    knAehnSW99TeU0lmib0XmOt4TN3sY=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Seal: a=rsa-sha256;
    b=IAqZJ5HwfNxxsrn9R4ayQgiu9RibPKEUVevbt7XFTkSh1baJ533D2Z6IZ2NaBreUhDBb2e
    K9Gtcv+eyUhWkD8VTmE6fq/F8CDIK3ScIiJykF8hNL1wpa/mGwWWwBnkozIJGAbTAAX7AgnH
    knAehnSW99TeU0lmib0XmOt4TN3sY=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=2cDGNznUmp4YSSThCe9nrQIH2Gpd5qPFw3OU8sWFzZgEQ5UZtaVQifVUXUrsSyEzjro3Ul
    YPPDx+C1K+LbKRlOZ06il4ws2zlPafsrx1piKsKSCUq0KjFs01hYCDBa3tfdyITSfoWu2HHY
    pCjrhPMPH1jruIdBV/5Gk2Fvy+mW8=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("duplicate AS i=2", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_i_missing ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=uQtcSwvWdWWtq6seWx/+hrglo0DIevtxBse073F81rkPD9R2U9I11RE+rTyP1f49VmtxOX
    dQQY3hMLr+174d1LdaOrO7w98KKt/sAHkuVGaeUrNCsaPWSVyECdoQwEIh140FzHkW+6DGcC
    KYTb3l2Kb0/AH9RdhJ1kOft2hrOeY=; cv=none; d=example.org; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as fields i= missing", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_a_sha1 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha1;
    b=4aSdpG91pnuWdSUXPchtTfnFSWkenWJh1zIKLwT2EVkCNJ+/5clRA5sFonDxmdOrcEgzrh
    jiJuxnZVYXdIkvW9rMe5BOG5walucWYuNkuO7ph0kRX8DRITxwiZYhFgk8OkCITDYNF6h5vr
    rMF5vOKCaWnpiGTUlPqBOgakyN9F0=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as a= is sha1", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_a_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal:
    b=gwz9qNZBYlrJY6xGdb0IUhGEwAlOmJsjSfyp8FmWwlJs9URrrikXoFcJ5dJkYbFAZNfXU4
    58XhxWSgJ8x2PMN2lkZ1TkL29SRhgdn5VAjnjHpr4xE/2i1hHcZ23Nj/bhXe0TOJq1n5hoKk
    Atsos2ADb7r+Nf0AOnNle+/vTnS+4=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as a= not avaliable", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_a_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=;
    b=ZVTTqGNJFz60CcTjJvJ2TBUgObDEGLOzdYTh+abJ+DiXUWfUJWxM5WD/dU3C0vjBu6Qcke
    8swPTOsTL3lL1v0ywSQCN+ZuFbEn7fy9AMvXadmDgEuht5qrtSQG9rsuF9m0VePnf6k45HlX
    3nICQtx7sQY16JBG4CTrBQWYSpaDU=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as a= is empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_a_unknown ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha42;
    b=cTOOMtw0jXisFRCtFshIVNExbNgtOyrGNUqWObVvJPmMYBNbAfG3y1101xcd4nrfZ2skNr
    xn12jM1JPwHBu5Ps4qjEeHDvxJK09vbxiOxviu5SDNhVUJS5V3l2VBagMpyuO5BL1OG6wjy/
    Xuzt1Iuhk23cJ5S98SqOVik9CCblE=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as a= unknown", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_b_ignores_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NV E6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT6 5S0+ vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= ignores whitespace", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_fields_b_head_case1 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-SEAL: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= canonicalization ignores header case", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_fields_b_head_unfold1 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= canonicalization headers unfolded", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_fields_b_eol_wsp1 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr    
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= canonicalization strips eol whitespace", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_fields_b_inl_wsp1 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=;    cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= canonicalization reduces inline whitespace", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_fields_b_col_wsp ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal:   a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= canonicalization strips whitespace around colon", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_fields_b_head_case2 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-SEAL: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= canonicalization header key case insensitive", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_fields_b_head_unfold2 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ 7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= canonicalization headers unfolded", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_fields_b_eol_wsp2 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ     
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= canonicalization eol whitespace stripped", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_fields_b_inl_wsp2 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=;       cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= canonicalization inline whitespace reduced", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_fields_b_512 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=DCbMvnfI7UzqahO9GFjYXa7DAcon0abOMQ7mWykqtdkEe+rqeQmsy1/pV9oAeSrT9giBqP
    +cBNepG4Nycj93KQ==; cv=none; d=example.org; i=1; s=512;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=BFnboE5xz5OBBIZeB04CaX0QVCRysZesZNKLQLDbq3ohfHL0eIkMWyt/ZkP3+bg7wVEtyb
    QfqbbfDRTQYC3GBA==;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=512; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= with 512 bit key", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_b_1024 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=JZIhBQD/1SCIn7IUrIoqCDFZ4k2tDd5joLebC7dCEbEXy6HURnayDygFjEiVwoVjF8XZPo
    tDSWEVj18YLFQ08HZigNNDmhAdtIAeHs5bTfhz3ZDKGISGSrVbUqvS5QaL2dwaY5V3FhH1QC
    VEohhbx3rJKMBiFCbQoCRo555WNL0=; cv=none; d=example.org; i=1; s=1024;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=jCTMZoXkSSVEusJyP9cbvAoKEDLphi95R/yaX9+gWw2t/RduqINzxPSVJZUq8uVCbKdB5F
    BlBb2m7zbwaq6/oemTqI1tcnRaAt66Z0cyOKfPjRINTm9C8E3hUoI9DzplkwEoqmhR0wOjcJ
    H6ASJr96Kl5qLu092VFaQYYxkwh2I=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=1024; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= with 1024 bit key", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_fields_b_2048 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=R6I8tV4Y0pBQWId+r4W9L3TDi82iVPot9d+ux5u69ET/VUTQUPFAiRfTBqMKAm0dY1HCdU
    JZggmlvj9BwZMOO9pFi8O1EXqkJ1CpNtFyNn76Get96owYXh7LlcP/C/a5AmxZMmvKblloh5
    1rL2cNWicsp8/y3NS8jO0KWpSis2jK2yMn+r9gJ5gM2sUiBsKDwiYAhFBhjD8SFQOaG6DzLa
    mJzCw9FkuGdpLfQoNDq2lLQq6APq8GihFJai7o/s8M4FItAMoteuqxIfyYuH60oX4qNOsaIT
    B/6DnRCFshABODpSHRRIH4EvCu2fYYo6YDIU3VvDH2wOO5fQMcgvUoNw==; cv=none;
    d=example.org; i=1; s=2048; t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=M0YyrXMDoG5zJ0ZjFzUqFNoDFatu/QxWTjyAH5wPvPRiSqw2Vvd4A1Al8VjYfmgbP4Jd8f
    TFDZg1kWwLYk2IO/th/P6iYPfyDg5qp6mgao/V8NBW9P/Mqlb+xhkn4R8c44vmen9atIUV3Z
    04QzziVeuBxj+NFqxprbxf42Faxv5XymGmW3ZWVhOLEpwfcjy933drLsfZQezhyYlx4klptI
    v3hKM76++GaIUc1nWXvmkeKKjEQLiUzqxd9Om7SRNArNe/q5xnVIaufxSfZNUtTT/o7Ic1Br
    t7ZV8qwmj37sYpdZUo6H7QN+dp8E/J0jnbI0ZQU2mv8Gj3FqGOGzKwGQ==;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=2048; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= with 2048 bit key", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_fields_b_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= not available", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_b_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256; b=;
    cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_b_base64 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256; b=yo-mama;
    cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= not base64", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_b_mod_sig ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=kKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= with modified signature", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_b_aar1 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=yo-mama; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= modified aar1", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_b_ams1 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=I+Px9EfvrAFqYUnYPrC+egeUwxCg1LdNSIJ6v5sQfwua0Ox37z2S5GdknSdfjYKVDju/3p
    49rDu1wy6xLD5byG2qV2IDUCKmNH4QY6yGhb7ADmfrHDdICMYf7UDIBL6nUQsZHPeAUn5HbK
    e/PCXEu9m+wpAEuDKvZxUNbNMWdQM=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= modified ams1", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_b_asb1 ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy2;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as b= modified asb1", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_cv_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=u92e/TiZvqtkvgZxGLd6EF9fZvoFJ5mqvwEkb3m5lXbgo8/wN+iliOx07lU/rOIsHa9YqL
    QYUapWkkowhdKKFkixhefUUoeo9n9SIcpV8wywx3szGOhrwyHJIBCWr7nqaEQAS4prJzVZCi
    4eEkPPQ5OdzZGMu7j53QCVzdmWsdo=; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as cv= not avaliable", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_cv_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=EhiACA5ymmD9SeCvreg2D+83hTBtus1JsP2KC0CUBOgmdMeiGuCIHiJG5WAWpXhd9RQhRo
    nxTU8kGfPl2sF2GAOujiie2cenkejRgwYQv+MLeRCT1MALEvrOsytShDl5reRltuX9ULPomS
    GBagChWg+NI9bZGMxnntr38QPFj5A=; cv=; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as cv= empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_cv_invalid ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=cqTZxmxNU17ZUNfRJp6FvTfpkak6t2MCK65F3ppS018sPBGTyupD5q8GHdozKW5iCIaFhE
    rZQmKOD2z33Z/h2eiFY101fFxesgDJpMgbeo0zGfgmJcj1v6nDXgq1FrUNPaauBOJV8Nb5vs
    B0H2qSdtArOeD0mlATMKfESPA5mGw=; cv=:); d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as cv= invalid", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_d_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=YPZ23nGkCxrMd1193xVDIR5/J9Kz0AOanbuHATqRJUDT+KkiB8z6+vk+3qsUiH8/7+YyI/
    Qqmd5O66qPq/ntXMaPnUhpQgKTj33KcGtD7j9m59imfosbjwTWrVatHo8okYmeh61ZxI8LGF
    ivZFDrqRI2YLfIG313PtmdUqZJFcY=; cv=none; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as d= not available", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_d_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=pp0092UMeSYNP/NMoINT5QeuCXJ/LKOw4Sotuzu+XM4RFHL8CbHLWT/stFYpif9tVsVaEu
    h0SgrYexYI+lvEqslpSdCIgvVanRoSVC2bn58OSVVpZ/8r6/8iIXdN/upGPZRhbJLtSwuRk/
    kbKzXfLTY9yy1SMSAqLXAG2MWBQpg=; cv=none; d=; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as d= empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_d_invalid ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=e44ivGolh5WOt+GK0xvrRrWcEQnLTpbmx4VqK+osiYiEceAJl6RdIuaG5Sdvl8JLbUcHJf
    7Z1nOuA71nrpTSGEh4kE5bgR/XAtxElq4czlU2B21nDUI5iO5IJTZx5uxYuhVh500OfFxKvP
    vk/65F1L8kU45uMhTjih304WuYZ7w=; cv=none; d=***; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as d= invalid", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_h_present ()
		{
			const string input = @"Return-Path: <jqd@d1.example.org>
ARC-Seal: i=1; cv=none; a=rsa-sha256; d=example.com; s=test; t=12345; 
 h=message-id : date : from : to : subject : from; 
 b=mIurIuLl0/wAxWhA4DBS1wsUE15IBnmJ7o3sH15hIuesdD4smz1cCLXVhRtxQE
 rVtVLv4OgNCgdFsB5zbSOUao2bSSYP6y0BGyCWvr+hU4tai5axIc1Kfwbtv/0Mqg
 waiGJPreOAAeZOJ4vPfdaAbSXlN5MI4PHW89U82FSIBKI=
ARC-Message-Signature: i=1; a=rsa-sha256; c=relaxed/relaxed; 
 d=example.com; s=test; t=12345; h=message-id : 
 date : from : to : subject : from; 
 bh=wE7NXSkgnx9PGiavN4OZhJztvkqPDlemV3OGuEnLwNo=; 
 b=a0f6qc3k9eECTSR155A0TQS+LjqPFWfI/brQBA83EUz00SNxj
 1wmWykvs1hhBVeM0r1kEQc6CKbzRYaBNSiFj4q8JBpRIujLz1qL
 yGmPuAI6ddu/Z/1hQxgpVcp/odmI1UMV2R+d+yQ7tUp3EQxF/GY
 Nt22rV4rNmDmANZVqJ90=
ARC-Authentication-Results: i=1; lists.example.org; arc=none;
  spf=pass smtp.mfrom=jqd@d1.example;
  dkim=pass (1024-bit key) header.i=@d1.example;
  dmarc=pass
Authentication-Results: lists.example.org; arc=none; spf=pass smtp.mfrom=jqd@d1.example; dkim=pass (1024-bit key) header.i=@d1.example; dmarc=pass
Received: from localhost
Message-ID: <example@example.com>
Date: Mon, 01 Jan 2011 01:02:03 +0400
From: Test User <test@example.com>
To: somebody@example.com
Subject: Testing

This is a test message.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as has h= which is invalid", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_s_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=P6fYgm79ZAak3Jov/xVCFA565vivmIK1TRc3a5bXLaK0ITMGov8fPDfBSlkicrEA7+klCS
    U+N4M70a873UxJAhtbW8aTgFfGA71WeXTJtsUO9k221Xg3TosedH0Pv7Hw6H5+xwfREaHwzW
    609JaRP4xYSgiRQwbV53oJLXsUBA4=; cv=none; d=example.org; i=1;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as s= not available", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_s_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=s5XP6OVPaP6aRAUllKkgklTcVFSRt0BuJ/KsHSBkzUlu8tlc8xHNLLQh8kSj93G91Nzrht
    2TSNCGbDv2n+fTkUBvUw0Gv+rS+w/cGv3487x/0D3kKMY/AsnmbmKYy7demRQZueTjZg4oBd
    ictli/7xMAt6WUmqpssBKMK5wsfp0=; cv=none; d=example.org; i=1; s=;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as s= empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_t_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=JBVkqSG5Cg5D7lzXzyoLhk5/SMvhUeqxndqDKDjLQSici34r5d+fQIkUosiU17/jueiGc9
    UpZl2Gv6wPs7TkgwxfK7GG/1d4P+/cYE6efo8xuPSZGxoSQEZhKTjXL9Apup8Up3e/J2xBjs
    veRG7RbXqMc/vL/tmGxsJ9aSjSIKI=; cv=none; d=example.org; i=1; s=dummy
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as t= na", input, locator, ArcSignatureValidationResult.Pass);
		}

		[Test]
		public void as_fields_t_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=s8yFqRcEb3EBxbl8zc3/Q7Yni59wWhj7+NDrxRaVvVJ2f/e4FKJTIcz+0i9z+VhmX41Zu5
    Rh2CQwD6bnkOvtHIuHoxI4LxOhs/lvhkcBieiqGR4ZeZlOy8n6mmnbuHIi151pNXK79ZxRdr
    2axc4DYl57RmQKI+jVPwiMygli/f0=; cv=none; d=example.org; i=1; s=dummy;
    t=
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as t= empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void as_fields_t_invalid ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=K4NE5fZ5bskXwVAbySXpU9ys/ls+gL97+qh/HSFgSAkBzAxQ355pWkGTKLG3SX95OEljIO
    tFiBuwYKIiBXwYbl6vpsZpjS3AwwdtV+rFqwVT0oCwRv2SU8v+wvg/2uzgUMciit+WNI0sYr
    +HgFzkt6yR3Jpg8Y/49qKPXZcYR3I=; cv=none; d=example.org; i=1; s=dummy;
    t=-123.4
ARC-Message-Signature: a=rsa-sha256;
    b=uB8ov69KIWfAiTqT9UOTg9p4m0u8Zi01NUVf0iyzeNBpJR9VecE81x2VzQBxfPnp5p3uSd
    H7A/ExuHutPbPzSJh62u0HpIIoSoxzZtSeESFwIJJe81Iv8SiuIuwCtih+wcNxPEoou7G0F3
    fRI+n99QEFryjk9dsPBGW4NFxNzIA=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("512._domainkey.example.org", "v=DKIM1; k=rsa; p= MFwwDQYJKoZIhvcNAQEBBQADSwAwSAJBAIWmlgix/84GJ+dfgjm7LTc9EPdfk ftlgiPpCq4/kbDAZmU0VvYKDljjleJ1dfvS+CGy9U/kk1tG3EeEvb82xAcCAwEAAQ==");
			locator.Add ("1024._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCyBwu6PiaDN87t3DVZ84zIrE hCoxtFuv7g52oCwAUXTDnXZ+0XHM/rhkm8XSGr1yLsDc1zLGX8IfITY1dL2CzptdgyiX7vgYjzZqG368 C8BtGB5m6nj26NyhSKEdlV7MS9KbASd359ggCeGTT5QjRKEMSauVyVSeapq6ZcpZ9JwQIDAQAB");
			locator.Add ("2048._domainkey.example.org", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAv+7VkwpTtICeJFM4Hf UZsvv2OaA+QMrW9Af1PpTOzVP0uvUFK20lcaxMvt81ia/sGYW4gHp/WUIk0BIQMPVhUeCIuM1mcOQNFS OflR8pLo916rjEZXpRP/XGo4HwWzdqD2qQeb3+fv1IrzfHiDb9THbamoz05EX7JX+wVSAhdSW/igwhA/ +beuzWR0RDDyGMT1b1Sb/lrGfwSXm7QoZQtj5PRiTX+fsL7WlzL+fBThySwS8ZBZcHcd8iWOSGKZ0gYK zxyuOf8VCX71C4xDhahN+HXWZFn9TZb+uZX9m+WXM3t+P8CdfxsaOdnVg6imgNDlUWX4ClLTZhco0Kmi BU+QIDAQAB");

			Validate ("as t= invalid", input, locator, ArcSignatureValidationResult.Fail);
		}

		#endregion
		#region AAR Set Structure

		[Test]
		public void aar_struct_i_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=XPyfGHSOsXbhiqnRuLe8aUcX7VI+ULipPwkVdyFW3vrDgWis0ZGj5Exi7MVCEZqHCPRrz6
    cE/MCiMIKvLKaNOoN2RiMmGxReyuMqxB1cFgrlYSsY2juOuKruRwnyvdojfJKxkZtuwbCbEI
    oP7AxLlImiJh8dL65hcqiVYYwkCVo=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=pT/KGdemFeMECKnNp/zUgBi7JEBkqLYi6OiuMNFk1lu9MvIVAphMo5Qd+HwcmduHcKnTuE
    BR6G1f+FvrikTsz71tFpmz7YMQDVfnd889YqzIMfkrHVmYz3Tqkm/leEozN+3QSDthphCGja
    elxeYITZ88vPyJgjqeB1RZbZA8w1w=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass                    
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("AAR i= NA", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void aar_struct_i_empty ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=2bCJqMNBHeHfGn4ydivoDNuXl5l4/uFJRjSq20B1IFuEwXRBItAHyrnJ2kAeKyN2vDyF38
    aeVMW3JpEr6YYbdWz/QIbn9PfQRLJw8mDAdIWIeDf77ckQL9pBS9u4KnPzSRlvBrMvTJcHI0
    X78AQ+sF7GHuW+TQE3w76leY5chNg=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=ssDGg3IC3l9eIU0QVB6442p+icX6zrpaCDc6MdnR9waROdPPl4ThcRHhU19nHBlEqvc6Nj
    heWBORRpH93dQ2gC1dh/kYpOu7GUr7rbTzO3FYICnbTCbZWHHkyRPjKGUDLKZxt2zOnT1g+I
    FiLSqI5zVyKX98w5q0cUroUaFYyOg=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass                    
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("AAR i= empty", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void aar_struct_i_zero ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=KylBhAsgxwZcei83U8RCLOL6VAKrUTEqgMRFrfs4wdvOfJhb4QLzeJoNuL0rru8pWd/y9/
    zEYgBxCGpOGkoEXyzrDE+2wbNPn04kjJuTpMfRbHljy6kjHXRf8jgy44iDS6zx2dkV+YRP4n
    STyZLdj+YcIrTvl/DSzidIX+QF8tE=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=G459JFdl2PXVaqhTFJwqpBaOCUiASVtWpkiQrIPiLPDpPoGT3AhaoPDpM3ogUhURRBAkQD
    bJqcY2XJ2F2NAWf260C7T/q0DlO6D0/E6IsqiY5seqiBCPIGf4B2yMjnQXf1qTHFNbbJGICi
    k6bE4r1ZCT7Cu9CWFCCq9TWEthhfw=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=0; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass                    
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("AAR i=0", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void aar_struct_invalid ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=ye9OTXk3fAWUMwhQFsBSTjaDkrXXFzifyP5c7No0TriPbeFK0cayi6FgudLVsSFLvibCAJ
    txOi+Zfx9rn0TyhsNspRg/PY8+VSZJZtxOW7cJ/6nLZPh3XKfhx39QDQPjyc3dd03bpAckRH
    b6vJuM9vmpgB4y3WnnvWH9H05wfMo=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=fusc8GIuNpkjd8jbi8g5feEaRugmDELIl/M3u+QCZjF4Sw4SVFS8tRy8DI8XA/49D4mfmc
    NuClgRzBZJSeyd1w6tDyt0mebBKMAWqJXK25B6ON3QTeXTudB5447VckaoUn0k+U75fkyiKk
    l2ZmwWGNx0jBif2Py0gSwhFajD77g=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=squanch; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass                    
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("AAR i= invalid value", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void aar_struct_dup ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass          
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("Duplicated AAR for i=1", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void aar_struct_missing ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=dOdFEyhrk/tw5wl3vMIogoxhaVsKJkrkEhnAcq2XqOLSQhPpGzhGBJzR7k1sWGokon3TmQ
    7TX9zQLO6ikRpwd/pUswiRW5DBupy58fefuclXJAhErsrebfvfiueGyhHXV7C1LyJTztywzn
    QGG4SCciU/FTlsJ0QANrnLRoadfps=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.      
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("Missing AAR for i=1", input, locator, ArcSignatureValidationResult.Fail);
		}

		#endregion
		#region Arc Authentication Results

		[Test]
		public void aar_missing ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("missing arc authentication results", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void aar_i_missing ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("arc authentication results no i= tag", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void aar_i_wrong ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=2; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("arc authentication results wrong i= tag", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void aar_i_not_prefixed ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: lists.example.org; i=1;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("arc authentication results i= not prefixed", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void aar_i_no_semi ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1 lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("arc authentication results i= no semicolon", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void aar2_missing ()
		{
			const string input = @"MIME-Version: 1.0
ARC-Seal: a=rsa-sha256;
    b=CiZp+ZloBeWiIyjY+Eq0lKt20KQDF3QIJNw7+/jdjtQ1XTSMhHsli7H/ocIXsiU/kLF5pn
    pABQiZPvAWfCaEcCA9lyb/7i3q2i72GLdK1vdrdD2nIM5e7L3u/5Z56SJdKTu46SyoFQve9b
    Cp7qoQB9/TUTxxvkDoapsSjDCDqZ0=; cv=pass; d=example.org; i=2; s=dummy;
    t=12346
ARC-Message-Signature: a=rsa-sha256;
    b=A2OCip1Cf9z6X7ML9/bRajnToeCD3H7IkP7YqmSKqDtn8Yu8oaJdwP0lZfCTjX++Qas9nj
    tGWMojFpj8Wd2rzdyMXwUWF3xlcFBD2gApO9xbehIASIF4lFQMyP6D80LjsjdtpstgwGZl9P
    y6WTyD1Kw/bNPZadxvNeDg3LVcQpo=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=2; s=dummy; t=12346
Received: by 10.157.11.240 with SMTP id 103csp420860oth;
    Fri, 6 Jan 2017 14:27:31 -0800 (PST)
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=RkKDOauVsqcsTEFv6NVE6J0sxj8LUE4kfwRzs0CvMg/+KOqRDQoFxxJsJkI77EHZqcSgwr
    QKpt6aKsl2zyUovVhAppT65S0+vo+h3utd3f8jph++1uiAUhVf57PihDC/GcdhyRGa6YNQGh
    GoArSHaJKb06/qF5OBif8o9lmRC8E=; cv=none; d=example.org; i=1; s=dummy;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=SMBCg/tHQkIAIzx7OFir0bMhCxk/zaMOx1nyOSAviXW88ERohOFOXIkBVGe74xfJDSh9ou
    ryKgNA4XhUt4EybBXOn1dlrMA07dDIUFOUE7n+8QsvX1Drii8aBIpiu+O894oBEDSYcd1R+z
    sZIdXhOjB/Lt4sTE1h5IT2p3UctgY=;
    bh=dHN66dCNljBC18wb03I1K6hlBvV0qqsKoDsetl+jxb8=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
Received: by 10.157.52.162 with SMTP id g31csp5274520otc;
        Tue, 3 Jan 2017 12:32:02 -0800 (PST)
X-Received: by 10.36.31.84 with SMTP id d81mr49584685itd.26.1483475522271;
        Tue, 03 Jan 2017 12:32:02 -0800 (PST)
Message-ID: <C3A9E208-6B5D-4D9F-B4DE-9323946993AC@d1.example.org>
Date: Thu, 5 Jan 2017 14:39:01 -0800
From: Gene Q Doe <gqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 2
Content-Type: multipart/alternative; boundary=001a113e15fcdd0f9e0545366e8f

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/plain; charset=UTF-8

This is a test message

--001a113e15fcdd0f9e0545366e8f
Content-Type: text/html; charset=UTF-8

<div dir=""ltr"">This is a test message</div>

--001a113e15fcdd0f9e0545366e8f--
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");

			Validate ("aar missing for i=2", input, locator, ArcSignatureValidationResult.Fail);
		}

		#endregion
		#region Public Key

		[Test]
		public void public_key_na ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=xEoL/6DZn2+/oIsSIAFRrnQdhyrH/aSGdRqBphcyZvTLhDyd8sPHIqNsr0HROjIybe3lUG
    /YlYIftmAUP3E7kWbfU7HrolZ/5f4eB0tciltpSyBUPzM2D30IxGmqUvQxk5ATb7WxKAUs4x
    XiTmx1MaAUKAExlm45pwp5wEoU/D8=; cv=none; d=example.org; i=1; s=na;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345      
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example2.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDR3lRpGZS+xO96Znv/BPNQxi m7ZD0v6yFmZa9Rni5FHCeWuQwcp+PH/XXOyF6JsmB+kS0ybxJnx594ulqH2KvLMNsGAD+yRl2bJSXbBH ea7K9C5WX8Vjx3oPoGgw7QCONptnjUsbIIoxUZBEUe17eG44H/PbDqGwCBiyI20KEC/wIDAQAB");
			locator.Add ("invalid._domainkey.example.org", "v=DKIM1; k=rsa; omgwhatsgoingon");

			Validate ("public key not available", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void public_key_invalid ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=G6sqFlzmC87EiD80V9Da8JURM2MUxp1tK3iUxrQdSJ6odUYPT8ApwE1GWodzs8UDuKemL+
    qn7E29nhcK8pwjLjWNilPTZJ1Bt1TS8QersJsEe4tD+rcbGd8ZU8C2UcUpv0TFv3m4GrNbwx
    JFFf9r1x5VkXulzTwIo1VW6avKShw=; cv=none; d=example.org; i=1; s=invalid;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example2.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDR3lRpGZS+xO96Znv/BPNQxi m7ZD0v6yFmZa9Rni5FHCeWuQwcp+PH/XXOyF6JsmB+kS0ybxJnx594ulqH2KvLMNsGAD+yRl2bJSXbBH ea7K9C5WX8Vjx3oPoGgw7QCONptnjUsbIIoxUZBEUe17eG44H/PbDqGwCBiyI20KEC/wIDAQAB");
			locator.Add ("invalid._domainkey.example.org", "v=DKIM1; k=rsa; omgwhatsgoingon");

			Validate ("public key invalid", input, locator, ArcSignatureValidationResult.Fail);
		}

		[Test]
		public void ams_as_diff_s_d ()
		{
			const string input = @"MIME-Version: 1.0
Return-Path: <jqd@d1.example.org>
ARC-Seal: a=rsa-sha256;
    b=Q6K/T+/5h+nkCtO8UVhb5uwy5ozplfBvOV0lSOCIuzDoTlPNg1chaN+04US/AWxvOrBTZf
    hzXXdVjXMv2sX4+4ebSegZN7GTakDCd+vfBtF30jR4csBqlhW25NSyLeleZnIMf5I5G4vu5+
    Ab38xWCoKnMKTPsPebT273ALMfzOw=; cv=none; d=example2.org; i=1; s=dummy2;
    t=12345
ARC-Message-Signature: a=rsa-sha256;
    b=QsRzR/UqwRfVLBc1TnoQomlVw5qi6jp08q8lHpBSl4RehWyHQtY3uOIAGdghDk/mO+/Xpm
    9JA5UVrPyDV0f+2q/YAHuwvP11iCkBQkocmFvgTSxN8H+DwFFPrVVUudQYZV7UDDycXoM6UE
    cdfzLLzVNPOAHEDIi/uzoV4sUqZ18=;
    bh=KWSe46TZKCcDbH4klJPo+tjk5LWJnVRlP5pvjXFZYLQ=; c=relaxed/relaxed;
    d=example.org; h=from:to:date:subject:mime-version:arc-authentication-results;
    i=1; s=dummy; t=12345      
ARC-Authentication-Results: i=1; lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: from segv.d1.example (segv.d1.example [72.52.75.15])
    by lists.example.org (8.14.5/8.14.5) with ESMTP id t0EKaNU9010123
    for <arc@example.org>; Thu, 14 Jan 2015 15:01:30 -0800 (PST)
    (envelope-from jqd@d1.example)
Authentication-Results: lists.example.org;
    spf=pass smtp.mfrom=jqd@d1.example;
    dkim=pass (1024-bit key) header.i=@d1.example;
    dmarc=pass
Received: by 10.157.14.6 with HTTP; Tue, 3 Jan 2017 12:22:54 -0800 (PST)
Message-ID: <54B84785.1060301@d1.example.org>
Date: Thu, 14 Jan 2015 15:00:01 -0800
From: John Q Doe <jqd@d1.example.org>
To: arc@dmarc.org
Subject: Example 1

Hey gang,
This is a test message.
--J.
";
			var locator = new DkimPublicKeyLocator ();

			locator.Add ("dummy._domainkey.example.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDkHlOQoBTzWRiGs5V6NpP3id Y6Wk08a5qhdR6wy5bdOKb2jLQiY/J16JYi0Qvx/byYzCNb3W91y3FutACDfzwQ/BC/e/8uBsCR+yz1Lx j+PL6lHvqMKrM3rG4hstT5QjvHO9PzoxZyVYLzBfO2EeC3Ip3G+2kryOTIKT+l/K4w3QIDAQAB");
			locator.Add ("dummy2._domainkey.example2.org", "v=DKIM1; k=rsa; p=MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDR3lRpGZS+xO96Znv/BPNQxi m7ZD0v6yFmZa9Rni5FHCeWuQwcp+PH/XXOyF6JsmB+kS0ybxJnx594ulqH2KvLMNsGAD+yRl2bJSXbBH ea7K9C5WX8Vjx3oPoGgw7QCONptnjUsbIIoxUZBEUe17eG44H/PbDqGwCBiyI20KEC/wIDAQAB");
			locator.Add ("invalid._domainkey.example.org", "v=DKIM1; k=rsa; omgwhatsgoingon");

			Validate ("differing domains & selectors across ams & as", input, locator, ArcSignatureValidationResult.Pass);
		}

		#endregion
	}
}
