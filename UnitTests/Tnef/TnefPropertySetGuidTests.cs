//
// TnefPropertySetGuidTests.cs
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

using System.Reflection;

using MimeKit.Tnef;

namespace UnitTests.Tnef {
	[TestFixture]
	public class TnefPropertySetGuidTests
	{
		static readonly (Guid Value, string Expected)[] PropertySets = {
			(TnefPropertySetGuid.PublicStrings, "00020329-0000-0000-C000-000000000046"),
			(TnefPropertySetGuid.Common, "00062008-0000-0000-C000-000000000046"),
			(TnefPropertySetGuid.Address, "00062004-0000-0000-C000-000000000046"),
			(TnefPropertySetGuid.InternetHeaders, "00020386-0000-0000-C000-000000000046"),
			(TnefPropertySetGuid.Appointment, "00062002-0000-0000-C000-000000000046"),
			(TnefPropertySetGuid.Meeting, "6ED8DA90-450B-101B-98DA-00AA003F1305"),
			(TnefPropertySetGuid.Log, "0006200A-0000-0000-C000-000000000046"),
			(TnefPropertySetGuid.Messaging, "41F28F13-83F4-4114-A584-EEDB5A6B0BFF"),
			(TnefPropertySetGuid.Note, "0006200E-0000-0000-C000-000000000046"),
			(TnefPropertySetGuid.PostRss, "00062041-0000-0000-C000-000000000046"),
			(TnefPropertySetGuid.Task, "00062003-0000-0000-C000-000000000046"),
			(TnefPropertySetGuid.UnifiedMessaging, "4442858E-A9E3-4E80-B900-317A210CC15B"),
			(TnefPropertySetGuid.Mapi, "00020328-0000-0000-C000-000000000046"),
			(TnefPropertySetGuid.AirSync, "71035549-0739-4DCB-9163-00F0580DBBDF"),
			(TnefPropertySetGuid.Sharing, "00062040-0000-0000-C000-000000000046"),
			(TnefPropertySetGuid.XmlExtractedEntities, "23239608-685D-4732-9C55-4C95CB4E8E33"),
			(TnefPropertySetGuid.Attachment, "96357F7F-59E1-47D0-99A7-46515C183B54"),
		};

		[Test]
		public void TestPropertySetGuidValues ()
		{
			foreach (var (value, expected) in PropertySets)
				Assert.That (value, Is.EqualTo (new Guid (expected)), expected);

			var fields = typeof (TnefPropertySetGuid).GetFields (BindingFlags.Public | BindingFlags.Static);
			Assert.That (fields.Length, Is.EqualTo (PropertySets.Length), "every property set GUID is covered");

			var values = fields.Select (f => (Guid) f.GetValue (null)).ToArray ();
			Assert.That (values, Is.Unique);
		}

		[Test]
		public void TestWellKnownNameIds ()
		{
			Assert.That (TnefNameId.ReminderDelta.Kind, Is.EqualTo (TnefNameIdKind.Id));
			Assert.That (TnefNameId.ReminderDelta.PropertySetGuid, Is.EqualTo (TnefPropertySetGuid.Common));
			Assert.That (TnefNameId.ReminderDelta.Id, Is.EqualTo (0x8501));

			Assert.That (TnefNameId.UseTnef.Kind, Is.EqualTo (TnefNameIdKind.Id));
			Assert.That (TnefNameId.UseTnef.PropertySetGuid, Is.EqualTo (TnefPropertySetGuid.Common));
			Assert.That (TnefNameId.UseTnef.Id, Is.EqualTo (0x8582));

			Assert.That (TnefNameId.StartRecurrenceDate.PropertySetGuid, Is.EqualTo (TnefPropertySetGuid.Meeting));
			Assert.That (TnefNameId.StartRecurrenceDate.Id, Is.EqualTo (0x000D));

			Assert.That (TnefNameId.Keywords.Kind, Is.EqualTo (TnefNameIdKind.Name));
			Assert.That (TnefNameId.Keywords.PropertySetGuid, Is.EqualTo (TnefPropertySetGuid.PublicStrings));
			Assert.That (TnefNameId.Keywords.Name, Is.EqualTo ("Keywords"));

			Assert.That (TnefNameId.AttachmentMacInfo.Kind, Is.EqualTo (TnefNameIdKind.Name));
			Assert.That (TnefNameId.AttachmentMacInfo.PropertySetGuid, Is.EqualTo (TnefPropertySetGuid.Attachment));
			Assert.That (TnefNameId.AttachmentMacInfo.Name, Is.EqualTo ("AttachmentMacInfo"));

			var fields = typeof (TnefNameId).GetFields (BindingFlags.Public | BindingFlags.Static).Where (f => f.FieldType == typeof (TnefNameId)).ToArray ();
			var knownSets = PropertySets.Select (x => x.Value).ToArray ();

			Assert.That (fields, Is.Not.Empty);

			var nameIds = fields.Select (f => (TnefNameId) f.GetValue (null)).ToArray ();
			Assert.That (nameIds, Is.Unique, "each well-known name is unique");

			foreach (var field in fields) {
				var nameId = (TnefNameId) field.GetValue (null);

				Assert.That (knownSets, Does.Contain (nameId.PropertySetGuid), field.Name);
				Assert.That (nameId.PropertySetGuid, Is.Not.EqualTo (TnefPropertySetGuid.InternetHeaders), field.Name);

				if (nameId.Kind == TnefNameIdKind.Name)
					Assert.That (nameId.Name, Is.EqualTo (field.Name), field.Name);
				else
					Assert.That (nameId.Id, Is.InRange (0, 0xFFFF), field.Name);
			}
		}
	}
}