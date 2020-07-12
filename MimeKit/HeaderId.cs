//
// HeaderId.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2020 .NET Foundation and Contributors
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
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using MimeKit.Utils;

namespace MimeKit {
	/// <summary>
	/// An enumeration of common header fields.
	/// </summary>
	/// <remarks>
	/// Comparing enum  values is not only faster, but less error prone than
	/// comparing strings.
	/// </remarks>
	public enum HeaderId {
		/// <summary>
		/// The Accept-Language header field.
		/// </summary>
		AcceptLanguage,

		/// <summary>
		/// The Ad-Hoc header field.
		/// </summary>
		AdHoc,

		/// <summary>
		/// The Alternate-Recipient header field.
		/// </summary>
		AlternateRecipient,

		/// <summary>
		/// The Apparently-To header field.
		/// </summary>
		ApparentlyTo,

		/// <summary>
		/// The Approved header field.
		/// </summary>
		Approved,

		/// <summary>
		/// The ARC-Authentication-Results header field.
		/// </summary>
		[HeaderName ("ARC-Authentication-Results")]
		ArcAuthenticationResults,

		/// <summary>
		/// The ARC-Message-Signature header field.
		/// </summary>
		[HeaderName ("ARC-Message-Signature")]
		ArcMessageSignature,

		/// <summary>
		/// The ARC-Seal header field.
		/// </summary>
		[HeaderName ("ARC-Seal")]
		ArcSeal,

		/// <summary>
		/// The Archive header field.
		/// </summary>
		Archive,

		/// <summary>
		/// The Archived-At header field.
		/// </summary>
		ArchivedAt,

		/// <summary>
		/// The Article header field.
		/// </summary>
		Article,

		/// <summary>
		/// The Authentication-Results header field.
		/// </summary>
		AuthenticationResults,

		/// <summary>
		/// The Autocrypt header field.
		/// </summary>
		Autocrypt,

		/// <summary>
		/// The Autocrypt-Gossip header field.
		/// </summary>
		AutocryptGossip,

		/// <summary>
		/// The Autocrypt-Setup-Message header field.
		/// </summary>
		AutocryptSetupMessage,

		/// <summary>
		/// The Autoforwarded header field.
		/// </summary>
		Autoforwarded,

		/// <summary>
		/// The Auto-Submitted header field.
		/// </summary>
		AutoSubmitted,

		/// <summary>
		/// The Autosubmitted header field.
		/// </summary>
		Autosubmitted,

		/// <summary>
		/// The Base header field.
		/// </summary>
		Base,

		/// <summary>
		/// The Bcc header field.
		/// </summary>
		Bcc,

		/// <summary>
		/// The Body header field.
		/// </summary>
		Body,

		/// <summary>
		/// The Bytes header field.
		/// </summary>
		Bytes,

		/// <summary>
		/// The Cc header field.
		/// </summary>
		Cc,

		/// <summary>
		/// The Comments header field.
		/// </summary>
		Comments,

		/// <summary>
		/// The Content-Alternative header field.
		/// </summary>
		ContentAlternative,

		/// <summary>
		/// The Content-Base header field.
		/// </summary>
		ContentBase,

		/// <summary>
		/// The Content-Class header field.
		/// </summary>
		ContentClass,

		/// <summary>
		/// The Content-Description header field.
		/// </summary>
		ContentDescription,

		/// <summary>
		/// The Content-Disposition header field.
		/// </summary>
		ContentDisposition,

		/// <summary>
		/// The Content-Duration header field.
		/// </summary>
		ContentDuration,

		/// <summary>
		/// The Content-Features header field.
		/// </summary>
		ContentFeatures,

		/// <summary>
		/// The Content-Id header field.
		/// </summary>
		ContentId,

		/// <summary>
		/// The Content-Identifier header field.
		/// </summary>
		ContentIdentifier,

		/// <summary>
		/// The Content-Language header field.
		/// </summary>
		ContentLanguage,

		/// <summary>
		/// The Content-Length header field.
		/// </summary>
		ContentLength,

		/// <summary>
		/// The Content-Location header field.
		/// </summary>
		ContentLocation,

		/// <summary>
		/// The Content-Md5 header field.
		/// </summary>
		ContentMd5,

		/// <summary>
		/// The Content-Return header field.
		/// </summary>
		ContentReturn,

		/// <summary>
		/// The Content-Transfer-Encoding header field.
		/// </summary>
		ContentTransferEncoding,

		/// <summary>
		/// The Content-Translation-Type header field.
		/// </summary>
		ContentTranslationType,

		/// <summary>
		/// The Content-Type header field.
		/// </summary>
		ContentType,

		/// <summary>
		/// The Control header field.
		/// </summary>
		Control,

		/// <summary>
		/// The Conversion header field.
		/// </summary>
		Conversion,

		/// <summary>
		/// The Conversion-With-Loss header field.
		/// </summary>
		ConversionWithLoss,

		/// <summary>
		/// The Date header field.
		/// </summary>
		Date,

		/// <summary>
		/// The Date-Received header field.
		/// </summary>
		DateReceived,

		/// <summary>
		/// The Deferred-Delivery header field.
		/// </summary>
		DeferredDelivery,

		/// <summary>
		/// The Delivery-Date header field.
		/// </summary>
		DeliveryDate,

		/// <summary>
		/// The Disclose-Recipients header field.
		/// </summary>
		DiscloseRecipients,

		/// <summary>
		/// The Disposition-Notification-Options header field.
		/// </summary>
		DispositionNotificationOptions,

		/// <summary>
		/// The Disposition-Notification-To header field.
		/// </summary>
		DispositionNotificationTo,

		/// <summary>
		/// The Distribution header field.
		/// </summary>
		Distribution,

		/// <summary>
		/// The DKIM-Signature header field.
		/// </summary>
		[HeaderName ("DKIM-Signature")]
		DkimSignature,

		/// <summary>
		/// The DomainKey-Signature header field.
		/// </summary>
		[HeaderName ("DomainKey-Signature")]
		DomainKeySignature,

		/// <summary>
		/// The Encoding header field.
		/// </summary>
		Encoding,

		/// <summary>
		/// The Encrypted header field.
		/// </summary>
		Encrypted,

		/// <summary>
		/// The Expires header field.
		/// </summary>
		Expires,

		/// <summary>
		/// The Expiry-Date header field.
		/// </summary>
		ExpiryDate,

		/// <summary>
		/// The Followup-To header field.
		/// </summary>
		FollowupTo,

		/// <summary>
		/// The From header field.
		/// </summary>
		From,

		/// <summary>
		/// The Generate-Delivery-Report header field.
		/// </summary>
		GenerateDeliveryReport,

		/// <summary>
		/// The Importance header field.
		/// </summary>
		Importance,

		/// <summary>
		/// The Injection-Date header field.
		/// </summary>
		InjectionDate,

		/// <summary>
		/// The Injection-Info header field.
		/// </summary>
		InjectionInfo,

		/// <summary>
		/// The In-Reply-To header field.
		/// </summary>
		InReplyTo,

		/// <summary>
		/// The Keywords header field.
		/// </summary>
		Keywords,

		/// <summary>
		/// The Language header.
		/// </summary>
		Language,

		/// <summary>
		/// The Latest-Delivery-Time header.
		/// </summary>
		LatestDeliveryTime,

		/// <summary>
		/// The Lines header field.
		/// </summary>
		Lines,

		/// <summary>
		/// THe List-Archive header field.
		/// </summary>
		ListArchive,

		/// <summary>
		/// The List-Help header field.
		/// </summary>
		ListHelp,

		/// <summary>
		/// The List-Id header field.
		/// </summary>
		ListId,

		/// <summary>
		/// The List-Owner header field.
		/// </summary>
		ListOwner,

		/// <summary>
		/// The List-Post header field.
		/// </summary>
		ListPost,

		/// <summary>
		/// The List-Subscribe header field.
		/// </summary>
		ListSubscribe,

		/// <summary>
		/// The List-Unsubscribe header field.
		/// </summary>
		ListUnsubscribe,

		/// <summary>
		/// The List-Unsubscribe-Post header field.
		/// </summary>
		ListUnsubscribePost,

		/// <summary>
		/// The Message-Id header field.
		/// </summary>
		MessageId,

		/// <summary>
		/// The MIME-Version header field.
		/// </summary>
		[HeaderName ("MIME-Version")]
		MimeVersion,

		/// <summary>
		/// The Newsgroups header field.
		/// </summary>
		Newsgroups,

		/// <summary>
		/// The Nntp-Posting-Host header field.
		/// </summary>
		NntpPostingHost,

		/// <summary>
		/// The Organization header field.
		/// </summary>
		Organization,

		/// <summary>
		/// The Original-From header field.
		/// </summary>
		OriginalFrom,

		/// <summary>
		/// The Original-Message-Id header field.
		/// </summary>
		OriginalMessageId,

		/// <summary>
		/// The Original-Recipient header field.
		/// </summary>
		OriginalRecipient,

		/// <summary>
		/// The Original-Return-Address header field.
		/// </summary>
		OriginalReturnAddress,

		/// <summary>
		/// The Original-Subject header field.
		/// </summary>
		OriginalSubject,

		/// <summary>
		/// The Path header field.
		/// </summary>
		Path,

		/// <summary>
		/// The Precedence header field.
		/// </summary>
		Precedence,

		/// <summary>
		/// The Prevent-NonDelivery-Report header field.
		/// </summary>
		[HeaderName ("Prevent-NonDelivery-Report")]
		PreventNonDeliveryReport,

		/// <summary>
		/// The Priority header field.
		/// </summary>
		Priority,

		/// <summary>
		/// The Received header field.
		/// </summary>
		Received,

		/// <summary>
		/// The Received-SPF header field.
		/// </summary>
		[HeaderName ("Received-SPF")]
		ReceivedSPF,

		/// <summary>
		/// The References header field.
		/// </summary>
		References,

		/// <summary>
		/// The Relay-Version header field.
		/// </summary>
		RelayVersion,

		/// <summary>
		/// The Reply-By header field.
		/// </summary>
		ReplyBy,

		/// <summary>
		/// The Reply-To header field.
		/// </summary>
		ReplyTo,

		/// <summary>
		/// The Require-Recipient-Valid-Since header field.
		/// </summary>
		RequireRecipientValidSince,

		/// <summary>
		/// The Resent-Bcc header field.
		/// </summary>
		ResentBcc,

		/// <summary>
		/// The Resent-Cc header field.
		/// </summary>
		ResentCc,

		/// <summary>
		/// The Resent-Date header field.
		/// </summary>
		ResentDate,

		/// <summary>
		/// The Resent-From header field.
		/// </summary>
		ResentFrom,

		/// <summary>
		/// The Resent-Message-Id header field.
		/// </summary>
		ResentMessageId,

		/// <summary>
		/// The Resent-Reply-To header field.
		/// </summary>
		ResentReplyTo,

		/// <summary>
		/// The Resent-Sender header field.
		/// </summary>
		ResentSender,

		/// <summary>
		/// The Resent-To header field.
		/// </summary>
		ResentTo,

		/// <summary>
		/// The Return-Path header field.
		/// </summary>
		ReturnPath,

		/// <summary>
		/// The Return-Receipt-To header field.
		/// </summary>
		ReturnReceiptTo,

		/// <summary>
		/// The See-Also header field.
		/// </summary>
		SeeAlso,

		/// <summary>
		/// The Sender header field.
		/// </summary>
		Sender,

		/// <summary>
		/// The Sensitivity header field.
		/// </summary>
		Sensitivity,

		/// <summary>
		/// The Solicitation header field.
		/// </summary>
		Solicitation,

		/// <summary>
		/// The Status header field.
		/// </summary>
		Status,

		/// <summary>
		/// The Subject header field.
		/// </summary>
		Subject,

		/// <summary>
		/// The Summary header field.
		/// </summary>
		Summary,

		/// <summary>
		/// The Supersedes header field.
		/// </summary>
		Supersedes,

		/// <summary>
		/// The TLS-Required header field.
		/// </summary>
		[HeaderName ("TLS-Required")]
		TLSRequired,

		/// <summary>
		/// The To header field.
		/// </summary>
		To,

		/// <summary>
		/// The User-Agent header field.
		/// </summary>
		UserAgent,

		/// <summary>
		/// The X400-Content-Identifier header field.
		/// </summary>
		[HeaderName ("X400-Content-Identifier")]
		X400ContentIdentifier,

		/// <summary>
		/// The X400-Content-Return header field.
		/// </summary>
		[HeaderName ("X400-Content-Return")]
		X400ContentReturn,

		/// <summary>
		/// The X400-Content-Type header field.
		/// </summary>
		[HeaderName ("X400-Content-Type")]
		X400ContentType,

		/// <summary>
		/// The X400-MTS-Identifier header field.
		/// </summary>
		[HeaderName ("X400-MTS-Identifier")]
		X400MTSIdentifier,

		/// <summary>
		/// The X400-Originator header field.
		/// </summary>
		[HeaderName ("X400-Originator")]
		X400Originator,

		/// <summary>
		/// The X400-Received header field.
		/// </summary>
		[HeaderName ("X400-Received")]
		X400Received,

		/// <summary>
		/// The X400-Recipients header field.
		/// </summary>
		[HeaderName ("X400-Recipients")]
		X400Recipients,

		/// <summary>
		/// The X400-Trace header field.
		/// </summary>
		[HeaderName ("X400-Trace")]
		X400Trace,

		/// <summary>
		/// The X-Mailer header field.
		/// </summary>
		XMailer,

		/// <summary>
		/// The X-MSMail-Priority header field.
		/// </summary>
		[HeaderName ("X-MSMail-Priority")]
		XMSMailPriority,

		/// <summary>
		/// The X-Priority header field.
		/// </summary>
		XPriority,

		/// <summary>
		/// The X-Status header field.
		/// </summary>
		XStatus,

		/// <summary>
		/// An unknown header field.
		/// </summary>
		Unknown = -1
	}

	[AttributeUsage (AttributeTargets.Field)]
	class HeaderNameAttribute : Attribute {
		public HeaderNameAttribute (string name)
		{
			HeaderName = name;
		}

		public string HeaderName {
			get; protected set;
		}
	}

	/// <summary>
	/// <see cref="HeaderId"/> extension methods.
	/// </summary>
	/// <remarks>
	/// <see cref="HeaderId"/> extension methods.
	/// </remarks>
	public static class HeaderIdExtensions
	{
		static readonly Dictionary<string, HeaderId> dict;

		static HeaderIdExtensions ()
		{
			var values = (HeaderId[]) Enum.GetValues (typeof (HeaderId));

			dict = new Dictionary<string, HeaderId> (values.Length - 1, MimeUtils.OrdinalIgnoreCase);

			for (int i = 0; i < values.Length - 1; i++)
				dict.Add (values[i].ToHeaderName (), values[i]);
		}

		/// <summary>
		/// Converts the enum value into the equivalent header field name.
		/// </summary>
		/// <remarks>
		/// Converts the enum value into the equivalent header field name.
		/// </remarks>
		/// <returns>The header name.</returns>
		/// <param name="value">The enum value.</param>
		public static string ToHeaderName (this HeaderId value)
		{
			var name = value.ToString ();

#if NETSTANDARD1_3 || NETSTANDARD1_6
			var field = typeof (HeaderId).GetTypeInfo ().GetDeclaredField (name);
			var attrs = field.GetCustomAttributes (typeof (HeaderNameAttribute), false).ToArray ();
#else
			var field = typeof (HeaderId).GetField (name);
			var attrs = field.GetCustomAttributes (typeof (HeaderNameAttribute), false);
#endif

			if (attrs != null && attrs.Length == 1)
				return ((HeaderNameAttribute) attrs[0]).HeaderName;

			var builder = new StringBuilder (name);

			for (int i = 1; i < builder.Length; i++) {
				if (char.IsUpper (builder[i]))
					builder.Insert (i++, '-');
			}

			return builder.ToString ();
		}

		internal static HeaderId ToHeaderId (this string name)
		{
			HeaderId value;

			if (!dict.TryGetValue (name, out value))
				return HeaderId.Unknown;

			return value;
		}
	}
}
