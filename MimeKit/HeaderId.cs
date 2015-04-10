//
// HeaderId.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2015 Xamarin Inc. (www.xamarin.com)
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
		/// The Ad-Hoc header field.
		/// </summary>
		AdHoc,

		/// <summary>
		/// The Apparently-To header field.
		/// </summary>
		ApparentlyTo,

		/// <summary>
		/// The Approved header field.
		/// </summary>
		Approved,

		/// <summary>
		/// The Article header field.
		/// </summary>
		Article,

		/// <summary>
		/// The Bcc header field.
		/// </summary>
		Bcc,

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
		/// The Content-Id header field.
		/// </summary>
		ContentId,

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
		/// The Content-Transfer-Encoding header field.
		/// </summary>
		ContentTransferEncoding,

		/// <summary>
		/// The Content-Type header field.
		/// </summary>
		ContentType,

		/// <summary>
		/// The Control header field.
		/// </summary>
		Control,

		/// <summary>
		/// The Date header field.
		/// </summary>
		Date,

		/// <summary>
		/// The Deferred-Delivery header field.
		/// </summary>
		DeferredDelivery,

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
		/// The Importance header field.
		/// </summary>
		Importance,

		/// <summary>
		/// The In-Reply-To header field.
		/// </summary>
		InReplyTo,

		/// <summary>
		/// The Keywords header field.
		/// </summary>
		Keywords,

		/// <summary>
		/// The Lines header field.
		/// </summary>
		Lines,

		/// <summary>
		/// The List-Help header field.
		/// </summary>
		ListHelp,

		/// <summary>
		/// The List-Subscribe header field.
		/// </summary>
		ListSubscribe,

		/// <summary>
		/// The List-Unsubscribe header field.
		/// </summary>
		ListUnsubscribe,

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
		/// The Path header field.
		/// </summary>
		Path,

		/// <summary>
		/// The Precedence header field.
		/// </summary>
		Precedence,

		/// <summary>
		/// The Priority header field.
		/// </summary>
		Priority,

		/// <summary>
		/// The Received header field.
		/// </summary>
		Received,

		/// <summary>
		/// The References header field.
		/// </summary>
		References,

		/// <summary>
		/// The Reply-By header field.
		/// </summary>
		ReplyBy,

		/// <summary>
		/// The Reply-To header field.
		/// </summary>
		ReplyTo,

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
		/// The Sender header field.
		/// </summary>
		Sender,

		/// <summary>
		/// The Sensitivity header field.
		/// </summary>
		Sensitivity,

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
		/// The To header field.
		/// </summary>
		To,

		/// <summary>
		/// The User-Agent header field.
		/// </summary>
		UserAgent,

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

			dict = new Dictionary<string, HeaderId> (values.Length - 1, StringComparer.OrdinalIgnoreCase);

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

#if PORTABLE
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
