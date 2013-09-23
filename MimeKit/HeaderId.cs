//
// HeaderId.cs
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
using System.Text;
using System.Reflection;

namespace MimeKit {
	public enum HeaderId {
		AdHoc,
		ApparentlyTo,
		Approved,
		Article,
		Bcc,
		Bytes,
		Cc,
		Comments,
		ContentBase,
		ContentClass,
		ContentDescription,
		ContentDisposition,
		ContentDuration,
		ContentId,
		ContentLanguage,
		ContentLocation,
		ContentMd5,
		ContentTransferEncoding,
		ContentType,
		Control,
		Date,
		DeferredDelivery,
		DispositionNotificationTo,
		Distribution,
		Encoding,
		Encrypted,
		Expires,
		ExpiryDate,
		FollowUpTo,
		From,
		Importance,
		InReplyTo,
		Keywords,
		Lines,
		ListHelp,
		ListSubscribe,
		ListUnsubscribe,
		MessageId,
		MimeVersion,
		NewsGroups,
		NntpPostingHost,
		Organization,
		Path,
		Precedence,
		Priority,
		Received,
		References,
		ReplyBy,
		ReplyTo,
		ResentBcc,
		ResentCc,
		ResentDate,
		ResentFrom,
		ResentMessageId,
		ResentReplyTo,
		ResentSender,
		ResentTo,
		ReturnPath,
		ReturnReceiptTo,
		Sender,
		Sensitivity,
		Subject,
		Summary,
		Supercedes,
		To,
		Unknown = -1
	}

	[AttributeUsage (AttributeTargets.Field)]
	public class HeaderNameAttribute : Attribute
	{
		public HeaderNameAttribute (string name)
		{
			HeaderName = name;
		}

		public string HeaderName {
			get; private set;
		}
	}

	public static class HeaderIdExtension
	{
		public static string ToHeaderName (this Enum value)
		{
			var name = value.ToString ();
			var type = value.GetType ();
			var field = type.GetField (name);

			var attrs = field.GetCustomAttributes (typeof (HeaderNameAttribute), true);
			if (attrs == null || attrs.Length == 0) {
				var builder = new StringBuilder (name);

				for (int i = 2; i < builder.Length; i++) {
					if (char.IsUpper (builder[i]))
						builder.Insert (i++, '-');
				}

				return builder.ToString ();
			}

			return ((HeaderNameAttribute) attrs[0]).HeaderName;
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
