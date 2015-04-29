//
// CancellationToken.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (www.xamarin.com)
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

namespace System.Threading {
	public struct CancellationToken
	{
		public static readonly CancellationToken None = new CancellationToken ();

		public bool CanBeCancelled {
			get { return false; }
		}

		public bool IsCancellationRequested {
			get { return false; }
		}

		public void ThrowIfCancellationRequested ()
		{
			if (IsCancellationRequested)
				throw new OperationCanceledException ();
		}

		public bool Equals (CancellationToken other)
		{
			return true;
		}

		public override bool Equals (object obj)
		{
			if (obj is CancellationToken)
				return Equals ((CancellationToken) obj);

			return false;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public static bool operator == (CancellationToken left, CancellationToken right)
		{
			return left.Equals (right);
		}

		public static bool operator != (CancellationToken left, CancellationToken right)
		{
			return !left.Equals (right);
		}
	}
}
