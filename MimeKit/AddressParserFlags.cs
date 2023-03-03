// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace MimeKit {
	[Flags]
	enum AddressParserFlags
	{
		AllowMailboxAddress = 1 << 0,
		AllowGroupAddress = 1 << 1,
		ThrowOnError = 1 << 2,
		Internal = 1 << 3,

		TryParse = AllowMailboxAddress | AllowGroupAddress,
		InternalTryParse = TryParse | Internal,
		Parse = TryParse | ThrowOnError
	}
}
