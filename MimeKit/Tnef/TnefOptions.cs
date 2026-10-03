//
// TnefOptions.cs
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

namespace MimeKit.Tnef {
	/// <summary>
	/// Options that control how a TNEF stream is read.
	/// </summary>
	/// <remarks>
	/// <para>Options that control how a TNEF stream is read.</para>
	/// <para>The options are consulted while the stream is being read, so they should not be modified
	/// while a <see cref="TnefReader"/> that uses them is in use.</para>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\TnefExamples.cs" region="Limits"/>
	/// </example>
	public class TnefOptions
	{
		/// <summary>
		/// The default maximum nesting depth of embedded messages.
		/// </summary>
		/// <remarks>
		/// The default maximum nesting depth of embedded messages.
		/// </remarks>
		public const int DefaultMaxNestingDepth = 32;

		/// <summary>
		/// The default maximum length of a single value that will be read into memory.
		/// </summary>
		/// <remarks>
		/// <para>The default maximum length, in bytes, of a single value that will be read into memory (32 MB).</para>
		/// <para>This is larger than any value that can fit within a message that is subject to the 25-35 MB
		/// message size limits that are common for SMTP servers.</para>
		/// </remarks>
		public const int DefaultMaxPropertyValueLength = 32 * 1024 * 1024;

		/// <summary>
		/// The default maximum number of bytes of value data that will be read into memory.
		/// </summary>
		/// <remarks>
		/// <para>The default maximum number of bytes of value data that will be read into memory for a single
		/// TNEF stream, including any embedded messages (64 MB).</para>
		/// </remarks>
		public const long DefaultMaxTotalDataBytes = 64L * 1024 * 1024;

		/// <summary>
		/// The default maximum number of attachments that will be loaded for a single message.
		/// </summary>
		/// <remarks>
		/// The default maximum number of attachments that will be loaded by <see cref="TnefMessage"/> for a single message.
		/// </remarks>
		public const int DefaultMaxAttachments = 1024;

		/// <summary>
		/// The default TNEF options.
		/// </summary>
		/// <remarks>
		/// <para>If a <see langword="null"/> <see cref="TnefOptions"/> is passed to any of the TNEF APIs,
		/// then the default options will be used.</para>
		/// </remarks>
		public static readonly TnefOptions Default = new TnefOptions ();

		int defaultCodepage;
		int maxNestingDepth = DefaultMaxNestingDepth;
		int maxPropertyValueLength = DefaultMaxPropertyValueLength;
		long maxTotalDataBytes = DefaultMaxTotalDataBytes;
		int maxAttachments = DefaultMaxAttachments;

		/// <summary>
		/// Initialize a new instance of the <see cref="TnefOptions"/> class.
		/// </summary>
		/// <remarks>
		/// Creates a new set of TNEF options with the default values.
		/// </remarks>
		public TnefOptions ()
		{
		}

		/// <summary>
		/// Get or set the codepage to use for 8-bit strings when the stream does not specify one.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the codepage to use for decoding 8-bit strings until the TNEF stream specifies
		/// its own codepage using a nonzero <see cref="TnefAttributeTag.OemCodepage"/> attribute or, if it has no
		/// such attribute, the message's <see cref="TnefPropertyId.InternetCodepage"/> property
		/// (see [MS-OXTNEF] section 2.3.3.2).</para>
		/// <para>A value of <c>0</c> means windows-1252. If the requested codepage is not available on the
		/// host (which is common on non-Windows platforms unless the application has registered the
		/// <c>System.Text.Encoding.CodePages</c> provider), a fallback encoding will be used instead.</para>
		/// </remarks>
		/// <value>The default codepage.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative.
		/// </exception>
		public int DefaultCodepage {
			get { return defaultCodepage; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException (nameof (value));

				defaultCodepage = value;
			}
		}

		/// <summary>
		/// Get or set the maximum nesting depth of embedded messages.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum depth of embedded messages that will be read before the TNEF
		/// stream is assumed to be maliciously formed.</para>
		/// <para>When the limit is exceeded, a <see cref="TnefComplianceViolation.NestingTooDeep"/> issue
		/// is reported and the embedded message is treated as empty.</para>
		/// </remarks>
		/// <value>The maximum nesting depth. The default is <see cref="DefaultMaxNestingDepth"/>.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative.
		/// </exception>
		public int MaxNestingDepth {
			get { return maxNestingDepth; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxNestingDepth = value;
			}
		}

		/// <summary>
		/// Get or set the maximum length of a single value that will be read into memory.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum length, in bytes, of a single attribute or property value that will be read
		/// into memory by methods such as <see cref="TnefReader.ReadValueAsBytes(System.Threading.CancellationToken)"/>,
		/// <see cref="TnefPropertyReader.ReadValue(System.Threading.CancellationToken)"/> and
		/// <see cref="TnefPropertyReader.ReadPropertySet(System.Threading.CancellationToken)"/>.</para>
		/// <para>When a value is longer than this limit, a <see cref="TnefComplianceViolation.DataSizeLimitExceeded"/>
		/// issue is reported and the value is skipped. The methods that return a single value return an empty value
		/// instead, and <see cref="TnefPropertyReader.ReadProperty(System.Threading.CancellationToken)"/> returns a
		/// <see cref="TnefProperty"/> without any values.</para>
		/// <para>The limit does not apply to values that are read using <see cref="TnefReader.OpenValueStream"/>,
		/// <see cref="TnefPropertyReader.OpenValueStream"/> or <see cref="TnefPropertyReader.OpenEmbeddedMessage"/>,
		/// since those values are not buffered in memory. It does apply to the message bodies and attachment content that
		/// are buffered by <see cref="TnefMessage"/>.</para>
		/// <para>The default limit is suitable for messages that are subject to the 25-35 MB message size limits that
		/// are common for SMTP servers. Applications that accept larger messages should increase it.</para>
		/// </remarks>
		/// <value>The maximum value length. The default is <see cref="DefaultMaxPropertyValueLength"/>.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative.
		/// </exception>
		public int MaxPropertyValueLength {
			get { return maxPropertyValueLength; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxPropertyValueLength = value;
			}
		}

		/// <summary>
		/// Get or set the maximum number of bytes of value data that will be read into memory.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum total number of bytes of attribute and property values that will be read into
		/// memory for a single TNEF stream, including the values of any embedded messages. This protects against
		/// maliciously formed streams that contain a large number of values that are each smaller than
		/// <see cref="MaxPropertyValueLength"/>.</para>
		/// <para>Once the limit has been reached, a <see cref="TnefComplianceViolation.DataSizeLimitExceeded"/> issue is
		/// reported for each additional string, binary or object value that would be read into memory and the value is
		/// skipped as described for <see cref="MaxPropertyValueLength"/>. Fixed-width values are still read.</para>
		/// <para>The limit does not apply to values that are read using <see cref="TnefReader.OpenValueStream"/>,
		/// <see cref="TnefPropertyReader.OpenValueStream"/> or <see cref="TnefPropertyReader.OpenEmbeddedMessage"/>, but
		/// it does apply to the message bodies and attachment content that are buffered by <see cref="TnefMessage"/>.</para>
		/// <para>The default limit is suitable for messages that are subject to the 25-35 MB message size limits that
		/// are common for SMTP servers. Applications that accept larger messages should increase it.</para>
		/// </remarks>
		/// <value>The maximum number of bytes. The default is <see cref="DefaultMaxTotalDataBytes"/>.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative.
		/// </exception>
		public long MaxTotalDataBytes {
			get { return maxTotalDataBytes; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxTotalDataBytes = value;
			}
		}

		/// <summary>
		/// Get or set the maximum number of attachments that will be loaded for a single message.
		/// </summary>
		/// <remarks>
		/// <para>Gets or sets the maximum number of attachments that <see cref="TnefMessage"/> will load for a single
		/// message. Attachments of embedded messages are counted separately.</para>
		/// <para>When the limit is exceeded, a <see cref="TnefComplianceViolation.TooManyAttachments"/> issue is reported
		/// and any further attachments are skipped.</para>
		/// </remarks>
		/// <value>The maximum number of attachments. The default is <see cref="DefaultMaxAttachments"/>.</value>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative.
		/// </exception>
		public int MaxAttachments {
			get { return maxAttachments; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException (nameof (value));

				maxAttachments = value;
			}
		}

		/// <summary>
		/// Clone an instance of <see cref="TnefOptions"/>.
		/// </summary>
		/// <remarks>
		/// Clones a set of options, allowing you to change a specific option
		/// without requiring you to change the original.
		/// </remarks>
		/// <returns>An identical copy of the current instance.</returns>
		public TnefOptions Clone ()
		{
			return new TnefOptions {
				defaultCodepage = defaultCodepage,
				maxNestingDepth = maxNestingDepth,
				maxPropertyValueLength = maxPropertyValueLength,
				maxTotalDataBytes = maxTotalDataBytes,
				maxAttachments = maxAttachments
			};
		}
	}
}
