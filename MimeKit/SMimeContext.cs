//
// SMimeContext.cs
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
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace MimeKit {
	public class SMimeContext
	{
		public string SignatureProtocol {
			get { return "application/pkcs7-signature"; }
		}

		public string EncryptionProtocol {
			get { return "application/pkcs7-mime"; }
		}

		public string KeyExchangeProtocol {
			get { return "application/pkcs7-keys"; }
		}

		public void Sign (CmsSigner signer, byte[] content, out MimePart signature)
		{
			var contentInfo = new ContentInfo (content);
			var cms = new SignedCms (contentInfo, true);

			cms.ComputeSignature (signer, false);

			var data = cms.Encode ();

			signature = new ApplicationPkcs7Signature ();
			signature.ContentObject = new ContentObject (new MemoryStream (data), ContentEncoding.Default);
		}

		public SignerInfoCollection Verify (byte[] content, byte[] signature)
		{
			var contentInfo = new ContentInfo (content);
			var cms = new SignedCms (contentInfo, true);

			cms.Decode (signature);
			cms.CheckSignature (false);

			return cms.SignerInfos;
		}

		public void Encrypt (string userId, bool sign, HashAlgorithm digestAlgo, string[] recipients, Stream input, Stream output)
		{
			throw new NotImplementedException ();
		}

		public void Decrypt (Stream input, Stream output)
		{
			throw new NotImplementedException ();
		}

		public void ImportKeys (Stream stream)
		{
			throw new NotImplementedException ();
		}

		public void ExportKeys (string[] keys, Stream stream)
		{
			throw new NotImplementedException ();
		}
	}
}
