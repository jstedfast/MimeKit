//
// CmsSigner.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Pkcs;

namespace MimeKit.Cryptography {
	public sealed class CmsSigner
	{
		public CmsSigner ()
		{
			UnsignedAttributes = new AttributeTable (new Dictionary<DerObjectIdentifier, Asn1Encodable> ());
			SignedAttributes = new AttributeTable (new Dictionary<DerObjectIdentifier, Asn1Encodable> ());
			DigestAlgorithm = DigestAlgorithm.Sha1;
		}

		public CmsSigner (IEnumerable<X509CertificateEntry> chain, AsymmetricKeyEntry key)
		{
			if (chain == null)
				throw new ArgumentNullException ("chain");

			if (key == null)
				throw new ArgumentNullException ("key");

			CertificateChain = new List<X509Certificate> ();
			foreach (var entry in chain) {
				CertificateChain.Add (entry.Certificate);
				if (Certificate == null)
					Certificate = entry.Certificate;
			}
		}

		public CmsSigner (X509Certificate certificate, AsymmetricKeyParameter key) : this ()
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (key == null)
				throw new ArgumentNullException ("key");

			CertificateChain = new List<X509Certificate> ();
			CertificateChain.Add (certificate);
			Certificate = certificate;
			PrivateKey = key;
		}

		public X509Certificate Certificate {
			get; set;
		}

		public IList<X509Certificate> CertificateChain {
			get; set;
		}

		public DigestAlgorithm DigestAlgorithm {
			get; set;
		}

		public AsymmetricKeyParameter PrivateKey {
			get; set;
		}

		public AttributeTable SignedAttributes {
			get; set;
		}

		public AttributeTable UnsignedAttributes {
			get; set;
		}
	}
}
