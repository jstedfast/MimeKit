using System.Runtime.CompilerServices;

namespace MimeKit.Cryptography {
	static class CryptographyModuleInitializer
	{
		[ModuleInitializer]
		internal static void Initialize ()
		{
			CryptographyContext.RegisterSecureMimeContextFactory (() => new DefaultSecureMimeContext ());
		}
	}
}
