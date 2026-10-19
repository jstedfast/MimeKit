//
// TestHelper.cs
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

using System.Text;

using MimeKit.Utils;
using MimeKit.Cryptography;

namespace UnitTests {
	[SetUpFixture]
	static class TestHelper
	{
		public static readonly string ProjectDir;

		/// <summary>
		/// Whether the MimeKit assemblies have been instrumented by AltCover for code coverage.
		/// </summary>
		/// <remarks>
		/// The coverage recorder allocates memory as it tracks visited sequence points, which
		/// makes allocation-budget assertions meaningless in instrumented builds.
		/// </remarks>
		public static readonly bool IsCodeCoverageEnabled = IsInstrumented (typeof (MimeKit.MimeMessage).Assembly) ||
			IsInstrumented (typeof (MimeKit.Cryptography.SecureMimeContext).Assembly);

		static bool IsInstrumented (System.Reflection.Assembly assembly)
		{
			foreach (var reference in assembly.GetReferencedAssemblies ()) {
				if (reference.Name != null && reference.Name.StartsWith ("AltCover.Recorder", StringComparison.Ordinal))
					return true;
			}

			return false;
		}

		static TestHelper ()
		{
#if NET5_0_OR_GREATER
			var codeBase = typeof (TestHelper).Assembly.Location;
#else
			var codeBase = typeof (TestHelper).Assembly.CodeBase;
			if (codeBase.StartsWith ("file://", StringComparison.OrdinalIgnoreCase))
				codeBase = codeBase.Substring ("file://".Length);

			if (Path.DirectorySeparatorChar == '\\') {
				if (codeBase[0] == '/')
					codeBase = codeBase.Substring (1);

				codeBase = codeBase.Replace ('/', '\\');
			}
#endif

			var dir = Path.GetDirectoryName (codeBase);

			while (Path.GetFileName (dir) != "UnitTests")
				dir = Path.GetFullPath (Path.Combine (dir, ".."));

			ProjectDir = Path.GetFullPath (dir);
		}

		static bool TryEstimateOutputLength (Func<int, int> estimate, int inputLength, out int outputLength)
		{
			try {
				outputLength = estimate (inputLength);
				return true;
			} catch (ArgumentOutOfRangeException) {
				outputLength = 0;
				return false;
			}
		}

		// Asserts that an EstimateOutputLength() implementation never wraps around: every input length that is accepted
		// must produce an estimate that is at least as large as the input length, and every input length above the
		// largest accepted input length (including int.MaxValue) must be rejected with an ArgumentOutOfRangeException.
		public static void AssertEstimateOutputLengthDoesNotOverflow (Func<int, int> estimate, string message = null)
		{
			int lo = 0, hi = int.MaxValue;

			// Note: Accepted input lengths are contiguous from 0, so binary search for the largest one.
			while (lo < hi) {
				int mid = lo + (int) (((long) hi - lo + 1) / 2);

				if (TryEstimateOutputLength (estimate, mid, out _))
					lo = mid;
				else
					hi = mid - 1;
			}

			Assert.That (lo, Is.GreaterThan (int.MaxValue / 4), $"{message}: largest accepted input length");

			for (int inputLength = lo; inputLength >= lo - 64; inputLength--) {
				Assert.That (TryEstimateOutputLength (estimate, inputLength, out var outputLength), Is.True, $"{message}: {inputLength}");
				Assert.That (outputLength, Is.GreaterThanOrEqualTo (inputLength), $"{message}: estimate for {inputLength}");
			}

			if (lo < int.MaxValue) {
				var ex = Assert.Throws<ArgumentOutOfRangeException> (() => estimate (lo + 1), $"{message}: {lo + 1}");
				Assert.That (ex.ParamName, Is.EqualTo ("inputLength"), message);
				Assert.Throws<ArgumentOutOfRangeException> (() => estimate (int.MaxValue), $"{message}: int.MaxValue");
			}
		}

		[OneTimeSetUp]
		public static void Init ()
		{
			lock (CodePagesEncodingProvider.Instance) {
				Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
				CharsetUtils.GetCodePage ("iso-2022-jp");
			}

			CryptographyModule.Initialize ();
		}
	}
}
