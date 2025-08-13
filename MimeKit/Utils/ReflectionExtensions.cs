//
// ReflectionExtensions.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2025 .NET Foundation and Contributors
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
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace MimeKit.Utils {
	static class ReflectionExtensions
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
