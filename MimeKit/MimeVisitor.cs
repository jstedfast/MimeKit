//
// MimeVisitor.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2024 .NET Foundation and Contributors
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

#if ENABLE_CRYPTO
using MimeKit.Cryptography;
#endif

using MimeKit.Tnef;

namespace MimeKit {
	/// <summary>
	/// Represents a visitor for MIME trees.
	/// </summary>
	/// <remarks>
	/// This class is designed to be inherited to create more specialized classes whose
	/// functionality requires traversing, examining or copying a MIME tree.
	/// </remarks>
	/// <example>
	/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
	/// </example>
	public abstract class MimeVisitor
	{
		/// <summary>
		/// Dispatches the entity to one of the more specialized visit methods in this class.
		/// </summary>
		/// <remarks>
		/// Dispatches the entity to one of the more specialized visit methods in this class.
		/// </remarks>
		/// <param name="entity">The MIME entity.</param>
		public virtual void Visit (MimeEntity entity)
		{
			entity?.Accept (this);
		}

		/// <summary>
		/// Dispatches the message to one of the more specialized visit methods in this class.
		/// </summary>
		/// <remarks>
		/// Dispatches the message to one of the more specialized visit methods in this class.
		/// </remarks>
		/// <param name="message">The MIME message.</param>
		public virtual void Visit (MimeMessage message)
		{
			message?.Accept (this);
		}

#if ENABLE_CRYPTO
		/// <summary>
		/// Visit the application/pgp-encrypted MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the application/pgp-encrypted MIME entity.
		/// </remarks>
		/// <seealso cref="MimeKit.Cryptography.MultipartEncrypted"/>
		/// <param name="entity">The application/pgp-encrypted MIME entity.</param>
		protected internal virtual void VisitApplicationPgpEncrypted (ApplicationPgpEncrypted entity)
		{
			VisitMimePart (entity);
		}

		/// <summary>
		/// Visit the application/pgp-signature MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the application/pgp-signature MIME entity.
		/// </remarks>
		/// <seealso cref="MimeKit.Cryptography.MultipartSigned"/>
		/// <param name="entity">The application/pgp-signature MIME entity.</param>
		protected internal virtual void VisitApplicationPgpSignature (ApplicationPgpSignature entity)
		{
			VisitMimePart (entity);
		}

		/// <summary>
		/// Visit the application/pkcs7-mime MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the application/pkcs7-mime MIME entity.
		/// </remarks>
		/// <param name="entity">The application/pkcs7-mime MIME entity.</param>
		protected internal virtual void VisitApplicationPkcs7Mime (ApplicationPkcs7Mime entity)
		{
			VisitMimePart (entity);
		}

		/// <summary>
		/// Visit the application/pkcs7-signature MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the application/pkcs7-signature MIME entity.
		/// </remarks>
		/// <seealso cref="MimeKit.Cryptography.MultipartSigned"/>
		/// <param name="entity">The application/pkcs7-signature MIME entity.</param>
		protected internal virtual void VisitApplicationPkcs7Signature (ApplicationPkcs7Signature entity)
		{
			VisitMimePart (entity);
		}
#endif

		/// <summary>
		/// Visit the message/disposition-notification MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the message/disposition-notification MIME entity.
		/// </remarks>
		/// <param name="entity">The message/disposition-notification MIME entity.</param>
		protected internal virtual void VisitMessageDispositionNotification (MessageDispositionNotification entity)
		{
			VisitMimePart (entity);
		}

		/// <summary>
		/// Visit the message/delivery-status MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the message/delivery-status MIME entity.
		/// </remarks>
		/// <param name="entity">The message/delivery-status MIME entity.</param>
		protected internal virtual void VisitMessageDeliveryStatus (MessageDeliveryStatus entity)
		{
			VisitMimePart (entity);
		}

		/// <summary>
		/// Visit the message/feedback-report MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the message/feedback-report MIME entity.
		/// </remarks>
		/// <param name="entity">The message/feedback-report MIME entity.</param>
		protected internal virtual void VisitMessageFeedbackReport (MessageFeedbackReport entity)
		{
			VisitMimePart (entity);
		}

		/// <summary>
		/// Visit the message contained within a message/rfc822 or message/news MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the message contained within a message/rfc822 or message/news MIME entity.
		/// </remarks>
		/// <param name="entity">The message/rfc822 or message/news MIME entity.</param>
		protected virtual void VisitMessage (MessagePart entity)
		{
			entity.Message?.Accept (this);
		}

		/// <summary>
		/// Visit the message/rfc822 or message/news MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the message/rfc822 or message/news MIME entity.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="entity">The message/rfc822 or message/news MIME entity.</param>
		protected internal virtual void VisitMessagePart (MessagePart entity)
		{
			VisitMimeEntity (entity);
			VisitMessage (entity);
		}

		/// <summary>
		/// Visit the message/partial MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the message/partial MIME entity.
		/// </remarks>
		/// <param name="entity">The message/partial MIME entity.</param>
		protected internal virtual void VisitMessagePartial (MessagePartial entity)
		{
			VisitMimePart (entity);
		}

		/// <summary>
		/// Visit the abstract MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the abstract MIME entity.
		/// </remarks>
		/// <param name="entity">The MIME entity.</param>
		protected internal virtual void VisitMimeEntity (MimeEntity entity)
		{
		}

		/// <summary>
		/// Visit the body of the message.
		/// </summary>
		/// <remarks>
		/// Visits the body of the message.
		/// </remarks>
		/// <param name="message">The message.</param>
		protected virtual void VisitBody (MimeMessage message)
		{
			message.Body?.Accept (this);
		}

		/// <summary>
		/// Visit the MIME message.
		/// </summary>
		/// <remarks>
		/// Visits the MIME message.
		/// </remarks>
		/// <param name="message">The MIME message.</param>
		protected internal virtual void VisitMimeMessage (MimeMessage message)
		{
			VisitBody (message);
		}

		/// <summary>
		/// Visit the abstract MIME part entity.
		/// </summary>
		/// <remarks>
		/// Visits the MIME part entity.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="entity">The MIME part entity.</param>
		protected internal virtual void VisitMimePart (MimePart entity)
		{
			VisitMimeEntity (entity);
		}

		/// <summary>
		/// Visit the children of a <see cref="Multipart"/>.
		/// </summary>
		/// <remarks>
		/// Visits the children of a <see cref="Multipart"/>.
		/// </remarks>
		/// <param name="multipart">Multipart.</param>
		protected virtual void VisitChildren (Multipart multipart)
		{
			for (int i = 0; i < multipart.Count; i++)
				multipart[i].Accept (this);
		}

		/// <summary>
		/// Visit the abstract multipart MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the abstract multipart MIME entity.
		/// </remarks>
		/// <param name="multipart">The multipart MIME entity.</param>
		protected internal virtual void VisitMultipart (Multipart multipart)
		{
			VisitMimeEntity (multipart);
			VisitChildren (multipart);
		}

		/// <summary>
		/// Visit the multipart/alternative MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the multipart/alternative MIME entity.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="alternative">The multipart/alternative MIME entity.</param>
		protected internal virtual void VisitMultipartAlternative (MultipartAlternative alternative)
		{
			VisitMultipart (alternative);
		}

#if ENABLE_CRYPTO
		/// <summary>
		/// Visit the multipart/encrypted MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the multipart/encrypted MIME entity.
		/// </remarks>
		/// <param name="encrypted">The multipart/encrypted MIME entity.</param>
		protected internal virtual void VisitMultipartEncrypted (MultipartEncrypted encrypted)
		{
			VisitMultipart (encrypted);
		}
#endif

		/// <summary>
		/// Visit the multipart/related MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the multipart/related MIME entity.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="related">The multipart/related MIME entity.</param>
		protected internal virtual void VisitMultipartRelated (MultipartRelated related)
		{
			VisitMultipart (related);
		}

		/// <summary>
		/// Visit the multipart/report MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the multipart/report MIME entity.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="report">The multipart/report MIME entity.</param>
		protected internal virtual void VisitMultipartReport (MultipartReport report)
		{
			VisitMultipart (report);
		}

#if ENABLE_CRYPTO
		/// <summary>
		/// Visit the multipart/signed MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the multipart/signed MIME entity.
		/// </remarks>
		/// <param name="signed">The multipart/signed MIME entity.</param>
		protected internal virtual void VisitMultipartSigned (MultipartSigned signed)
		{
			VisitMultipart (signed);
		}
#endif

		/// <summary>
		/// Visit the text-based MIME part entity.
		/// </summary>
		/// <remarks>
		/// Visits the text-based MIME part entity.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="entity">The text-based MIME part entity.</param>
		protected internal virtual void VisitTextPart (TextPart entity)
		{
			VisitMimePart (entity);
		}

		/// <summary>
		/// Visit the text/rfc822-headers MIME entity.
		/// </summary>
		/// <remarks>
		/// Visits the text/rfc822-headers MIME entity.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="entity">The text/rfc822-headers MIME entity.</param>
		protected internal virtual void VisitTextRfc822Headers (TextRfc822Headers entity)
		{
			VisitMessagePart (entity);
		}

		/// <summary>
		/// Visit the Microsoft TNEF MIME part entity.
		/// </summary>
		/// <remarks>
		/// Visits the Microsoft TNEF MIME part entity.
		/// </remarks>
		/// <example>
		/// <code language="c#" source="Examples\MimeVisitorExamples.cs" region="HtmlPreviewVisitor" />
		/// </example>
		/// <param name="entity">The Microsoft TNEF MIME part entity.</param>
		protected internal virtual void VisitTnefPart (TnefPart entity)
		{
			VisitMimePart (entity);
		}
	}
}
