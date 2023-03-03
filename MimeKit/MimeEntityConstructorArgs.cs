// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace MimeKit {
	/// <summary>
	/// MIME entity constructor arguments.
	/// </summary>
	/// <remarks>
	/// MIME entity constructor arguments.
	/// </remarks>
	public sealed class MimeEntityConstructorArgs
	{
		internal readonly ParserOptions ParserOptions;
		internal readonly IEnumerable<Header> Headers;
		internal readonly ContentType ContentType;
		internal readonly bool IsTopLevel;

		internal MimeEntityConstructorArgs (ParserOptions options, ContentType ctype, IEnumerable<Header> headers, bool toplevel)
		{
			ParserOptions = options;
			IsTopLevel = toplevel;
			ContentType = ctype;
			Headers = headers;
		}
	}
}
