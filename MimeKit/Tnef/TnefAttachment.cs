//
// TnefAttachment.cs
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MimeKit.IO;

namespace MimeKit.Tnef {
	/// <summary>
	/// An attachment of a TNEF message.
	/// </summary>
	/// <remarks>
	/// <para>A <see cref="TnefAttachment"/> holds the properties and content of a single attachment of a
	/// <see cref="TnefMessage"/>.</para>
	/// <para>The content is buffered in memory when the message is loaded, so it may be read any number of times.
	/// If the attachment is an embedded message (see <see cref="IsEmbeddedMessage"/>), the content is the embedded
	/// TNEF message, which can be loaded on demand using <see cref="LoadEmbeddedMessage"/>.</para>
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\TnefExamples.cs" region="ExtractAttachments"/>
	/// </example>
	public sealed class TnefAttachment : IDisposable
	{
		readonly TnefEmbeddedMessageContext? embedded;
		readonly MemoryBlockStream? content;
		readonly long contentOffset;
		bool disposed;

		internal TnefAttachment (TnefPropertySet properties, MemoryBlockStream? content, long contentOffset, TnefEmbeddedMessageContext? embedded)
		{
			this.contentOffset = content != null ? Math.Min (contentOffset, content.Length) : 0;
			this.embedded = embedded;
			this.content = content;
			Properties = properties;
		}

		/// <summary>
		/// Get the attachment's MAPI properties.
		/// </summary>
		/// <remarks>
		/// <para>Gets the attachment's MAPI properties, including the properties that were decoded from the legacy
		/// attachment attributes (such as <see cref="TnefAttributeTag.AttachTitle"/>).</para>
		/// <para>The attachment's content (the <see cref="TnefPropertyId.AttachData"/> property and the legacy
		/// <see cref="TnefAttributeTag.AttachData"/> attribute) is not included; use <see cref="OpenRead"/> instead.</para>
		/// </remarks>
		/// <value>The properties.</value>
		public TnefPropertySet Properties {
			get;
		}

		/// <summary>
		/// Get the attachment method.
		/// </summary>
		/// <remarks>
		/// Gets the attachment method from the <see cref="TnefPropertyId.AttachMethod"/> property. If the attachment does
		/// not have that property, the method is <see cref="TnefAttachMethod.EmbeddedMessage"/> for an embedded message and
		/// <see cref="TnefAttachMethod.ByValue"/> for anything else.
		/// </remarks>
		/// <value>The attachment method.</value>
		public TnefAttachMethod Method {
			get {
				var value = Properties.GetInt32 (TnefPropertyTag.AttachMethod);

				if (value.HasValue)
					return (TnefAttachMethod) value.Value;

				return embedded != null ? TnefAttachMethod.EmbeddedMessage : TnefAttachMethod.ByValue;
			}
		}

		/// <summary>
		/// Get whether the attachment is an embedded TNEF message.
		/// </summary>
		/// <remarks>
		/// Gets whether the attachment is an embedded TNEF message that can be loaded using
		/// <see cref="LoadEmbeddedMessage"/>.
		/// </remarks>
		/// <value><see langword="true"/> if the attachment is an embedded message; otherwise, <see langword="false"/>.</value>
		public bool IsEmbeddedMessage {
			get { return embedded != null; }
		}

		/// <summary>
		/// Get the file name of the attachment.
		/// </summary>
		/// <remarks>
		/// <para>Gets the file name of the attachment from the <see cref="TnefPropertyId.AttachLongFilename"/> property
		/// or, if that is not present, from the <see cref="TnefPropertyId.AttachFilename"/> property.</para>
		/// <para>The file name comes from an untrusted source. It must be sanitized before it is used to create a file.</para>
		/// </remarks>
		/// <value>The file name, or <see langword="null"/> if the attachment does not have one.</value>
		public string? FileName {
			get {
				return Properties.GetString (TnefPropertyTag.AttachLongFilenameW)
					?? Properties.GetString (TnefPropertyTag.AttachFilenameW);
			}
		}

		/// <summary>
		/// Get the MIME type of the attachment.
		/// </summary>
		/// <remarks>
		/// Gets the MIME type of the attachment from the <see cref="TnefPropertyId.AttachMimeTag"/> property.
		/// </remarks>
		/// <value>The MIME type, or <see langword="null"/> if it is not known.</value>
		public string? MimeType {
			get { return Properties.GetString (TnefPropertyTag.AttachMimeTagW); }
		}

		/// <summary>
		/// Get the content identifier of the attachment.
		/// </summary>
		/// <remarks>
		/// Gets the content identifier of the attachment from the <see cref="TnefPropertyId.AttachContentId"/> property,
		/// exactly as it was stored in the TNEF stream.
		/// </remarks>
		/// <value>The content identifier, or <see langword="null"/> if the attachment does not have one.</value>
		public string? ContentId {
			get { return Properties.GetString (TnefPropertyTag.AttachContentIdW); }
		}

		/// <summary>
		/// Get the attachment flags.
		/// </summary>
		/// <remarks>
		/// Gets the attachment flags from the <see cref="TnefPropertyId.AttachFlags"/> property.
		/// </remarks>
		/// <value>The attachment flags.</value>
		public TnefAttachFlags Flags {
			get { return (TnefAttachFlags) (Properties.GetInt32 (TnefPropertyTag.AttachFlags) ?? 0); }
		}

		/// <summary>
		/// Get whether the attachment has any content.
		/// </summary>
		/// <remarks>
		/// <para>Gets whether the attachment has any content.</para>
		/// <para>An attachment has no content if the TNEF stream did not contain any, or if its content was skipped
		/// because it was larger than <see cref="TnefOptions.MaxPropertyValueLength"/> or the remaining
		/// <see cref="TnefOptions.MaxTotalDataBytes"/> allowed.</para>
		/// </remarks>
		/// <value><see langword="true"/> if the attachment has content; otherwise, <see langword="false"/>.</value>
		public bool HasContent {
			get { return content != null; }
		}

		/// <summary>
		/// Get the length of the attachment's content.
		/// </summary>
		/// <remarks>
		/// Gets the length of the attachment's content, in bytes.
		/// </remarks>
		/// <value>The length of the content.</value>
		/// <exception cref="System.ObjectDisposedException">
		/// The attachment has been disposed.
		/// </exception>
		public long Length {
			get {
				CheckDisposed ();
				return content != null ? content.Length - contentOffset : 0;
			}
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (nameof (TnefAttachment));
		}

		/// <summary>
		/// Open a stream for reading the attachment's content.
		/// </summary>
		/// <remarks>
		/// <para>Opens a stream for reading the attachment's content.</para>
		/// <para>The 16-byte interface identifier that prefixes a <see cref="TnefPropertyType.Object"/> value is not
		/// included. For an embedded message, the content is the embedded TNEF stream.</para>
		/// <para>Each call returns a new, independent stream. The stream must be disposed by the caller, and must not be
		/// used once the attachment has been disposed.</para>
		/// </remarks>
		/// <returns>The content stream, which is empty if the attachment has no content.</returns>
		/// <exception cref="System.ObjectDisposedException">
		/// The attachment has been disposed.
		/// </exception>
		public Stream OpenRead ()
		{
			CheckDisposed ();

			if (content is null)
				return new MemoryStream (Array.Empty<byte> (), false);

			return new BoundStream (content, contentOffset, content.Length, true);
		}

		TnefReader CreateEmbeddedReader ()
		{
			CheckDisposed ();

			if (embedded is null)
				throw new InvalidOperationException ("The attachment is not an embedded message.");

			return embedded.CreateReader (OpenRead ());
		}

		/// <summary>
		/// Load the embedded message.
		/// </summary>
		/// <remarks>
		/// <para>Loads the embedded TNEF message contained within the attachment.</para>
		/// <para>The embedded message is loaded using the same <see cref="TnefOptions"/> and compliance logger as the
		/// message that contains it, and has its own <see cref="TnefOptions.MaxTotalDataBytes"/> budget. If it is nested
		/// more deeply than <see cref="TnefOptions.MaxNestingDepth"/> allows, a
		/// <see cref="TnefComplianceViolation.NestingTooDeep"/> issue is reported and an empty message is returned.</para>
		/// <para>The returned message is independent of the attachment and must be disposed by the caller.</para>
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\TnefExamples.cs" region="ExtractAttachments"/>
		/// </example>
		/// <returns>The embedded message.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The attachment has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The attachment is not an embedded message.
		/// </exception>
		/// <exception cref="TnefException">
		/// The embedded message does not begin with the TNEF signature.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public TnefMessage LoadEmbeddedMessage (CancellationToken cancellationToken = default)
		{
			using (var reader = CreateEmbeddedReader ())
				return TnefMessage.Load (reader, cancellationToken);
		}

		/// <summary>
		/// Asynchronously load the embedded message.
		/// </summary>
		/// <remarks>
		/// <para>Asynchronously loads the embedded TNEF message contained within the attachment.</para>
		/// <para>The embedded message is loaded using the same <see cref="TnefOptions"/> and compliance logger as the
		/// message that contains it, and has its own <see cref="TnefOptions.MaxTotalDataBytes"/> budget. If it is nested
		/// more deeply than <see cref="TnefOptions.MaxNestingDepth"/> allows, a
		/// <see cref="TnefComplianceViolation.NestingTooDeep"/> issue is reported and an empty message is returned.</para>
		/// <para>The returned message is independent of the attachment and must be disposed by the caller.</para>
		/// </remarks>
		/// <returns>The embedded message.</returns>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <exception cref="System.ObjectDisposedException">
		/// The attachment has been disposed.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// The attachment is not an embedded message.
		/// </exception>
		/// <exception cref="TnefException">
		/// The embedded message does not begin with the TNEF signature.
		/// </exception>
		/// <exception cref="System.OperationCanceledException">
		/// The operation was canceled via the cancellation token.
		/// </exception>
		public async Task<TnefMessage> LoadEmbeddedMessageAsync (CancellationToken cancellationToken = default)
		{
			using (var reader = CreateEmbeddedReader ())
				return await TnefMessage.LoadAsync (reader, cancellationToken).ConfigureAwait (false);
		}

		/// <summary>
		/// Release all resources used by the <see cref="TnefAttachment"/> object.
		/// </summary>
		/// <remarks>
		/// Releases the buffered content of the attachment.
		/// </remarks>
		public void Dispose ()
		{
			if (disposed)
				return;

			disposed = true;
			content?.Dispose ();
		}
	}

	// The state needed to load an embedded message on demand, captured from the reader of the containing message.
	sealed class TnefEmbeddedMessageContext
	{
		readonly ITnefComplianceLogger? complianceLogger;
		readonly int maxComplianceIssuesPerViolation;
		readonly TnefOptions options;
		readonly Encoding encoding;
		readonly long baseOffset;
		readonly int depth;

		public TnefEmbeddedMessageContext (TnefReader reader, long baseOffset)
		{
			maxComplianceIssuesPerViolation = reader.MaxComplianceIssuesPerViolation;
			complianceLogger = reader.ComplianceLogger;
			options = reader.Options;
			encoding = reader.Encoding;
			depth = reader.Depth + 1;
			this.baseOffset = baseOffset;
		}

		public TnefReader CreateReader (Stream stream)
		{
			return TnefReader.CreateDetached (stream, options, depth, baseOffset, encoding, complianceLogger, maxComplianceIssuesPerViolation);
		}
	}
}
