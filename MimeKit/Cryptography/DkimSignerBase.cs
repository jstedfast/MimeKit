//
// DkimSignerBase.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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

namespace MimeKit.Cryptography {
	/// <summary>
	/// A base class for DKIM and ARC signers.
	/// </summary>
	/// <remarks>
	/// The base class for <see cref="DkimSigner"/> and <see cref="ArcSigner"/>.
	/// </remarks>
	public abstract class DkimSignerBase
	{
		/// <summary>
		/// Initialize a new instance of the <see cref="DkimSignerBase"/> class.
		/// </summary>
		/// <remarks>
		/// <para>Creates a new <see cref="DkimSignerBase"/>.</para>
		/// <note type="security">Due to the recognized weakness of the SHA-1 hash algorithm
		/// and the wide availability of the SHA-256 hash algorithm (it has been a required
		/// part of DKIM since it was originally standardized in 2007), it is recommended
		/// that <see cref="DkimSignatureAlgorithm.RsaSha1"/> NOT be used.</note>
		/// </remarks>
		/// <param name="domain">The domain that the signer represents.</param>
		/// <param name="selector">The selector subdividing the domain.</param>
		/// <param name="algorithm">The signature algorithm.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <para><paramref name="domain"/> is <see langword="null"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="selector"/> is <see langword="null"/>.</para>
		/// </exception>
		protected DkimSignerBase (string domain, string selector, DkimSignatureAlgorithm algorithm = DkimSignatureAlgorithm.RsaSha256)
		{
			if (domain == null)
				throw new ArgumentNullException (nameof (domain));

			if (selector == null)
				throw new ArgumentNullException (nameof (selector));

			SignatureAlgorithm = algorithm;
			Selector = selector;
			Domain = domain;
		}

		/// <summary>
		/// Get the domain that the signer represents.
		/// </summary>
		/// <remarks>
		/// Gets the domain that the signer represents.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
		/// </example>
		/// <value>The domain.</value>
		public string Domain {
			get; private set;
		}

		/// <summary>
		/// Get the selector subdividing the domain.
		/// </summary>
		/// <remarks>
		/// Gets the selector subdividing the domain.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
		/// </example>
		/// <value>The selector.</value>
		public string Selector {
			get; private set;
		}

		/// <summary>
		/// Get or set the algorithm to use for signing.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the algorithm to use for signing.</para>
		/// <para>Creates a new <see cref="DkimSigner"/>.</para>
		/// <note type="security">Due to the recognized weakness of the SHA-1 hash algorithm
		/// and the wide availability of the SHA-256 hash algorithm (it has been a required
		/// part of DKIM since it was originally standardized in 2007), it is recommended
		/// that <see cref="DkimSignatureAlgorithm.RsaSha1"/> NOT be used.</note>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
		/// </example>
		/// <value>The signature algorithm.</value>
		public DkimSignatureAlgorithm SignatureAlgorithm {
			get; set;
		}

		/// <summary>
		/// Get or set the canonicalization algorithm to use for the message body.
		/// </summary>
		/// <remarks>
		/// Gets or sets the canonicalization algorithm to use for the message body.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
		/// </example>
		/// <value>The canonicalization algorithm.</value>
		public DkimCanonicalizationAlgorithm BodyCanonicalizationAlgorithm {
			get; set;
		}
		
		/// <summary>
		/// Get or set the timespan after which signatures are no longer valid.
		/// </summary>
		/// <remarks>
		/// Get or set the timespan after which signatures are no longer valid.
		/// </remarks>
		/// <value>The signatures expiration timespan value.</value>
		public TimeSpan? SignaturesExpireAfter {
			get; set;
		}

		/// <summary>
		/// Get or set the canonicalization algorithm to use for the message headers.
		/// </summary>
		/// <remarks>
		/// Gets or sets the canonicalization algorithm to use for the message headers.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\DkimExamples.cs" region="DkimSign" />
		/// </example>
		/// <value>The canonicalization algorithm.</value>
		public DkimCanonicalizationAlgorithm HeaderCanonicalizationAlgorithm {
			get; set;
		}

		/// <summary>
		/// Gets the private key.
		/// </summary>
		/// <remarks>
		/// The private key used for signing.
		/// </remarks>
		/// <value>The private key.</value>
		protected IDkimPrivateKey PrivateKey {
			get; set;
		}
	}
}
