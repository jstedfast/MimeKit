//
// NullableAttributes.cs
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

#if NETFRAMEWORK || NETSTANDARD2_0 || NETSTANDARD2_1

namespace System.Diagnostics.CodeAnalysis {
#if !NETSTANDARD2_1
	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
	sealed class AllowNullAttribute : Attribute
	{
		public AllowNullAttribute () { }
	}

	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
	sealed class DisallowNullAttribute : Attribute
	{
		public DisallowNullAttribute () { }
	}

	[AttributeUsage (AttributeTargets.Parameter, Inherited = false)]
	sealed class DoesNotReturnAttribute : Attribute
	{
		public DoesNotReturnAttribute () { }
	}

	[AttributeUsage (AttributeTargets.Parameter, Inherited = false)]
	sealed class DoesNotReturnIfAttribute : Attribute
	{
		public DoesNotReturnIfAttribute (bool parameterValue) => ParameterValue = parameterValue;

		public bool ParameterValue { get; }
	}

	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
	sealed class MaybeNullAttribute : Attribute
	{
		public MaybeNullAttribute () { }
	}

	[AttributeUsage (AttributeTargets.Parameter, Inherited = false)]
	sealed class MaybeNullWhenAttribute : Attribute
	{
		public MaybeNullWhenAttribute (bool returnValue) => ReturnValue = returnValue;

		public bool ReturnValue { get; }
	}

	[AttributeUsage (AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
	sealed class NotNullAttribute : Attribute
	{
		public NotNullAttribute () { }
	}

	[AttributeUsage (AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
	sealed class NotNullIfNotNullAttribute : Attribute
	{
		public NotNullIfNotNullAttribute (string parameterName) => ParameterName = parameterName;

		public string ParameterName { get; }
	}

	[AttributeUsage (AttributeTargets.Parameter, Inherited = false)]
	sealed class NotNullWhenAttribute : Attribute
	{
		public NotNullWhenAttribute (bool returnValue) => ReturnValue = returnValue;

		public bool ReturnValue { get; }
	}
#endif

	[AttributeUsage (AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
	sealed class MemberNotNullAttribute : Attribute
	{
		public MemberNotNullAttribute (string member) => Members = new string[] { member };

		public MemberNotNullAttribute (params string[] members) => Members = members;

		public string[] Members { get; }
	}

	[AttributeUsage (AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
	sealed class MemberNotNullWhenAttribute : Attribute
	{
		public MemberNotNullWhenAttribute (bool returnValue, string member)
		{
			Members = new string[] { member };
			ReturnValue = returnValue;
		}

		public MemberNotNullWhenAttribute (bool returnValue, string[] members)
		{
			Members = members;
			ReturnValue = returnValue;
		}

		public string[] Members { get; }

		public bool ReturnValue { get; }
	}
}

#endif
