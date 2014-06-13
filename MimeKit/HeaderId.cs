//
// HeaderId.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (www.xamarin.com)
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
using System.Text;

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
		FollowUpTo,

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
		MimeVersion,

		/// <summary>
		/// The News-Groups header field.
		/// </summary>
		NewsGroups,

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
		/// The Subject header field.
		/// </summary>
		Subject,

		/// <summary>
		/// The Summary header field.
		/// </summary>
		Summary,

		/// <summary>
		/// The Supercedes header field.
		/// </summary>
		Supercedes,

		/// <summary>
		/// The To header field.
		/// </summary>
		To,

		/// <summary>
		/// An unknown header field.
		/// </summary>
		Unknown = -1
	}

	static class HeaderIdExtension
	{
		public static string ToHeaderName (this Enum value)
		{
			var builder = new StringBuilder (value.ToString ());

			for (int i = 2; i < builder.Length; i++) {
				if (char.IsUpper (builder[i]))
					builder.Insert (i++, '-');
			}

			return builder.ToString ();
		}

		public static HeaderId ToHeaderId (this string name)
		{
			var canonical = new StringBuilder ();
			bool dash = true;
			HeaderId id;
			char c;

			if (name == null)
				throw new ArgumentNullException ("name");

			for (int i = 0; i < name.Length; i++) {
				if (name[i] == '-') {
					dash = true;
					continue;
				}

				c = dash ? char.ToUpperInvariant (name[i]) : char.ToLowerInvariant (name[i]);
				canonical.Append (c);
				dash = false;
			}

			if (!Enum.TryParse<HeaderId> (canonical.ToString (), out id))
				return HeaderId.Unknown;

			return id;
		}
	}
}
