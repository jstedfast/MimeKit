// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

using MimeKit.Utils;

namespace UnitTests.Utils {
	public class ValueStringBuilderTests
	{
		[Test]
		public void Ctor_Default_CanAppend ()
		{
			var vsb = default (ValueStringBuilder);
			Assert.That (vsb.Length, Is.EqualTo (0));

			vsb.Append ('a');
			Assert.That (vsb.Length, Is.EqualTo (1));
			Assert.That (vsb.ToString (), Is.EqualTo ("a"));
		}

#if UNUSED_VALUESTRINGBUILDER_API
		[Test]
		public void Ctor_Span_CanAppend ()
		{
			var vsb = new ValueStringBuilder (new char[1]);
			Assert.AreEqual (0, vsb.Length);

			vsb.Append ('a');
			Assert.AreEqual (1, vsb.Length);
			Assert.AreEqual ("a", vsb.ToString ());
		}
#endif

		[Test]
		public void Ctor_InitialCapacity_CanAppend ()
		{
			var vsb = new ValueStringBuilder (1);
			Assert.That (vsb.Length, Is.EqualTo (0));

			vsb.Append ('a');
			Assert.That (vsb.Length, Is.EqualTo (1));
			Assert.That (vsb.ToString (), Is.EqualTo ("a"));
		}

		[Test]
		public void Append_Char_MatchesStringBuilder ()
		{
			var sb = new StringBuilder ();
			var vsb = new ValueStringBuilder ();
			for (int i = 1; i <= 100; i++) {
				sb.Append ((char) i);
				vsb.Append ((char) i);
			}

			Assert.That (vsb.Length, Is.EqualTo (sb.Length));
			Assert.That (vsb.ToString (), Is.EqualTo (sb.ToString ()));
		}

		[Test]
		public void Append_String_MatchesStringBuilder ()
		{
			var sb = new StringBuilder ();
			var vsb = new ValueStringBuilder ();
			for (int i = 1; i <= 100; i++) {
				string s = i.ToString ();
				sb.Append (s);
				vsb.Append (s);
			}

			Assert.That (vsb.Length, Is.EqualTo (sb.Length));
			Assert.That (vsb.ToString (), Is.EqualTo (sb.ToString ()));
		}

		[Test]
		public void Append_NullString_MatchesStringBuilder ()
		{
			var sb = new StringBuilder ();
			var vsb = new ValueStringBuilder ();

#pragma warning disable CA1834 // Consider using 'StringBuilder.Append(char)' when applicable
			sb.Append ("a");
			sb.Append ((string) null);
			sb.Append ("b");
#pragma warning restore CA1834 // Consider using 'StringBuilder.Append(char)' when applicable

			vsb.Append ("a");
			vsb.Append ((string?) null);
			vsb.Append ("b");

			Assert.That (vsb.Length, Is.EqualTo (sb.Length));
			Assert.That (vsb.ToString (), Is.EqualTo (sb.ToString ()));
		}

		[Test]
		public void Insert_NullString_MatchesStringBuilder ()
		{
			var sb = new StringBuilder ();
			var vsb = new ValueStringBuilder ();

#pragma warning disable CA1834 // Consider using 'StringBuilder.Append(char)' when applicable
			sb.Append ("b");
			sb.Insert (0, (string) null);
			sb.Insert (0, "a");
#pragma warning restore CA1834 // Consider using 'StringBuilder.Append(char)' when applicable

			vsb.Append ("b");
			vsb.Insert (0, (string?) null);
			vsb.Insert (0, "a");

			Assert.That (vsb.Length, Is.EqualTo (sb.Length));
			Assert.That (vsb.ToString (), Is.EqualTo (sb.ToString ()));
		}

		[Test]
		public void Insert_Grows_MatchesStringBuilder ()
		{
			var vsb = new ValueStringBuilder ();

			vsb.Append ("s");
			vsb.Insert (0, "Make sure that the ValueStringBuilder's capacity grow");

			Assert.That (vsb.ToString (), Is.EqualTo ("Make sure that the ValueStringBuilder's capacity grows"));
		}

#if UNUSED_VALUESTRINGBUILDER_API
		[TestCase (0, 4 * 1024 * 1024)]
		[TestCase (1025, 4 * 1024 * 1024)]
		[TestCase (3 * 1024 * 1024, 6 * 1024 * 1024)]
		public void Append_String_Large_MatchesStringBuilder (int initialLength, int stringLength)
		{
			var sb = new StringBuilder (initialLength);
			var vsb = new ValueStringBuilder (new char[initialLength]);

			string s = new string ('a', stringLength);
			sb.Append (s);
			vsb.Append (s);

			Assert.AreEqual (sb.Length, vsb.Length);
			Assert.AreEqual (sb.ToString (), vsb.ToString ());
		}

		[Test]
		public void Append_CharInt_MatchesStringBuilder ()
		{
			var sb = new StringBuilder ();
			var vsb = new ValueStringBuilder ();
			for (int i = 1; i <= 100; i++) {
				sb.Append ((char) i, i);
				vsb.Append ((char) i, i);
			}

			Assert.AreEqual (sb.Length, vsb.Length);
			Assert.AreEqual (sb.ToString (), vsb.ToString ());
		}

		[Test]
		public unsafe void Append_PtrInt_MatchesStringBuilder ()
		{
			var sb = new StringBuilder ();
			var vsb = new ValueStringBuilder ();
			for (int i = 1; i <= 100; i++) {
				string s = i.ToString ();
				fixed (char* p = s) {
					sb.Append (p, s.Length);
					vsb.Append (p, s.Length);
				}
			}

			Assert.AreEqual (sb.Length, vsb.Length);
			Assert.AreEqual (sb.ToString (), vsb.ToString ());
		}

		[Test]
		public void AppendSpan_DataAppendedCorrectly ()
		{
			var sb = new StringBuilder ();
			var vsb = new ValueStringBuilder ();

			for (int i = 1; i <= 1000; i++) {
				string s = i.ToString ();

				sb.Append (s);

				Span<char> span = vsb.AppendSpan (s.Length);
				Assert.AreEqual (sb.Length, vsb.Length);

				s.AsSpan ().CopyTo (span);
			}

			Assert.AreEqual (sb.Length, vsb.Length);
			Assert.AreEqual (sb.ToString (), vsb.ToString ());
		}

		[Test]
		public void Insert_IntCharInt_MatchesStringBuilder ()
		{
			var sb = new StringBuilder ();
			var vsb = new ValueStringBuilder ();
			var rand = new Random (42);

			for (int i = 1; i <= 100; i++) {
				int index = rand.Next (sb.Length);
				sb.Insert (index, new string ((char) i, 1), i);
				vsb.Insert (index, (char) i, i);
			}

			Assert.AreEqual (sb.Length, vsb.Length);
			Assert.AreEqual (sb.ToString (), vsb.ToString ());
		}

		[Test]
		public void AsSpan_ReturnsCorrectValue_DoesntClearBuilder ()
		{
			var sb = new StringBuilder ();
			var vsb = new ValueStringBuilder ();

			for (int i = 1; i <= 100; i++) {
				string s = i.ToString ();
				sb.Append (s);
				vsb.Append (s);
			}

			var resultString = new string (vsb.AsSpan ());
			Assert.AreEqual (sb.ToString (), resultString);

			Assert.NotEqual (0, sb.Length);
			Assert.AreEqual (sb.Length, vsb.Length);
		}

		[Test]
		public void ToString_ClearsBuilder_ThenReusable ()
		{
			const string Text1 = "test";
			var vsb = new ValueStringBuilder ();

			vsb.Append (Text1);
			Assert.AreEqual (Text1.Length, vsb.Length);

			string s = vsb.ToString ();
			Assert.AreEqual (Text1, s);

			Assert.AreEqual (0, vsb.Length);
			Assert.AreEqual (string.Empty, vsb.ToString ());
			Assert.True (vsb.TryCopyTo (Span<char>.Empty, out _));

			const string Text2 = "another test";
			vsb.Append (Text2);
			Assert.AreEqual (Text2.Length, vsb.Length);
			Assert.AreEqual (Text2, vsb.ToString ());
		}

		[Test]
		public void TryCopyTo_FailsWhenDestinationIsTooSmall_SucceedsWhenItsLargeEnough ()
		{
			var vsb = new ValueStringBuilder ();

			const string Text = "expected text";
			vsb.Append (Text);
			Assert.AreEqual (Text.Length, vsb.Length);

			Span<char> dst = new char[Text.Length - 1];
			Assert.False (vsb.TryCopyTo (dst, out int charsWritten));
			Assert.AreEqual (0, charsWritten);
			Assert.AreEqual (0, vsb.Length);
		}

		[Test]
		public void TryCopyTo_ClearsBuilder_ThenReusable ()
		{
			const string Text1 = "test";
			var vsb = new ValueStringBuilder ();

			vsb.Append (Text1);
			Assert.AreEqual (Text1.Length, vsb.Length);

			Span<char> dst = new char[Text1.Length];
			Assert.True (vsb.TryCopyTo (dst, out int charsWritten));
			Assert.AreEqual (Text1.Length, charsWritten);
			Assert.AreEqual (Text1, new string (dst));

			Assert.AreEqual (0, vsb.Length);
			Assert.AreEqual (string.Empty, vsb.ToString ());
			Assert.True (vsb.TryCopyTo (Span<char>.Empty, out _));

			const string Text2 = "another test";
			vsb.Append (Text2);
			Assert.AreEqual (Text2.Length, vsb.Length);
			Assert.AreEqual (Text2, vsb.ToString ());
		}

		[Test]
		public void Dispose_ClearsBuilder_ThenReusable ()
		{
			const string Text1 = "test";
			var vsb = new ValueStringBuilder ();

			vsb.Append (Text1);
			Assert.AreEqual (Text1.Length, vsb.Length);

			vsb.Dispose ();

			Assert.AreEqual (0, vsb.Length);
			Assert.AreEqual (string.Empty, vsb.ToString ());
			Assert.True (vsb.TryCopyTo (Span<char>.Empty, out _));

			const string Text2 = "another test";
			vsb.Append (Text2);
			Assert.AreEqual (Text2.Length, vsb.Length);
			Assert.AreEqual (Text2, vsb.ToString ());
		}
#endif

		[Test]
		public void Indexer ()
		{
			const string Text1 = "foobar";
			var vsb = new ValueStringBuilder ();

			vsb.Append (Text1);

			Assert.That (vsb[3], Is.EqualTo ('b'));
			vsb[3] = 'c';
			Assert.That (vsb[3], Is.EqualTo ('c'));
		}

#if UNUSED_VALUESTRINGBUILDER_API
		[Test]
		public void EnsureCapacity_IfRequestedCapacityWins ()
		{
			// Note: constants used here may be dependent on minimal buffer size
			// the ArrayPool is able to return.
			var builder = new ValueStringBuilder (stackalloc char[32]);

			builder.EnsureCapacity (65);

			Assert.AreEqual (128, builder.Capacity);
		}

		[Test]
		public void EnsureCapacity_IfBufferTimesTwoWins ()
		{
			var builder = new ValueStringBuilder (stackalloc char[32]);

			builder.EnsureCapacity (33);

			Assert.AreEqual (64, builder.Capacity);
		}

		[Test]
		public void EnsureCapacity_NoAllocIfNotNeeded ()
		{
			// Note: constants used here may be dependent on minimal buffer size
			// the ArrayPool is able to return.
			var builder = new ValueStringBuilder (stackalloc char[64]);

			builder.EnsureCapacity (16);

			Assert.AreEqual (64, builder.Capacity);
		}
#endif
	}
}
