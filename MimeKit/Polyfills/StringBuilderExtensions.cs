#if NETSTANDARD2_0 || NETFRAMEWORK

using System;
using System.Text;

namespace MimeKit;

internal static class StringBuilderExtensions
{
	public static void Append (this StringBuilder sb, ReadOnlySpan<char> value)
	{
		sb.Append (value.ToString ());
	}
}
#endif
