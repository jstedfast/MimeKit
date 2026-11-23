//
// HtmlFormattingTracker.cs
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

namespace MimeKit.Text {
	// Experimental: retain bounded structural keys, never source tokens or their attribute collections.
	sealed class HtmlFormattingTracker
	{
		public enum StopReason
		{
			None,
			Capacity,
			AttributeEquivalence,
			ComplexAdoption,
			StackRemapping
		}

		struct Slot
		{
			public HtmlTagId Id;
			public int Index, Instance;
			public string? AttributeKey;
		}

		readonly Slot[] slots;
		int count;

		public HtmlFormattingTracker (int capacity)
		{
			slots = new Slot[capacity];
		}

		public int PeakCount { get; private set; }
		public StopReason Reason { get; private set; }
		public bool Exceeded => Reason != StopReason.None;

		public bool Pending => count > 0 && slots[count - 1].Index < 0;

		void Remove (int index)
		{
			Array.Copy (slots, index + 1, slots, index, --count - index);
			slots[count] = default;
		}

		// Empty means no attributes; null means that exact comparison is unavailable. Encode
		// length-prefixed name/value pairs sorted by ASCII-lowercased name; keep the first duplicate.
		static string? GetAttributeKey (HtmlAttributeCollection? attributes)
		{
			if (attributes == null || attributes.Count == 0)
				return string.Empty;

			if (attributes.Count > 4)
				return null;

			Span<char> key = stackalloc char[128];
			int length = 0;

			foreach (var attribute in attributes) {
				string name = attribute.Name, value = attribute.Value ?? string.Empty;

				if (name.Length > key.Length - 2 || value.Length > key.Length - 2 - name.Length)
					return null;

				if (name.IndexOf ('\r') >= 0 || value.IndexOf ('\r') >= 0)
					return null;

				int pos = 0, order = 1;
				while (pos < length) {
					int n = key[pos];
					order = 0;

					for (int i = 0; i < Math.Min (n, name.Length) && order == 0; i++)
						order = FoldAscii (name[i]) - key[pos + 1 + i];

					if (order == 0)
						order = name.Length - n;

					if (order <= 0)
						break;

					pos += 2 + n + key[pos + 1 + n];
				}

				if (order == 0)
					continue;

				int size = 2 + name.Length + value.Length;

				if (size > key.Length - length)
					return null;

				key.Slice (pos, length - pos).CopyTo (key.Slice (pos + size));
				key[pos++] = (char) name.Length;

				foreach (char c in name)
					key[pos++] = FoldAscii (c);

				key[pos++] = (char) value.Length;
				value.AsSpan ().CopyTo (key.Slice (pos));
				length += size;
			}

			return key.Slice (0, length).ToString ();
		}

		static char FoldAscii (char c) => c >= 'A' && c <= 'Z' ? (char) (c + 32) : c;

		public void Push (HtmlTagId id, HtmlAttributeCollection? attributes, HtmlOpenElementStack stack)
		{
			if (Exceeded || stack.DepthExceeded)
				return;

			var key = GetAttributeKey (attributes);

			if (id != HtmlTagId.Unknown) {
				int matches = 0;
				bool uncertain = false;

				for (int i = count - 1; i >= 0 && slots[i].Id != HtmlTagId.Unknown; i--) {
					if (slots[i].Id != id)
						continue;

					var other = slots[i].AttributeKey;

					if (key != other && (key == string.Empty || other == string.Empty || (key != null && other != null)))
						continue;

					uncertain |= key == null || other == null;

					if (++matches != 3)
						continue;

					if (uncertain) {
						Reason = StopReason.AttributeEquivalence;
						return;
					}

					Remove (i);
					break;
				}
			}

			if (count == slots.Length) {
				Reason = StopReason.Capacity;
				return;
			}

			slots[count++] = new Slot { Id = id, Index = stack.Top, Instance = stack[stack.Top].Count, AttributeKey = key };
			PeakCount = Math.Max (PeakCount, count);
		}

		public void Popped (HtmlOpenElementStack stack)
		{
			for (int i = 0; i < count; i++) {
				ref var slot = ref slots[i];

				if (slot.Index < 0 || (slot.Index < stack.Count && slot.Instance <= stack[slot.Index].Count))
					continue;

				if (slot.Id == HtmlTagId.Unknown) {
					Array.Clear (slots, i, count - i);
					count = i;
					return;
				}

				slot.Index = -1;
			}
		}

		public void Reconstruct (HtmlOpenElementStack stack)
		{
			int start = count;

			while (start > 0 && slots[start - 1].Index < 0)
				start--;

			for (int i = start; i < count; i++) {
				ref var slot = ref slots[i];

				// HTML formatting elements have no special/scope flags.
				stack.Push (slot.Id.ToHtmlTagName (), slot.Id, HtmlNamespace.Html, HtmlOpenElementStack.ElementFlags.None);

				if (stack.DepthExceeded)
					return;

				slot.Index = stack.Top;
				slot.Instance = stack[stack.Top].Count;
			}
		}

		public enum CloseAction
		{
			None,
			PopThrough,
			RemoveBelowBlock,
			RemoveBelowTwoBlocks,
			PopInlineThenRemoveBelowBlock,
			RemoveBelowTrackedFormatting
		}

		static bool IsRecoverableBlock (in HtmlOpenElementStack.Entry entry)
		{
			// None of these blocks owns a formatting marker or a form pointer.
			return entry.Count == 1 && entry.Namespace == HtmlNamespace.Html &&
				(entry.Id == HtmlTagId.P || entry.Id == HtmlTagId.Div || entry.Id == HtmlTagId.LI || entry.Id == HtmlTagId.Center);
		}

		bool CanRetainFormatting (HtmlOpenElementStack stack, int index, int slot)
		{
			// The adoption inner loop retains at most three physical formatting nodes. Verify
			// that each is active after the same marker, including instances folded together.
			int next = index + 1, nodes = 0;

			while (next <= stack.Top && stack[next].Namespace == HtmlNamespace.Html && stack[next].Id.IsFormattingElement ()) {
				if (stack[next].Count > 3 - nodes)
					return false;

				for (int instance = 1; instance <= stack[next].Count; instance++) {
					if (++slot >= count || slots[slot].Index != next || slots[slot].Instance != instance || slots[slot].Id != stack[next].Id)
						return false;
				}

				nodes += stack[next++].Count;
			}

			// Each special block takes one outer iteration. Reserve the eighth for
			// actually closing the formatting element, rather than merely moving it.
			int blocks = 0;

			while (next <= stack.TopSpecial) {
				ref readonly var entry = ref stack[next];

				if (entry.Namespace != HtmlNamespace.Html)
					return false;

				if (!(entry.Id == HtmlTagId.Div || entry.Id == HtmlTagId.P || (entry.Flags & HtmlOpenElementStack.ElementFlags.Heading) != 0))
					return false;

				if (entry.Count > 7 - blocks)
					return false;

				blocks += entry.Count;
				next++;
			}

			if (blocks == 0)
				return false;

			for (; next <= stack.Top; next++) {
				ref readonly var entry = ref stack[next];

				if (entry.Namespace == HtmlNamespace.Html ? entry.Id != HtmlTagId.Span : entry.Flags != 0)
					return false;
			}

			return true;
		}

		public void RemoveBelowTrackedFormatting (HtmlOpenElementStack stack, int index)
		{
			if (stack[index].Count == 1) {
				// Removing the target may fold the first surviving entry into its predecessor.
				bool merge = index > 0 && stack[index - 1].Namespace == HtmlNamespace.Html && stack[index - 1].Id == stack[index + 1].Id;

				for (int i = 0; i < count; i++) {
					ref var slot = ref slots[i];

					if (slot.Index <= index)
						continue;

					if (merge && slot.Index == index + 1)
						slot.Instance += stack[index - 1].Count;

					slot.Index -= merge ? 2 : 1;
				}
			}

			stack.RemoveEntry (index);
		}

		public CloseAction Close (HtmlTagId id, HtmlOpenElementStack stack, bool nestedAnchor)
		{
			for (int i = count - 1; i >= 0 && slots[i].Id != HtmlTagId.Unknown; i--) {
				if (slots[i].Id != id)
					continue;

				int index = slots[i].Index;

				if (index < 0) {
					Remove (i);
					return CloseAction.None;
				}

				if (index < stack.TopScope) {
					if (nestedAnchor)
						Reason = StopReason.ComplexAdoption;

					return CloseAction.None;
				}

				if (index != stack.IndexOf (id) || slots[i].Instance != stack[index].Count) {
					Reason = StopReason.ComplexAdoption;
					return CloseAction.None;
				}

				if (index <= stack.TopSpecial) {
					if (stack.Top > index && stack.Top <= index + 2 && IsRecoverableBlock (stack[index + 1])) {
						// Each adjacent block moves the replacement formatting node one place up.
						// The final pass pops the replacement and any remaining inline descendants.
						var action = CloseAction.RemoveBelowBlock;

						if (stack.Top == index + 2) {
							ref readonly var top = ref stack[stack.Top];

							if (IsRecoverableBlock (top))
								action = CloseAction.RemoveBelowTwoBlocks;
							else if (top.Namespace == HtmlNamespace.Html && (top.Id.IsFormattingElement () || top.Id == HtmlTagId.Span))
								action = CloseAction.PopInlineThenRemoveBelowBlock;
							else
								action = CloseAction.None;
						}

						if (action != CloseAction.None) {
							Remove (i);
							return action;
						}
					}

					if (CanRetainFormatting (stack, index, i)) {
						Remove (i);
						return CloseAction.RemoveBelowTrackedFormatting;
					}

					Reason = StopReason.ComplexAdoption;
					return CloseAction.None;
				}

				Remove (i);
				return CloseAction.PopThrough;
			}

			// With no active entry, the ordinary end-tag walk cannot pass a special element.
			return !nestedAnchor && stack.IndexOf (id) > stack.TopSpecial ? CloseAction.PopThrough : CloseAction.None;
		}

		public void BeforeMiddleRemoval (int index)
		{
			// Rebuilding a stack suffix may merge folded entries. Don't reuse ambiguous indexes.
			for (int i = 0; i < count; i++) {
				if (slots[i].Index >= index) {
					Reason = StopReason.StackRemapping;
					break;
				}
			}
		}
	}
}
