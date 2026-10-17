//
// MimeTreeDisposer.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2026 .NET Foundation and Contributors
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
using System.Runtime.ExceptionServices;

namespace MimeKit.Utils {
	[Flags]
	enum MimeDisposalState : byte
	{
		None = 0,
		LocalComplete = 1,
		SubtreeComplete = 2,
		Active = 4
	}

	interface IMimeDisposable
	{
		MimeDisposalState DisposalState { get; set; }
		IMimeDisposable? GetChild (int index);
		void DisposeLocal ();
	}

	// Ownership traversal is separate from virtual, node-local cleanup so that neither tree depth nor width
	// increases the call stack, and retrying a failed subtree does not repeat successful cleanup hooks.
	static class MimeTreeDisposer
	{
		struct Frame
		{
			public readonly IMimeDisposable Node;
			public int ChildIndex;
			public bool ChildrenFailed;

			public Frame (IMimeDisposable node)
			{
				Node = node;
				ChildIndex = 0;
				ChildrenFailed = false;
			}
		}

		public static void Dispose (IMimeDisposable root)
		{
			if ((root.DisposalState & (MimeDisposalState.SubtreeComplete | MimeDisposalState.Active)) != 0)
				return;

			Stack<Frame>? ancestors = null;
			List<Exception>? exceptions = null;
			var frame = new Frame (root);

			root.DisposalState |= MimeDisposalState.Active;
			try {
				while (true) {
					var child = frame.Node.GetChild (frame.ChildIndex);

					if (child != null) {
						frame.ChildIndex++;

						if ((child.DisposalState & MimeDisposalState.SubtreeComplete) != 0)
							continue;

						if ((child.DisposalState & MimeDisposalState.Active) != 0) {
							exceptions ??= new List<Exception> ();
							exceptions.Add (new InvalidOperationException ("The MIME ownership graph contains a cycle."));
							frame.ChildrenFailed = true;
							continue;
						}

						ancestors ??= new Stack<Frame> ();
						ancestors.Push (frame);
						frame = new Frame (child);
						child.DisposalState |= MimeDisposalState.Active;
						continue;
					}

					if ((frame.Node.DisposalState & MimeDisposalState.LocalComplete) == 0) {
						try {
							frame.Node.DisposeLocal ();
							frame.Node.DisposalState |= MimeDisposalState.LocalComplete;
							GC.SuppressFinalize (frame.Node);
						} catch (Exception ex) {
							exceptions ??= new List<Exception> ();
							exceptions.Add (ex);
						}
					}

					bool complete = !frame.ChildrenFailed && (frame.Node.DisposalState & MimeDisposalState.LocalComplete) != 0;

					if (complete)
						frame.Node.DisposalState |= MimeDisposalState.SubtreeComplete;
					frame.Node.DisposalState &= ~MimeDisposalState.Active;

					if (ancestors == null || ancestors.Count == 0)
						break;

					frame = ancestors.Pop ();
					frame.ChildrenFailed |= !complete;
				}
			} finally {
				frame.Node.DisposalState &= ~MimeDisposalState.Active;
				if (ancestors != null) {
					foreach (var ancestor in ancestors)
						ancestor.Node.DisposalState &= ~MimeDisposalState.Active;
				}
			}

			if (exceptions != null) {
				if (exceptions.Count == 1)
					ExceptionDispatchInfo.Capture (exceptions[0]).Throw ();

				throw new AggregateException (exceptions);
			}
		}
	}
}