//
// WindowsSecureMimeDigitalSignature.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2017 Xamarin Inc. (www.xamarin.com)
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
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace MimeKit.Cryptography
{
    /// <summary>
    /// An S/MIME digital signature.
    /// </summary>
    /// <remarks>
    /// An S/MIME digital signature.
    /// </remarks>
    public class WindowsSecureMimeDigitalSignature : IDigitalSignature
    {
        DigitalSignatureVerifyException vex;
        bool? valid;

        internal WindowsSecureMimeDigitalSignature (SignerInfo signerInfo)
        {
            SignerInfo = signerInfo;
        }

        WindowsSecureMimeDigitalSignature ()
        {
        }

        /// <summary>
        /// Gets the signer info.
        /// </summary>
        /// <remarks>
        /// Gets the signer info.
        /// </remarks>
        /// <value>The signer info.</value>
        public SignerInfo SignerInfo {
            get; private set;
        }

        /// <summary>
        /// Gets the list of encryption algorithms, in preferential order,
        /// that the signer's client supports.
        /// </summary>
        /// <remarks>
        /// Gets the list of encryption algorithms, in preferential order,
        /// that the signer's client supports.
        /// </remarks>
        /// <value>The S/MIME encryption algorithms.</value>
        public EncryptionAlgorithm[] EncryptionAlgorithms {
            get; internal set;
        }

        #region IDigitalSignature implementation

        /// <summary>
        /// Gets certificate used by the signer.
        /// </summary>
        /// <remarks>
        /// Gets certificate used by the signer.
        /// </remarks>
        /// <value>The signer's certificate.</value>
        public IDigitalCertificate SignerCertificate {
            get; internal set;
        }

        /// <summary>
        /// Gets the public key algorithm used for the signature.
        /// </summary>
        /// <remarks>
        /// Gets the public key algorithm used for the signature.
        /// </remarks>
        /// <value>The public key algorithm.</value>
        public PublicKeyAlgorithm PublicKeyAlgorithm {
            get; internal set;
        }

        /// <summary>
        /// Gets the digest algorithm used for the signature.
        /// </summary>
        /// <remarks>
        /// Gets the digest algorithm used for the signature.
        /// </remarks>
        /// <value>The digest algorithm.</value>
        public DigestAlgorithm DigestAlgorithm {
            get; internal set;
        }

        /// <summary>
        /// Gets the creation date of the digital signature.
        /// </summary>
        /// <remarks>
        /// Gets the creation date of the digital signature.
        /// </remarks>
        /// <value>The creation date in coordinated universal time (UTC).</value>
        public DateTime CreationDate {
            get; internal set;
        }

        /// <summary>
        /// Verifies the digital signature.
        /// </summary>
        /// <remarks>
        /// Verifies the digital signature.
        /// </remarks>
        /// <returns><c>true</c> if the signature is valid; otherwise <c>false</c>.</returns>
        /// <exception cref="DigitalSignatureVerifyException">
        /// An error verifying the signature has occurred.
        /// </exception>
        public bool Verify ()
        {
            if (valid.HasValue)
                return valid.Value;

            if (vex != null)
                throw vex;

            if (SignerCertificate == null) {
                var message = string.Format ("Failed to verify digital signature: missing certificate.");
                vex = new DigitalSignatureVerifyException (message);
                throw vex;
            }

            try  {
                var certificate = ((SecureMimeDigitalCertificate) SignerCertificate).Certificate;
                SignerInfo.CheckSignature (false);
                valid = true;

                return true;
            } catch (Exception ex) {
                var message = string.Format ("Failed to verify digital signature: {0}", ex.Message);
                vex = new DigitalSignatureVerifyException (message, ex);
                throw vex;
            }
        }

        #endregion
    }
}