#if !NET5_0_OR_GREATER
using System;

namespace System.Runtime.CompilerServices {
	[AttributeUsage (AttributeTargets.Method, Inherited = false)]
	internal sealed class ModuleInitializerAttribute : Attribute { }
}
#endif
