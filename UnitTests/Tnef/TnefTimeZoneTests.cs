//
// TnefTimeZoneTests.cs
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

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefTimeZoneTests
	{
		// The first Sunday of November and the second Sunday of March, both at 2:00.
		static readonly TnefSystemTime November = new TnefSystemTime (0, 11, 0, 1, 2, 0, 0, 0);
		static readonly TnefSystemTime March = new TnefSystemTime (0, 3, 0, 2, 2, 0, 0, 0);

		static TnefTimeZone Eastern (int bias = 300, int daylightBias = -60, TnefSystemTime? standardDate = null, TnefSystemTime? daylightDate = null)
		{
			return new TnefTimeZone ("Eastern", bias, 0, daylightBias, standardDate ?? November, daylightDate ?? March);
		}

		static readonly TnefSystemTime[] SystemTimes = {
			new TnefSystemTime (1, 11, 0, 1, 2, 0, 0, 0), // Year
			new TnefSystemTime (0, 10, 0, 1, 2, 0, 0, 0), // Month
			new TnefSystemTime (0, 11, 1, 1, 2, 0, 0, 0), // DayOfWeek
			new TnefSystemTime (0, 11, 0, 2, 2, 0, 0, 0), // Day
			new TnefSystemTime (0, 11, 0, 1, 3, 0, 0, 0), // Hour
			new TnefSystemTime (0, 11, 0, 1, 2, 1, 0, 0), // Minute
			new TnefSystemTime (0, 11, 0, 1, 2, 0, 1, 0), // Second
			new TnefSystemTime (0, 11, 0, 1, 2, 0, 0, 1), // Milliseconds
		};

		[Test]
		public void TestSystemTimeEquality ([Range (0, 7)] int index)
		{
			var other = SystemTimes[index];
			var copy = new TnefSystemTime (0, 11, 0, 1, 2, 0, 0, 0);

			Assert.That (November.Equals (copy), Is.True);
			Assert.That (November.Equals ((object) copy), Is.True);
			Assert.That (November.GetHashCode (), Is.EqualTo (copy.GetHashCode ()));

			Assert.That (November.Equals (other), Is.False);
			Assert.That (November.Equals ((object) other), Is.False);
			Assert.That (November.Equals ((object) null), Is.False);
			Assert.That (November.Equals ("November"), Is.False);
		}

		[Test]
		public void TestHasSameRules ()
		{
			var eastern = Eastern ();

			Assert.That (eastern.HasSameRules (eastern), Is.True);
			Assert.That (eastern.HasSameRules (Eastern ()), Is.True);
			Assert.That (eastern.HasSameRules (new TnefTimeZone ("Other", 300, 0, -60, November, March)), Is.True, "the name does not matter");

			Assert.That (eastern.HasSameRules (Eastern (bias: 360)), Is.False, "Bias");
			Assert.That (eastern.HasSameRules (new TnefTimeZone ("Eastern", 300, 1, -60, November, March)), Is.False, "StandardBias");
			Assert.That (eastern.HasSameRules (Eastern (daylightBias: -30)), Is.False, "DaylightBias");
			Assert.That (eastern.HasSameRules (Eastern (standardDate: new TnefSystemTime (0, 10, 0, 5, 2, 0, 0, 0))), Is.False, "StandardDate");
			Assert.That (eastern.HasSameRules (Eastern (daylightDate: new TnefSystemTime (0, 4, 0, 1, 2, 0, 0, 0))), Is.False, "DaylightDate");
			Assert.That (eastern.HasSameRules (new TnefTimeZone ("Eastern", 300, 0, -60, November, default)), Is.False, "HasDaylightTime");
		}

		[Test]
		public void TestHasSameRulesWithoutDaylightTime ()
		{
			var standard = new TnefTimeZone ("Standard", 300, 0, 0, default, default);

			Assert.That (standard.HasDaylightTime, Is.False);
			Assert.That (standard.DaylightOffset, Is.EqualTo (standard.StandardOffset));

			// Without daylight time, the daylight bias and transition dates are ignored.
			Assert.That (standard.HasSameRules (new TnefTimeZone ("Other", 300, 0, -60, November, default)), Is.True);
			Assert.That (standard.HasSameRules (new TnefTimeZone ("Other", 300, 0, 0, default, March)), Is.True);
			Assert.That (standard.HasSameRules (new TnefTimeZone ("Other", 360, 0, 0, default, default)), Is.False);
			Assert.That (standard.HasSameRules (Eastern ()), Is.False);
		}

		[Test]
		public void TestTnefException ()
		{
			var inner = new IOException ("Inner");
			var ex = new TnefException (TnefComplianceViolation.InvalidAttributeLength, "Message", inner);

			Assert.That (ex.Violation, Is.EqualTo (TnefComplianceViolation.InvalidAttributeLength));
			Assert.That (ex.Message, Is.EqualTo ("Message"));
			Assert.That (ex.InnerException, Is.SameAs (inner));

			ex = new TnefException (TnefComplianceViolation.InvalidAttributeValue, "Checksum");

			Assert.That (ex.Violation, Is.EqualTo (TnefComplianceViolation.InvalidAttributeValue));
			Assert.That (ex.Message, Is.EqualTo ("Checksum"));
			Assert.That (ex.InnerException, Is.Null);
		}
	}
}
