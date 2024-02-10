//
// MultipartRelatedUrlCache.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2014-2024 Jeffrey Stedfast
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
using System.Collections.Generic;

using Foundation;

using MimeKit;

namespace MessageReader.iOS {
	/// <summary>
	/// An implementation of an NSUrlCache based on a multipart/related stack.
	/// </summary>
	public class MultipartRelatedUrlCache : NSUrlCache
	{
		readonly IList<MultipartRelated> stack;

		public MultipartRelatedUrlCache (IList<MultipartRelated> stack)
		{
			// Note: we need to clone the multipart/related stack because iOS
			// may not render the HTML immediately and we need to preserve
			// state until it does.
			this.stack = new List<MultipartRelated> (stack);
		}

		public override NSCachedUrlResponse CachedResponseForRequest (NSUrlRequest request)
		{
			var uri = (Uri) request.Url;
			int index;

			// walk up our multipart/related stack looking for the MIME part for the requested URL
			for (int i = stack.Count - 1; i >= 0; i--) {
				var related = stack[i];

				if ((index = related.IndexOf (uri)) == -1)
					continue;

				if (related[index] is MimePart part) {
					var mimeType = part.ContentType.MimeType;
					var charset = part.ContentType.Charset;
					NSUrlResponse response;
					NSData data;

					// create an NSData wrapper for the MIME part's decoded content stream
					using (var content = part.Content.Open ())
						data = NSData.FromStream (content);

					// construct our response
					response = new NSUrlResponse (request.Url, mimeType, (int) data.Length, charset);

					return new NSCachedUrlResponse (response, data);
				}
			}

			return base.CachedResponseForRequest (request);
		}
	}
}
