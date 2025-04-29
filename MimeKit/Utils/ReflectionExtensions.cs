using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace MimeKit.Utils {
	internal static class ReflectionExtensions
	{
#if NET5_0_OR_GREATER
		[RequiresUnreferencedCode ("Types might be removed")]
#endif
		public static Type GetRequiredType (this Assembly assembly, string name)
		{
			return assembly.GetType (name) ?? throw new TargetException ($"Type '{name}' not found in assembly '{assembly.FullName}'");
		}

		public static PropertyInfo GetRequiredProperty (
#if NET5_0_OR_GREATER
			[DynamicallyAccessedMembers (DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
			this Type type,
			string name)
		{
			return type.GetProperty (name) ?? throw new TargetException ($"Property '{name}' not found on type '{type.FullName}'");
		}
	}
}
